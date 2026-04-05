using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianCapacitor : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.BandofStarpower}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 5);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianDuration", "+30% absorption duration"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianMove", "+8% movement speed while absorbed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionDurationMultiplier *= 1.3f;
        omp.absorptionMoveSpeedBonus += 0.08f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<OsmosianRegulator>()
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
            .AddIngredient<IllegalCircuits>(4)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddIngredient(ItemID.FallenStar, 10)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
