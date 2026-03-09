using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Ben10Mod.Enums;
using System.Collections.Generic;
using Ben10Mod.Content.Interface;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace Ben10Mod.Content.Items.Accessories
{
    public class Ultimatrix : Omnitrix {

        private Player player = null;
        public int transformationNum = 0;
        public TransformationEnum[] transformations = new TransformationEnum[5];

        bool wasEquipedLastFrame = false;

        Texture2D dynamicTexture;

        public override string Texture => $"Ben10Mod/Content/Items/Accessories/{this.Name}";

        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.HandsOn}", EquipType.HandsOn, name: "Ultimatrix");
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.HandsOn}", EquipType.HandsOn, name: "UltimatrixAlt");
        }

        public override ModItem Clone(Item item) {
            Ultimatrix clone = (Ultimatrix)base.Clone(item);
            clone.transformationNum = transformationNum;
            clone.transformations = (TransformationEnum[])transformations?.Clone();
            return clone;
        }

        public override void SaveData(TagCompound tag) {
            tag["selectedAlien"] = transformationNum;
        }

        public override void LoadData(TagCompound tag)
        {
            tag.TryGet("selectedAlien", out transformationNum);
        }

        public override void OnCreated(ItemCreationContext context)
        {
            transformationNum = 0;
        }

        public override void SetStaticDefaults() {
            dynamicTexture = ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/Ultimatrix").Value;
        }

        public override void SetDefaults() {
            Item.maxStack             = 1;
            Item.width                = 22;
            Item.height               = 28;
            Item.rare                 = ItemRarityID.Master;
            Item.accessory            = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "AlienSelection",
                "Alien " + (transformationNum + 1) + ": " + transformations[transformationNum].ToString()));
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            
            if (player.whoAmI != Main.myPlayer) return;
            
            this.player = player;
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            omp.omnitrixEnergyMax += 700;
            omp.omnitrixEquipped  =  true;
            wasEquipedLastFrame   =  true;
            
            if (omp.isTransformed) 
                omp.omnitrixEnergyRegen -= 1;

            if (omp.omnitrixEnergy <= 0 && omp.isTransformed) {
                TransformationHandler.Detransform(player, 120);
            }

            if (omp.omnitrixEnergy > 0 && omp.isTransformed) {
                TransformationHandler.Transform(player, omp.currTransformation, 2, false,false);
            }
            if (omp.isTransformed) 
                omp.omnitrixEnergyRegen -= 1;
            else 
                omp.omnitrixEnergyRegen += 3;
            

            transformations = omp.transformations;

            if (KeybindSystem.TransformationKeybind.JustPressed && !omp.isTransformed && !omp.onCooldown) {
                TransformationHandler.Transform(player, transformations[transformationNum], 2);
            }
            else if (KeybindSystem.TransformationKeybind.JustPressed && omp.isTransformed && !omp.onCooldown) {
                if (omp.currTransformation != transformations[transformationNum]) {
                    omp.omnitrixEnergy -= 50;
                    omp.omnitrixEnergy =  Math.Max(omp.omnitrixEnergy, 0);
                    if (omp.omnitrixEnergy > 0) {
                        TransformationHandler.Detransform(player, 0, false, false);
                        TransformationHandler.Transform(player, transformations[transformationNum], 2);
                    }
                    else {
                        TransformationHandler.Detransform(player, 60);
                    }
                } else if (omp.currTransformation.HasUltimateForm() && !omp.ultimateForm) {
                    omp.omnitrixEnergy -= 150;
                    omp.omnitrixEnergy =  Math.Max(omp.omnitrixEnergy, 0);
                    if (omp.omnitrixEnergy > 0) {
                        TransformationHandler.GoUltimate(player, omp.currTransformation);
                    }
                } else { 
                    TransformationHandler.Detransform(player, 0, true, false);
                }
            }
            else if (KeybindSystem.AlienOneKeybind.JustPressed) {
                transformationNum = 0;
                Main.NewText("Transformation " + (transformationNum + 1) + ": " + transformations[transformationNum].GetName() + "!", Color.Green);
            }
            else if (KeybindSystem.AlienTwoKeybind.JustPressed) {
                transformationNum = 1;
                Main.NewText("Transformation " + (transformationNum + 1) + ": " + transformations[transformationNum].GetName() + "!", Color.Green);
            }
            else if (KeybindSystem.AlienThreeKeybind.JustPressed) {
                transformationNum = 2;
                Main.NewText("Transformation " + (transformationNum + 1) + ": " + transformations[transformationNum].GetName() + "!", Color.Green);
            }
            else if (KeybindSystem.AlienFourKeybind.JustPressed) {
                transformationNum = 3;
                Main.NewText("Transformation " + (transformationNum + 1) + ": " + transformations[transformationNum].GetName() + "!", Color.Green);
            }
            else if (KeybindSystem.AlienFiveKeybind.JustPressed) {
                transformationNum = 4;
                Main.NewText("Transformation " + (transformationNum + 1) + ": " + transformations[transformationNum].GetName() + "!", Color.Green);
            }
            else if (KeybindSystem.AlienNextKeybind.JustPressed) {
                transformationNum++;
                if (transformationNum > transformations.Length - 1) {
                    transformationNum = 0;
                }
                SoundEngine.PlaySound(SoundID.MenuTick, player.position);
                Main.NewText("Transformation " + (transformationNum + 1) + ": " + transformations[transformationNum].GetName() + "!", Color.Green);
            }
            else if (KeybindSystem.AlienPrevKeybind.JustPressed) {
                transformationNum--;
                if (transformationNum < 0) {
                    transformationNum = transformations.Length - 1;
                }
                SoundEngine.PlaySound(SoundID.MenuTick, player.position);
                Main.NewText("Transformation " + (transformationNum + 1) + ": " + transformations[transformationNum].GetName() + "!", Color.Green);
            }

            base.UpdateAccessory(player, hideVisual);
        }

        public override void UpdateInventory(Player player)
        {
            
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            
            base.UpdateInventory(player);

            if (wasEquipedLastFrame)
            {
                wasEquipedLastFrame = false;
                ModContent.GetInstance<UISystem>().HideMyUI();
                if (player.GetModPlayer<OmnitrixPlayer>().isTransformed) {
                    TransformationHandler.Detransform(player, omp.cooldownTime, true, true);
                } else {
                    TransformationHandler.Detransform(player, omp.cooldownTime, false, false, false);
                }
            }
        }

        public override bool CanEquipAccessory(Player player, int slot, bool modded) {
            return modded;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {

            if (player == null)
                return true;

            dynamicTexture = player.GetModPlayer<OmnitrixPlayer>().onCooldown ? ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/UltimatrixAlt").Value : ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/Ultimatrix").Value;

            spriteBatch.Draw(dynamicTexture, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void AddRecipes() {
            base.AddRecipes();

            // Recipe recipeAlt = CreateRecipe()
            //     .AddIngredient(ModContent.ItemType<RecalibratedOmnitrix>())
            //     .AddIngredient(ItemID.SoulofNight, 8)
            //     .AddIngredient(ItemID.SoulofLight, 8)
            //     .AddTile(TileID.MythrilAnvil).Register();

        }
    }
}