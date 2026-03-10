using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class HeroEmblem : ModItem {

    public override void SetDefaults() {
        Item.width     = 32;
        Item.height    = 32;
        Item.accessory = true;
        Item.value     = 100000;
        Item.rare     = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        TooltipLine damageLine = new TooltipLine(Mod, "HeroDamageBonus", "+15% increased hero damage");

        tooltips.Add(damageLine);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetDamage<HeroDamage>() += 0.15f;
    }
}