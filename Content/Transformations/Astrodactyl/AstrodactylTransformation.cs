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

namespace Ben10Mod.Content.Transformations.Astrodactyl;

public class AstrodactylTransformation : Transformation {
    private const int HyperflightDuration = 10 * 60;
    private const int HyperflightCooldown = 28 * 60;
    private const int HyperflightCost = 18;
    private const int JetDiveEnergyCost = 24;
    private const int JetDiveCooldown = 14 * 60;
    private const float PrimaryDamageMultiplier = 0.84f;
    private const float SecondaryDamageMultiplier = 1.06f;
    private const float DiveDamageMultiplier = 1.28f;
    private const float UltimateDamageMultiplier = 1.2f;
    private const float BaseDiveRange = 400f;
    private const float HyperflightDiveRange = 540f;

    public override string FullID => "Ben10Mod:Astrodactyl";
    public override string TransformationName => "Astrodactyl";
    public override int TransformationBuffId => ModContent.BuffType<Astrodactyl_Buff>();
    public override string Description =>
        "A soaring aerial hunter that peppers enemies with plasma bolts, ruptures them with starbursts, dives through formations, and calls down a cosmic comet barrage.";

    public override List<string> Abilities => new() {
        "Rapid plasma bolt primary",
        "Bursting star plasma orb",
        "Hyperflight aerial stance",
        "Jet dive rush attack",
        "Cosmic comet barrage ultimate"
    };

    public override string PrimaryAttackName => "Plasma Bolt";
    public override string SecondaryAttackName => "Starburst";
    public override string SecondaryAbilityAttackName => "Jet Dive";
    public override string UltimateAttackName => "Cosmic Barrage";

    public override int PrimaryAttack => ModContent.ProjectileType<AstrodactylPlasmaBoltProjectile>();
    public override int PrimaryAttackSpeed => 12;
    public override int PrimaryShootSpeed => 20;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<AstrodactylStarburstProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 13;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => HyperflightDuration;
    public override int PrimaryAbilityCooldown => HyperflightCooldown;
    public override int PrimaryAbilityCost => HyperflightCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<AstrodactylDiveProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => DiveDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => JetDiveEnergyCost;
    public override int SecondaryAbilityCooldown => JetDiveCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<AstrodactylCometProjectile>();
    public override int UltimateAttackSpeed => 30;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => UltimateDamageMultiplier;
    public override int UltimateEnergyCost => 58;
    public override int UltimateAbilityCooldown => 56 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.moveSpeed += 0.18f;
        player.maxRunSpeed += 1.4f;
        player.accRunSpeed += 1.2f;
        player.jumpSpeedBoost += 1.8f;
        player.noFallDmg = true;
        player.ignoreWater = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetAttackSpeed<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 4f;
        player.moveSpeed += 0.24f;
        player.maxRunSpeed += 1.8f;
        player.accRunSpeed += 1.5f;
        player.jumpSpeedBoost += 1.2f;
        player.endurance += 0.03f;
        player.armorEffectDrawShadow = true;
        player.wingTimeMax += 70;
        player.wingTime = Math.Max(player.wingTime, 24f);
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        if (!omp.PrimaryAbilityEnabled)
            return;

        item.useTime = item.useAnimation = Math.Max(7, (int)Math.Round(item.useTime * 0.82f));
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        ApplyHyperflight(player);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + direction * 14f;

        if (omp.ultimateAttack) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 targetPosition = Main.MouseWorld;
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            int cometCount = omp.PrimaryAbilityEnabled ? 7 : 6;
            float spacing = 36f;

            for (int i = 0; i < cometCount; i++) {
                float offset = (i - (cometCount - 1) * 0.5f) * spacing;
                Vector2 impactPoint = targetPosition + new Vector2(offset, i % 2 == 0 ? -18f : 18f);
                Vector2 skySpawn = impactPoint + new Vector2(Main.rand.NextFloat(-22f, 22f), -420f - i * 26f);
                Vector2 cometVelocity = (impactPoint - skySpawn).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(17f, 21f);
                Projectile.NewProjectile(source, skySpawn, cometVelocity, UltimateAttack, finalDamage,
                    knockback + 1.2f, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
            }

            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 offset = Main.MouseWorld - player.MountedCenter;
            if (offset == Vector2.Zero)
                offset = new Vector2(player.direction, 0f);

            bool hyperflight = omp.PrimaryAbilityEnabled;
            float maxRange = hyperflight ? HyperflightDiveRange : BaseDiveRange;
            float requestedDistance = Math.Min(offset.Length(), maxRange);
            Vector2 diveDirection = offset.SafeNormalize(new Vector2(player.direction, 0f));
            float diveSpeed = AstrodactylDiveProjectile.GetDiveSpeed(hyperflight);
            int diveFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / diveSpeed),
                AstrodactylDiveProjectile.MinDiveFrames, AstrodactylDiveProjectile.MaxDiveFrames);
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));

            int projectileIndex = Projectile.NewProjectile(source, player.Center + diveDirection * 20f,
                diveDirection * diveSpeed, SecondaryAbilityAttack, finalDamage, knockback + 1.5f, player.whoAmI,
                hyperflight ? 1f : 0f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.timeLeft = diveFrames;
                projectile.netUpdate = true;
            }

            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed, SecondaryAttack, finalDamage,
                knockback + 0.8f, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
            return false;
        }

        int boltCount = omp.PrimaryAbilityEnabled ? 2 : 1;
        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        for (int i = 0; i < boltCount; i++) {
            float spread = boltCount == 1 ? 0f : (i == 0 ? -0.05f : 0.05f);
            Vector2 shotVelocity = direction.RotatedBy(spread) * PrimaryShootSpeed;
            Projectile.NewProjectile(source, spawnPosition, shotVelocity, PrimaryAttack, primaryDamage, knockback,
                player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
        }

        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.NecroHelmet;
        player.body = ArmorIDs.Body.NecroBreastplate;
        player.legs = ArmorIDs.Legs.NecroGreaves;
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

    private static void ApplyHyperflight(Player player) {
        if (player.controlJump || player.controlUp) {
            float ascentAcceleration = player.controlUp ? 0.54f : 0.38f;
            float maxRiseSpeed = player.controlUp ? -7.4f : -5.8f;
            player.velocity.Y = Math.Max(maxRiseSpeed, player.velocity.Y - ascentAcceleration);
        }
        else if (player.velocity.Y > -0.8f) {
            player.velocity.Y = Math.Min(player.velocity.Y, 2.2f);
        }

        if (player.controlDown)
            player.velocity.Y = Math.Min(player.velocity.Y + 0.3f, 8f);
        else if (player.velocity.Y > 0f)
            player.velocity.Y *= 0.86f;

        player.fallStart = (int)(player.position.Y / 16f);
        player.maxFallSpeed = 8f;
    }
}
