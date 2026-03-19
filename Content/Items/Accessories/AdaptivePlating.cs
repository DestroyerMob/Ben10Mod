using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class AdaptivePlating : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.CrossNecklace}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 5);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "TransformDefense", "+8 defense while transformed"));
        tooltips.Add(new TooltipLine(Mod, "TransformEndurance", "+5% endurance while transformed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedDefenseBonus += 8;
        omp.transformedEnduranceBonus += 0.05f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.CrossNecklace)
            .AddIngredient(ItemID.HallowedBar, 8)
            .AddIngredient(ItemID.SoulofNight, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
