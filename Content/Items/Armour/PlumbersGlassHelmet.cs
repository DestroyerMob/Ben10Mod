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
    public class PlumbersGlassHelmet : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 14;

            Item.defense = 2;
            Item.value = Item.buyPrice(silver: 95);
            Item.rare = ItemRarityID.White;
        }

        public override void UpdateEquip(Player player) {
            player.GetCritChance<HeroDamage>() += 6f;
            player.GetAttackSpeed<HeroDamage>() += 0.04f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "EquipBonus", "+6 hero crit and +4% hero attack speed"));
        }

        public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
            ref Color glowMaskColor) {
            color = PlumberArmorPalette.Blend(color, PlumberArmorPalette.Scout);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
            Color itemColor, Vector2 origin, float scale) {
            return PlumberArmorPalette.DrawInventory(this, spriteBatch, position, frame, drawColor, origin, scale,
                PlumberArmorPalette.Scout);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI) {
            return PlumberArmorPalette.DrawWorld(this, spriteBatch, alphaColor, ref rotation, ref scale,
                PlumberArmorPalette.Scout);
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) {
            bool bodyMatch = body.type == ModContent.ItemType<PlumbersShirt>();
            bool legsMatch = legs.type == ModContent.ItemType<PlumbersPants>();

            return bodyMatch && legsMatch;
        }

        public override void UpdateArmorSet(Player player) {

            player.setBonus = "While transformed: +12% movement speed and improved jump height. Also grants +10 hero crit";
            
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (omp.isTransformed) {
                omp.transformedMoveSpeedBonus += 0.12f;
                omp.transformedJumpSpeedBonus += 1.6f;
            }

            player.GetCritChance<HeroDamage>() += 10f;
        }

        public override void AddRecipes()
        {
            base.AddRecipes();

            Recipe recipe = CreateRecipe()
                .AddIngredient(ItemID.IronBar, 5)
                .AddIngredient(ItemID.Glass, 10)
                .AddTile(TileID.Anvils).Register();

            recipe = CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 5)
                .AddIngredient(ItemID.Glass, 10)
                .AddTile(TileID.Anvils).Register();

        }
    }
}
