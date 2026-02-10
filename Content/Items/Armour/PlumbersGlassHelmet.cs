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
    public class PlumbersGlassHelmet : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 14;

            Item.defense = 3;

            Item.value = 010000;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) {
            bool bodyMatch = body.type == ModContent.ItemType<PlumbersShirt>();
            bool legsMatch = legs.type == ModContent.ItemType<PlumbersPants>();

            return bodyMatch && legsMatch;
        }

        public override void UpdateArmorSet(Player player) {

            player.setBonus = "+5 Hero damage";
            
            player.GetDamage(ModContent.GetInstance<HeroDamage>()).Flat       += 5;
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
