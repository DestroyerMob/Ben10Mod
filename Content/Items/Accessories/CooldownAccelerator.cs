using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class CooldownAccelerator : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.Stopwatch}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "TransformCooldown", "15% shorter transformation cooldowns"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().cooldownDurationMultiplier *= 0.85f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
            .AddIngredient<IllegalCircuits>(2)
            .AddIngredient(ItemID.MeteoriteBar, 8)
            .AddIngredient(ItemID.Wire, 25)
            .AddIngredient(ItemID.Lens, 4)
            .AddIngredient(ItemID.FallenStar, 6)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
