using System.Collections.Generic;
using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class ChronoAccelerator : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.FastClock}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 6);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "ChronoCooldown", "8% shorter transformation cooldowns"));
        tooltips.Add(new TooltipLine(Mod, "ChronoPrimary", "12% shorter primary ability cooldowns"));
        tooltips.Add(new TooltipLine(Mod, "ChronoField", "Ability and ultimate attacks tear a chrono field near your cursor"));
        tooltips.Add(new TooltipLine(Mod, "ChronoFieldEffect", "Chrono fields slow enemies and pulse hero damage"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.cooldownDurationMultiplier *= 0.92f;
        omp.primaryAbilityCooldownMultiplier *= 0.88f;
        omp.chronoAcceleratorEquipped = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CooldownAccelerator>())
            .AddIngredient(ModContent.ItemType<PrimaryConduit>())
            .AddIngredient<IllegalCircuits>(8)
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 10)
            .AddIngredient(ItemID.HallowedBar, 10)
            .AddIngredient(ItemID.SoulofLight, 8)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
