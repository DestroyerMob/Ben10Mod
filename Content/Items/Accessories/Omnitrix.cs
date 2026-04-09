using System;
using System.Collections.Generic;
using Ben10Mod.Common.Command;
using Ben10Mod.Common.Systems;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Prefixes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.Utilities;

namespace Ben10Mod.Content.Items.Accessories
{
    public abstract class Omnitrix : ModItem, IHeroAlterationAccessory
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
        public virtual int  ItemValue                  => Item.buyPrice(gold: 2);
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
            Item.value      = ItemValue;
            Item.DamageType = ModContent.GetInstance<HeroDamage>();
            Item.accessory  = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            if (IsBlacklisted()) {
                tooltips.Add(new TooltipLine(Mod, "Disabled",
                    "Disabled by the Ben10Mod feature blacklist."));
                return;
            }

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

        public override bool CanEquipAccessory(Player player, int slot, bool modded) {
            return !IsBlacklisted() && base.CanEquipAccessory(player, slot, modded);
        }

        public override int ChoosePrefix(UnifiedRandom rand) {
            List<int> rollablePrefixes = OmnitrixPrefix.GetRollablePrefixTypes(Item);
            if (rollablePrefixes.Count == 0)
                return -1;

            return rollablePrefixes[rand.Next(rollablePrefixes.Count)];
        }

        public override bool CanReforge() {
            return !IsBlacklisted();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (IsBlacklisted())
                return;

            var omp = player.GetModPlayer<OmnitrixPlayer>();
            if (omp.osmosianEquipped)
                return;

            omp.omnitrixEquipped  = true;
            omp.equippedOmnitrix  = this;

            omp.omnitrixEnergyMax += GetEffectiveOmnitrixEnergyMax(omp);

            omp.omnitrixEnergyRegen = omp.isTransformed
                ? omp.omnitrixEnergyRegen - GetEffectiveOmnitrixEnergyDrain(omp)
                : omp.omnitrixEnergyRegen + GetEffectiveOmnitrixEnergyRegen(omp);

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
            if (transformationSlots == null || transformationSlots.Length == 0)
                return;

            if (KeybindSystem.AlienOneKeybind.JustPressed)
                TrySelectRosterSlot(player, 0, playSound: false, showText: true);
            else if (KeybindSystem.AlienTwoKeybind.JustPressed)
                TrySelectRosterSlot(player, 1, playSound: false, showText: true);
            else if (KeybindSystem.AlienThreeKeybind.JustPressed)
                TrySelectRosterSlot(player, 2, playSound: false, showText: true);
            else if (KeybindSystem.AlienFourKeybind.JustPressed)
                TrySelectRosterSlot(player, 3, playSound: false, showText: true);
            else if (KeybindSystem.AlienFiveKeybind.JustPressed)
                TrySelectRosterSlot(player, 4, playSound: false, showText: true);
            else if (KeybindSystem.AlienNextKeybind.JustPressed)
                TrySelectRosterSlot(player, (transformationNum + 1) % transformationSlots.Length, playSound: true,
                    showText: true);
            else if (KeybindSystem.AlienPrevKeybind.JustPressed)
                TrySelectRosterSlot(player, (transformationNum - 1 + transformationSlots.Length) % transformationSlots.Length,
                    playSound: true, showText: true);
        }

        private void HandleTransformationKey(OmnitrixPlayer omp)
        {
            if (!KeybindSystem.TransformationKeybind.JustPressed)
                return;

            TryTransformSelectedSlot(player, omp);
        }

        private string GetRosterSlotDisplayName(Player player, int slotIndex) {
            if (slotIndex < 0 || slotIndex >= transformationSlots.Length)
                return "Empty Slot";

            string transformationId = transformationSlots[slotIndex];
            if (string.IsNullOrEmpty(transformationId))
                return "Empty Slot";

            var trans = TransformationLoader.Get(transformationId);
            if (trans == null)
                return "Empty Slot";

            return player.GetModPlayer<OmnitrixPlayer>().GetTransformationBaseName(trans);
        }

        public bool TrySelectRosterSlot(Player player, int slotIndex, bool playSound, bool showText) {
            if (player == null || transformationSlots == null || transformationSlots.Length == 0)
                return false;

            if (slotIndex < 0 || slotIndex >= transformationSlots.Length)
                return false;

            bool changed = transformationNum != slotIndex;
            transformationNum = slotIndex;

            if (playSound && changed)
                SoundEngine.PlaySound(SoundID.MenuTick, player.position);

            if (showText)
                Main.NewText($"Transformation {slotIndex + 1}: {GetRosterSlotDisplayName(player, slotIndex)}!", Color.Green);

            return true;
        }

        public bool TryTransformToSlot(Player player, OmnitrixPlayer omp, int slotIndex) {
            if (!TrySelectRosterSlot(player, slotIndex, playSound: false, showText: false)) {
                omp?.ShowTransformFailureFeedback("That Omnitrix slot is unavailable.");
                return false;
            }

            return TryTransformSelectedSlot(player, omp);
        }

        public bool TryTransformToTransformationId(Player player, OmnitrixPlayer omp, string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null) {
                omp?.ShowTransformFailureFeedback("That transformation is unavailable.");
                return false;
            }

            return TryTransformToTransformation(player, omp, transformation.FullID);
        }

        public bool TryTransformSelectedSlot(Player player, OmnitrixPlayer omp) {
            if (player == null || omp == null)
                return false;

            if (transformationNum < 0 || transformationNum >= transformationSlots.Length)
                return false;

            string desiredId = transformationSlots[transformationNum];
            if (string.IsNullOrEmpty(desiredId)) {
                omp.ShowTransformFailureFeedback($"Slot {transformationNum + 1} is empty.");
                return false;
            }

            return TryTransformToTransformation(player, omp, desiredId);
        }

        private bool TryTransformToTransformation(Player player, OmnitrixPlayer omp, string desiredId) {
            if (player == null || omp == null)
                return false;

            if (omp.onCooldown) {
                omp.ShowTransformFailureFeedback($"Omnitrix cooling down. {omp.GetTransformationCooldownDisplayText()}");
                return false;
            }

            Transformation desiredTransformation = TransformationLoader.Resolve(desiredId);
            if (desiredTransformation == null) {
                omp.ShowTransformFailureFeedback("That transformation is unavailable.");
                return false;
            }

            desiredId = desiredTransformation.FullID;
            if (!omp.unlockedTransformations.Contains(desiredId)) {
                omp.ShowTransformFailureFeedback($"{omp.GetTransformationBaseName(desiredTransformation)} is not unlocked.");
                return false;
            }

            bool wasRandomized = false;
            string targetId = omp.ResolveRandomizedTransformationTarget(desiredId, out wasRandomized);
            if (string.IsNullOrEmpty(targetId))
                targetId = desiredId;

            Transformation targetTransformation = TransformationLoader.Resolve(targetId);
            if (targetTransformation == null) {
                omp.ShowTransformFailureFeedback("That transformation is unavailable.");
                return false;
            }

            targetId = targetTransformation.FullID;

            for (int i = 0; i < transformationSlots.Length; i++) {
                if (string.Equals(transformationSlots[i], desiredId, StringComparison.OrdinalIgnoreCase)) {
                    transformationNum = i;
                    break;
                }
            }

            if (!omp.isTransformed) {
                TransformationHandler.Transform(player, targetId, GetTransformationDuration(omp));
                if (wasRandomized)
                    omp.ShowTransformationRandomizerFeedback(targetId);
                return true;
            }

            if (omp.currentTransformationId != targetId) {
                var currentTransformation = omp.CurrentTransformation;
                if (currentTransformation?.TryHandleTransformKeyWhileActive(player, omp, this, targetId) == true) {
                    if (wasRandomized)
                        omp.ShowTransformationRandomizerFeedback(targetId);
                    return true;
                }

                if (!omp.masterControl && !UseEnergyForTransformation) {
                    omp.ShowTransformFailureFeedback("Detransform first or unlock Master Control to switch forms.");
                    return false;
                }

                int swapCost = GetEffectiveTransformationSwapCost(omp);
                if (!omp.masterControl && UseEnergyForTransformation && omp.omnitrixEnergy < swapCost) {
                    omp.ShowTransformFailureFeedback($"Need {swapCost} OE to swap forms.");
                    return false;
                }

                if (!omp.masterControl && UseEnergyForTransformation)
                    omp.omnitrixEnergy -= swapCost;

                int nextDuration = UseEnergyForTransformation
                    ? GetTransformationDuration(omp)
                    : GetRemainingTransformationDurationSeconds(omp);

                TransformationHandler.Detransform(player, 0, showParticles: false, addCooldown: false);
                TransformationHandler.Transform(player, targetId, nextDuration);
                if (wasRandomized)
                    omp.ShowTransformationRandomizerFeedback(targetId);
                return true;
            }

            var activeTransformation = omp.CurrentTransformation;
            if (activeTransformation?.TryHandleTransformKeyWhileActive(player, omp, this, targetId) == true) {
                if (wasRandomized)
                    omp.ShowTransformationRandomizerFeedback(targetId);
                return true;
            }

            if (!UseEnergyForTransformation && !omp.masterControl) {
                omp.ShowTransformFailureFeedback("Detransform first or unlock Master Control to cancel the active form.");
                return false;
            }

            TransformationHandler.Detransform(player, 0, addCooldown: false);
            return true;
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
            if (resultType <= 0)
                return;

            equippedItem ??= omp.GetActiveOmnitrixItem();
            if (equippedItem == null || equippedItem.IsAir || equippedItem.type == resultType)
                return;

            equippedItem.SetDefaults(resultType);

            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);

                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.SyncOmnitrixEvolution);
                packet.Write((byte)player.whoAmI);
                packet.Write(resultType);
                packet.Send(toClient: player.whoAmI);
            }
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

            var heroSlot = ModContent.GetInstance<OmnitrixSlot>();
            int handShader = heroSlot?.DyeItem?.dye ?? 0;
            player.cHandOn = handShader;
            player.cHandOff = handShader;
            player.handon = EquipLoader.GetEquipSlot(Mod, textureKey, EquipType.HandsOn);
        }

        protected virtual float GetTransformationDurationMultiplier(OmnitrixPlayer omp) {
            float prefixMultiplier = GetActiveOmnitrixPrefix()?.TransformationDurationMultiplier ?? 1f;
            return omp.transformationDurationMultiplier * prefixMultiplier;
        }

        protected virtual float GetCooldownDurationMultiplier(OmnitrixPlayer omp) {
            float prefixMultiplier = GetActiveOmnitrixPrefix()?.CooldownDurationMultiplier ?? 1f;
            if (!string.IsNullOrEmpty(omp.currentTransformationId))
                return omp.activeCooldownDurationMultiplier * prefixMultiplier;

            return omp.cooldownDurationMultiplier * prefixMultiplier;
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

        private OmnitrixPrefix GetActiveOmnitrixPrefix() {
            return PrefixLoader.GetPrefix(Item.prefix) as OmnitrixPrefix;
        }

        private int GetEffectiveOmnitrixEnergyMax(OmnitrixPlayer omp) {
            int prefixBonus = GetActiveOmnitrixPrefix()?.OmnitrixEnergyMaxBonus ?? 0;
            return Math.Max(0, MaxOmnitrixEnergy + omp.omnitrixEnergyMaxBonus + prefixBonus);
        }

        private int GetEffectiveOmnitrixEnergyRegen(OmnitrixPlayer omp) {
            int prefixBonus = GetActiveOmnitrixPrefix()?.OmnitrixEnergyRegenBonus ?? 0;
            return Math.Max(0, OmnitrixEnergyRegen + omp.omnitrixEnergyRegenBonus + prefixBonus);
        }

        private int GetEffectiveOmnitrixEnergyDrain(OmnitrixPlayer omp) {
            int prefixBonus = GetActiveOmnitrixPrefix()?.OmnitrixEnergyDrainBonus ?? 0;
            return Math.Max(0, OmnitrixEnergyDrain + prefixBonus);
        }

        private int GetEffectiveTransformationSwapCost(OmnitrixPlayer omp) {
            int prefixBonus = GetActiveOmnitrixPrefix()?.TransformationSwapCostBonus ?? 0;
            return Math.Max(0, TranformationSwapCost + prefixBonus);
        }

        private bool IsBlacklisted() {
            return Ben10FeatureBlacklistRegistry.IsFeatureBlacklisted(Ben10FeatureType.Omnitrix, Mod);
        }
    }
}
