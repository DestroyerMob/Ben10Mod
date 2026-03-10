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

namespace Ben10Mod.Content.Items.Accessories {
    public class PrototypeOmnitrix : Omnitrix {
        public override int MaxOmnitrixEnergy => 300;

        public override string Texture => $"Ben10Mod/Content/Items/Accessories/{this.Name}";

        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.HandsOn}", EquipType.HandsOn, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.HandsOn}", EquipType.HandsOn,
                name: "PrototypeOmnitrixAlt");
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Updating_{EquipType.HandsOn}", EquipType.HandsOn,
                name: "PrototypeOmnitrixUpdating");
        }
        public override ModItem Clone(Item item) {
            PrototypeOmnitrix clone = (PrototypeOmnitrix)base.Clone(item);
            clone.transformationNum = transformationNum;
            clone.transformations   = (TransformationEnum[])transformations?.Clone();
            return clone;
        }
        public override void SaveData(TagCompound tag) {
            tag["selectedAlien"] = transformationNum;
        }
        public override void LoadData(TagCompound tag) {
            tag.TryGet("selectedAlien", out transformationNum);
        }
        public override void OnCreated(ItemCreationContext context) {
            transformationNum = 0;
        }
        public override void SetStaticDefaults() {
            dynamicTexture = ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/PrototypeOmnitrix")
                .Value;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            base.UpdateAccessory(player, hideVisual);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale) {

            if (player == null)
                return true;

            dynamicTexture = player.GetModPlayer<OmnitrixPlayer>().omnitrixUpdating
                ?
                ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/PrototypeOmnitrixUpdating").Value
                : player.GetModPlayer<OmnitrixPlayer>().onCooldown
                    ? ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/PrototypeOmnitrixAlt").Value
                    : ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/PrototypeOmnitrix").Value;

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