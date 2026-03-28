using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Arctiguana;

public class ArctiguanaTransformation : Transformation {
    private const int FreezeRayPropulsionCooldown = 12 * 60;
    private const int FreezeRayPropulsionCost = 18;
    private const int IceConstructEnergyCost = 24;
    private const int IceConstructCooldown = 15 * 60;
    private const int MaxActiveConstructs = 3;
    private const float FreezeRayDamageMultiplier = 0.92f;
    private const float FreezingBreathDamageMultiplier = 0.42f;
    private const float IceConstructDamageMultiplier = 1.12f;
    private const float UltimateDamageMultiplier = 0.82f;

    public override string FullID => "Ben10Mod:Arctiguana";
    public override string TransformationName => "Arctiguana";
    public override int TransformationBuffId => ModContent.BuffType<Arctiguana_Buff>();
    public override string Description =>
        "A cold-blooded reptilian bruiser that controls space with focused freeze rays, icy breath, solid constructs, and recoil-powered movement.";

    public override List<string> Abilities => new() {
        "Freeze ray that chills and locks enemies down",
        "Freezing breath for close-range control",
        "Freeze-Ray Propulsion for instant repositioning",
        "Ice constructs that block space and punish approach",
        "Enhanced strength, durability, and jumping",
        "Cold immunity and wall climbing",
        "Absolute Zero Ray for sustained freezing pressure"
    };

    public override string PrimaryAttackName => "Freeze Ray";
    public override string SecondaryAttackName => "Freezing Breath";
    public override string PrimaryAbilityName => "Freeze-Ray Propulsion";
    public override string SecondaryAbilityAttackName => "Ice Construct";
    public override string UltimateAttackName => "Absolute Zero Ray";

    public override int PrimaryAttack => ModContent.ProjectileType<ArctiguanaFreezeRayProjectile>();
    public override int PrimaryAttackSpeed => 14;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => FreezeRayDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<ArctiguanaBreathProjectile>();
    public override int SecondaryAttackSpeed => 12;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => FreezingBreathDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => FreezeRayPropulsionCooldown;
    public override int PrimaryAbilityCost => FreezeRayPropulsionCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<ArctiguanaIceConstructProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => IceConstructDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => IceConstructEnergyCost;
    public override int SecondaryAbilityCooldown => IceConstructCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<ArctiguanaUltimateBeamProjectile>();
    public override int UltimateAttackSpeed => 8;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override bool UltimateChannel => true;
    public override float UltimateAttackModifier => UltimateDamageMultiplier;
    public override int UltimateAbilityCost => 50;
    public override int UltimateEnergyCost => 12;
    public override int UltimateAttackSustainEnergyCost => 8;
    public override int UltimateAttackSustainInterval => 8;
    public override int UltimateAbilityCooldown => 60 * 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<ArctiguanaIceConstructProjectile>(),
            ModContent.ProjectileType<ArctiguanaUltimateBeamProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.GetKnockback<HeroDamage>() += 0.45f;
        player.GetArmorPenetration<HeroDamage>() += 6;
        player.statDefense += 8;
        player.endurance += 0.04f;
        player.jumpSpeedBoost += 2.3f;
        player.moveSpeed += 0.08f;
        player.runAcceleration += 0.06f;
        player.noFallDmg = true;
        player.iceSkate = true;
        player.spikedBoots = 2;
        player.buffImmune[BuffID.Chilled] = true;
        player.buffImmune[BuffID.Frozen] = true;
        player.buffImmune[BuffID.Frostburn] = true;
        player.buffImmune[BuffID.Frostburn2] = true;
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!TryGetWallDirection(player, out int wallDirection))
            return;

        bool pressingIntoWall = wallDirection < 0 ? player.controlLeft : player.controlRight;
        if (!pressingIntoWall || player.mount.Active)
            return;

        player.fallStart = (int)(player.position.Y / 16f);
        player.velocity.X *= 0.82f;

        if (player.controlUp) {
            player.velocity.Y = -4.2f;
        }
        else if (player.controlDown) {
            player.velocity.Y = 3.1f;
        }
        else if (player.velocity.Y > 0.6f) {
            player.velocity.Y = 0.6f;
        }
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled || omp.PrimaryAbilityWasEnabled || Main.myPlayer != player.whoAmI)
            return;

        ExecuteFreezeRayPropulsion(player);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        Vector2 mouthOffset = new(player.direction * 10f, -10f);
        Vector2 spawnPosition = player.MountedCenter + mouthOffset + direction * 12f;

        if (omp.ultimateAttack) {
            if (HasActiveOwnedProjectile(player, UltimateAttack))
                return false;

            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction, UltimateAttack, finalDamage, knockback, player.whoAmI);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            int constructType = ModContent.ProjectileType<ArctiguanaIceConstructProjectile>();
            CullOldestConstruct(player, constructType);

            Vector2 constructPosition = Main.MouseWorld;
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));
            int projectileIndex = Projectile.NewProjectile(source, constructPosition, Vector2.Zero, constructType,
                finalDamage, knockback + 1f, player.whoAmI);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                omp.transformationAttackSerial++;
                Main.projectile[projectileIndex].localAI[1] = omp.transformationAttackSerial;
                Main.projectile[projectileIndex].netUpdate = true;
            }

            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction, SecondaryAttack, finalDamage, knockback,
                player.whoAmI);
            return false;
        }

        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed, PrimaryAttack, primaryDamage,
            knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.SilverHelmet;
        player.body = ArmorIDs.Body.SilverChainmail;
        player.legs = ArmorIDs.Legs.SilverGreaves;
    }

    private static void ExecuteFreezeRayPropulsion(Player player) {
        Vector2 direction = ResolveAimDirection(player, new Vector2(player.direction, -0.45f));
        if (direction.Y > 0.45f) {
            direction.Y = 0.45f;
            direction.Normalize();
        }

        player.velocity = direction * 16.5f;
        if (direction.Y < -0.1f)
            player.velocity.Y -= 2.4f;

        player.fallStart = (int)(player.position.Y / 16f);
        SoundEngine.PlaySound(SoundID.Item30 with { Pitch = -0.3f, Volume = 0.9f }, player.Center);

        if (Main.dedServ)
            return;

        Vector2 exhaustDirection = -direction;
        for (int i = 0; i < 18; i++) {
            Dust dust = Dust.NewDustPerfect(player.MountedCenter + exhaustDirection * 14f +
                Main.rand.NextVector2Circular(10f, 10f), i % 3 == 0 ? DustID.IceTorch : DustID.Frost,
                exhaustDirection * Main.rand.NextFloat(2.2f, 5f) + Main.rand.NextVector2Circular(0.8f, 0.8f), 110,
                new Color(175, 235, 255), Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackDirection) {
        Vector2 direction = fallbackDirection.SafeNormalize(new Vector2(player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction.SafeNormalize(new Vector2(player.direction, 0f));
    }

    private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
                return true;
        }

        return false;
    }

    private static void CullOldestConstruct(Player player, int projectileType) {
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

        if (activeCount >= MaxActiveConstructs && oldestIndex != -1)
            Main.projectile[oldestIndex].Kill();
    }

    private static bool TryGetWallDirection(Player player, out int wallDirection) {
        wallDirection = 0;
        if (player.velocity.Y <= -0.1f)
            return false;

        const int sampleWidth = 4;
        int sampleHeight = Math.Max(10, player.height - 12);
        Vector2 leftSample = player.position + new Vector2(-sampleWidth, 6f);
        if (Collision.SolidCollision(leftSample, sampleWidth, sampleHeight)) {
            wallDirection = -1;
            return true;
        }

        Vector2 rightSample = player.position + new Vector2(player.width, 6f);
        if (Collision.SolidCollision(rightSample, sampleWidth, sampleHeight)) {
            wallDirection = 1;
            return true;
        }

        return false;
    }

    private static void KillOwnedProjectiles(Player player, params int[] projectileTypes) {
        if (projectileTypes == null || projectileTypes.Length == 0)
            return;

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
