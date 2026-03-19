using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianLens : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.MagicQuiver}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 4);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianCrit", "+12% crit chance while absorbed"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().absorptionCritChanceBonus += 12;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Lens, 6)
            .AddIngredient(ItemID.Diamond, 2)
            .AddIngredient(ItemID.MeteoriteBar, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
