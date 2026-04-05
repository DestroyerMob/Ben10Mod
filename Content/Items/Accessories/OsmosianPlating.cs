using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianPlating : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.CobaltShield}";

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 32;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 4);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianLife", "+20 max life while absorbed"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDefense", "+5 defense while absorbed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionMaxLifeBonus += 20;
        omp.absorptionFlatDefenseBonus += 5;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<OsmosianHarness>()
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
            .AddIngredient<IllegalCircuits>(3)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddIngredient(ItemID.Bone, 20)
            .AddIngredient(ItemID.LifeCrystal, 1)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
