using System.Collections.Generic;
using Terraria;
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
