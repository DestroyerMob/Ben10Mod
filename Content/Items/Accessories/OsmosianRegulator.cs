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
        tooltips.Add(new TooltipLine(Mod, "OsmosianStrength", "+55% absorption strength"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDebuff", "+100% absorption debuff duration"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionStrengthMultiplier *= 1.55f;
        omp.absorptionDebuffDurationMultiplier *= 2f;
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
