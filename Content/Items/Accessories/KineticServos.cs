using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class KineticServos : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.FeralClaws}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "HeroSpeed", "+12% hero attack speed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().heroAttackSpeedBonus += 0.12f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
            .AddIngredient<IllegalCircuits>(2)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddIngredient(ItemID.Chain, 8)
            .AddIngredient(ItemID.Stinger, 10)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
