using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WildVine;

public class WildVineTransformation : Transformation {
    private const int VineGrappleEnergyCost = 10;
    private const int VineGrappleCooldown = 6 * 60;
    private const int BriarSnareEnergyCost = 18;
    private const int BriarSnareCooldown = 12 * 60;
    private const int VerdantBloomEnergyRequirement = 60;
    private const int VerdantBloomEnergyCost = 36;
    private const int VerdantBloomCooldown = 42 * 60;

    public override string FullID => "Ben10Mod:WildVine";
    public override string TransformationName => "Wildvine";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<WildVine_Buff>();

    public override string Description =>
        "A flexible Florauna who controls space with thorn lashes, toxic seed blooms, mobile vine grapples, and a snaring briar throw.";

    public override List<string> Abilities => new() {
        "Thorn whip that poisons on contact",
        "Gas seed bomb with a cursor-driven lob and lingering toxic cloud",
        "Vine Grapple for fast repositioning",
        "Briar Snare that hooks smaller enemies and drags them in",
        "Verdant Bloom that floods a target zone with oversized toxic blooms"
    };

    public override string PrimaryAttackName => "Thorn Whip";
    public override string SecondaryAttackName => "Gas Seed Bomb";
    public override string PrimaryAbilityName => "Vine Grapple";
    public override string SecondaryAbilityName => "Briar Snare";
    public override string UltimateAbilityName => "Verdant Bloom";
    public override string PrimaryAbilityAttackName => "Vine Grapple";
    public override string SecondaryAbilityAttackName => "Briar Snare";
    public override string UltimateAttackName => "Verdant Bloom";

    public override int PrimaryAttack => ModContent.ProjectileType<WildVineWhipProjectile>();
    public override float PrimaryAttackModifier => 0.95f;
    public override int PrimaryAttackSpeed => 26;
    public override int PrimaryShootSpeed => 4;

    public override int SecondaryAttack => ModContent.ProjectileType<WildVineBomb>();
    public override float SecondaryAttackModifier => 0.82f;
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 10;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;

    public override int PrimaryAbilityAttack => ModContent.ProjectileType<WildVineGrapple>();
    public override float PrimaryAbilityAttackModifier => 0f;
    public override int PrimaryAbilityAttackSpeed => 20;
    public override int PrimaryAbilityAttackShootSpeed => 18;
    public override int PrimaryAbilityAttackUseStyle => ItemUseStyleID.Shoot;
    public override int PrimaryAbilityAttackEnergyCost => VineGrappleEnergyCost;
    public override int PrimaryAbilityCooldown => VineGrappleCooldown;
    public override bool PrimaryAbilityAttackSingleUse => true;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<WildVineProjectile>();
    public override float SecondaryAbilityAttackModifier => 1.12f;
    public override int SecondaryAbilityAttackSpeed => 22;
    public override int SecondaryAbilityAttackShootSpeed => 18;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAbilityAttackEnergyCost => BriarSnareEnergyCost;
    public override int SecondaryAbilityCooldown => BriarSnareCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<WildVineBomb>();
    public override float UltimateAttackModifier => 0.58f;
    public override int UltimateAttackSpeed => 34;
    public override int UltimateShootSpeed => 12;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override int UltimateEnergyCost => VerdantBloomEnergyCost;
    public override int UltimateAbilityCost => VerdantBloomEnergyRequirement;
    public override int UltimateAbilityCooldown => VerdantBloomCooldown;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetAttackSpeed<HeroDamage>() += 0.05f;
        player.moveSpeed += 0.08f;
        player.runAcceleration += 0.06f;
        player.jumpSpeedBoost += 1.8f;
        player.noFallDmg = true;
        player.lifeRegen += player.velocity.Y == 0f ? 3 : 1;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        var profile = GetSelectedAttackProfile(omp);
        if (profile == null || profile.ProjectileType <= 0)
            return false;

        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.ultimateAttack) {
            FireVerdantBloom(player, source, damage, knockback, direction, profile.DamageMultiplier, profile.ShootSpeed);
            return false;
        }

        if (omp.IsPrimaryAbilityAttackLoaded) {
            Vector2 grappleSpawn = player.MountedCenter + direction * 12f;
            Projectile.NewProjectile(source, grappleSpawn, direction * profile.ShootSpeed, profile.ProjectileType,
                0, 0f, player.whoAmI);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            int snareDamage = ScaleDamage(damage, profile.DamageMultiplier);
            Vector2 spawnPosition = player.MountedCenter + direction * 14f;
            Projectile.NewProjectile(source, spawnPosition, direction * profile.ShootSpeed, profile.ProjectileType,
                snareDamage, knockback + 1f, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            Vector2 targetPosition = ResolveAimTarget(player, direction, 240f, 80f, 360f);
            SpawnSeedBomb(player, source, damage, knockback, direction, targetPosition,
                profile.DamageMultiplier, profile.ShootSpeed, WildVineBomb.VariantRegular);
            return false;
        }

        int whipDamage = ScaleDamage(damage, profile.DamageMultiplier);
        Projectile.NewProjectile(source, position, velocity, profile.ProjectileType, whipDamage, knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<WildVine>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    private static void FireVerdantBloom(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback,
        Vector2 direction, float damageMultiplier, float shootSpeed) {
        Vector2 focusPoint = ResolveAimTarget(player, direction, 300f, 120f, 460f);
        Vector2[] targetOffsets = {
            new Vector2(-110f, 14f),
            new Vector2(-55f, -12f),
            new Vector2(0f, -24f),
            new Vector2(55f, -12f),
            new Vector2(110f, 14f)
        };

        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        for (int i = 0; i < targetOffsets.Length; i++) {
            Vector2 spawnPosition = GetSeedSpawnPosition(player, direction) + perpendicular * (i - 2) * 6f;
            Vector2 targetPosition = focusPoint + targetOffsets[i];
            SpawnSeedBomb(player, source, damage, knockback + 0.6f, direction, targetPosition,
                damageMultiplier, shootSpeed + 1.25f, WildVineBomb.VariantBloom, spawnPosition);
        }
    }

    private static void SpawnSeedBomb(Player player, EntitySource_ItemUse_WithAmmo source, int baseDamage, float knockback,
        Vector2 direction, Vector2 targetPosition, float damageMultiplier, float shootSpeed, float variant,
        Vector2? spawnOverride = null) {
        Vector2 spawnPosition = spawnOverride ?? GetSeedSpawnPosition(player, direction);
        Vector2 lobVelocity = CreateSeedBombVelocity(spawnPosition, targetPosition, direction, shootSpeed,
            variant >= WildVineBomb.VariantBloom ? 0.9f : 0.55f,
            variant >= WildVineBomb.VariantBloom ? 13f : 11f,
            variant >= WildVineBomb.VariantBloom ? 30f : 28f);
        int finalDamage = ScaleDamage(baseDamage, damageMultiplier);

        Projectile.NewProjectile(source, spawnPosition, lobVelocity, ModContent.ProjectileType<WildVineBomb>(),
            finalDamage, knockback, player.whoAmI, variant);
    }

    private static Vector2 CreateSeedBombVelocity(Vector2 spawnPosition, Vector2 targetPosition, Vector2 fallbackDirection,
        float shootSpeed, float extraArcLift, float minTravelFrames, float maxTravelFrames) {
        Vector2 delta = targetPosition - spawnPosition;
        Vector2 safeFallback = fallbackDirection.SafeNormalize(Vector2.UnitX);
        if (delta.LengthSquared() < 16f)
            return safeFallback * Math.Max(shootSpeed, 10f) + new Vector2(0f, -extraArcLift);

        float speed = Math.Max(shootSpeed, 10f);
        float travelFrames = MathHelper.Clamp(delta.Length() / speed, minTravelFrames, maxTravelFrames);
        Vector2 velocity = new(delta.X / travelFrames,
            (delta.Y - 0.5f * WildVineBomb.Gravity * travelFrames * travelFrames) / travelFrames);
        velocity.Y -= extraArcLift;

        float maxSpeed = speed * 1.6f;
        if (velocity.LengthSquared() > maxSpeed * maxSpeed)
            velocity = velocity.SafeNormalize(safeFallback) * maxSpeed;

        return velocity;
    }

    private static Vector2 GetSeedSpawnPosition(Player player, Vector2 direction) {
        return player.MountedCenter + direction * 18f + new Vector2(0f, -player.height * 0.16f);
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
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

    private static Vector2 ResolveAimTarget(Player player, Vector2 direction, float fallbackDistance,
        float minDistance, float maxDistance) {
        Vector2 targetPosition = (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer)
            ? Main.MouseWorld
            : player.Center + direction * fallbackDistance;
        Vector2 targetOffset = targetPosition - player.Center;

        if (targetOffset == Vector2.Zero)
            return player.Center + direction * fallbackDistance;

        float clampedDistance = MathHelper.Clamp(targetOffset.Length(), minDistance, maxDistance);
        return player.Center + targetOffset.SafeNormalize(direction) * clampedDistance;
    }
}
