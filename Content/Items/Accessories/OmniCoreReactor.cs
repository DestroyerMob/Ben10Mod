using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OmniCoreReactor : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.ManaCloak}";

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 32;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 8);
        Item.rare = ItemRarityID.Pink;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "EnergyMax", "+40 Omnitrix energy"));
        tooltips.Add(new TooltipLine(Mod, "EnergyRegen", "+1 Omnitrix energy regen"));
        tooltips.Add(new TooltipLine(Mod, "TransformDefense", "+4 defense while transformed"));
        tooltips.Add(new TooltipLine(Mod, "ReactorCharge", "Badge attacks charge the Omni-Core reactor"));
        tooltips.Add(new TooltipLine(Mod, "ReactorPulse", "At full charge, unleash an energy pulse that refunds OE on hits"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.omnitrixEnergyMaxBonus += 40;
        omp.omnitrixEnergyRegenBonus += 1;
        omp.transformedDefenseBonus += 4;
        omp.omniCoreReactorEquipped = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<EnergyRecycler>())
            .AddIngredient(ModContent.ItemType<AdaptivePlating>())
            .AddIngredient(ModContent.ItemType<BurstGreaves>())
            .AddIngredient<IllegalCircuits>(10)
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddIngredient(ItemID.SoulofLight, 10)
            .AddIngredient(ItemID.SoulofNight, 10)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
