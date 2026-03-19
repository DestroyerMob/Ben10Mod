using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianHarness : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.FleshKnuckles}";

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 6);
        Item.rare = ItemRarityID.LightRed;
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
            .AddIngredient(ItemID.FleshKnuckles)
            .AddIngredient(ItemID.MeteoriteBar, 18)
            .AddIngredient(ItemID.SoulofNight, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
