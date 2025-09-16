using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons {
    public class PlumbersBadge : ModItem {
        public override void SetDefaults() {
            Item.maxStack = 1;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.DamageType = ModContent.GetInstance<HeroDamage>();
        }

        public override bool CanUseItem(Player player) {
            return player.GetModPlayer<OmnitrixPlayer>().isTransformed;
        }

        public override void UpdateInventory(Player player) {
            player.GetModPlayer<OmnitrixPlayer>().heroAttackSpeed = (int)(Item.useTime / player.GetAttackSpeed(DamageClass.Generic));
        }

        public override void AddRecipes() {
            base.AddRecipes();

            Recipe recipe = CreateRecipe()
                .AddIngredient(ItemID.IronBar, 15)
                .AddIngredient(ItemID.Ruby, 1)
                .AddTile(TileID.Anvils).Register();

            recipe = CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 15)
                .AddIngredient(ItemID.Ruby, 1)
                .AddTile(TileID.Anvils).Register();

        }
    }
}
