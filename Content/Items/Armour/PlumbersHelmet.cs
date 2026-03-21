using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Armour {
    [AutoloadEquip(EquipType.Head)]
    public class PlumbersHelmet : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 14;

            Item.value = Item.buyPrice(silver: 90);
            Item.rare = ItemRarityID.White;
            Item.defense = 3;
        }

        public override void UpdateEquip(Player player) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            omp.transformedDefenseBonus += 2;
            player.GetArmorPenetration<HeroDamage>() += 4;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "EquipBonus", "+2 defense while transformed and +4 hero armor penetration"));
        }

        public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
            ref Color glowMaskColor) {
            color = PlumberArmorPalette.Blend(color, PlumberArmorPalette.Vanguard);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
            Color itemColor, Vector2 origin, float scale) {
            return PlumberArmorPalette.DrawInventory(this, spriteBatch, position, frame, drawColor, origin, scale,
                PlumberArmorPalette.Vanguard);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI) {
            return PlumberArmorPalette.DrawWorld(this, spriteBatch, alphaColor, ref rotation, ref scale,
                PlumberArmorPalette.Vanguard);
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) {
            bool bodyMatch = body.type == ModContent.ItemType<PlumbersShirt>();
            bool legsMatch = legs.type == ModContent.ItemType<PlumbersPants>();

            return bodyMatch && legsMatch;
        }

        public override void UpdateArmorSet(Player player) {

            player.setBonus = "While transformed: +8 defense and +4% endurance. Also grants +0.6 hero knockback";
            
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (omp.isTransformed) {
                omp.transformedDefenseBonus += 8;
                omp.transformedEnduranceBonus += 0.04f;
            }

            player.GetKnockback<HeroDamage>() += 0.6f;
        }

        public override void AddRecipes()
        {
            base.AddRecipes();

            Recipe recipe = CreateRecipe()
                .AddIngredient(ItemID.IronBar, 15)
                .AddTile(TileID.Anvils).Register();

            recipe = CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 15)
                .AddTile(TileID.Anvils).Register();

        }
    }
}
