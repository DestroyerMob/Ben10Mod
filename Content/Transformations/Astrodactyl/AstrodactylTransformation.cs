using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Astrodactyl;

public class AstrodactylTransformation : Transformation {
    private const int HyperflightDuration = 10 * 60;
    private const int HyperflightCooldown = 28 * 60;
    private const int HyperflightCost = 18;
    private const int JetDiveEnergyCost = 24;
    private const int JetDiveCooldown = 14 * 60;
    private const float PrimaryDamageMultiplier = 0.84f;
    private const float SecondaryDamageMultiplier = 1.06f;
    private const float DiveDamageMultiplier = 1.28f;
    private const float UltimateDamageMultiplier = 1.2f;
    private const float BaseDiveRange = 400f;
    private const float HyperflightDiveRange = 580f;

    public override string FullID => "Ben10Mod:Astrodactyl";
    public override string TransformationName => "Astrodactyl";
    public override int TransformationBuffId => ModContent.BuffType<Astrodactyl_Buff>();
    public override string Description =>
        "A sky-dominating hunter who gets deadlier the longer he stays airborne, marking prey from above before diving through them and calling down comets.";

    public override List<string> Abilities => new() {
        "Rapid plasma bolts from the air",
        "Starburst orb that breaks open on impact",
        "Natural flight",
        "Hyperflight that rewards staying airborne",
        "Jet Dive that spears marked targets",
        "Cosmic Barrage that rains comets onto exposed prey"
    };

    public override string PrimaryAttackName => "Plasma Bolt";
    public override string SecondaryAttackName => "Starburst";
    public override string PrimaryAbilityName => "Hyperflight";
    public override string SecondaryAbilityAttackName => "Jet Dive";
    public override string UltimateAttackName => "Cosmic Barrage";

    public override int PrimaryAttack => ModContent.ProjectileType<AstrodactylPlasmaBoltProjectile>();
    public override int PrimaryAttackSpeed => 12;
    public override int PrimaryShootSpeed => 20;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<AstrodactylStarburstProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 13;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => HyperflightDuration;
    public override int PrimaryAbilityCooldown => HyperflightCooldown;
    public override int PrimaryAbilityCost => HyperflightCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<AstrodactylDiveProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => DiveDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => JetDiveEnergyCost;
    public override int SecondaryAbilityCooldown => JetDiveCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<AstrodactylCometProjectile>();
    public override int UltimateAttackSpeed => 30;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => UltimateDamageMultiplier;
    public override int UltimateEnergyCost => 58;
    public override int UltimateAbilityCooldown => 56 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<JetrayWings>());
        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        float airSupremacyRatio = identity.AstrodactylAirSupremacyRatio;

        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.moveSpeed += 0.18f;
        player.maxRunSpeed += 1.2f + airSupremacyRatio * 1.6f;
        player.accRunSpeed += 1.2f;
        player.jumpSpeedBoost += 1.8f;
        player.noFallDmg = true;
        player.ignoreWater = true;
        player.wingTimeMax += (int)Math.Round(22f + 28f * airSupremacyRatio);

        if (airSupremacyRatio >= 0.5f)
            player.armorEffectDrawShadow = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        identity.AddAstrodactylAirSupremacy(0.5f);
        player.moveSpeed += 0.12f;
        player.maxRunSpeed += 1.1f;
        player.accRunSpeed += 1.2f;
        player.jumpSpeedBoost += 0.8f;
        player.endurance += 0.02f;
        player.armorEffectDrawShadow = true;
        player.wingTimeMax += 80;
        player.wingTime = Math.Max(player.wingTime, 24f);
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);
        AlienIdentityPlayer identity = omp.Player.GetModPlayer<AlienIdentityPlayer>();
        float airSupremacyRatio = identity.AstrodactylAirSupremacyRatio;
        float useMultiplier = MathHelper.Lerp(1f, omp.PrimaryAbilityEnabled ? 0.82f : 0.9f, airSupremacyRatio);
        item.useTime = item.useAnimation = Math.Max(8, (int)Math.Round(item.useTime * useMultiplier));
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        ApplyHyperflight(player);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + direction * 14f;
        float airSupremacyRatio = identity.AstrodactylAirSupremacyRatio;
        bool hyperflight = omp.PrimaryAbilityEnabled;

        if (omp.ultimateAttack) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 targetPosition = Main.MouseWorld;
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            int cometCount = 5 + (hyperflight ? 1 : 0) + (airSupremacyRatio >= 0.4f ? 1 : 0) + (airSupremacyRatio >= 0.8f ? 1 : 0);
            float spacing = 36f;

            for (int i = 0; i < cometCount; i++) {
                float offset = (i - (cometCount - 1) * 0.5f) * spacing;
                Vector2 impactPoint = targetPosition + new Vector2(offset, i % 2 == 0 ? -18f : 18f);
                Vector2 skySpawn = impactPoint + new Vector2(Main.rand.NextFloat(-22f, 22f), -420f - i * 26f);
                Vector2 cometVelocity = (impactPoint - skySpawn).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(17f, 21f);
                Projectile.NewProjectile(source, skySpawn, cometVelocity, UltimateAttack, finalDamage,
                    knockback + 1.2f, player.whoAmI, hyperflight ? 1f : 0f, airSupremacyRatio);
            }

            identity.ConsumeAstrodactylAirSupremacy(34f);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 offset = Main.MouseWorld - player.MountedCenter;
            if (offset == Vector2.Zero)
                offset = new Vector2(player.direction, 0f);

            float maxRange = MathHelper.Lerp(BaseDiveRange, HyperflightDiveRange, airSupremacyRatio) + (hyperflight ? 18f : 0f);
            float requestedDistance = Math.Min(offset.Length(), maxRange);
            Vector2 diveDirection = offset.SafeNormalize(new Vector2(player.direction, 0f));
            float diveSpeed = AstrodactylDiveProjectile.GetDiveSpeed(hyperflight) + airSupremacyRatio * 3f;
            int diveFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / diveSpeed),
                AstrodactylDiveProjectile.MinDiveFrames, AstrodactylDiveProjectile.MaxDiveFrames);
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));

            int projectileIndex = Projectile.NewProjectile(source, player.Center + diveDirection * 20f,
                diveDirection * diveSpeed, SecondaryAbilityAttack, finalDamage, knockback + 1.5f, player.whoAmI,
                hyperflight ? 1f : 0f, airSupremacyRatio);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.timeLeft = diveFrames;
                projectile.netUpdate = true;
            }

            identity.ConsumeAstrodactylAirSupremacy(18f);
            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed, SecondaryAttack, finalDamage,
                knockback + 0.8f, player.whoAmI, hyperflight ? 1f : 0f, airSupremacyRatio);
            return false;
        }

        int boltCount = airSupremacyRatio >= 0.8f ? 3 : airSupremacyRatio >= 0.35f ? 2 : 1;
        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        for (int i = 0; i < boltCount; i++) {
            float spread = boltCount switch {
                3 => (i - 1) * 0.09f,
                2 => i == 0 ? -0.05f : 0.05f,
                _ => 0f
            };
            Vector2 shotVelocity = direction.RotatedBy(spread) * PrimaryShootSpeed;
            Projectile.NewProjectile(source, spawnPosition, shotVelocity, PrimaryAttack, primaryDamage, knockback,
                player.whoAmI, hyperflight ? 1f : 0f, airSupremacyRatio);
        }

        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (!IsAstrodactylProjectile(projectile.type))
            return;

        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!state.IsSkyMarkedFor(player.whoAmI))
            return;

        modifiers.FinalDamage *= projectile.type switch {
            _ when projectile.type == SecondaryAbilityAttack => 1.32f,
            _ when projectile.type == UltimateAttack => 1.18f,
            _ => 1.1f
        };
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsAstrodactylProjectile(projectile.type))
            return;

        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        bool aerialHit = !AlienIdentityPlayer.IsGrounded(player) || omp.PrimaryAbilityEnabled || player.velocity.Y < -0.1f;
        if (aerialHit)
            state.ApplySkyMark(player.whoAmI, 5 * 60);

        if (projectile.type == PrimaryAttack || projectile.type == SecondaryAttack)
            identity.AddAstrodactylAirSupremacy(4f);

        if (projectile.type == SecondaryAbilityAttack && state.IsSkyMarkedFor(player.whoAmI)) {
            player.velocity = new Vector2(player.velocity.X * 0.75f, Math.Min(player.velocity.Y, -5.8f));
            player.wingTime = Math.Min(player.wingTimeMax, player.wingTime + 30f);
        }
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.NecroHelmet;
        player.body = ArmorIDs.Body.NecroBreastplate;
        player.legs = ArmorIDs.Legs.NecroGreaves;
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

    private static void ApplyHyperflight(Player player) {
        if (player.controlJump || player.controlUp) {
            float ascentAcceleration = player.controlUp ? 0.54f : 0.38f;
            float maxRiseSpeed = player.controlUp ? -7.4f : -5.8f;
            player.velocity.Y = Math.Max(maxRiseSpeed, player.velocity.Y - ascentAcceleration);
        }
        else if (player.velocity.Y > -0.8f) {
            player.velocity.Y = Math.Min(player.velocity.Y, 2.2f);
        }

        if (player.controlDown)
            player.velocity.Y = Math.Min(player.velocity.Y + 0.3f, 8f);
        else if (player.velocity.Y > 0f)
            player.velocity.Y *= 0.86f;

        player.fallStart = (int)(player.position.Y / 16f);
        player.maxFallSpeed = 8f;
    }

    private bool IsAstrodactylProjectile(int projectileType) {
        return projectileType == PrimaryAttack ||
               projectileType == SecondaryAttack ||
               projectileType == SecondaryAbilityAttack ||
               projectileType == UltimateAttack;
    }
}
