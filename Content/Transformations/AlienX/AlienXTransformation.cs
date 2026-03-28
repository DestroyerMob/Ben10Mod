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

namespace Ben10Mod.Content.Transformations.AlienX;

public class AlienXTransformation : Transformation {
    private const int DeliberationDuration = 7 * 60;
    private const int DeliberationCooldown = 30 * 60;
    private const int DeliberationCost = 22;
    private const int RepulseBurstCost = 28;
    private const int RepulseBurstCooldown = 16 * 60;

    public override string FullID => "Ben10Mod:AlienX";
    public override string TransformationName => "Alien X";
    public override int TransformationBuffId => ModContent.BuffType<AlienX_Buff>();
    public override string Description =>
        "A Celestialsapien who bends force and space at will, blasting enemies away, collapsing them into singularities, and ending the fight in a star-bright supernova.";

    public override List<string> Abilities => new() {
        "Cosmic waves that fling enemies back",
        "Pocket singularity that drags foes inward",
        "Deliberation for precise cursor-cast control",
        "Repulsion burst that clears whole groups",
        "Supernova that erupts from Alien X himself"
    };

    public override string PrimaryAttackName => "Cosmic Wave";
    public override string SecondaryAttackName => "Pocket Singularity";
    public override string PrimaryAbilityName => "Deliberation";
    public override string SecondaryAbilityAttackName => "Cosmic Repulse";
    public override string UltimateAttackName => "Supernova";

    public override int PrimaryAttack => ModContent.ProjectileType<AlienXGravityPulseProjectile>();
    public override int PrimaryAttackSpeed => 15;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.92f;

    public override int SecondaryAttack => ModContent.ProjectileType<AlienXBlackHoleProjectile>();
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAttackModifier => 0.78f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => DeliberationDuration;
    public override int PrimaryAbilityCooldown => DeliberationCooldown;
    public override int PrimaryAbilityCost => DeliberationCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<AlienXForceWaveProjectile>();
    public override int SecondaryAbilityAttackSpeed => 24;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 1.12f;
    public override int SecondaryAbilityAttackEnergyCost => RepulseBurstCost;
    public override int SecondaryAbilityCooldown => RepulseBurstCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<AlienXVerdictProjectile>();
    public override int UltimateAttackSpeed => 30;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => 2.2f;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 60 * 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<AlienXBlackHoleProjectile>(),
            ModContent.ProjectileType<AlienXForceWaveProjectile>(),
            ModContent.ProjectileType<AlienXVerdictProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 4f;
        player.statDefense += 10;
        player.endurance += 0.06f;
        player.moveSpeed += 0.04f;
        player.noFallDmg = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.moveSpeed -= 0.08f;
        player.statDefense += 6;
        player.endurance += 0.05f;
        player.noKnockback = true;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.34f, 0.34f, 0.48f));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 targetPosition = ResolveTargetPosition(player, direction, 180f);

        if (omp.ultimateAttack) {
            int verdictType = ModContent.ProjectileType<AlienXVerdictProjectile>();
            if (HasActiveOwnedProjectile(player, verdictType))
                return false;

            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, verdictType, finalDamage, knockback + 1.2f,
                player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            int repulseType = ModContent.ProjectileType<AlienXForceWaveProjectile>();
            if (HasActiveOwnedProjectile(player, repulseType))
                return false;

            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));
            Projectile.NewProjectile(source, player.Center, direction, repulseType, finalDamage, knockback + 1.4f,
                player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
            return false;
        }

        if (omp.altAttack) {
            int singularityType = ModContent.ProjectileType<AlienXBlackHoleProjectile>();
            if (HasActiveOwnedProjectile(player, singularityType))
                return false;

            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Vector2 singularityPosition = omp.PrimaryAbilityEnabled ? targetPosition : player.Center + direction * 96f;
            Projectile.NewProjectile(source, singularityPosition, Vector2.Zero, singularityType,
                finalDamage, knockback + 0.4f, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
            return false;
        }

        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Vector2 spawnPosition = omp.PrimaryAbilityEnabled ? targetPosition : player.Center + direction * 16f;
        Vector2 pulseVelocity = omp.PrimaryAbilityEnabled ? direction * 6f : direction * PrimaryShootSpeed;
        Projectile.NewProjectile(source, spawnPosition, pulseVelocity, ModContent.ProjectileType<AlienXGravityPulseProjectile>(),
            primaryDamage, knockback, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.PlatinumHelmet;
        player.body = ArmorIDs.Body.PlatinumChainmail;
        player.legs = ArmorIDs.Legs.PlatinumGreaves;
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
        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer)
            return Main.MouseWorld;

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
