using Ben10Mod.Content.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class AdvancedCircuitMatrix : ModItem {

    public override void SetDefaults() {
        Item.width     = 64;
        Item.height    = 64;
        Item.accessory = true;
        Item.value     = 100000;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.advancedCircuitMatrix    = true;
        omp.wasAdvancedCircuitMatrix = true;
    }
}