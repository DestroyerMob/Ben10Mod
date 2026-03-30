using System.Collections.Generic;
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
        tooltips.Add(new TooltipLine(Mod, "ChronoField", "Ability and ultimate attacks tear a chrono field near your cursor"));
        tooltips.Add(new TooltipLine(Mod, "ChronoFieldEffect", "Chrono fields slow enemies and pulse hero damage"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().chronoAcceleratorEquipped = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CooldownAccelerator>())
            .AddIngredient(ModContent.ItemType<PrimaryConduit>())
            .AddIngredient(ItemID.FastClock)
            .AddIngredient(ItemID.HallowedBar, 10)
            .AddIngredient(ItemID.SoulofLight, 8)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
