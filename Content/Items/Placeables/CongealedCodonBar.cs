using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Ben10Mod.Content.Tiles;

namespace Ben10Mod.Content.Items.Placeables {
    public class CongealedCodonBar : ModItem {

        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 59;
        }

        public override void SetDefaults() {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.value = Item.buyPrice(silver: 1, copper: 75);

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;

            Item.createTile = ModContent.TileType<Bars>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes() {
            CreateRecipe().AddTile(TileID.Furnaces).AddIngredient(ModContent.ItemType<CongealedCodonOre>(), 3).Register();
        }

    }
}
