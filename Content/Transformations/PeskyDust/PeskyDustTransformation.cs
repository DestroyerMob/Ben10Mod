using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.PeskyDust;

public class PeskyDustTransformation : Transformation {
    private const int PixieDriftDuration = 10 * 60;
    private const int PixieDriftCooldown = 24 * 60;
    private const int PixieDriftCost = 18;
    private const int DreamSnareEnergyCost = 24;
    private const int DreamSnareCooldown = 15 * 60;
    private const int MaxActiveDreamSnares = 2;
    private const float PrimaryDamageMultiplier = 0.76f;
    private const float SecondaryDamageMultiplier = 0.88f;
    private const float DreamSnareDamageMultiplier = 1.02f;
    private const float UltimateDamageMultiplier = 1.14f;

    public override string FullID => "Ben10Mod:PeskyDust";
    public override string TransformationName => "Pesky Dust";
    public override int TransformationBuffId => ModContent.BuffType<PeskyDust_Buff>();
    public override string Description =>
        "A nimble Nemuina trickster that blankets enemies in soporific dust, glides on dream currents, pins targets in dream snares, and overwhelms whole areas with a sandman storm.";

    public override List<string> Abilities => new() {
        "Sleep-dust bolt primary fire",
        "Lullaby cloud secondary burst",
        "Pixie drift aerial stance",
        "Dream snare field placement",
        "Sandman storm ultimate"
    };

    public override string PrimaryAttackName => "Sleep Dust";
    public override string SecondaryAttackName => "Lullaby Cloud";
    public override string SecondaryAbilityAttackName => "Dream Snare";
    public override string UltimateAttackName => "Sandman Storm";

    public override int PrimaryAttack => ModContent.ProjectileType<PeskyDustSleepDustProjectile>();
    public override int PrimaryAttackSpeed => 13;
    public override int PrimaryShootSpeed => 15;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<PeskyDustLullabyCloudProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => PixieDriftDuration;
    public override int PrimaryAbilityCooldown => PixieDriftCooldown;
    public override int PrimaryAbilityCost => PixieDriftCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<PeskyDustDreamSnareProjectile>();
    public override int SecondaryAbilityAttackSpeed => 20;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => DreamSnareDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => DreamSnareEnergyCost;
    public override int SecondaryAbilityCooldown => DreamSnareCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<PeskyDustSandmanStormProjectile>();
    public override int UltimateAttackSpeed => 28;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => UltimateDamageMultiplier;
    public override int UltimateEnergyCost => 55;
    public override int UltimateAbilityCooldown => 56 * 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<PeskyDustDreamSnareProjectile>(),
            ModContent.ProjectileType<PeskyDustSandmanStormProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetKnockback<HeroDamage>() += 0.3f;
        player.statDefense += 4;
        player.moveSpeed += 0.16f;
        player.runAcceleration += 0.08f;
        player.maxRunSpeed += 0.9f;
        player.jumpSpeedBoost += 1.8f;
        player.noFallDmg = true;
        player.ignoreWater = true;
        player.buffImmune[BuffID.Confused] = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetAttackSpeed<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.moveSpeed += 0.2f;
        player.runAcceleration += 0.1f;
        player.maxRunSpeed += 1.15f;
        player.jumpSpeedBoost += 1.2f;
        player.wingTimeMax += 48;
        player.wingTime = Math.Max(player.wingTime, 16f);
        player.endurance += 0.03f;
        player.armorEffectDrawShadow = true;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        if (!omp.PrimaryAbilityEnabled)
            return;

        item.useTime = item.useAnimation = Math.Max(8, (int)Math.Round(item.useTime * 0.84f));
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled || player.mount.Active)
            return;

        ApplyPixieDrift(player);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + new Vector2(player.direction * 6f, -14f) + direction * 10f;
        bool drifting = omp.PrimaryAbilityEnabled;

        if (omp.ultimateAttack) {
            if (HasActiveOwnedProjectile(player, UltimateAttack))
                return false;

            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 stormPosition = Main.MouseWorld;
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, stormPosition, Vector2.Zero, UltimateAttack, finalDamage, knockback + 0.6f,
                player.whoAmI, drifting ? 1f : 0f);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            int snareType = ModContent.ProjectileType<PeskyDustDreamSnareProjectile>();
            CullOldestDreamSnare(player, snareType);

            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));
            int projectileIndex = Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, snareType, finalDamage,
                knockback + 0.4f, player.whoAmI, drifting ? 1f : 0f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                omp.transformationAttackSerial++;
                Main.projectile[projectileIndex].localAI[1] = omp.transformationAttackSerial;
                Main.projectile[projectileIndex].netUpdate = true;
            }

            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed, SecondaryAttack, finalDamage,
                knockback + 0.3f, player.whoAmI, drifting ? 1f : 0f);
            return false;
        }

        int boltCount = drifting ? 2 : 1;
        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        for (int i = 0; i < boltCount; i++) {
            float spread = boltCount == 1 ? 0f : (i == 0 ? -0.1f : 0.1f);
            Vector2 shotVelocity = direction.RotatedBy(spread) * PrimaryShootSpeed;
            Projectile.NewProjectile(source, spawnPosition, shotVelocity, PrimaryAttack, primaryDamage, knockback,
                player.whoAmI, drifting ? 1f : 0f);
        }

        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.JungleHat;
        player.body = ArmorIDs.Body.JungleShirt;
        player.legs = ArmorIDs.Legs.JunglePants;
    }

    private static void ApplyPixieDrift(Player player) {
        if (player.controlJump || player.controlUp) {
            float ascentAcceleration = player.controlUp ? 0.42f : 0.3f;
            float maxRiseSpeed = player.controlUp ? -6.2f : -4.8f;
            player.velocity.Y = Math.Max(maxRiseSpeed, player.velocity.Y - ascentAcceleration);
        }
        else if (player.velocity.Y > 0f) {
            player.velocity.Y *= 0.84f;
        }

        if (player.controlDown)
            player.velocity.Y = Math.Min(player.velocity.Y + 0.24f, 5.2f);

        player.maxFallSpeed = Math.Min(player.maxFallSpeed, 5.2f);
        player.fallStart = (int)(player.position.Y / 16f);
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction;
    }

    private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
                return true;
        }

        return false;
    }

    private static void CullOldestDreamSnare(Player player, int projectileType) {
        int activeCount = 0;
        int oldestIndex = -1;
        float oldestSpawnOrder = float.MaxValue;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active || projectile.owner != player.whoAmI || projectile.type != projectileType)
                continue;

            activeCount++;
            float spawnOrder = projectile.localAI[1] <= 0f ? projectile.identity : projectile.localAI[1];
            if (spawnOrder < oldestSpawnOrder) {
                oldestSpawnOrder = spawnOrder;
                oldestIndex = i;
            }
        }

        if (activeCount >= MaxActiveDreamSnares && oldestIndex != -1)
            Main.projectile[oldestIndex].Kill();
    }

    private static void KillOwnedProjectiles(Player player, params int[] projectileTypes) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active || projectile.owner != player.whoAmI)
                continue;

            for (int j = 0; j < projectileTypes.Length; j++) {
                if (projectile.type != projectileTypes[j])
                    continue;

                projectile.Kill();
                break;
            }
        }
    }
}
