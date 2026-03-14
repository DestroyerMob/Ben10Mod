using System;
using System.Collections.Generic;
using Ben10Mod.Common.Command;
using Ben10Mod.Content.Transformations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace Ben10Mod.Content.Items.Accessories
{
    public abstract class Omnitrix : ModItem
    {
        public virtual int  MaxOmnitrixEnergy          => 0;
        public virtual int  OmnitrixEnergyRegen        => 0;
        public virtual int  OmnitrixEnergyDrain        => 0;
        public virtual int  EnergyPerDamageDivisor     => OmnitrixEnergyRegen == 0 ? 25 : 0;
        public virtual int  MinimumEnergyGainPerHit    => 1;
        public virtual bool UseEnergyForTransformation => false;
        public virtual int  TranformationSwapCost      => 50;
        public virtual int  TimeoutDuration            => 120;
        public virtual int  TransformationDuration     => 300;
        public virtual bool EvolutionFeature           => false;
        public virtual int  EvolutionCost              => 150;
        public virtual int  EvolutionResultItemType    => 0;
        public virtual int  EvolutionAnimationDuration => 120 * 60;
        public virtual bool HideWhileUpdating          => true;
        public virtual string HandsOnTextureKey        => Name;
        public virtual string CooldownHandsOnTextureKey => HandsOnTextureKey;
        public virtual string UpdatingHandsOnTextureKey => HandsOnTextureKey;

        public int         transformationNum   = 0;
        public string[]    transformationSlots = new string[5];

        public bool wasEquipedLastFrame = false;
        public bool showingUI           = false;

        public Player player = null;

        public Texture2D dynamicTexture;

        public override string Texture => $"Terraria/Images/Item_{ItemID.None}";

        public override void SetDefaults()
        {
            Item.maxStack   = 1;
            Item.width      = 22;
            Item.height     = 28;
            Item.rare       = ItemRarityID.Master;
            Item.DamageType = ModContent.GetInstance<HeroDamage>();
            Item.accessory  = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();

            if (string.IsNullOrEmpty(player.currentTransformationId)) {
                tooltips.Add(new TooltipLine(Mod, "Status", "Current Form: None - Transform to begin!"));
                return;
            }

            var trans = TransformationLoader.Get(player.currentTransformationId);
            if (trans == null) {
                tooltips.Add(
                    new TooltipLine(Mod, "Status", $"Current Form: Unknown ({player.currentTransformationId})"));
                return;
            }

            tooltips.Add(new TooltipLine(Mod, "CurrentForm", $"Current Form: {trans.GetDisplayName(player)}"));
            tooltips.Add(new TooltipLine(Mod, "Description", trans.GetDescription(player)));
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            omp.omnitrixEquipped  = true;
            omp.equippedOmnitrix  = this;

            omp.omnitrixEnergyMax += MaxOmnitrixEnergy;

            omp.omnitrixEnergyRegen = omp.isTransformed
                ? omp.omnitrixEnergyRegen - OmnitrixEnergyDrain
                : omp.omnitrixEnergyRegen + OmnitrixEnergyRegen;

            transformationSlots = omp.transformationSlots;

            if (player.whoAmI != Main.myPlayer)
                return;

            this.player = player;
            wasEquipedLastFrame = true;

            HandleAlienSelection(omp);
            HandleTransformationKey(omp);

            if (!omp.isTransformed || !UseEnergyForTransformation) return;

            if (omp.omnitrixEnergy > 0 && !string.IsNullOrEmpty(omp.currentTransformationId))
            {
                TransformationHandler.Transform(player, omp.currentTransformationId, GetTransformationDuration(omp),
                    showParticles: false, playSound: false);
            }
            else
            {
                DetransformFromEnergyDepletion(player, omp);
            }
        }

        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (wasEquipedLastFrame)
            {
                wasEquipedLastFrame = false;
                ModContent.GetInstance<UISystem>().HideMyUI();

                if (player.GetModPlayer<OmnitrixPlayer>().isTransformed)
                {
                    HandleUnequip(player, omp);
                }
            }
        }

        private void HandleAlienSelection(OmnitrixPlayer omp)
        {
            bool selectionChanged = false;

            if (KeybindSystem.AlienOneKeybind.JustPressed)
            {
                transformationNum = 0;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienTwoKeybind.JustPressed)
            {
                transformationNum = 1;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienThreeKeybind.JustPressed)
            {
                transformationNum = 2;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienFourKeybind.JustPressed)
            {
                transformationNum = 3;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienFiveKeybind.JustPressed)
            {
                transformationNum = 4;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienNextKeybind.JustPressed)
            {
                transformationNum = (transformationNum + 1) % transformationSlots.Length;
                selectionChanged  = true;
                SoundEngine.PlaySound(SoundID.MenuTick, player.position);
            }
            else if (KeybindSystem.AlienPrevKeybind.JustPressed)
            {
                transformationNum = (transformationNum - 1 + transformationSlots.Length) % transformationSlots.Length;
                selectionChanged  = true;
                SoundEngine.PlaySound(SoundID.MenuTick, player.position);
            }

            if (selectionChanged)
            {
                string name = "Empty Slot";
                if (transformationNum < transformationSlots.Length)
                {
                    var trans = TransformationLoader.Get(transformationSlots[transformationNum]);
                    if (trans != null)
                        name = trans.TransformationName;
                }

                Main.NewText($"Transformation {transformationNum + 1}: {name}!", Color.Green);
            }
        }

        private void HandleTransformationKey(OmnitrixPlayer omp)
        {
            if (!KeybindSystem.TransformationKeybind.JustPressed || omp.onCooldown)
                return;

            if (transformationNum >= transformationSlots.Length) return;

            string desiredId = transformationSlots[transformationNum];
            if (string.IsNullOrEmpty(desiredId)) return;

            if (!omp.isTransformed)
            {
                TransformationHandler.Transform(player, desiredId, GetTransformationDuration(omp));
            }
            else
            {
                if (omp.currentTransformationId != desiredId)
                {
                    var currentTransformation = omp.CurrentTransformation;
                    if (currentTransformation?.TryHandleTransformKeyWhileActive(player, omp, this, desiredId) == true)
                        return;

                    if (omp.masterControl || (UseEnergyForTransformation && omp.omnitrixEnergy >= TranformationSwapCost))
                    {
                        if (!omp.masterControl && UseEnergyForTransformation)
                            omp.omnitrixEnergy -= TranformationSwapCost;

                        int nextDuration = UseEnergyForTransformation
                            ? GetTransformationDuration(omp)
                            : GetRemainingTransformationDurationSeconds(omp);

                        TransformationHandler.Detransform(player, 0, showParticles: false, addCooldown: false);
                        TransformationHandler.Transform(player, desiredId, nextDuration);
                    }
                }
                else
                {
                    var currentTransformation = omp.CurrentTransformation;
                    if (currentTransformation?.TryHandleTransformKeyWhileActive(player, omp, this, desiredId) == true)
                        return;

                    if (UseEnergyForTransformation || omp.masterControl)
                    {
                        TransformationHandler.Detransform(player, 0, addCooldown: false);
                    }
                }
            }
        }

        public virtual int GetTransformationDuration(OmnitrixPlayer omp) {
            int baseDuration = UseEnergyForTransformation ? 2 : TransformationDuration;
            return ApplyTransformationDurationModifiers(baseDuration, omp);
        }

        public virtual int GetDetransformCooldownDuration(OmnitrixPlayer omp) {
            int baseDuration = UseEnergyForTransformation ? 0 : TimeoutDuration;
            return ApplyCooldownDurationModifiers(baseDuration, omp);
        }

        public virtual int GetBranchTransformationDuration(OmnitrixPlayer omp) {
            return UseEnergyForTransformation
                ? GetTransformationDuration(omp)
                : GetRemainingTransformationDurationSeconds(omp);
        }

        public virtual bool ShouldAddDetransformCooldown(OmnitrixPlayer omp) {
            return !UseEnergyForTransformation;
        }

        public virtual void HandleForcedDetransform(Player player, OmnitrixPlayer omp) {
            TransformationHandler.Detransform(player, GetDetransformCooldownDuration(omp),
                addCooldown: ShouldAddDetransformCooldown(omp));
        }

        public virtual void HandleUnequip(Player player, OmnitrixPlayer omp) {
            TransformationHandler.Detransform(player, GetDetransformCooldownDuration(omp), showParticles: true,
                addCooldown: true);
        }

        public virtual void DetransformFromEnergyDepletion(Player player, OmnitrixPlayer omp) {
            TransformationHandler.Detransform(player, GetDetransformCooldownDuration(omp));
        }

        public virtual int GetEnergyGainFromDamage(int damageDone) {
            if (EnergyPerDamageDivisor <= 0 || damageDone <= 0)
                return 0;

            return Math.Max(damageDone / EnergyPerDamageDivisor, MinimumEnergyGainPerHit);
        }

        public virtual bool ShouldStartEvolution(Player player, OmnitrixPlayer omp, int defeatedNpcType) {
            return false;
        }

        public virtual bool CanUseEvolutionFeature(Player player, OmnitrixPlayer omp, Transformation transformation) {
            return EvolutionFeature;
        }

        public virtual bool CanUseChildTransformation(Player player, OmnitrixPlayer omp, Transformation current,
            Transformation child, Transformation selectedTransformation) {
            return true;
        }

        public virtual bool CanDNASplice(Player player, OmnitrixPlayer omp, Transformation current,
            Transformation selectedTransformation, Transformation child) {
            return false;
        }

        public virtual void StartEvolution(Player player, OmnitrixPlayer omp) {
            TransformationHandler.Detransform(player, GetDetransformCooldownDuration(omp));
            player.AddBuff(ModContent.BuffType<Buffs.Abilities.OmnitrixUpdating>(), EvolutionAnimationDuration);
        }

        public virtual int GetEvolutionResultItemType(Player player, OmnitrixPlayer omp) {
            return EvolutionResultItemType;
        }

        public virtual void CompleteEvolution(Player player, OmnitrixPlayer omp, Item equippedItem) {
            int resultType = GetEvolutionResultItemType(player, omp);
            if (resultType <= 0 || equippedItem.type == resultType)
                return;

            equippedItem.SetDefaults(resultType);
        }

        public virtual string GetHandsOnTextureKey(Player player, OmnitrixPlayer omp) {
            if (omp.omnitrixUpdating && !string.IsNullOrEmpty(UpdatingHandsOnTextureKey))
                return UpdatingHandsOnTextureKey;

            if (omp.onCooldown && !string.IsNullOrEmpty(CooldownHandsOnTextureKey))
                return CooldownHandsOnTextureKey;

            return HandsOnTextureKey;
        }

        public virtual void ApplyHandVisuals(Player player, OmnitrixPlayer omp, bool hideVisuals) {
            if (hideVisuals || omp.isTransformed)
                return;

            string textureKey = GetHandsOnTextureKey(player, omp);
            if (string.IsNullOrEmpty(textureKey))
                return;

            player.handon = EquipLoader.GetEquipSlot(Mod, textureKey, EquipType.HandsOn);
        }

        protected virtual float GetTransformationDurationMultiplier(OmnitrixPlayer omp) {
            return omp.transformationDurationMultiplier;
        }

        protected virtual float GetCooldownDurationMultiplier(OmnitrixPlayer omp) {
            if (!string.IsNullOrEmpty(omp.currentTransformationId))
                return omp.activeCooldownDurationMultiplier;

            return omp.cooldownDurationMultiplier;
        }

        protected int ApplyTransformationDurationModifiers(int baseDuration, OmnitrixPlayer omp) {
            return ApplyDurationMultiplier(baseDuration, GetTransformationDurationMultiplier(omp));
        }

        protected int ApplyCooldownDurationModifiers(int baseDuration, OmnitrixPlayer omp) {
            return ApplyDurationMultiplier(baseDuration, GetCooldownDurationMultiplier(omp));
        }

        protected static int ApplyDurationMultiplier(int baseDuration, float multiplier) {
            if (baseDuration <= 0)
                return 0;

            float safeMultiplier = Math.Max(0f, multiplier);
            return Math.Max(1, (int)Math.Round(baseDuration * safeMultiplier));
        }

        private int GetRemainingTransformationDurationSeconds(OmnitrixPlayer omp) {
            var currentTransformation = omp.CurrentTransformation;
            if (currentTransformation?.TransformationBuffId > 0) {
                for (int i = 0; i < player.buffType.Length; i++) {
                    if (player.buffType[i] == currentTransformation.TransformationBuffId)
                        return Math.Max(1, (int)Math.Ceiling(player.buffTime[i] / 60f));
                }
            }

            return GetTransformationDuration(omp);
        }
    }
}
