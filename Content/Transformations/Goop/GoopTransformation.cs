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

namespace Ben10Mod.Content.Transformations.Goop;

public class GoopTransformation : Transformation {
    private const float LandingSquishVelocityThreshold = 3.5f;
    private const float LiquefyTrailMultiplier = 1.8f;

    public override string FullID => "Ben10Mod:Goop";
    public override string TransformationName => "Goop";
    public override int TransformationBuffId => ModContent.BuffType<Goop_Buff>();
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override string Description => "A fluid Polymorph that fights with corrosive globs and lingering slime puddles.";
    public override List<string> Abilities => new() { "Corrosive glob primary", "Lobbed puddle secondary", "Liquefy" };
    public override string PrimaryAttackName => "Corrosive Glob";
    public override string SecondaryAttackName => "Slime Puddle";
    public override string UltimateAttackName => "Toxic Deluge";
    public override int PrimaryAttack => ModContent.ProjectileType<GoopGlobProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 14;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.92f;
    public override int SecondaryAttack => ModContent.ProjectileType<GoopPuddleBombProjectile>();
    public override int SecondaryAttackSpeed => 30;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 0.75f;
    public override int UltimateAttack => ModContent.ProjectileType<GoopDelugeProjectile>();
    public override int UltimateAttackSpeed => 34;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 50 * 60;
    public override float UltimateAttackModifier => 1.9f;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 8 * 60;
    public override int PrimaryAbilityCooldown => 24 * 60;

    public override void OnTransform(Player player, OmnitrixPlayer omp) {
        ResetGoopVisualState(omp);
    }

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        ResetGoopVisualState(omp);
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.GetDamage<HeroDamage>() += 0.04f;
        player.statDefense += 4;
        player.moveSpeed += 0.08f;
        player.runAcceleration += 0.04f;
        player.maxRunSpeed += 0.4f;
        player.noFallDmg = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.moveSpeed += 0.18f;
        player.runAcceleration += 0.12f;
        player.maxRunSpeed += 1.2f;
        player.gravity *= 0.72f;
        player.maxFallSpeed *= 0.72f;
        player.jumpSpeedBoost += 1.6f;
        player.noKnockback = true;
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        bool grounded = Math.Abs(player.velocity.Y) <= 0.05f;
        float previousVerticalVelocity = omp.goopPreviousVerticalVelocity;
        if (!omp.goopWasGrounded && grounded && previousVerticalVelocity > LandingSquishVelocityThreshold) {
            omp.goopLandingSquish = MathHelper.Clamp(previousVerticalVelocity / 14f, 0.25f, 0.9f);
            omp.goopLandingSplashTime = 8;
        }

        float horizontalFactor = MathHelper.Clamp(Math.Abs(player.velocity.X) / 7.5f, 0f, 1f);
        float risingFactor = MathHelper.Clamp(-player.velocity.Y / 8f, 0f, 1f);
        float fallingFactor = MathHelper.Clamp(player.velocity.Y / 14f, 0f, 1f);

        Vector2 targetScale = Vector2.One;
        if (omp.PrimaryAbilityEnabled) {
            targetScale.X += 0.32f;
            targetScale.Y -= 0.24f;
        }

        if (grounded) {
            targetScale.X += 0.12f * horizontalFactor;
            targetScale.Y -= 0.10f * horizontalFactor;
            if (omp.PrimaryAbilityEnabled) {
                targetScale.X += 0.18f + 0.18f * horizontalFactor;
                targetScale.Y -= 0.16f + 0.08f * horizontalFactor;
            }
        }
        else {
            targetScale.X -= 0.05f * risingFactor;
            targetScale.X -= 0.08f * fallingFactor;
            targetScale.Y += 0.08f * risingFactor;
            targetScale.Y += 0.16f * fallingFactor;
        }

        targetScale.X += 0.24f * omp.goopLandingSquish;
        targetScale.Y -= 0.20f * omp.goopLandingSquish;
        targetScale.X = MathHelper.Clamp(targetScale.X, 0.7f, 1.8f);
        targetScale.Y = MathHelper.Clamp(targetScale.Y, 0.45f, 1.45f);

        float response = grounded ? 0.28f : 0.18f;
        omp.GoopVisualScale = Vector2.Lerp(omp.GoopVisualScale, targetScale, response);
        if (Vector2.DistanceSquared(omp.GoopVisualScale, targetScale) < 0.0001f)
            omp.GoopVisualScale = targetScale;

        omp.goopLandingSquish = MathHelper.Lerp(omp.goopLandingSquish, 0f, grounded ? 0.2f : 0.1f);
        if (omp.goopLandingSquish < 0.01f)
            omp.goopLandingSquish = 0f;

        if (omp.goopLandingSplashTime > 0)
            omp.goopLandingSplashTime--;

        omp.goopWasGrounded = grounded;
        omp.goopPreviousVerticalVelocity = player.velocity.Y;
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();

        int trailChance = omp.PrimaryAbilityEnabled ? 2 : 3;
        if ((Math.Abs(player.velocity.X) > 1.2f || omp.PrimaryAbilityEnabled) && Main.rand.NextBool(trailChance)) {
            Dust trailDust = Dust.NewDustDirect(
                player.BottomLeft + new Vector2(Main.rand.NextFloat(0f, player.width), -4f),
                1,
                1,
                DustID.GreenTorch,
                -player.velocity.X * 0.18f * (omp.PrimaryAbilityEnabled ? LiquefyTrailMultiplier : 1f),
                Main.rand.NextFloat(-1.2f, -0.2f) * (omp.PrimaryAbilityEnabled ? 1.25f : 1f),
                100,
                new Color(110, 220, 130),
                Main.rand.NextFloat(0.9f, 1.2f) * (omp.PrimaryAbilityEnabled ? 1.2f : 1f)
            );
            trailDust.noGravity = false;
            trailDust.velocity *= 0.6f;
        }

        if (omp.PrimaryAbilityEnabled && Main.rand.NextBool(2)) {
            Dust bodyDrip = Dust.NewDustDirect(
                player.position + Main.rand.NextVector2Circular(player.width * 0.35f, player.height * 0.35f),
                1,
                1,
                Main.rand.NextBool() ? DustID.GreenTorch : DustID.GreenMoss,
                Main.rand.NextFloat(-0.6f, 0.6f),
                Main.rand.NextFloat(0.2f, 1.2f),
                90,
                new Color(120, 245, 145),
                Main.rand.NextFloat(0.95f, 1.25f)
            );
            bodyDrip.noGravity = false;
            bodyDrip.velocity *= 0.45f;
        }

        if (omp.goopLandingSplashTime <= 0)
            return;

        int splashCount = omp.goopLandingSplashTime > 5 ? 2 : 1;
        for (int i = 0; i < splashCount; i++) {
            float progress = omp.goopLandingSplashTime / 8f;
            Dust splashDust = Dust.NewDustDirect(
                player.BottomLeft + new Vector2(Main.rand.NextFloat(0f, player.width), -6f),
                1,
                1,
                DustID.GreenTorch,
                Main.rand.NextFloat(-2.6f, 2.6f) * progress,
                Main.rand.NextFloat(-2.4f, -0.8f) * progress,
                80,
                new Color(120, 235, 145),
                Main.rand.NextFloat(1.0f, 1.35f)
            );
            splashDust.noGravity = false;
        }
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 fireDirection = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.ultimateAttack) {
            int projectileIndex = Projectile.NewProjectile(source, player.Center + fireDirection * 18f, fireDirection * 10.5f,
                ModContent.ProjectileType<GoopDelugeProjectile>(), damage, knockback + 1f, player.whoAmI);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Main.projectile[projectileIndex].scale = omp.PrimaryAbilityEnabled ? 1.18f : 1f;
                Main.projectile[projectileIndex].netUpdate = true;
            }

            if (!Main.dedServ) {
                for (int i = 0; i < 18; i++) {
                    Dust surge = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(14f, 18f),
                        i % 3 == 0 ? DustID.GreenTorch : DustID.GreenMoss,
                        fireDirection * Main.rand.NextFloat(1.4f, 4.2f) + Main.rand.NextVector2Circular(1.2f, 1.2f),
                        85, new Color(125, 245, 145), Main.rand.NextFloat(1f, 1.4f));
                    surge.noGravity = false;
                }
            }

            return false;
        }

        if (omp.altAttack) {
            Vector2 lobVelocity = fireDirection * 7.5f + new Vector2(0f, -2.5f);
            Projectile.NewProjectile(source, player.Center + fireDirection * 14f, lobVelocity,
                ModContent.ProjectileType<GoopPuddleBombProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        Vector2 shotVelocity = fireDirection * 14f;
        Projectile.NewProjectile(source, player.Center + fireDirection * 12f, shotVelocity,
            ModContent.ProjectileType<GoopGlobProjectile>(), damage, knockback, player.whoAmI);
        return false;
    }

    private static void ResetGoopVisualState(OmnitrixPlayer omp) {
        omp.GoopVisualScale = Vector2.One;
        omp.goopWasGrounded = false;
        omp.goopPreviousVerticalVelocity = 0f;
        omp.goopLandingSquish = 0f;
        omp.goopLandingSplashTime = 0;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.CopperHelmet;
        player.body = ArmorIDs.Body.CopperChainmail;
        player.legs = ArmorIDs.Legs.CopperGreaves;
    }
}
