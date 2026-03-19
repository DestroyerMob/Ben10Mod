using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class ImpactHarness : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.PowerGlove}";

    public override void SetDefaults() {
        Item.width = 32;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 5);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "HeroArmorPen", "+8 hero armor penetration"));
        tooltips.Add(new TooltipLine(Mod, "HeroKnockback", "+1.2 hero knockback"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.heroArmorPenBonus += 8;
        omp.heroKnockbackBonus += 1.2f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.PowerGlove)
            .AddIngredient(ItemID.SoulofMight, 8)
            .AddIngredient(ItemID.HallowedBar, 8)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
