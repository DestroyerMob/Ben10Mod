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

namespace Ben10Mod.Content.Transformations.Frankenstrike;

public class FrankenstrikeTransformation : Transformation {
    private const int OverchargeDuration = 10 * 60;
    private const int OverchargeCooldown = 30 * 60;
    private const int OverchargeCost = 20;
    private const int StormLeapEnergyCost = 26;
    private const int StormLeapCooldown = 14 * 60;
    private const float PrimaryDamageMultiplier = 0.94f;
    private const float SecondaryDamageMultiplier = 1.14f;
    private const float LeapDamageMultiplier = 1.32f;
    private const float UltimateStrikeDamageMultiplier = 1.36f;
    private const float BaseLeapRange = 360f;
    private const float OverchargedLeapRange = 450f;

    public override string FullID => "Ben10Mod:Frankenstrike";
    public override string TransformationName => "Frankenstrike";
    public override int TransformationBuffId => ModContent.BuffType<Frankenstrike_Buff>();
    public override string Description =>
        "A storm-forged Transylian bruiser that hurls tesla bolts, detonates thunderclaps, overcharges his body, and calls down crushing lightning barrages.";

    public override List<string> Abilities => new() {
        "Heavy tesla bolt primary",
        "Expanding thunderclap burst",
        "Overcharged body combat mode",
        "Storm leap lunge attack",
        "Thunderstorm strike barrage ultimate"
    };

    public override string PrimaryAttackName => "Tesla Bolt";
    public override string SecondaryAttackName => "Thunderclap";
    public override string SecondaryAbilityAttackName => "Storm Leap";
    public override string UltimateAttackName => "Thunderstorm Barrage";

    public override int PrimaryAttack => ModContent.ProjectileType<FrankenstrikeTeslaProjectile>();
    public override int PrimaryAttackSpeed => 15;
    public override int PrimaryShootSpeed => 17;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<FrankenstrikeThunderclapProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => OverchargeDuration;
    public override int PrimaryAbilityCooldown => OverchargeCooldown;
    public override int PrimaryAbilityCost => OverchargeCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<FrankenstrikeStormLeapProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => LeapDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => StormLeapEnergyCost;
    public override int SecondaryAbilityCooldown => StormLeapCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<FrankenstrikeLightningStrikeProjectile>();
    public override int UltimateAttackSpeed => 30;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => UltimateStrikeDamageMultiplier;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 58 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 4f;
        player.GetKnockback<HeroDamage>() += 0.7f;
        player.GetArmorPenetration<HeroDamage>() += 8;
        player.statDefense += 10;
        player.endurance += 0.05f;
        player.moveSpeed += 0.04f;
        player.runAcceleration += 0.03f;
        player.noFallDmg = true;
        player.buffImmune[BuffID.Electrified] = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetAttackSpeed<HeroDamage>() += 0.15f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.statDefense += 6;
        player.endurance += 0.02f;
        player.moveSpeed += 0.1f;
        player.runAcceleration += 0.06f;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.3f, 0.52f, 0.95f));
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        if (!omp.PrimaryAbilityEnabled)
            return;

        item.useTime = item.useAnimation = Math.Max(9, (int)Math.Round(item.useTime * 0.84f));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + direction * 16f;

        if (omp.ultimateAttack) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 targetPosition = Main.MouseWorld;
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            float[] offsets = omp.PrimaryAbilityEnabled
                ? new[] { -110f, -66f, -22f, 22f, 66f, 110f }
                : new[] { -92f, -46f, 0f, 46f, 92f };

            for (int i = 0; i < offsets.Length; i++) {
                Vector2 strikePosition = targetPosition + new Vector2(offsets[i], i % 2 == 0 ? -10f : 12f);
                int delay = i * 4;
                Projectile.NewProjectile(source, strikePosition, Vector2.Zero, UltimateAttack, finalDamage,
                    knockback + 1.8f, player.whoAmI, delay, omp.PrimaryAbilityEnabled ? 1f : 0f);
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

            bool overcharged = omp.PrimaryAbilityEnabled;
            float maxRange = overcharged ? OverchargedLeapRange : BaseLeapRange;
            float requestedDistance = Math.Min(offset.Length(), maxRange);
            Vector2 leapDirection = offset.SafeNormalize(new Vector2(player.direction, 0f));
            float leapSpeed = FrankenstrikeStormLeapProjectile.GetLeapSpeed(overcharged);
            int leapFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / leapSpeed),
                FrankenstrikeStormLeapProjectile.MinLeapFrames, FrankenstrikeStormLeapProjectile.MaxLeapFrames);
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));

            int projectileIndex = Projectile.NewProjectile(source, player.Center + leapDirection * 20f,
                leapDirection * leapSpeed, SecondaryAbilityAttack, finalDamage, knockback + 2f, player.whoAmI,
                overcharged ? 1f : 0f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.timeLeft = leapFrames;
                projectile.netUpdate = true;
            }

            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, player.Center + direction * 8f, Vector2.Zero, SecondaryAttack,
                finalDamage, knockback + 1.2f, player.whoAmI);
            return false;
        }

        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed, PrimaryAttack, primaryDamage,
            knockback, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MoltenHelmet;
        player.body = ArmorIDs.Body.MoltenBreastplate;
        player.legs = ArmorIDs.Legs.MoltenGreaves;
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
