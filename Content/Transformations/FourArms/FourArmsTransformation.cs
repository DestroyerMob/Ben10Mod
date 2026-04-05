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

namespace Ben10Mod.Content.Transformations.FourArms;

public class FourArmsTransformation : Transformation {
    private const int BattleRoarDuration = 8 * 60;
    private const int BattleRoarCooldown = 26 * 60;
    private const int BattleRoarCost = 14;
    private const int ShoulderRushEnergyCost = 18;
    private const int ShoulderRushCooldown = 12 * 60;
    private const int QuadSmashEnergyRequirement = 52;
    private const int QuadSmashEnergyCost = 52;
    private const int QuadSmashCooldown = 42 * 60;
    private const float PrimaryDamageMultiplier = 1.14f;
    private const float SecondaryDamageMultiplier = 1.42f;
    private const float ShoulderRushDamageMultiplier = 1.24f;
    private const float QuadSmashDamageMultiplier = 1.38f;

    public override string FullID => "Ben10Mod:FourArms";
    public override string TransformationName => "Fourarms";
    public override string IconPath => "Ben10Mod/Content/Interface/FourArmsSelect";
    public override int TransformationBuffId => ModContent.BuffType<FourArms_Buff>();

    public override string Description =>
        "A powerhouse Tetramand who overwhelms enemies with crushing punches, shockwave claps, aggressive rushdowns, and seismic finishers.";

    public override List<string> Abilities => new() {
        "Power Punch for heavy close-range hits",
        "Shockwave Clap that blasts enemies away",
        "Battle Roar for a burst of attack speed, defense, and mobility",
        "Shoulder Rush that barrels through targets and slams the ground on exit",
        "Quad Smash that unloads all four arms in one seismic burst",
        "Passive high jumps, no fall damage, and fast-fall ground slams"
    };

    public override string PrimaryAttackName => "Power Punch";
    public override string SecondaryAttackName => "Shockwave Clap";
    public override string PrimaryAbilityName => "Battle Roar";
    public override string SecondaryAbilityName => "Shoulder Rush";
    public override string SecondaryAbilityAttackName => "Shoulder Rush";
    public override string UltimateAbilityName => "Quad Smash";
    public override string UltimateAttackName => "Quad Smash";
    public override int PrimaryAttack => ModContent.ProjectileType<FourArmsPunchProjectile>();
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;
    public override int PrimaryAttackSpeed => 15;
    public override int PrimaryShootSpeed => 15;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int PrimaryArmorPenetration => 10;

    public override int SecondaryAttack => ModContent.ProjectileType<FourArmsClap>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 18;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;
    public override int SecondaryArmorPenetration => 14;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => BattleRoarDuration;
    public override int PrimaryAbilityCooldown => BattleRoarCooldown;
    public override int PrimaryAbilityCost => BattleRoarCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<FourArmsRushProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAbilityAttackModifier => ShoulderRushDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => ShoulderRushEnergyCost;
    public override int SecondaryAbilityCooldown => ShoulderRushCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<FourArmsPunchProjectile>();
    public override float UltimateAttackModifier => QuadSmashDamageMultiplier;
    public override int UltimateAttackSpeed => 26;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override int UltimateEnergyCost => QuadSmashEnergyCost;
    public override int UltimateAbilityCost => QuadSmashEnergyRequirement;
    public override int UltimateAbilityCooldown => QuadSmashCooldown;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.15f;
        player.GetAttackSpeed<HeroDamage>() += 0.14f;
        player.GetCritChance<HeroDamage>() += 12f;
        player.GetKnockback<HeroDamage>() += 0.8f;
        player.statDefense += 6;
        player.noFallDmg = true;
        player.jumpSpeedBoost += 2.8f;
        player.moveSpeed += 0.06f;
        player.runAcceleration += 0.04f;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.16f;
        player.GetAttackSpeed<HeroDamage>() += 0.2f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetKnockback<HeroDamage>() += 0.45f;
        player.statDefense += 8;
        player.endurance += 0.04f;
        player.moveSpeed += 0.12f;
        player.runAcceleration += 0.12f;
        player.armorEffectDrawShadow = true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        float rageBonus = omp.IsPrimaryAbilityActive ? 1.12f : 1f;

        if (omp.ultimateAttack) {
            FireQuadSmash(player, source, damage, knockback, direction, rageBonus);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            FireShoulderRush(player, source, damage, knockback, direction, rageBonus);
            return false;
        }

        if (omp.altAttack) {
            int clapDamage = ScaleDamage(damage, SecondaryAttackModifier * rageBonus);
            Projectile.NewProjectile(source, player.MountedCenter + direction * 16f, direction * SecondaryShootSpeed,
                SecondaryAttack, clapDamage, knockback + 2f, player.whoAmI);
            return false;
        }

        float punchScale = omp.IsPrimaryAbilityActive ? 1.32f : 1.16f;
        int punchDamage = ScaleDamage(damage, PrimaryAttackModifier * rageBonus);
        Projectile.NewProjectile(source, player.MountedCenter + direction * 18f, direction * Math.Max(PrimaryShootSpeed, 10),
            PrimaryAttack, punchDamage, knockback + 1f, player.whoAmI, punchScale);
        return false;
    }
    
    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => [
        new TransformationPaletteChannel(
            "skin",
            "Skin",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/FourArms/FourArms_Head",
                "Ben10Mod/Content/Transformations/FourArms/FourArmsSkinMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/FourArms/FourArms_Body",
                "Ben10Mod/Content/Transformations/FourArms/FourArmsSkinMask_Body"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/FourArms/FourArms_Legs",
                "Ben10Mod/Content/Transformations/FourArms/FourArmsSkinMask_Legs")),
        new TransformationPaletteChannel(
            "eye",
            "Eye",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/FourArms/FourArms_Head",
                "Ben10Mod/Content/Transformations/FourArms/FourArmsEyeMask_Head"))
    ];

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<FourArms>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    private static void FireShoulderRush(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback,
        Vector2 direction, float rageBonus) {
        Vector2 offset = ResolveRushTarget(player, direction) - player.MountedCenter;
        if (offset == Vector2.Zero)
            offset = direction;

        bool empowered = rageBonus > 1f;
        float rushSpeed = FourArmsRushProjectile.GetRushSpeed(empowered);
        float maxRange = empowered ? 340f : 285f;
        float requestedDistance = Math.Min(offset.Length(), maxRange);
        Vector2 rushDirection = offset.SafeNormalize(direction);
        int rushFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / rushSpeed),
            FourArmsRushProjectile.MinRushFrames, FourArmsRushProjectile.MaxRushFrames);
        int finalDamage = ScaleDamage(damage, ShoulderRushDamageMultiplier * rageBonus);

        int projectileIndex = Projectile.NewProjectile(source, player.Center + rushDirection * 18f,
            rushDirection * rushSpeed, ModContent.ProjectileType<FourArmsRushProjectile>(), finalDamage,
            knockback + 2.5f, player.whoAmI, empowered ? 1f : 0f);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Projectile projectile = Main.projectile[projectileIndex];
            projectile.timeLeft = rushFrames;
            projectile.netUpdate = true;
        }
    }

    private static void FireQuadSmash(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback,
        Vector2 direction, float rageBonus) {
        float[] angleOffsets = { -0.22f, -0.08f, 0.08f, 0.22f };
        float[] verticalOffsets = { -12f, -4f, 4f, 12f };
        int punchDamage = ScaleDamage(damage, QuadSmashDamageMultiplier * 0.38f * rageBonus);

        for (int i = 0; i < angleOffsets.Length; i++) {
            Vector2 fistDirection = direction.RotatedBy(angleOffsets[i]).SafeNormalize(direction);
            Vector2 spawnPosition = player.MountedCenter + fistDirection * 18f + new Vector2(0f, verticalOffsets[i]);
            float fistScale = 1.22f + (Math.Abs(verticalOffsets[i]) > 8f ? 0.08f : 0f);
            Projectile.NewProjectile(source, spawnPosition, fistDirection * 12f,
                ModContent.ProjectileType<FourArmsPunchProjectile>(), punchDamage, knockback + 1.3f,
                player.whoAmI, fistScale);
        }

        int clapDamage = ScaleDamage(damage, QuadSmashDamageMultiplier * 0.72f * rageBonus);
        Projectile.NewProjectile(source, player.MountedCenter + direction * 16f, direction * 16f,
            ModContent.ProjectileType<FourArmsClap>(), clapDamage, knockback + 3f, player.whoAmI);

        int shockDamage = ScaleDamage(damage, QuadSmashDamageMultiplier * 0.48f * rageBonus);
        Projectile.NewProjectile(source, player.Bottom + new Vector2(0f, -10f), Vector2.Zero,
            ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>(), shockDamage, knockback + 2f,
            player.whoAmI, 1.35f);
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

    private static Vector2 ResolveRushTarget(Player player, Vector2 fallbackDirection) {
        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer)
            return Main.MouseWorld;

        return player.MountedCenter + fallbackDirection * 260f;
    }
}
