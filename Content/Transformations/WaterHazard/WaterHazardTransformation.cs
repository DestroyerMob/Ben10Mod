using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WaterHazard;

public class WaterHazardTransformation : Transformation {
    private const int TidalSnareEnergyCost = 30;
    private const int TidalSnareCooldown = 18 * 60;

    public override string FullID => "Ben10Mod:WaterHazard";
    public override string TransformationName => "Water Hazard";
    public override int TransformationBuffId => ModContent.BuffType<WaterHazard_Buff>();

    public override string Description =>
        "An armored Orishan who controls the battlefield with pressurized water jets, crushing bursts, trapping geysers, and a massive tidal shockwave.";

    public override List<string> Abilities => new() {
        "Pressurized water jets",
        "Close-range riptide burst",
        "Pressurized reservoir stance",
        "Tidal snare geyser",
        "Monsoon break ultimate"
    };

    public override string PrimaryAttackName => "Pressure Jet";
    public override string SecondaryAttackName => "Riptide Burst";
    public override string SecondaryAbilityAttackName => "Tidal Snare";
    public override string UltimateAttackName => "Monsoon Break";

    public override int PrimaryAttack => ModContent.ProjectileType<WaterHazardPressureProjectile>();
    public override int PrimaryAttackSpeed => 14;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.82f;

    public override int SecondaryAttack => ModContent.ProjectileType<WaterHazardBurstProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAttackModifier => 1.2f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAbilityCooldown => 28 * 60;
    public override int PrimaryAbilityCost => 20;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<WaterHazardSnareProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 0.95f;
    public override int SecondaryAbilityAttackEnergyCost => TidalSnareEnergyCost;
    public override int SecondaryAbilityCooldown => TidalSnareCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<WaterHazardUltimateProjectile>();
    public override int UltimateAttackSpeed => 28;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => 2.2f;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 50 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.statDefense += 8;
        player.endurance += 0.04f;
        player.moveSpeed += 0.08f;
        player.maxRunSpeed += 0.6f;
        player.ignoreWater = true;
        player.accFlipper = true;
        player.gills = true;
        player.noFallDmg = true;

        if (player.wet || (Main.raining && player.ZoneRain)) {
            player.GetDamage<HeroDamage>() += 0.08f;
            player.GetAttackSpeed<HeroDamage>() += 0.1f;
            player.moveSpeed += 0.14f;
            player.maxRunSpeed += 1f;
        }

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.statDefense += 12;
        player.endurance += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.14f;
        player.GetKnockback<HeroDamage>() += 0.6f;
        player.moveSpeed += 0.12f;
        player.maxRunSpeed += 0.8f;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.1f, 0.35f, 0.55f));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.ultimateAttack) {
            Projectile.NewProjectile(source, player.Center + direction * 30f, direction,
                ModContent.ProjectileType<WaterHazardUltimateProjectile>(), damage, knockback + 2f, player.whoAmI);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            Vector2 trapCenter = player.Center + direction * 108f;
            Projectile.NewProjectile(source, trapCenter, Vector2.Zero, ModContent.ProjectileType<WaterHazardSnareProjectile>(),
                damage, knockback, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            int burstDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, player.Center + direction * 16f, Vector2.Zero,
                ModContent.ProjectileType<WaterHazardBurstProjectile>(), burstDamage, knockback + 1.5f, player.whoAmI);
            return false;
        }

        int shotCount = omp.PrimaryAbilityEnabled ? 2 : 1;
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        for (int i = 0; i < shotCount; i++) {
            float lateralOffset = shotCount == 1 ? 0f : (i == 0 ? -8f : 8f);
            Vector2 spawnOffset = direction * 18f + perpendicular * lateralOffset;
            Vector2 shotVelocity = direction.RotatedBy(lateralOffset * 0.0025f) * PrimaryShootSpeed;
            Projectile.NewProjectile(source, player.Center + spawnOffset, shotVelocity,
                ModContent.ProjectileType<WaterHazardPressureProjectile>(), damage, knockback, player.whoAmI);
        }

        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.GoldHelmet;
        player.body = ArmorIDs.Body.GoldChainmail;
        player.legs = ArmorIDs.Legs.GoldGreaves;
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
