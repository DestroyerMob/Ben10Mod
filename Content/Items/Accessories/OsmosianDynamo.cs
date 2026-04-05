using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianDynamo : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.AvengerEmblem}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 6);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianStrength", "+35% absorption strength"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianPen", "+6 armor penetration while absorbed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionStrengthMultiplier *= 1.35f;
        omp.absorptionArmorPenBonus += 6;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<OsmosianAmplifier>()
            .AddIngredient<OsmosianBreacher>()
            .AddIngredient(ItemID.HallowedBar, 10)
            .AddIngredient(ItemID.SoulofMight, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
