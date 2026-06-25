using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.AlienX;

public class AlienXTransformation : Transformation {
    private const int DeliberationCooldown = 20 * 60;
    private const int RepulseBurstCost = 28;
    private const int RepulseBurstCooldown = 16 * 60;
    private const int SingularityCascadeCount = 5;
    private const int SingularityCascadeIntervalTicks = 7;
    private const float SingularityCascadeWaveSpeed = 26f;

    public override string FullID => "Ben10Mod:AlienX";
    public override string TransformationName => "Alien X";
    public override int TransformationBuffId => ModContent.BuffType<AlienX_Buff>();
    public override string Description =>
        "A Celestialsapien who controls space with gravity pulses, black holes, and calm Judgement. Stand still at the right moment, then let the universe answer for you.";

    public override List<string> Abilities => new() {
        "Cosmic Wave knocks enemies back and gives Alien X room to breathe.",
        "Pocket Singularity places a black hole at the cursor, dragging enemies into one dangerous point.",
        "Hold Deliberation while standing still to build Judgement. Moving breaks the channel.",
        "At high Judgement, Pocket Singularity becomes a short cascade of black holes.",
        "Cosmic Repulse spends OE to blast nearby enemies away when the screen gets crowded.",
        "Supernova erupts from Alien X, catching enemies in stasis before the blast lands."
    };

    public override string PrimaryAttackName => "Cosmic Wave";
    public override string SecondaryAttackName => "Pocket Singularity";
    public override string PrimaryAbilityName => "Deliberation";
    public override string SecondaryAbilityAttackName => "Cosmic Repulse";
    public override string UltimateAttackName => "Supernova";

    public override int PrimaryAttack => ModContent.ProjectileType<AlienXGravityPulseProjectile>();
    public override int PrimaryAttackSpeed => 15;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.92f;

    public override int SecondaryAttack => ModContent.ProjectileType<AlienXBlackHoleProjectile>();
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAttackModifier => 0.78f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => DeliberationCooldown;
    public override int PrimaryAbilityCost => 0;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<AlienXForceWaveProjectile>();
    public override int SecondaryAbilityAttackSpeed => 24;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 1.12f;
    public override int SecondaryAbilityAttackEnergyCost => RepulseBurstCost;
    public override int SecondaryAbilityCooldown => RepulseBurstCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<AlienXVerdictProjectile>();
    public override int UltimateAttackSpeed => 30;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => 2.2f;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 60 * 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<AlienXJudgementPlayer>().ResetJudgement();
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<AlienXBlackHoleProjectile>(),
            ModContent.ProjectileType<AlienXForceWaveProjectile>(),
            ModContent.ProjectileType<AlienXVerdictProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        UpdateDeliberationChannel(player, omp);

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 4f;
        player.statDefense += 10;
        player.endurance += 0.06f;
        player.moveSpeed += 0.04f;
        player.noFallDmg = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.statDefense += 6;
        player.endurance += 0.05f;
        player.noKnockback = true;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.34f, 0.34f, 0.48f));
        FreezeForDeliberation(player);
    }

    public override void UpdateActiveAbilityVisuals(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        SpawnDeliberationAura(player);
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (IsDeliberationHeld(player, omp))
            FreezeForDeliberation(player);
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<AlienXJudgementPlayer>().UpdateSingularityCascade(player);

        if (omp.PrimaryAbilityEnabled)
            FreezeForDeliberation(player);
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        if (!base.CanStartCurrentAttack(player, omp))
            return false;

        TransformationAttackProfile profile = GetSelectedAttackProfile(omp);
        if (profile == null)
            return true;

        if (profile.ProjectileType == ModContent.ProjectileType<AlienXVerdictProjectile>() ||
            profile.ProjectileType == ModContent.ProjectileType<AlienXForceWaveProjectile>())
            return !HasActiveOwnedProjectile(player, profile.ProjectileType);

        if (profile.ProjectileType == ModContent.ProjectileType<AlienXBlackHoleProjectile>()) {
            AlienXJudgementPlayer judgementPlayer = player.GetModPlayer<AlienXJudgementPlayer>();
            return judgementPlayer.HasSingularityCascadeThreshold ||
                   (!judgementPlayer.SingularityCascadeActive && !HasActiveOwnedProjectile(player, profile.ProjectileType));
        }

        return true;
    }

    public override bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) {
        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 targetPosition = ResolveTargetPosition(player, direction, 180f);

        if (omp.ultimateAttack) {
            int verdictType = ModContent.ProjectileType<AlienXVerdictProjectile>();
            if (HasActiveOwnedProjectile(player, verdictType))
                return false;

            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, verdictType, finalDamage, knockback + 1.2f,
                player.whoAmI);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            int repulseType = ModContent.ProjectileType<AlienXForceWaveProjectile>();
            if (HasActiveOwnedProjectile(player, repulseType))
                return false;

            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));
            Projectile.NewProjectile(source, player.Center, direction, repulseType, finalDamage, knockback + 1.4f,
                player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            int singularityType = ModContent.ProjectileType<AlienXBlackHoleProjectile>();
            AlienXJudgementPlayer judgementPlayer = player.GetModPlayer<AlienXJudgementPlayer>();
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));

            if (judgementPlayer.TryStartSingularityCascade(player, targetPosition, direction, finalDamage,
                    knockback + 0.4f, SingularityCascadeCount, SingularityCascadeIntervalTicks,
                    SingularityCascadeWaveSpeed)) {
                return false;
            }

            if (judgementPlayer.SingularityCascadeActive)
                return false;

            if (HasActiveOwnedProjectile(player, singularityType))
                return false;

            Projectile.NewProjectile(source, targetPosition, Vector2.Zero, singularityType,
                finalDamage, knockback + 0.4f, player.whoAmI);
            return false;
        }

        int primaryDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Vector2 spawnPosition = player.Center + direction * 16f;
        Vector2 pulseVelocity = direction * PrimaryShootSpeed;
        Projectile.NewProjectile(source, spawnPosition, pulseVelocity, ModContent.ProjectileType<AlienXGravityPulseProjectile>(),
            primaryDamage, knockback, player.whoAmI);
        return false;
    }

    public override IReadOnlyList<string> GetCombatSlotSummaries(OmnitrixPlayer omp) {
        List<string> summaries = new(base.GetCombatSlotSummaries(omp));
        AlienXJudgementPlayer judgementPlayer = omp.Player.GetModPlayer<AlienXJudgementPlayer>();
        summaries.Add($"Judgement: {judgementPlayer.Judgement}%");
        if (judgementPlayer.SingularityCascadeActive)
            summaries.Add($"Cascade: {judgementPlayer.SingularityCascadeRemaining} left");
        return summaries;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.PlatinumHelmet;
        player.body = ArmorIDs.Body.PlatinumChainmail;
        player.legs = ArmorIDs.Legs.PlatinumGreaves;
    }

    private void UpdateDeliberationChannel(Player player, OmnitrixPlayer omp) {
        AlienXJudgementPlayer judgementPlayer = player.GetModPlayer<AlienXJudgementPlayer>();
        if (!IsDeliberationHeld(player, omp)) {
            judgementPlayer.StopDeliberating();
            return;
        }

        omp.PrimaryAbilityEnabled = true;
        omp.Abilities.SetTransformationId(OmnitrixPlayer.AttackSelection.PrimaryAbility, FullID);
        judgementPlayer.ChannelDeliberation();
    }

    private static bool IsDeliberationHeld(Player player, OmnitrixPlayer omp) {
        return player != null &&
               omp != null &&
               player.whoAmI == Main.myPlayer &&
               KeybindSystem.PrimaryAbility.Current &&
               !player.HasBuff(ModContent.BuffType<PrimaryAbilityCooldown>());
    }

    private static void FreezeForDeliberation(Player player) {
        player.velocity = Vector2.Zero;
        player.controlLeft = false;
        player.controlRight = false;
        player.controlUp = false;
        player.controlDown = false;
        player.controlJump = false;
        player.controlHook = false;
        player.controlMount = false;
        player.fallStart = (int)(player.position.Y / 16f);
    }

    private static void SpawnDeliberationAura(Player player) {
        if (Main.dedServ)
            return;

        int dustCount = Main.rand.NextBool(2) ? 2 : 1;
        for (int i = 0; i < dustCount; i++) {
            float radius = Main.rand.NextFloat(56f, 104f);
            Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius * 0.82f);
            Vector2 position = player.Center + offset;
            Vector2 velocity = (player.Center - position).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.4f, 3.2f);
            int dustType = Main.rand.NextBool(3) ? DustID.WhiteTorch :
                Main.rand.NextBool() ? DustID.GemDiamond : DustID.ShadowbeamStaff;
            Color color = Color.Lerp(new Color(145, 170, 255), new Color(245, 248, 255), Main.rand.NextFloat());
            Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 105, color, Main.rand.NextFloat(0.9f, 1.35f));
            dust.noGravity = true;
        }
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

    private static Vector2 ResolveTargetPosition(Player player, Vector2 fallbackDirection, float fallbackDistance) {
        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer)
            return Main.MouseWorld;

        return player.Center + fallbackDirection * fallbackDistance;
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
