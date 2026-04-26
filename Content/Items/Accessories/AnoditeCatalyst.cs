using System.Collections.Generic;
using Ben10Mod.Keybinds;
using Ben10Mod.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class AnoditeCatalyst : ModItem, IHeroAlterationAccessory {
    private const string AnoditeTransformationId = "Ben10Mod:Anodite";
    private bool _wasEquippedLastFrame;

    public override string Texture => $"Terraria/Images/Item_{ItemID.CelestialCuffs}";

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 32;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 6);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "AnoditeSlot", "Fits in the DNA Alteration slot"));
        tooltips.Add(new TooltipLine(Mod, "AnoditeUse", "Press the transformation key to assume Anodite form"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        _wasEquippedLastFrame = true;
        player.GetModPlayer<OmnitrixPlayer>().anoditeCatalystEquipped = true;

        if (player.whoAmI != Main.myPlayer || !KeybindSystem.TransformationKeybind.JustPressed)
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.onCooldown)
            return;

        if (!omp.IsTransformed) {
            TransformationHandler.Transform(player, AnoditeTransformationId);
            return;
        }

        if (omp.currentTransformationId == AnoditeTransformationId) {
            TransformationHandler.Detransform(player, cooldownSeconds: 60);
            return;
        }

        TransformationHandler.Detransform(player, cooldownSeconds: 0, showParticles: false, addCooldown: false,
            playSound: false);
        TransformationHandler.Transform(player, AnoditeTransformationId);
    }

    public override void UpdateInventory(Player player) {
        if (!_wasEquippedLastFrame)
            return;

        _wasEquippedLastFrame = false;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != AnoditeTransformationId)
            return;

        TransformationHandler.Detransform(player, cooldownSeconds: 60, showParticles: true, addCooldown: true,
            playSound: true);
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 12)
            .AddIngredient(ItemID.CrystalShard, 18)
            .AddIngredient(ItemID.SoulofLight, 14)
            .AddIngredient(ItemID.PixieDust, 20)
            .AddIngredient(ItemID.FallenStar, 10)
            .AddIngredient(ModContent.ItemType<Content.Items.Weapons.Gwen.AnoditeOrb>())
            .AddIngredient(ModContent.ItemType<Content.Items.Weapons.Gwen.ManaBarrier>())
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
