using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class HeroVisor : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.EyeoftheGolem}";

    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 24;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 6);
        Item.rare = ItemRarityID.Yellow;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "HeroCrit", "+10% hero crit chance"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().heroCritChanceBonus += 10;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.EyeoftheGolem)
            .AddIngredient(ItemID.SoulofSight, 10)
            .AddIngredient(ItemID.HallowedBar, 10)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
