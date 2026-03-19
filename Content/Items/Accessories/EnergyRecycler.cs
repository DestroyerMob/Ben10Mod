using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class EnergyRecycler : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.ManaCloak}";

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 32;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 7);
        Item.rare = ItemRarityID.Yellow;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "EnergyMax", "+60 Omnitrix energy"));
        tooltips.Add(new TooltipLine(Mod, "EnergyRegen", "+1 Omnitrix energy regen"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.omnitrixEnergyMaxBonus += 60;
        omp.omnitrixEnergyRegenBonus += 1;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ManaCloak)
            .AddIngredient(ItemID.SoulofLight, 10)
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
