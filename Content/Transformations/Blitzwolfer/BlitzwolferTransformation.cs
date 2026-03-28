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

namespace Ben10Mod.Content.Transformations.Blitzwolfer;

public class BlitzwolferTransformation : Transformation {
    private const int HeightenedSensesDuration = 8 * 60;
    private const int HeightenedSensesCooldown = 26 * 60;
    private const int HeightenedSensesCost = 18;
    private const int PounceEnergyCost = 24;
    private const int PounceCooldown = 14 * 60;
    private const float PrimaryDamageMultiplier = 0.9f;
    private const float SecondaryDamageMultiplier = 1.1f;
    private const float PounceDamageMultiplier = 1.34f;
    private const float UltimateDamageMultiplier = 1.38f;
    private const float BasePounceRange = 340f;
    private const float HeightenedPounceRange = 430f;

    public override string FullID => "Ben10Mod:Blitzwolfer";
    public override string TransformationName => "Blitzwolfer";
    public override int TransformationBuffId => ModContent.BuffType<Blitzwolfer_Buff>();
    public override string Description =>
        "A sonic apex hunter that overwhelms targets with rapid bark blasts, destabilizing howls, predatory lunges, and a devastating lunar roar.";

    public override List<string> Abilities => new() {
        "Rapid sonic bark projectiles",
        "Wide destabilizing howl burst",
        "Heightened senses combat mode",
        "Lupine pounce lunge attack",
        "Massive lunar howl ultimate"
    };

    public override string PrimaryAttackName => "Sonic Bark";
    public override string SecondaryAttackName => "Howl Burst";
    public override string SecondaryAbilityAttackName => "Lupine Pounce";
    public override string UltimateAttackName => "Lunar Howl";

    public override int PrimaryAttack => ModContent.ProjectileType<BlitzwolferSonicBoltProjectile>();
    public override int PrimaryAttackSpeed => 13;
    public override int PrimaryShootSpeed => 17;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<BlitzwolferHowlProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => HeightenedSensesDuration;
    public override int PrimaryAbilityCooldown => HeightenedSensesCooldown;
    public override int PrimaryAbilityCost => HeightenedSensesCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<BlitzwolferPounceProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => PounceDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => PounceEnergyCost;
    public override int SecondaryAbilityCooldown => PounceCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<BlitzwolferLunarHowlProjectile>();
    public override int UltimateAttackSpeed => 30;
    public override int UltimateShootSpeed => 9;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override float UltimateAttackModifier => UltimateDamageMultiplier;
    public override int UltimateEnergyCost => 55;
    public override int UltimateAbilityCooldown => 60 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetAttackSpeed<HeroDamage>() += 0.06f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetKnockback<HeroDamage>() += 0.55f;
        player.statDefense += 6;
        player.moveSpeed += 0.14f;
        player.runAcceleration += 0.08f;
        player.maxRunSpeed += 0.8f;
        player.jumpSpeedBoost += 1.8f;
        player.noFallDmg = true;
        player.blackBelt = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.moveSpeed += 0.2f;
        player.runAcceleration += 0.12f;
        player.maxRunSpeed += 1.1f;
        player.jumpSpeedBoost += 1.2f;
        player.detectCreature = true;
        player.nightVision = true;
        player.armorEffectDrawShadow = true;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        if (!omp.PrimaryAbilityEnabled)
            return;

        item.useTime = item.useAnimation = Math.Max(8, (int)Math.Round(item.useTime * 0.86f));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + direction * 14f;

        if (omp.ultimateAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, player.Center + direction * 18f, direction * UltimateShootSpeed,
                UltimateAttack, finalDamage, knockback + 1.8f, player.whoAmI);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 offset = Main.MouseWorld - player.MountedCenter;
            if (offset == Vector2.Zero)
                offset = new Vector2(player.direction, 0f);

            bool heightened = omp.PrimaryAbilityEnabled;
            float maxRange = heightened ? HeightenedPounceRange : BasePounceRange;
            float requestedDistance = Math.Min(offset.Length(), maxRange);
            Vector2 pounceDirection = offset.SafeNormalize(new Vector2(player.direction, 0f));
            float pounceSpeed = BlitzwolferPounceProjectile.GetPounceSpeed(heightened);
            int pounceFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / pounceSpeed),
                BlitzwolferPounceProjectile.MinPounceFrames, BlitzwolferPounceProjectile.MaxPounceFrames);
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));

            int projectileIndex = Projectile.NewProjectile(source, player.Center + pounceDirection * 18f,
                pounceDirection * pounceSpeed, SecondaryAbilityAttack, finalDamage, knockback + 1.8f, player.whoAmI,
                heightened ? 1f : 0f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.timeLeft = pounceFrames;
                projectile.netUpdate = true;
            }

            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed, SecondaryAttack,
                finalDamage, knockback + 0.8f, player.whoAmI);
            return false;
        }

        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed, PrimaryAttack, primaryDamage,
            knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.ShadowHelmet;
        player.body = ArmorIDs.Body.ShadowScalemail;
        player.legs = ArmorIDs.Legs.ShadowGreaves;
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
}
