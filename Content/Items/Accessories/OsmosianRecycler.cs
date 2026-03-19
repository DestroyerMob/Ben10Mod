using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianRecycler : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.DiscountCard}";

    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 26;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianCost", "35% lower absorption material cost"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDuration", "+20% absorption duration"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionCostMultiplier *= 0.65f;
        omp.absorptionDurationMultiplier *= 1.2f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.GoldCoin, 1)
            .AddIngredient(ItemID.Chain, 6)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddIngredient(ItemID.ShadowScale, 6)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.GoldCoin, 1)
            .AddIngredient(ItemID.Chain, 6)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddIngredient(ItemID.TissueSample, 6)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
