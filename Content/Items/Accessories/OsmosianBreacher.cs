using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianBreacher : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.StingerNecklace}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 5);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianPen", "+12 armor penetration while absorbed"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDebuff", "+15% absorption debuff duration"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionArmorPenBonus += 12;
        omp.absorptionDebuffDurationMultiplier *= 1.15f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<OsmosianAmplifier>()
            .AddIngredient(ItemID.StingerNecklace)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddIngredient(ItemID.SharkToothNecklace)
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 6)
            .AddIngredient<IllegalCircuits>(4)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
