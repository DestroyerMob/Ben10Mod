using System.Collections.Generic;
using Ben10Mod.Common.Command;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Enums;
using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace Ben10Mod.Content.Items.Accessories {
    public abstract class Omnitrix : ModItem {

        public virtual int  MaxOmnitrixEnergy          => 0;
        public virtual int  OmnitrixEnergyRegen        => 0;
        public virtual int  OmnitrixEnergyDrain        => 0;
        public virtual bool UseEnergyForTransformation => false;
        public virtual int  TranformationSwapCost      => 50;
        public virtual int  TimeoutDuration            => 120;
        public virtual int  TransformationDuration     => 300;
        public virtual bool EvolutionFeature           => false;
        public virtual int  EvolutionCost              => 150;

        public int                  transformationNum = 0;
        public TransformationEnum[] transformations   = new TransformationEnum[5];

        public bool wasEquipedLastFrame = false;
        public bool showingUI           = false;

        public Player player = null;

        public Texture2D dynamicTexture;

        public override string Texture => $"Terraria/Images/Item_{ItemID.None}";

        public override void SetDefaults() {
            Item.maxStack   = 1;
            Item.width      = 22;
            Item.height     = 28;
            Item.rare       = ItemRarityID.Master;
            Item.DamageType = ModContent.GetInstance<HeroDamage>();
            Item.accessory  = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "AlienSelection",
                "Alien " + (transformationNum + 1) + ": " + transformations[transformationNum].ToString()));
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            if (player.whoAmI != Main.myPlayer) return;
            this.player = player;
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            omp.omnitrixEquipped = true;
            wasEquipedLastFrame  = true;

            omp.omnitrixEnergyMax += MaxOmnitrixEnergy;

            omp.omnitrixEnergyRegen = omp.isTransformed ? omp.omnitrixEnergyRegen - OmnitrixEnergyDrain : omp.omnitrixEnergyRegen + OmnitrixEnergyRegen;

            transformations = omp.transformations;
            
            HandleAlienSelection(omp);
            
            HandleTransformationKey(omp);

            if (!omp.isTransformed || !UseEnergyForTransformation) return;
            if (omp.omnitrixEnergy > 0)
                TransformationHandler.Transform(player, omp.currTransformation, 2, false, false, EvolutionFeature);
            else
                TransformationHandler.Detransform(player, TimeoutDuration);
        }

        public override void UpdateInventory(Player player) {
            base.UpdateInventory(player);
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (wasEquipedLastFrame) {
                wasEquipedLastFrame = false;
                ModContent.GetInstance<UISystem>().HideMyUI();
                if (player.GetModPlayer<OmnitrixPlayer>().isTransformed) {
                    TransformationHandler.Detransform(player, TimeoutDuration, true, true);
                }
            }
        }

        private void HandleAlienSelection(OmnitrixPlayer omp) {
            bool selectionChanged = false;

            if (KeybindSystem.AlienOneKeybind.JustPressed) {
                transformationNum = 0;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienTwoKeybind.JustPressed) {
                transformationNum = 1;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienThreeKeybind.JustPressed) {
                transformationNum = 2;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienFourKeybind.JustPressed) {
                transformationNum = 3;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienFiveKeybind.JustPressed) {
                transformationNum = 4;
                selectionChanged  = true;
            }
            else if (KeybindSystem.AlienNextKeybind.JustPressed) {
                transformationNum = (transformationNum + 1) % transformations.Length;
                selectionChanged  = true;
                SoundEngine.PlaySound(SoundID.MenuTick, player.position);
            }
            else if (KeybindSystem.AlienPrevKeybind.JustPressed) {
                transformationNum = (transformationNum - 1 + transformations.Length) % transformations.Length;
                selectionChanged  = true;
                SoundEngine.PlaySound(SoundID.MenuTick, player.position);
            }

            if (selectionChanged)
                Main.NewText($"Transformation {transformationNum + 1}: {transformations[transformationNum].GetName()}!",
                    Color.Green);
        }

        private void HandleTransformationKey(OmnitrixPlayer omp) {
            if (!KeybindSystem.TransformationKeybind.JustPressed || omp.onCooldown)
                return;

            TransformationEnum desiredAlien = transformations[transformationNum];

            if (!omp.isTransformed) {
                // Normal transformation
                if (UseEnergyForTransformation)
                    TransformationHandler.Transform(player, desiredAlien, 2);
                else
                    TransformationHandler.Transform(player, desiredAlien, TransformationDuration);
            }
            else {
                // Already transformed
                if (omp.currTransformation != desiredAlien) {
                    // Swap to a different alien while transformed
                    if (UseEnergyForTransformation && omp.omnitrixEnergy >= TranformationSwapCost) {
                        omp.omnitrixEnergy -= TranformationSwapCost;
                        TransformationHandler.Detransform(player, 0, addCooldown: false);
                        TransformationHandler.Transform(player, desiredAlien, 2);
                    }
                }
                else {
                    // Same alien → Ultimate or Detransform
                    if (EvolutionFeature && desiredAlien.HasUltimateForm() && omp.omnitrixEnergy >= EvolutionCost) {
                        if (!omp.ultimateForm) {
                            omp.omnitrixEnergy -= EvolutionCost;
                            TransformationHandler.GoUltimate(player, desiredAlien);
                        }
                        else {
                            TransformationHandler.Detransform(player, 0, addCooldown: false);
                        }
                    }
                    else if (UseEnergyForTransformation || omp.masterControl) {
                        TransformationHandler.Detransform(player, 0, addCooldown: false);
                    }
                }
            }
        }
    }
}