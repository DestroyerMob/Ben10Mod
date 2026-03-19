using Ben10Mod.Content.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace Ben10Mod.Content.Items.Accessories;

public class AdvancedCircuitMatrix : ModItem {

    public override void SetDefaults() {
        Item.width     = 32;
        Item.height    = 32;
        Item.accessory = true;
        Item.value     = 100000;
        Item.rare      = ItemRarityID.LightPurple;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "CircuitDuration", "+100% transformation duration"));
        tooltips.Add(new TooltipLine(Mod, "CircuitCooldown", "+100% transformation cooldown duration"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.advancedCircuitMatrix = true;
        omp.transformationDurationMultiplier *= 2f;
        omp.cooldownDurationMultiplier *= 2f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 18)
            .AddIngredient(ItemID.SoulofSight, 10)
            .AddIngredient(ItemID.Wire, 40)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
