using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianBreacher : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.StingerNecklace}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 5);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianPen", "+15 armor penetration while absorbed"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDebuff", "+20% absorption debuff duration"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionArmorPenBonus += 15;
        omp.absorptionDebuffDurationMultiplier *= 1.2f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.StingerNecklace)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddIngredient(ItemID.SharkToothNecklace)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
