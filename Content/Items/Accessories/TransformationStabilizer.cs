using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class TransformationStabilizer : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.BandofStarpower}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "TransformDuration", "+35% transformation duration"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().transformationDurationMultiplier *= 1.35f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
            .AddIngredient<IllegalCircuits>(2)
            .AddIngredient(ItemID.MeteoriteBar, 8)
            .AddIngredient(ItemID.Chain, 6)
            .AddIngredient(ItemID.Lens, 4)
            .AddIngredient(ItemID.FallenStar, 10)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
