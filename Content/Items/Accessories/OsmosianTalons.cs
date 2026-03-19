using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianTalons : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.FeralClaws}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 4);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianSpeed", "+18% melee speed while absorbed"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianKnockback", "+0.8 melee knockback while absorbed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionMeleeSpeedBonus += 0.18f;
        omp.absorptionMeleeKnockbackBonus += 0.8f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.FeralClaws)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddIngredient(ItemID.Stinger, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
