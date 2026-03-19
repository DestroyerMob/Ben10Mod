using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class PrimaryConduit : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.CelestialCuffs}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 6);
        Item.rare = ItemRarityID.Yellow;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "PrimaryCooldown", "20% shorter primary ability cooldowns"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().primaryAbilityCooldownMultiplier *= 0.8f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.CelestialCuffs)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddIngredient(ItemID.HallowedBar, 10)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
