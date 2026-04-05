using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Materials;
using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class ConquestDroneRelay : ModItem {
    internal const int BaseDroneDamage = 28;

    public override string Texture => $"Terraria/Images/Item_{ItemID.OpticStaff}";

    public override void SetDefaults() {
        Item.width = 32;
        Item.height = 32;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 7);
        Item.rare = ItemRarityID.Pink;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "RelaySummary",
            "Deploys two Vilgaxian mechadroids to guard you"));
        tooltips.Add(new TooltipLine(Mod, "RelayFire",
            "The drones hover overhead and blast nearby enemies"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        ConquestDroneRelayPlayer relayPlayer = player.GetModPlayer<ConquestDroneRelayPlayer>();
        relayPlayer.conquestDroneRelayEquipped = true;

        if (player.whoAmI != Main.myPlayer)
            return;

        int projectileType = ModContent.ProjectileType<ConquestDroneProjectile>();
        int missingCount = 2 - player.ownedProjectileCounts[projectileType];
        if (missingCount <= 0)
            return;

        int damage = System.Math.Max(1, (int)System.Math.Round(player.GetDamage<HeroDamage>().ApplyTo(BaseDroneDamage)));
        for (int i = 0; i < missingCount; i++) {
            int projectileIndex = Projectile.NewProjectile(player.GetSource_Misc("ConquestDroneRelay"),
                player.MountedCenter + new Vector2(0f, -48f), Vector2.Zero, projectileType, damage, 1f, player.whoAmI);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].netUpdate = true;
        }
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<HeroEmblem>())
            .AddIngredient<IllegalCircuits>(8)
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 12)
            .AddIngredient(ItemID.ChlorophyteBar, 12)
            .AddIngredient(ItemID.Ectoplasm, 8)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddIngredient(ItemID.Wire, 40)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}

public class ConquestDroneRelayPlayer : ModPlayer {
    public bool conquestDroneRelayEquipped;

    public override void ResetEffects() {
        conquestDroneRelayEquipped = false;
    }
}
