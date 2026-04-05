using System.Collections.Generic;
using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class PotisAltiare : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.AvengerEmblem}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 14);
        Item.rare = ItemRarityID.Lime;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "PotisAltiare", "Enables Potis Altiare kit upgrades when equipped"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<PotisAltiarePlayer>().potisAltiareEquipped = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<HeroEmblem>())
            .AddIngredient<IllegalCircuits>(8)
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
            .AddIngredient(ItemID.Ectoplasm, 10)
            .AddIngredient(ItemID.ChlorophyteBar, 12)
            .AddIngredient(ItemID.SoulofFright, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}

public class PotisAltiarePlayer : ModPlayer {
    public bool potisAltiareEquipped;

    public override void ResetEffects() {
        potisAltiareEquipped = false;
    }
}
