using System.Collections.Generic;
using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class ImpactHarness : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.SharkToothNecklace}";

    public override void SetDefaults() {
        Item.width = 32;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "HeroArmorPen", "+6 hero armor penetration"));
        tooltips.Add(new TooltipLine(Mod, "HeroKnockback", "+0.8 hero knockback"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.heroArmorPenBonus += 6;
        omp.heroKnockbackBonus += 0.8f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.SharkFin, 6)
            .AddIngredient(ItemID.Bone, 20)
            .AddIngredient(ItemID.Chain, 10)
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 6)
            .AddIngredient<IllegalCircuits>(2)
            .AddIngredient(ItemID.HellstoneBar, 10)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
