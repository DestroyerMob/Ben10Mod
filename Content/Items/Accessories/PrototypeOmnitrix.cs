using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Ben10Mod.Enums;
using System.Collections.Generic;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Placeables;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace Ben10Mod.Content.Items.Accessories
{
    public class PrototypeOmnitrix : Omnitrix
    {
        private Player player = null;
        public int transformationNum = 0;
        public TransformationEnum[] transformations = new TransformationEnum[5];

        bool wasEquipedLastFrame = false;
        bool showingUI = false;

        Texture2D dynamicTexture;

        public override string Texture => $"Ben10Mod/Content/Items/Accessories/{this.Name}";

        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.HandsOn}", EquipType.HandsOn, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.HandsOn}", EquipType.HandsOn, name: "PrototypeOmnitrixAlt");
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Updating_{EquipType.HandsOn}", EquipType.HandsOn, name: "PrototypeOmnitrixUpdating");
        }

        public override ModItem Clone(Item item) {
            PrototypeOmnitrix clone = (PrototypeOmnitrix)base.Clone(item);
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
            dynamicTexture = ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/PrototypeOmnitrix").Value;
        }

        public override void SetDefaults() {
            Item.maxStack = 1;
            Item.width = 22;
            Item.height = 28;
            Item.rare = ItemRarityID.Master;
            Item.accessory = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "AlienSelection", "Alien " + (transformationNum + 1) + ": " + transformations[transformationNum].ToString()));
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            if (player.whoAmI != Main.myPlayer) return;
            this.player = player;
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            omp.omnitrixEquipped  = true;
            omp.prototypeOmnitrix = true;
            wasEquipedLastFrame   = true;

            omp.omnitrixEnergyMax += 300;

            transformations = player.GetModPlayer<OmnitrixPlayer>().transformations;

            if (KeybindSystem.TransformationKeybind.JustPressed && !player.GetModPlayer<OmnitrixPlayer>().isTransformed && !player.GetModPlayer<OmnitrixPlayer>().onCooldown) {
                omp.advancedCircuitMatrixEquippedWhileTransformed = omp.advancedCircuitMatrix;
                TransformationHandler.Transform(player, transformations[transformationNum],
                    omp.advancedCircuitMatrixEquippedWhileTransformed ? omp.transformationTime * 2 : omp.transformationTime);
            }
            else if (KeybindSystem.TransformationKeybind.JustPressed && player.GetModPlayer<OmnitrixPlayer>().isTransformed && !player.GetModPlayer<OmnitrixPlayer>().onCooldown && player.GetModPlayer<OmnitrixPlayer>().masterControl) {
                if (player.GetModPlayer<OmnitrixPlayer>().currTransformation != transformations[transformationNum]) {
                    TransformationHandler.Detransform(player, 0, false, false, false);
                    TransformationHandler.Transform(player, transformations[transformationNum], 
                        omp.advancedCircuitMatrixEquippedWhileTransformed ? omp.transformationTime * 2 : omp.transformationTime);
                } else {
                    TransformationHandler.Detransform(player, omp.cooldownTime, true, false);
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
            base.UpdateInventory(player);
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (wasEquipedLastFrame)
            {
                wasEquipedLastFrame = false;
                ModContent.GetInstance<UISystem>().HideMyUI();
                if (player.GetModPlayer<OmnitrixPlayer>().isTransformed) {
                    TransformationHandler.Detransform(player, omp.cooldownTime, true, true);
                }
            }
        }

        public override bool CanEquipAccessory(Player player, int slot, bool modded) {
            return modded;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {

            if (player == null)
                return true;

            dynamicTexture = player.GetModPlayer<OmnitrixPlayer>().omnitrixUpdating ? ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/PrototypeOmnitrixUpdating").Value : player.GetModPlayer<OmnitrixPlayer>().onCooldown ? ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/PrototypeOmnitrixAlt").Value : ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/PrototypeOmnitrix").Value;

            spriteBatch.Draw(dynamicTexture, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void AddRecipes() {
            base.AddRecipes();

           CreateRecipe()
                .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 25)
                .AddIngredient(ItemID.Lens, 6)
                .AddIngredient(ItemID.Emerald, 3)
                .AddTile(TileID.Anvils).Register();
        }

    }
}