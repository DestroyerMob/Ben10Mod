using Ben10Mod.Content.Items.Placeables;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class AdvancedCircuitMatrix : ModItem {

    public override void SetDefaults() {
        Item.width     = 32;
        Item.height    = 32;
        Item.accessory = true;
        Item.value     = 100000;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.advancedCircuitMatrix =  true;
        // This simply exists for my dev needs
        // omp.omnitrixEnergyRegen   += 500;
    }
}