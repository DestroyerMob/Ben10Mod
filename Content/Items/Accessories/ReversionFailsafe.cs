using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class ReversionFailsafe : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.CrossNecklace}";

    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 6);
        Item.rare = ItemRarityID.Pink;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "FailsafeSummary",
            "While transformed, a killing blow forces a detransformation instead"));
        tooltips.Add(new TooltipLine(Mod, "FailsafeLife",
            "Leaves you at 1 life and grants 3 seconds of immunity"));
        tooltips.Add(new TooltipLine(Mod, "FailsafeCooldown",
            "Applies transformation cooldown even with Master Control"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().transformationFailsafeEquipped = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.CrossNecklace)
            .AddIngredient(ModContent.ItemType<TransformationStabilizer>())
            .AddIngredient(ItemID.HallowedBar, 10)
            .AddIngredient(ItemID.SoulofFright, 6)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
