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
        public virtual bool UseEnergyForTransformation => false;
        public virtual int  TranformationSwapCost      => 50;
        public virtual int  TimeoutDuration            => 120;
        public virtual int  TransformationDuration     => 300;
        public virtual bool EvolutionFeature           => false;
        public virtual int  EvolutionCost              => 150;

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
            // Keep any existing tooltip lines you already added here

            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();

            // Safety net - no transformation active yet
            if (string.IsNullOrEmpty(player
                    .currentTransformationId)) // ← change variable name if yours is different (e.g. CurrentForm, ActiveAlien, etc.)
            {
                tooltips.Add(new TooltipLine(Mod, "Status", "Current Form: None - Transform to begin!"));
                return;
            }

            var trans = TransformationLoader.Get(player.currentTransformationId); // ← your loader's Get method
            if (trans == null) {
                tooltips.Add(
                    new TooltipLine(Mod, "Status", $"Current Form: Unknown ({player.currentTransformationId})"));
                return;
            }

            // Safe data usage now that we know it exists
            tooltips.Add(new TooltipLine(Mod, "CurrentForm", $"Current Form: {trans.TransformationName}"));
            tooltips.Add(new TooltipLine(Mod, "Description", trans.Description));
            // Add any other lines you had (attacks, cooldowns, etc.) using trans.XXX
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.whoAmI != Main.myPlayer) return;

            this.player = player;
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            omp.omnitrixEquipped = true;
            wasEquipedLastFrame  = true;

            omp.omnitrixEnergyMax += MaxOmnitrixEnergy;

            omp.omnitrixEnergyRegen = omp.isTransformed
                ? omp.omnitrixEnergyRegen - OmnitrixEnergyDrain
                : omp.omnitrixEnergyRegen + OmnitrixEnergyRegen;

            // Sync the current roster from the player
            transformationSlots = omp.transformationSlots;

            HandleAlienSelection(omp);
            HandleTransformationKey(omp);

            // Energy-based transformation maintenance (for Recalibrated Omnitrix etc.)
            if (!omp.isTransformed || !UseEnergyForTransformation) return;

            if (omp.omnitrixEnergy > 0 && !string.IsNullOrEmpty(omp.currentTransformationId))
            {
                TransformationHandler.Transform(player, omp.currentTransformationId, 2, showParticles: false, playSound: false);
            }
            else
            {
                TransformationHandler.Detransform(player, TimeoutDuration);
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
                    TransformationHandler.Detransform(player, TimeoutDuration, showParticles: true, addCooldown: true);
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
            if (string.IsNullOrEmpty(desiredId)) return; // empty slot

            if (!omp.isTransformed)
            {
                // Normal transformation
                if (UseEnergyForTransformation)
                    TransformationHandler.Transform(player, desiredId, 2);
                else
                    TransformationHandler.Transform(player, desiredId, TransformationDuration);
            }
            else
            {
                // Already transformed
                if (omp.currentTransformationId != desiredId)
                {
                    // Swap to a different alien while transformed
                    if (UseEnergyForTransformation && omp.omnitrixEnergy >= TranformationSwapCost)
                    {
                        omp.omnitrixEnergy -= TranformationSwapCost;
                        TransformationHandler.Detransform(player, 0, showParticles: false, addCooldown: false);
                        TransformationHandler.Transform(player, desiredId, 2);
                    }
                }
                else
                {
                    // Same alien → Detransform (or Ultimate if you re-enable that later)
                    if (UseEnergyForTransformation || omp.masterControl)
                    {
                        TransformationHandler.Detransform(player, 0, addCooldown: false);
                    }
                }
            }
        }
    }
}
