using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
        tooltips.Add(new TooltipLine(Mod, "CircuitDuration", "+80% transformation duration"));
        tooltips.Add(new TooltipLine(Mod, "CircuitCooldown", "+25% transformation cooldown duration"));
        tooltips.Add(new TooltipLine(Mod, "CircuitEnergy", "+30 Omnitrix energy"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.advancedCircuitMatrix = true;
        omp.transformationDurationMultiplier *= 1.8f;
        omp.cooldownDurationMultiplier *= 1.25f;
        omp.omnitrixEnergyMaxBonus += 30;
    }

}
