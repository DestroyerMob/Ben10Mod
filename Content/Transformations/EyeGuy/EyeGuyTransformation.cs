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

namespace Ben10Mod.Content.Transformations.EyeGuy;

public class EyeGuyTransformation : Transformation {
    private const float CrossfireVolleyDamageMultiplier = 0.82f;
    private const float OmniBurstDamageMultiplier = 0.74f;
    private const int PanopticFocusDuration = 9 * 60;
    private const int PanopticFocusCooldown = 28 * 60;
    private const int PanopticFocusCost = 20;
    private const int OmniBurstEnergyCost = 26;
    private const int OmniBurstCooldown = 18 * 60;

    public override string FullID => "Ben10Mod:EyeGuy";
    public override string TransformationName => "Eye Guy";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<EyeGuy_Buff>();
    public override string Description =>
        "A precision Opticoid that floods the field with crossfire, locks onto weak points, and then burns a hole through priority targets with a gigantic optic beam.";

    public override List<string> Abilities => new() {
        "Rapid piercing eye lasers",
        "Crossfire Volley for multi-angle pressure",
        "Panoptic Focus to sharpen sight and split attacks across more eyes",
        "Omni Burst for close-range crowd clearing in every direction",
        "Optic Beam for sustained eye-blast pressure"
    };

    public override string PrimaryAttackName => "Eye Laser";
    public override string SecondaryAttackName => "Crossfire Volley";
    public override string PrimaryAbilityName => "Panoptic Focus";
    public override string SecondaryAbilityAttackName => "Omni Burst";
    public override string UltimateAttackName => "Optic Beam";

    public override int PrimaryAttack => ModContent.ProjectileType<EyeGuyLaserbeam>();
    public override int PrimaryAttackSpeed => 15;
    public override int PrimaryShootSpeed => 30;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;

    public override int SecondaryAttack => ModContent.ProjectileType<EyeGuyLaserbeam>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 24;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => CrossfireVolleyDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => PanopticFocusDuration;
    public override int PrimaryAbilityCooldown => PanopticFocusCooldown;
    public override int PrimaryAbilityCost => PanopticFocusCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<EyeGuyLaserbeam>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => OmniBurstDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => OmniBurstEnergyCost;
    public override int SecondaryAbilityCooldown => OmniBurstCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<EyeGuyUltimateBeam>();
    public override int UltimateAttackSpeed => 10;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override bool UltimateChannel => true;
    public override bool UltimateNoMelee => true;
    public override int UltimateEnergyCost => 10;
    public override int UltimateAttackSustainEnergyCost => UltimateEnergyCost;
    public override int UltimateAttackSustainInterval => UltimateAttackSpeed;
    public override int UltimateAbilityCooldown => 30 * 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player, ModContent.ProjectileType<EyeGuyUltimateBeam>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 12f;
        player.GetArmorPenetration<HeroDamage>() += 6;
        player.nightVision = true;
        player.detectCreature = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.GetArmorPenetration<HeroDamage>() += 4;
        player.moveSpeed += 0.08f;
        player.runAcceleration += 0.05f;
        player.maxRunSpeed += 0.4f;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.08f, 0.55f, 0.18f));
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.PrimaryAbilityEnabled || Main.rand.NextBool(2))
            return;

        Dust dust = Dust.NewDustPerfect(
            player.Center + Main.rand.NextVector2Circular(player.width * 0.4f, player.height * 0.5f),
            Main.rand.NextBool() ? DustID.GreenTorch : DustID.GreenFairy,
            Main.rand.NextVector2Circular(0.55f, 0.55f),
            95,
            new Color(135, 255, 170),
            Main.rand.NextFloat(0.92f, 1.2f));
        dust.noGravity = true;
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        if (!base.CanStartCurrentAttack(player, omp))
            return false;

        TransformationAttackProfile profile = GetSelectedAttackProfile(omp);
        if (profile?.ProjectileType == UltimateAttack)
            return !HasActiveOwnedProjectile(player, profile.ProjectileType);

        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        bool focused = omp.PrimaryAbilityEnabled;

        if (omp.ultimateAttack) {
            TransformationAttackProfile profile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.Ultimate, omp);
            if (profile == null || profile.ProjectileType <= 0 || HasActiveOwnedProjectile(player, profile.ProjectileType))
                return false;

            int finalDamage = ScaleDamage(damage, profile, focused ? 1.1f : 1f);
            Projectile.NewProjectile(source, player.Center, direction, profile.ProjectileType, finalDamage, knockback + 1f,
                player.whoAmI, focused ? 1f : 0f);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            TransformationAttackProfile profile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.SecondaryAbility, omp);
            if (profile == null || profile.ProjectileType <= 0)
                return false;

            FireOmniBurst(player, source, profile, direction, damage, knockback, focused);
            return false;
        }

        if (omp.altAttack) {
            TransformationAttackProfile profile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.Secondary, omp);
            if (profile == null || profile.ProjectileType <= 0)
                return false;

            FireCrossfireVolley(player, source, profile, direction, damage, knockback, focused);
            return false;
        }

        TransformationAttackProfile primaryProfile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.Primary, omp);
        if (primaryProfile == null || primaryProfile.ProjectileType <= 0)
            return false;

        FirePrimaryVolley(player, source, primaryProfile, direction, damage, knockback, focused);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<EyeGuy>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    private static void FirePrimaryVolley(Player player, EntitySource_ItemUse_WithAmmo source,
        TransformationAttackProfile profile, Vector2 direction, int damage, float knockback, bool focused) {
        float[] spreads = focused ? new[] { -0.055f, 0f, 0.055f } : new[] { 0f };
        float[] damageWeights = focused ? new[] { 0.38f, 0.7f, 0.38f } : new[] { 1f };
        float[] lateralOffsets = focused ? new[] { -10f, 0f, 10f } : new[] { 0f };

        FireLaserPattern(player, source, profile, direction, damage, knockback, spreads, damageWeights, lateralOffsets,
            EyeGuyLaserbeam.VariantPrimary, focused);
    }

    private static void FireCrossfireVolley(Player player, EntitySource_ItemUse_WithAmmo source,
        TransformationAttackProfile profile, Vector2 direction, int damage, float knockback, bool focused) {
        float[] spreads = focused
            ? new[] { -0.2f, -0.1f, 0f, 0.1f, 0.2f }
            : new[] { -0.12f, 0f, 0.12f };
        float[] damageWeights = focused
            ? new[] { 0.42f, 0.56f, 0.74f, 0.56f, 0.42f }
            : new[] { 0.62f, 0.86f, 0.62f };
        float[] lateralOffsets = focused
            ? new[] { -18f, -9f, 0f, 9f, 18f }
            : new[] { -12f, 0f, 12f };

        FireLaserPattern(player, source, profile, direction, damage, knockback + 0.4f, spreads, damageWeights,
            lateralOffsets, EyeGuyLaserbeam.VariantCrossfire, focused);
    }

    private static void FireOmniBurst(Player player, EntitySource_ItemUse_WithAmmo source,
        TransformationAttackProfile profile, Vector2 direction, int damage, float knockback, bool focused) {
        int rayCount = focused ? 12 : 10;
        float shootSpeed = profile.ShootSpeed > 0f ? profile.ShootSpeed : 15f;
        float angleOffset = direction.ToRotation();
        float damageWeight = focused ? 0.56f : 0.64f;

        for (int i = 0; i < rayCount; i++) {
            float angle = angleOffset + MathHelper.TwoPi * i / rayCount;
            Vector2 shotDirection = angle.ToRotationVector2();
            Vector2 spawnPosition = player.Center + shotDirection * 14f;
            int finalDamage = ScaleDamage(damage, profile, damageWeight);

            Projectile.NewProjectile(source, spawnPosition, shotDirection * shootSpeed, profile.ProjectileType, finalDamage,
                knockback + 0.9f, player.whoAmI, EyeGuyLaserbeam.VariantOmniBurst, focused ? 1f : 0f);
        }
    }

    private static void FireLaserPattern(Player player, EntitySource_ItemUse_WithAmmo source,
        TransformationAttackProfile profile, Vector2 direction, int damage, float knockback, float[] spreads,
        float[] damageWeights, float[] lateralOffsets, int variant, bool focused) {
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        float shootSpeed = profile.ShootSpeed > 0f ? profile.ShootSpeed : 20f;
        Vector2 basePosition = player.MountedCenter + direction * 16f;

        for (int i = 0; i < spreads.Length; i++) {
            Vector2 shotDirection = direction.RotatedBy(spreads[i]);
            Vector2 spawnPosition = basePosition + perpendicular * lateralOffsets[i];
            int finalDamage = ScaleDamage(damage, profile, damageWeights[i]);

            Projectile.NewProjectile(source, spawnPosition, shotDirection * shootSpeed, profile.ProjectileType, finalDamage,
                knockback, player.whoAmI, variant, focused ? 1f : 0f);
        }
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
    }

    private static int ScaleDamage(int baseDamage, TransformationAttackProfile profile, float extraMultiplier = 1f) {
        return Math.Max(1, (int)Math.Round(baseDamage * (profile?.DamageMultiplier ?? 1f) * extraMultiplier));
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
