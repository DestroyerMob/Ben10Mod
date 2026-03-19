using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class CooldownAccelerator : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.FastClock}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 4);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "TransformCooldown", "25% shorter transformation cooldowns"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().cooldownDurationMultiplier *= 0.75f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.FastClock)
            .AddIngredient(ItemID.HallowedBar, 8)
            .AddIngredient(ItemID.SoulofLight, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
