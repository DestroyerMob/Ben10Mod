using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Fasttrack;

public class FasttrackTransformation : Transformation {
    private const int AdrenalineSurgeDuration = 10 * 60;
    private const int AdrenalineSurgeCooldown = 24 * 60;
    private const int AdrenalineSurgeCost = 18;
    private const int PursuitRushEnergyCost = 24;
    private const int PursuitRushCooldown = 13 * 60;
    private const float PrimaryDamageMultiplier = 0.84f;
    private const float SecondaryDamageMultiplier = 0.94f;
    private const float PursuitRushDamageMultiplier = 1.28f;
    private const float UltimateDamageMultiplier = 0.54f;
    private const float BaseRushRange = 340f;
    private const float MaxMomentumRushRange = 520f;

    public override string FullID => "Ben10Mod:Fasttrack";
    public override string TransformationName => "Fasttrack";
    public override int TransformationBuffId => ModContent.BuffType<Fasttrack_Buff>();
    public override string Description =>
        "A relentless speed bruiser who gets stronger by staying in motion, chaining hits into faster combos before cashing that momentum out in tackles and barrages.";

    public override List<string> Abilities => new() {
        "Rapid rush punches that build combo pressure",
        "Claw wave that cuts through a line of enemies",
        "Adrenaline Surge that supercharges movement",
        "Pursuit Rush that cashes combo stacks out in one hit",
        "Velocity Barrage that floods the screen with speed strikes"
    };

    public override string PrimaryAttackName => "Rush Punch";
    public override string SecondaryAttackName => "Claw Wave";
    public override string PrimaryAbilityName => "Adrenaline Surge";
    public override string SecondaryAbilityAttackName => "Pursuit Rush";
    public override string UltimateAttackName => "Velocity Barrage";

    public override int PrimaryAttack => ModContent.ProjectileType<FasttrackPunchProjectile>();
    public override int PrimaryAttackSpeed => 10;
    public override int PrimaryShootSpeed => 12;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<FasttrackClawWaveProjectile>();
    public override int SecondaryAttackSpeed => 18;
    public override int SecondaryShootSpeed => 18;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => AdrenalineSurgeDuration;
    public override int PrimaryAbilityCooldown => AdrenalineSurgeCooldown;
    public override int PrimaryAbilityCost => AdrenalineSurgeCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<FasttrackPursuitRushProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => PursuitRushDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => PursuitRushEnergyCost;
    public override int SecondaryAbilityCooldown => PursuitRushCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<FasttrackVelocityBarrageProjectile>();
    public override int UltimateAttackSpeed => 28;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => UltimateDamageMultiplier;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 56 * 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player, UltimateAttack);
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        float momentumRatio = identity.FasttrackMomentumRatio;

        player.GetDamage<HeroDamage>() += 0.11f;
        player.GetKnockback<HeroDamage>() += 0.4f;
        player.statDefense += 6;
        player.moveSpeed += 0.2f + momentumRatio * 0.16f;
        player.runAcceleration += 0.1f + momentumRatio * 0.12f;
        player.accRunSpeed += 1.35f + momentumRatio * 1.8f;
        player.maxRunSpeed += 1.6f + momentumRatio * 2.2f;
        player.jumpSpeedBoost += 1.7f;
        player.pickSpeed *= 0.88f;
        player.noFallDmg = true;

        if (momentumRatio >= 0.45f)
            player.waterWalk = true;
        if (momentumRatio >= 0.75f)
            player.armorEffectDrawShadow = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        identity.AddFasttrackMomentum(0.65f);
        player.moveSpeed += 0.12f;
        player.runAcceleration += 0.14f;
        player.accRunSpeed += 1.45f;
        player.maxRunSpeed += 1.8f;
        player.jumpSpeedBoost += 0.9f;
        player.endurance += 0.02f + momentumRatio * 0.02f;
        player.blackBelt = momentumRatio >= 0.3f;
        player.armorEffectDrawShadow = true;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);
        float momentumRatio = omp.Player.GetModPlayer<AlienIdentityPlayer>().FasttrackMomentumRatio;
        float useMultiplier = MathHelper.Lerp(1f, omp.PrimaryAbilityEnabled ? 0.72f : 0.82f, momentumRatio);
        item.useTime = item.useAnimation = Math.Max(6, (int)Math.Round(item.useTime * useMultiplier));
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        if (!base.CanStartCurrentAttack(player, omp))
            return false;

        if (omp.ultimateAttack && HasActiveOwnedProjectile(player, UltimateAttack))
            return false;

        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + direction * 14f;
        float momentumRatio = identity.FasttrackMomentumRatio;
        bool overdrive = omp.PrimaryAbilityEnabled;

        if (omp.ultimateAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, player.Center, direction, UltimateAttack, finalDamage, knockback + 0.8f,
                player.whoAmI, momentumRatio, overdrive ? 1f : 0f);
            identity.ConsumeFasttrackMomentum(34f);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 offset = Main.MouseWorld - player.Center;
            if (offset == Vector2.Zero)
                offset = new Vector2(player.direction, 0f);

            float maxRange = MathHelper.Lerp(BaseRushRange, MaxMomentumRushRange, momentumRatio) + (overdrive ? 24f : 0f);
            float requestedDistance = Math.Min(offset.Length(), maxRange);
            Vector2 rushDirection = offset.SafeNormalize(new Vector2(player.direction, 0f));
            float rushSpeed = FasttrackPursuitRushProjectile.GetRushSpeed(momentumRatio >= 0.6f);
            int rushFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / rushSpeed),
                FasttrackPursuitRushProjectile.MinRushFrames, FasttrackPursuitRushProjectile.MaxRushFrames);
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));

            int projectileIndex = Projectile.NewProjectile(source, player.Center + rushDirection * 16f,
                rushDirection * rushSpeed, SecondaryAbilityAttack, finalDamage, knockback + 1.2f, player.whoAmI,
                momentumRatio, overdrive ? 1f : 0f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.timeLeft = rushFrames;
                projectile.netUpdate = true;
            }

            identity.ConsumeFasttrackMomentum(24f);
            return false;
        }

        if (omp.altAttack) {
            int waveCount = momentumRatio >= 0.85f ? 3 : momentumRatio >= 0.45f ? 2 : 1;
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            for (int i = 0; i < waveCount; i++) {
                float spread = waveCount switch {
                    3 => (i - 1) * 0.1f,
                    2 => i == 0 ? -0.08f : 0.08f,
                    _ => 0f
                };
                Vector2 waveVelocity = direction.RotatedBy(spread) * SecondaryShootSpeed;
                Projectile.NewProjectile(source, spawnPosition, waveVelocity, SecondaryAttack, finalDamage,
                    knockback + 0.8f, player.whoAmI, momentumRatio, overdrive ? 1f : 0f);
            }

            return false;
        }

        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        float punchScale = 1f + momentumRatio * 0.2f + (overdrive ? 0.05f : 0f);
        int punchCount = momentumRatio >= 0.82f ? 2 : 1;
        for (int i = 0; i < punchCount; i++) {
            float spread = punchCount == 1 ? 0f : (i == 0 ? -0.05f : 0.05f);
            Projectile.NewProjectile(source, spawnPosition, direction.RotatedBy(spread), PrimaryAttack, primaryDamage,
                knockback, player.whoAmI, punchScale, momentumRatio);
        }

        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (!IsFasttrackProjectile(projectile.type))
            return;

        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!state.IsFasttrackComboActiveFor(player.whoAmI))
            return;

        modifiers.FinalDamage *= 1f + 0.05f * Math.Min(state.FasttrackComboStacks, 4);
        if (projectile.type == SecondaryAbilityAttack && state.FasttrackComboStacks >= 4)
            modifiers.FinalDamage *= 1.14f;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsFasttrackProjectile(projectile.type))
            return;

        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int existingStacks = state.IsFasttrackComboActiveFor(player.whoAmI) ? state.FasttrackComboStacks : 0;
        int stacksToAdd = projectile.type switch {
            _ when projectile.type == PrimaryAttack => 1,
            _ when projectile.type == SecondaryAttack => 2,
            _ when projectile.type == SecondaryAbilityAttack => 3,
            _ => 1
        };

        state.ApplyFasttrackCombo(player.whoAmI, stacksToAdd, 90);

        if (projectile.type == SecondaryAbilityAttack && existingStacks >= 3)
            player.GetModPlayer<AlienIdentityPlayer>().AddFasttrackMomentum(18f);
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.TinHelmet;
        player.body = ArmorIDs.Body.TinChainmail;
        player.legs = ArmorIDs.Legs.TinGreaves;
        player.armorEffectDrawShadow = true;
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

    private bool IsFasttrackProjectile(int projectileType) {
        return projectileType == PrimaryAttack ||
               projectileType == SecondaryAttack ||
               projectileType == SecondaryAbilityAttack ||
               projectileType == UltimateAttack;
    }
}
