using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class OsmosianInjector : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.CharmofMyths}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 5);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OsmosianRegen", "+3 life regeneration while absorbed"));
        tooltips.Add(new TooltipLine(Mod, "OsmosianDuration", "+20% absorption duration"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorptionLifeRegenBonus += 3;
        omp.absorptionDurationMultiplier *= 1.2f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<OsmosianRegulator>()
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
            .AddIngredient<IllegalCircuits>(4)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddIngredient(ItemID.LifeCrystal, 2)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
