using Ben10Mod.Content.DamageClasses;
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

namespace Ben10Mod.Content.Items.Armour {
    [AutoloadEquip(EquipType.Head)]
    public class PlumbersHelmet : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 14;

            Item.value = 010000;

            Item.defense = 2;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) {
            bool bodyMatch = body.type == ModContent.ItemType<PlumbersShirt>();
            bool legsMatch = legs.type == ModContent.ItemType<PlumbersPants>();

            return bodyMatch && legsMatch;
        }

        public override void UpdateArmorSet(Player player) {

            player.setBonus = "+8 defence while transformed";
            
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (omp.isTransformed) {
                player.statDefense += 8;
            }
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
