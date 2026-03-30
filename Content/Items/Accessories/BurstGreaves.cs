using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class BurstGreaves : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.LightningBoots}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "TransformMove", "+12% movement speed while transformed"));
        tooltips.Add(new TooltipLine(Mod, "TransformJump", "Improves acceleration and jump height while transformed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedMoveSpeedBonus += 0.12f;
        omp.transformedRunAccelerationBonus += 0.08f;
        omp.transformedJumpSpeedBonus += 1.6f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.LightningBoots)
            .AddIngredient(ItemID.HellstoneBar, 10)
            .AddIngredient(ItemID.Feather, 6)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
