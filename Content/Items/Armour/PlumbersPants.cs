using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Armour {
    [AutoloadEquip(EquipType.Legs)]
    public class PlumbersPants : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 14;

            Item.value = Item.buyPrice(silver: 100);
            Item.rare = ItemRarityID.White;
            Item.defense = 3;

        }

        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OmnitrixPlayer>().transformedMoveSpeedBonus += 0.05f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "EquipBonus", "+5% movement speed while transformed"));
        }

        public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
            ref Color glowMaskColor) {
            color = PlumberArmorPalette.Blend(color, PlumberArmorPalette.ResolveSharedEarlySetColor(drawPlayer));
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
            Color itemColor, Vector2 origin, float scale) {
            return PlumberArmorPalette.DrawInventory(this, spriteBatch, position, frame, drawColor, origin, scale,
                PlumberArmorPalette.Neutral);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI) {
            return PlumberArmorPalette.DrawWorld(this, spriteBatch, alphaColor, ref rotation, ref scale,
                PlumberArmorPalette.Neutral);
        }

        public override void AddRecipes()
        {
            base.AddRecipes();

            Recipe recipe = CreateRecipe()
                .AddIngredient(ItemID.IronBar, 20)
                .AddTile(TileID.Anvils).Register();

            recipe = CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 20)
                .AddTile(TileID.Anvils).Register();

        }
    }
}
