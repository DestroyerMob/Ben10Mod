using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
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
            .AddIngredient(ModContent.ItemType<CooldownAccelerator>())
            .AddIngredient<IllegalCircuits>(5)
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 10)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddIngredient(ItemID.HallowedBar, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
