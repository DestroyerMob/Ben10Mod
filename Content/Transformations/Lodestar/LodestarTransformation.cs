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

namespace Ben10Mod.Content.Transformations.Lodestar;

public class LodestarTransformation : Transformation {
    private const int MagLevDuration = 9 * 60;
    private const int MagLevCooldown = 24 * 60;
    private const int MagLevCost = 20;
    private const int AnchorEnergyCost = 26;
    private const int AnchorCooldown = 16 * 60;
    private const int MaxActiveAnchors = 2;
    private const float PrimaryDamageMultiplier = 0.9f;
    private const float SecondaryDamageMultiplier = 1.04f;
    private const float AnchorDamageMultiplier = 0.96f;
    private const float UltimateDamageMultiplier = 1.16f;

    public override string FullID => "Ben10Mod:Lodestar";
    public override string TransformationName => "Lodestar";
    public override int TransformationBuffId => ModContent.BuffType<Lodestar_Buff>();
    public override string Description =>
        "A magnetic controller who flips whole encounters between pull and repel, reshaping every bolt, field, anchor, and vortex around his current polarity.";

    public override List<string> Abilities => new() {
        "Polarized bolts that shift with your current polarity",
        "Magnetic field orb that pulls or shoves enemies",
        "Mag-Lev that flips your polarity and lets you hover",
        "Magnetic Anchor that pins down a point in space",
        "Polar Vortex that implodes or erupts based on polarity"
    };

    public override string PrimaryAttackName => "Polarized Bolt";
    public override string SecondaryAttackName => "Magnetic Drag";
    public override string PrimaryAbilityName => "Mag-Lev";
    public override string SecondaryAbilityAttackName => "Magnetic Anchor";
    public override string UltimateAttackName => "Polar Vortex";

    public override int PrimaryAttack => ModContent.ProjectileType<LodestarMagnetBoltProjectile>();
    public override int PrimaryAttackSpeed => 14;
    public override int PrimaryShootSpeed => 16;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<LodestarMagneticOrbProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => MagLevDuration;
    public override int PrimaryAbilityCooldown => MagLevCooldown;
    public override int PrimaryAbilityCost => MagLevCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<LodestarAnchorProjectile>();
    public override int SecondaryAbilityAttackSpeed => 20;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => AnchorDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => AnchorEnergyCost;
    public override int SecondaryAbilityCooldown => AnchorCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<LodestarPolarVortexProjectile>();
    public override int UltimateAttackSpeed => 28;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => UltimateDamageMultiplier;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 58 * 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<LodestarAnchorProjectile>(),
            ModContent.ProjectileType<LodestarPolarVortexProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetArmorPenetration<HeroDamage>() += 8;
        player.GetKnockback<HeroDamage>() += 0.55f;
        player.statDefense += 8;
        player.endurance += 0.04f;
        player.moveSpeed += 0.06f;
        player.runAcceleration += 0.04f;
        player.noFallDmg = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.ignoreWater = true;
        player.wingTimeMax += 30;
        player.wingTime = Math.Max(player.wingTime, 14f);
        player.armorEffectDrawShadow = true;
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled || player.mount.Active)
            return;

        ApplyMagLev(player);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + direction * 16f;

        if (omp.ultimateAttack) {
            if (HasActiveOwnedProjectile(player, UltimateAttack))
                return false;

            Vector2 vortexPosition = ResolveTargetPosition(player, direction, 180f);
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, vortexPosition, Vector2.Zero, UltimateAttack, finalDamage, knockback + 1f,
                player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            Vector2 anchorPosition = ResolveTargetPosition(player, direction, 140f);
            int anchorType = ModContent.ProjectileType<LodestarAnchorProjectile>();
            CullOldestAnchor(player, anchorType);

            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));
            int projectileIndex = Projectile.NewProjectile(source, anchorPosition, Vector2.Zero, anchorType, finalDamage,
                knockback + 0.6f, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
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
                knockback + 0.7f, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
            return false;
        }

        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed, PrimaryAttack, primaryDamage,
            knockback, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.LeadHelmet;
        player.body = ArmorIDs.Body.LeadChainmail;
        player.legs = ArmorIDs.Legs.LeadGreaves;
    }

    private static void ApplyMagLev(Player player) {
        if (player.controlJump || player.controlUp) {
            float maxRiseSpeed = player.controlUp ? -6.4f : -5.4f;
            float riseAcceleration = player.controlUp ? 0.4f : 0.28f;
            player.velocity.Y = Math.Max(maxRiseSpeed, player.velocity.Y - riseAcceleration);
        }
        else if (player.velocity.Y > 0f) {
            player.velocity.Y *= 0.9f;
        }

        if (player.controlDown)
            player.velocity.Y = Math.Min(player.velocity.Y + 0.24f, 6f);

        player.maxFallSpeed = Math.Min(player.maxFallSpeed, 6f);
        player.fallStart = (int)(player.position.Y / 16f);

        if (!Main.dedServ && Main.rand.NextBool(5)) {
            Dust dust = Dust.NewDustPerfect(player.Bottom + Main.rand.NextVector2Circular(10f, 4f),
                Main.rand.NextBool() ? DustID.Iron : DustID.Firework_Red,
                new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(0.3f, 1f)), 110,
                new Color(225, 105, 95), Main.rand.NextFloat(0.85f, 1.08f));
            dust.noGravity = true;
        }
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

    private static Vector2 ResolveTargetPosition(Player player, Vector2 fallbackDirection, float fallbackDistance) {
        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            return Main.MouseWorld;
        }

        return player.Center + fallbackDirection * fallbackDistance;
    }

    private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
                return true;
        }

        return false;
    }

    private static void CullOldestAnchor(Player player, int projectileType) {
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

        if (activeCount >= MaxActiveAnchors && oldestIndex != -1)
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
