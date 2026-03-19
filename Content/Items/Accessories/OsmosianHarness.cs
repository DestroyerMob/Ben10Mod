using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianHarness : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.Shackle}";

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianAbility", "Grants Osmosian material absorption"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianRestriction", "Cannot be equipped with an Omnitrix"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().osmosianEquipped = true;
    }

    public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) {
        return equippedItem.ModItem is not Omnitrix && incomingItem.ModItem is not Omnitrix;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Shackle)
            .AddIngredient(ItemID.Chain, 10)
            .AddIngredient(ItemID.MeteoriteBar, 14)
            .AddIngredient(ItemID.ShadowScale, 10)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.Shackle)
            .AddIngredient(ItemID.Chain, 10)
            .AddIngredient(ItemID.MeteoriteBar, 14)
            .AddIngredient(ItemID.TissueSample, 10)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
