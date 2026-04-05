using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
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
        tooltips.Add(new TooltipLine(Mod, "HeroDamage", "+5% hero damage"));
        tooltips.Add(new TooltipLine(Mod, "HeroCrit", "+10% hero crit chance"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDamage<HeroDamage>() += 0.05f;
        player.GetModPlayer<OmnitrixPlayer>().heroCritChanceBonus += 10;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.RifleScope)
            .AddIngredient(ItemID.Lens, 8)
            .AddIngredient(ItemID.ChlorophyteBar, 10)
            .AddIngredient(ItemID.Ectoplasm, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
