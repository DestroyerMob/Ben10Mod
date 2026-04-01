using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberHellfireBadge : PlumbersBadge {
    private const int ProcCooldownFrames = 24;

    public override string Texture => "Ben10Mod/Content/Items/Weapons/PlumberCadetBadge";

    public override int BaseDamage => 48;
    public override string BadgeRankName => "Hellfire";
    public override int BadgeRankValue => 4;

    public override void SetDefaults() {
        base.SetDefaults();
        Item.value = Item.buyPrice(gold: 3, silver: 50);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        base.ModifyTooltips(tooltips);
        tooltips.Add(new TooltipLine(Mod, "HellfireBadgeEffect",
            "While untransformed, casts infernal scythes instead of energy bolts"));
        tooltips.Add(new TooltipLine(Mod, "HellfireBadgeProc",
            "Primary and secondary badge attacks can conjure bonus demon scythes"));
    }

    protected override void ConfigureUntransformedBadgeStats(Player player, OmnitrixPlayer omp) {
        Item.noUseGraphic = false;
        Item.useTurn = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 30;
        Item.shoot = ProjectileID.DemonScythe;
        Item.shootSpeed = 10.75f;
        Item.damage = Math.Max(1, (int)Math.Round(BaseDamage * 0.55f));
        Item.knockBack = 3.5f;
        Item.UseSound = SoundID.Item8 with { Pitch = -0.08f, Volume = 0.74f };
    }

    protected override bool ShootUntransformedBadge(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int damage, float knockback) {
        SpawnDemonScythe(player, source, position, velocity, damage, knockback, Item.shootSpeed, 0f);
        return false;
    }

    protected override void OnTransformationAttackFired(Player player, OmnitrixPlayer omp,
        EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int damage, float knockback,
        bool firingUltimate, bool firingLoadedAbilityAttack) {
        if (firingUltimate || firingLoadedAbilityAttack ||
            omp.setAttack is not OmnitrixPlayer.AttackSelection.Primary and not OmnitrixPlayer.AttackSelection.Secondary)
            return;

        PlumberHellfireBadgePlayer badgePlayer = player.GetModPlayer<PlumberHellfireBadgePlayer>();
        if (badgePlayer.ScytheProcCooldown > 0 || !Main.rand.NextBool(3))
            return;

        badgePlayer.ScytheProcCooldown = ProcCooldownFrames;

        if (omp.setAttack == OmnitrixPlayer.AttackSelection.Secondary) {
            int scytheDamage = Math.Max(1, (int)Math.Round(damage * 0.32f));
            SpawnDemonScythe(player, source, position, velocity, scytheDamage, knockback + 0.5f, 11.5f, -0.09f);
            SpawnDemonScythe(player, source, position, velocity, scytheDamage, knockback + 0.5f, 11.5f, 0.09f);
            return;
        }

        SpawnDemonScythe(player, source, position, velocity,
            Math.Max(1, (int)Math.Round(damage * 0.55f)), knockback + 0.75f, 12.5f, Main.rand.NextFloat(-0.03f, 0.03f));
    }

    private static void SpawnDemonScythe(Player player, IEntitySource source, Vector2 position, Vector2 velocity,
        int damage, float knockback, float speed, float spreadRadians) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        if (spreadRadians != 0f)
            direction = direction.RotatedBy(spreadRadians);

        Vector2 spawnPosition = position + direction * 14f;
        Vector2 shotVelocity = direction * speed;
        int projectileIndex = Projectile.NewProjectile(source, spawnPosition, shotVelocity, ProjectileID.DemonScythe,
            damage, knockback, player.whoAmI);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;
    }
}

public class PlumberHellfireBadgePlayer : ModPlayer {
    public int ScytheProcCooldown { get; set; }

    public override void PostUpdate() {
        if (ScytheProcCooldown > 0)
            ScytheProcCooldown--;
    }
}
