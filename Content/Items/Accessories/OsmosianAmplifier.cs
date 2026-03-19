using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianAmplifier : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.SharkToothNecklace}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 26;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 4);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianStrength", "+125% absorption strength"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDebuff", "+140% absorption debuff duration"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionStrengthMultiplier *= 2.25f;
        omp.absorptionDebuffDurationMultiplier *= 2.4f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.SharkToothNecklace)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddIngredient(ItemID.Chain, 6)
            .AddIngredient(ItemID.ShadowScale, 8)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.SharkToothNecklace)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddIngredient(ItemID.Chain, 6)
            .AddIngredient(ItemID.TissueSample, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
