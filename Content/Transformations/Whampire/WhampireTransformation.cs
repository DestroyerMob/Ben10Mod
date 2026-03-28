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

namespace Ben10Mod.Content.Transformations.Whampire;

public class WhampireTransformation : Transformation {
    private const int NightcloakDuration = 10 * 60;
    private const int NightcloakCooldown = 28 * 60;
    private const int NightcloakCost = 20;
    private const int HypnosisEnergyCost = 26;
    private const int HypnosisCooldown = 16 * 60;
    private const float PrimaryDamageMultiplier = 0.9f;
    private const float SecondaryDamageMultiplier = 1.02f;
    private const float HypnosisDamageMultiplier = 0.96f;
    private const float UltimateDamageMultiplier = 1.16f;

    public override string FullID => "Ben10Mod:Whampire";
    public override string TransformationName => "Whampire";
    public override int TransformationBuffId => ModContent.BuffType<Whampire_Buff>();
    public override string Description =>
        "A nocturnal Vladat predator that fires corruptura bolts, disorients prey with screeches, locks targets down with hypnosis, and floods arenas with a midnight swarm.";

    public override List<string> Abilities => new() {
        "Corruptura bolt primary fire",
        "Vampiric screech burst",
        "Nightcloak aerial stance",
        "Hypnotic gaze beam",
        "Midnight swarm ultimate"
    };

    public override string PrimaryAttackName => "Corruptura Bolt";
    public override string SecondaryAttackName => "Vampiric Screech";
    public override string SecondaryAbilityAttackName => "Hypnotic Gaze";
    public override string UltimateAttackName => "Midnight Swarm";

    public override int PrimaryAttack => ModContent.ProjectileType<WhampireCorrupturaBoltProjectile>();
    public override int PrimaryAttackSpeed => 14;
    public override int PrimaryShootSpeed => 16;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<WhampireScreechProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => NightcloakDuration;
    public override int PrimaryAbilityCooldown => NightcloakCooldown;
    public override int PrimaryAbilityCost => NightcloakCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<WhampireHypnosisProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => HypnosisDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => HypnosisEnergyCost;
    public override int SecondaryAbilityCooldown => HypnosisCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<WhampireNightSwarmProjectile>();
    public override int UltimateAttackSpeed => 30;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => UltimateDamageMultiplier;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 60 * 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<WhampireHypnosisProjectile>(),
            ModContent.ProjectileType<WhampireNightSwarmProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.11f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetArmorPenetration<HeroDamage>() += 4;
        player.statDefense += 7;
        player.endurance += 0.03f;
        player.moveSpeed += 0.12f;
        player.runAcceleration += 0.06f;
        player.maxRunSpeed += 0.7f;
        player.jumpSpeedBoost += 1.7f;
        player.noFallDmg = true;
        player.nightVision = true;
        player.buffImmune[BuffID.Blackout] = true;
        player.buffImmune[BuffID.Darkness] = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetAttackSpeed<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 4f;
        player.moveSpeed += 0.18f;
        player.runAcceleration += 0.12f;
        player.maxRunSpeed += 1f;
        player.jumpSpeedBoost += 1.2f;
        player.wingTimeMax += 70;
        player.wingTime = Math.Max(player.wingTime, 20f);
        player.endurance += 0.04f;
        player.blackBelt = true;
        player.armorEffectDrawShadow = true;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        if (!omp.PrimaryAbilityEnabled)
            return;

        item.useTime = item.useAnimation = Math.Max(8, (int)Math.Round(item.useTime * 0.88f));
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled || player.mount.Active)
            return;

        ApplyNightcloak(player);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + new Vector2(player.direction * 8f, -10f) + direction * 12f;
        bool cloaked = omp.PrimaryAbilityEnabled;

        if (omp.ultimateAttack) {
            if (HasActiveOwnedProjectile(player, UltimateAttack))
                return false;

            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, UltimateAttack, finalDamage, knockback + 0.8f,
                player.whoAmI, cloaked ? 1f : 0f);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction, SecondaryAbilityAttack, finalDamage, knockback + 0.5f,
                player.whoAmI, cloaked ? 1f : 0f);
            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed, SecondaryAttack, finalDamage,
                knockback + 0.8f, player.whoAmI, cloaked ? 1f : 0f);
            return false;
        }

        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed, PrimaryAttack, primaryDamage,
            knockback, player.whoAmI, cloaked ? 1f : 0f);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.ShadowHelmet;
        player.body = ArmorIDs.Body.ShadowScalemail;
        player.legs = ArmorIDs.Legs.ShadowGreaves;
    }

    private static void ApplyNightcloak(Player player) {
        if (player.controlJump || player.controlUp) {
            float ascentAcceleration = player.controlUp ? 0.48f : 0.34f;
            float maxRiseSpeed = player.controlUp ? -6.6f : -5.2f;
            player.velocity.Y = Math.Max(maxRiseSpeed, player.velocity.Y - ascentAcceleration);
        }
        else if (player.velocity.Y > -1f) {
            player.velocity.Y = Math.Min(player.velocity.Y, 2.8f);
        }

        if (player.controlDown)
            player.velocity.Y = Math.Min(player.velocity.Y + 0.3f, 6.6f);
        else if (player.velocity.Y > 0f)
            player.velocity.Y *= 0.9f;

        player.maxFallSpeed = 6.6f;
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
