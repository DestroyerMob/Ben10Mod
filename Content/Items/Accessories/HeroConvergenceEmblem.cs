using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class HeroConvergenceEmblem : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.AvengerEmblem}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 8);
        Item.rare = ItemRarityID.Pink;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "HeroDamage", "+8% hero damage"));
        tooltips.Add(new TooltipLine(Mod, "Convergence", "Hero weapon hits build Convergence"));
        tooltips.Add(new TooltipLine(Mod, "ConvergenceBurst", "At full Convergence, unleash a burst of guided emblem bolts"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetModPlayer<OmnitrixPlayer>().heroConvergenceEmblemEquipped = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<HeroEmblem>())
            .AddIngredient(ModContent.ItemType<KineticServos>())
            .AddIngredient(ModContent.ItemType<ImpactHarness>())
            .AddIngredient(ItemID.AvengerEmblem)
            .AddIngredient(ItemID.HallowedBar, 10)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
