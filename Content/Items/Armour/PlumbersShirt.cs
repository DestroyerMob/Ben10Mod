using Ben10Mod.Content.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Armour{
    [AutoloadEquip(EquipType.Body)]
    public class PlumbersShirt : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 14;

            Item.value = 010000;

            Item.defense = 6;
        }

        public override void AddRecipes()
        {
            base.AddRecipes();

            Recipe recipe = CreateRecipe()
                .AddIngredient(ItemID.IronBar, 25)
                .AddTile(TileID.Anvils).Register();

            recipe = CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 25)
                .AddTile(TileID.Anvils).Register();

        }

    }
}
