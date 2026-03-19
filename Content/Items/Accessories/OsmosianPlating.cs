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
        tooltips.Add(new TooltipLine(Mod, "OsmosianLife", "+24 max life while absorbed"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDefense", "+6 defense while absorbed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionMaxLifeBonus += 24;
        omp.absorptionFlatDefenseBonus += 6;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.CobaltShield)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddIngredient(ItemID.Bone, 20)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
