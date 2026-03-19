using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianRegulator : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.BandofRegeneration}";

    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianDuration", "+60% absorption duration"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDebuff", "+25% absorption debuff duration"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianCost", "+20% material cost"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionDurationMultiplier *= 1.6f;
        omp.absorptionDebuffDurationMultiplier *= 1.25f;
        omp.absorptionCostMultiplier *= 1.2f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.BandofRegeneration)
            .AddIngredient(ItemID.Chain, 8)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddIngredient(ItemID.FallenStar, 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
