using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Players;
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
    private const float PrimaryPressureGain = 4f;
    private const float VentPrimaryPressureGain = 6f;
    private const float SaturatedPrimaryPressureBonus = 2f;
    private const float RiptidePressureSpend = 18f;
    private const float TidalSnarePressureSpend = 34f;
    private const float MonsoonBreakPressureSpend = 72f;

    public override string FullID => "Ben10Mod:WaterHazard";
    public override string TransformationName => "Water Hazard";
    public override int TransformationBuffId => ModContent.BuffType<WaterHazard_Buff>();

    public override string Description =>
        "A pressure-resource artillery form that builds reservoir pressure, drenches enemies, and spends that stored force on bursts, snares, and Monsoon Break.";

    public override List<string> Abilities => new() {
        "Pressure Jet builds reservoir pressure",
        "Riptide Burst spends pressure to pop soaked targets",
        "Reservoir Vent builds pressure faster",
        "Tidal Snare spends heavier pressure for control",
        "Monsoon Break spends the biggest pressure charge for payoff"
    };

    public override string PrimaryAttackName => "Pressure Jet";
    public override string SecondaryAttackName => "Riptide Burst";
    public override string PrimaryAbilityName => "Reservoir Vent";
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
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 4f;
        player.statDefense += 8;
        player.endurance += 0.04f;
        player.moveSpeed += 0.08f;
        player.maxRunSpeed += 0.6f;
        player.ignoreWater = true;
        player.accFlipper = true;
        player.gills = true;
        player.noFallDmg = true;

        if (IsSaturated(player)) {
            player.GetDamage<HeroDamage>() += 0.08f;
            player.GetAttackSpeed<HeroDamage>() += 0.1f;
            player.moveSpeed += 0.14f;
            player.maxRunSpeed += 1f;
        }

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.1f, 0.35f, 0.55f));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        AlienIdentityPlayer identityPlayer = player.GetModPlayer<AlienIdentityPlayer>();
        float reservoirRatio = identityPlayer.WaterHazardPressureRatio;
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.ultimateAttack) {
            float pressureRatio = GetPressureSpendRatio(identityPlayer, MonsoonBreakPressureSpend);
            int finalDamage = System.Math.Max(1,
                (int)System.Math.Round(damage * UltimateAttackModifier * MathHelper.Lerp(0.55f, 1.36f, pressureRatio)));
            Projectile.NewProjectile(source, player.Center + direction * 30f, direction,
                ModContent.ProjectileType<WaterHazardUltimateProjectile>(), finalDamage, knockback + 2f, player.whoAmI,
                pressureRatio);
            identityPlayer.ConsumeWaterHazardPressure(MonsoonBreakPressureSpend);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            float pressureRatio = GetPressureSpendRatio(identityPlayer, TidalSnarePressureSpend);
            int snareDamage = System.Math.Max(1,
                (int)System.Math.Round(damage * MathHelper.Lerp(0.58f, 1.18f, pressureRatio)));
            Vector2 trapCenter = player.Center + direction * 108f;
            Projectile.NewProjectile(source, trapCenter, Vector2.Zero, ModContent.ProjectileType<WaterHazardSnareProjectile>(),
                snareDamage, knockback, player.whoAmI, pressureRatio);
            identityPlayer.ConsumeWaterHazardPressure(TidalSnarePressureSpend);
            return false;
        }

        if (omp.altAttack) {
            float pressureRatio = GetPressureSpendRatio(identityPlayer, RiptidePressureSpend);
            int burstDamage = System.Math.Max(1,
                (int)System.Math.Round(damage * SecondaryAttackModifier * MathHelper.Lerp(0.6f, 1.28f, pressureRatio)));
            Projectile.NewProjectile(source, player.Center + direction * 16f, Vector2.Zero,
                ModContent.ProjectileType<WaterHazardBurstProjectile>(), burstDamage, knockback + 1.5f, player.whoAmI,
                pressureRatio);
            identityPlayer.ConsumeWaterHazardPressure(RiptidePressureSpend);
            return false;
        }

        int shotCount = omp.PrimaryAbilityEnabled ? 3 : 1;
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        for (int i = 0; i < shotCount; i++) {
            float lateralOffset = shotCount switch {
                3 when i == 0 => -10f,
                3 when i == 2 => 10f,
                2 when i == 0 => -8f,
                2 => 8f,
                _ => 0f
            };
            Vector2 spawnOffset = direction * 18f + perpendicular * lateralOffset;
            Vector2 shotVelocity = direction.RotatedBy(lateralOffset * (omp.PrimaryAbilityEnabled ? 0.01f : 0.0025f)) *
                PrimaryShootSpeed;
            Projectile.NewProjectile(source, player.Center + spawnOffset, shotVelocity,
                ModContent.ProjectileType<WaterHazardPressureProjectile>(), damage, knockback, player.whoAmI,
                reservoirRatio, omp.PrimaryAbilityEnabled ? 1f : 0f);
        }

        identityPlayer.AddWaterHazardPressure(GetPrimaryPressureGain(player, omp.PrimaryAbilityEnabled));
        return false;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.PrimaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.SecondaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Ultimate)
            return base.GetAttackResourceSummary(selection, omp, compact);

        AlienIdentityPlayer identityPlayer = omp.Player.GetModPlayer<AlienIdentityPlayer>();
        string pressureText = compact
            ? $"Press {GetPressurePercent(identityPlayer)}%"
            : $"Reservoir pressure {GetPressurePercent(identityPlayer)}%";
        string identityText = resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"{pressureText} • Build"
                : $"{pressureText} • Pressure Jet builds pressure; wet/rain builds faster",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{pressureText} • Spend {RiptidePressureSpend:0}"
                : $"{pressureText} • spends {RiptidePressureSpend:0} pressure; weak if starved",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? $"{pressureText} • Vent build"
                : $"{pressureText} • Reservoir Vent triples jets and builds pressure faster",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"{pressureText} • Spend {TidalSnarePressureSpend:0}"
                : $"{pressureText} • spends {TidalSnarePressureSpend:0} pressure for whirlpool control",
            OmnitrixPlayer.AttackSelection.Ultimate => compact
                ? $"{pressureText} • Spend {MonsoonBreakPressureSpend:0}"
                : $"{pressureText} • spends {MonsoonBreakPressureSpend:0} pressure for Monsoon Break payoff",
            _ => pressureText
        };

        string baseText = base.GetAttackResourceSummary(selection, omp, compact);
        return string.IsNullOrWhiteSpace(baseText) ? identityText : $"{baseText} • {identityText}";
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

    public static bool IsSaturated(Player player) {
        return player.wet || (Main.raining && player.ZoneRain);
    }

    public static float GetPrimaryPressureGain(Player player, bool venting) {
        float gain = venting ? VentPrimaryPressureGain : PrimaryPressureGain;
        if (IsSaturated(player))
            gain += SaturatedPrimaryPressureBonus;

        return gain;
    }

    private static float GetPressureSpendRatio(AlienIdentityPlayer identityPlayer, float pressureSpend) {
        if (pressureSpend <= 0f)
            return 1f;

        return MathHelper.Clamp(identityPlayer.WaterHazardPressure / pressureSpend, 0f, 1f);
    }

    private static int GetPressurePercent(AlienIdentityPlayer identityPlayer) {
        return (int)MathHelper.Clamp(identityPlayer.WaterHazardPressureRatio * 100f, 0f, 100f);
    }
}
