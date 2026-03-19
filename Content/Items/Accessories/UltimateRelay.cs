using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class UltimateRelay : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.DestroyerEmblem}";

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 8);
        Item.rare = ItemRarityID.Yellow;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "UltimateCooldown", "18% shorter ultimate ability cooldowns"));
        tooltips.Add(new TooltipLine(Mod, "UltimateEnergy", "+25 Omnitrix energy"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.ultimateAbilityCooldownMultiplier *= 0.82f;
        omp.omnitrixEnergyMaxBonus += 25;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.DestroyerEmblem)
            .AddIngredient(ItemID.SoulofMight, 10)
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
