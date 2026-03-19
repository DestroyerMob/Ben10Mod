using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianTreads : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.HermesBoots}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 4);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianMove", "+18% movement speed while absorbed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().absorptionMoveSpeedBonus += 0.18f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HermesBoots)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddIngredient(ItemID.Feather, 4)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
