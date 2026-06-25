using System;
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

namespace Ben10Mod.Content.Transformations.Armodrillo;

public class ArmodrilloTransformation : Transformation {
    private const int GroundChargeDuration = 7 * 60;
    private const float SeismicWaveDamageMultiplier = 1.16f;
    private const float AirSeismicWaveDamageScale = 0.48f;
    private const float SiegeGroundAttackScale = 1.24f;
    private const float GroundChargeDamageBonus = 0.17f;
    private const float SplitWaveBaseDamageScale = 0.64f;

    public override string FullID => ArmodrilloSeismicPlayer.TransformationId;
    public override string TransformationName => "Armodrillo";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Armodrillo_Buff>();

    public override string Description =>
        "A grounded seismic engineer that charges the terrain with piston drills, then splits fault lines through enemies.";

    public override List<string> Abilities => new() {
        "Piston Drill tears into enemies and charges the ground beneath the fight.",
        "Seismic Wave travels through connected ground and splits harder after the terrain is charged.",
        "Siege Plating roots Armodrillo in place, trading agility for tougher armor and stronger ground attacks.",
        "Fault Line sends a travelling rupture across the arena when you commit to the ground fight."
    };

    public override string PrimaryAttackName => "Piston Drill";
    public override string SecondaryAttackName => "Seismic Wave";
    public override string PrimaryAbilityName => "Siege Plating";
    public override string UltimateAttackName => "Fault Line";
    public override int PrimaryAttack => ModContent.ProjectileType<ArmodrilloDrillProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 14;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<ArmodrilloQuakeProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HiddenAnimation;
    public override float SecondaryAttackModifier => SeismicWaveDamageMultiplier;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAbilityCooldown => 32 * 60;
    public override int UltimateAttack => ModContent.ProjectileType<ArmodrilloUltimateSlamProjectile>();
    public override float UltimateAttackModifier => 3.25f;
    public override int UltimateAttackSpeed => 26;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override int UltimateEnergyCost => 65;
    public override int UltimateAbilityCost => 65;
    public override int UltimateAbilityCooldown => 50 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetAttackSpeed<HeroDamage>() += 0.05f;
        player.statDefense += 13;
        player.endurance += 0.07f;
        player.GetKnockback<HeroDamage>() += 0.65f;
        player.GetArmorPenetration<HeroDamage>() += 10;
        player.moveSpeed -= 0.01f;
        player.noKnockback = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() -= 0.04f;
        player.statDefense += 18;
        player.endurance += 0.12f;
        player.GetKnockback<HeroDamage>() += 0.75f;
        player.GetArmorPenetration<HeroDamage>() += 12;
        player.moveSpeed -= 0.12f;
        player.maxRunSpeed *= 0.58f;
        player.accRunSpeed *= 0.62f;
        player.runAcceleration *= 0.42f;
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        if (AlienIdentityPlayer.IsGrounded(player)) {
            player.velocity.X *= 0.62f;
            if (Math.Abs(player.velocity.X) < 0.08f)
                player.velocity.X = 0f;
            return;
        }

        player.velocity.X *= 0.9f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        int horizontalDirection = direction.X >= 0f ? 1 : -1;
        bool grounded = AlienIdentityPlayer.IsGrounded(player);
        bool siege = omp.PrimaryAbilityEnabled;
        ArmodrilloSeismicPlayer seismic = player.GetModPlayer<ArmodrilloSeismicPlayer>();

        if (omp.ultimateAttack) {
            int charge = seismic.ConsumeGroundCharge();
            float ultimateMultiplier = UltimateAttackModifier * (siege ? 1.12f : 1f) * (1f + charge * 0.12f);
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, UltimateAttack,
                ScaleDamage(damage, ultimateMultiplier), knockback + 2.2f + charge * 0.35f, player.whoAmI,
                0f, siege ? 1f : 0f, charge);
            return false;
        }

        if (omp.altAttack) {
            int charge = seismic.ConsumeGroundCharge();
            int waveFlags = BuildWaveFlags(grounded, siege, false);
            float waveMultiplier = SecondaryAttackModifier * (grounded ? 1f : AirSeismicWaveDamageScale);
            if (grounded && siege)
                waveMultiplier *= SiegeGroundAttackScale;
            waveMultiplier *= 1f + charge * GroundChargeDamageBonus;

            int waveDamage = ScaleDamage(damage, waveMultiplier);
            Vector2 quakeOrigin = player.Bottom + new Vector2(direction.X * 12f, -8f);
            SpawnSeismicWave(source, quakeOrigin, horizontalDirection, waveDamage, knockback + 1.2f + charge * 0.25f,
                waveFlags, charge, player.whoAmI);

            if (grounded && charge > 0)
                SpawnSplitWaves(source, player, quakeOrigin, horizontalDirection, waveDamage, knockback, waveFlags, charge);

            return false;
        }

        Projectile.NewProjectile(source, player.Center + direction * 18f, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<ArmodrilloDrillProjectile>(), ScaleDamage(damage, grounded ? 1f : 0.86f),
            knockback + 1f + (grounded ? 0.35f : 0f) + (siege ? 0.3f : 0f), player.whoAmI, grounded ? 1f : 0f,
            siege ? 1f : 0f);

        if (grounded) {
            int chargeGain = siege ? 2 : 1;
            seismic.AddGroundCharge(chargeGain, GroundChargeDuration);
            SpawnGroundChargeDust(player, horizontalDirection, chargeGain);
        }

        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (projectile.type != PrimaryAttack && projectile.type != SecondaryAttack && projectile.type != UltimateAttack)
            return;

        if (projectile.type == PrimaryAttack) {
            bool groundedDrill = projectile.ai[0] > 0.5f;
            bool siegeDrill = projectile.ai[1] > 0.5f;
            if (groundedDrill)
                modifiers.ArmorPenetration += siegeDrill ? 12 : 6;
            if (groundedDrill && siegeDrill)
                modifiers.FinalDamage *= 1.1f;
            return;
        }

        if (projectile.type == SecondaryAttack) {
            int waveFlags = (int)MathF.Round(projectile.ai[1]);
            int charge = Math.Clamp((int)MathF.Round(projectile.ai[2]), 0, ArmodrilloSeismicPlayer.MaxGroundCharge);
            bool groundedWave = (waveFlags & ArmodrilloQuakeProjectile.GroundedWaveFlag) != 0;
            bool siegeWave = (waveFlags & ArmodrilloQuakeProjectile.SiegeWaveFlag) != 0;
            bool faultLine = (waveFlags & ArmodrilloQuakeProjectile.FaultLineWaveFlag) != 0;

            modifiers.ArmorPenetration += groundedWave ? 12 + charge * 4 : 4;
            if (groundedWave && siegeWave)
                modifiers.FinalDamage *= 1.08f;
            if (faultLine)
                modifiers.FinalDamage *= 1.06f + charge * 0.025f;
        }
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (projectile.type != PrimaryAttack || projectile.ai[0] <= 0.5f || projectile.localAI[1] > 0f)
            return;

        projectile.localAI[1] = 1f;
        player.GetModPlayer<ArmodrilloSeismicPlayer>().AddGroundCharge(1, GroundChargeDuration);
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.PrimaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Ultimate)
            return base.GetAttackResourceSummary(selection, omp, compact);

        Player player = omp.Player;
        ArmodrilloSeismicPlayer seismic = player.GetModPlayer<ArmodrilloSeismicPlayer>();
        bool grounded = AlienIdentityPlayer.IsGrounded(player);
        string chargeText = compact
            ? $"Charge {seismic.GroundCharge}/{ArmodrilloSeismicPlayer.MaxGroundCharge}"
            : $"Ground charge {seismic.GroundCharge}/{ArmodrilloSeismicPlayer.MaxGroundCharge}";
        string stanceText = omp.PrimaryAbilityEnabled
            ? compact ? "Sieged" : "Siege stance"
            : grounded ? "Grounded" : compact ? "Air weak" : "Air waves weakened";
        string baseText = base.GetAttackResourceSummary(selection, omp, compact);
        string identityText = resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"{chargeText} • Drill"
                : $"{chargeText} • drills charge the ground",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{chargeText} • {stanceText}"
                : $"{chargeText} • {stanceText} • charged waves split",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? $"Rooted • {chargeText}"
                : $"Rooted siege stance • ground attacks amplified • {chargeText}",
            OmnitrixPlayer.AttackSelection.Ultimate => compact
                ? $"Fault line • {chargeText}"
                : $"Travelling fault line • consumes {chargeText.ToLowerInvariant()}",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(baseText) ? identityText : $"{baseText} • {identityText}";
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MeteorHelmet;
        player.body = ArmorIDs.Body.MeteorSuit;
        player.legs = ArmorIDs.Legs.MeteorLeggings;
    }

    private static void SpawnSplitWaves(EntitySource_ItemUse_WithAmmo source, Player player, Vector2 origin,
        int direction, int waveDamage, float knockback, int waveFlags, int charge) {
        int reverseDamage = ScaleDamage(waveDamage, SplitWaveBaseDamageScale + charge * 0.06f);
        SpawnSeismicWave(source, origin + new Vector2(-direction * 8f, 0f), -direction, reverseDamage,
            knockback + 0.8f + charge * 0.2f, waveFlags, Math.Max(1, charge - 1), player.whoAmI);

        if (charge < 2)
            return;

        int forwardBranchDamage = ScaleDamage(waveDamage, 0.58f + charge * 0.05f);
        SpawnSeismicWave(source, origin + new Vector2(direction * 30f, -2f), direction, forwardBranchDamage,
            knockback + 0.6f, waveFlags, charge - 1, player.whoAmI);

        if (charge < ArmodrilloSeismicPlayer.MaxGroundCharge)
            return;

        int backBranchDamage = ScaleDamage(waveDamage, 0.48f);
        SpawnSeismicWave(source, origin + new Vector2(-direction * 30f, -2f), -direction, backBranchDamage,
            knockback + 0.5f, waveFlags, 1, player.whoAmI);
    }

    private static int BuildWaveFlags(bool grounded, bool siege, bool faultLine) {
        int flags = 0;
        if (grounded)
            flags |= ArmodrilloQuakeProjectile.GroundedWaveFlag;
        if (siege)
            flags |= ArmodrilloQuakeProjectile.SiegeWaveFlag;
        if (faultLine)
            flags |= ArmodrilloQuakeProjectile.FaultLineWaveFlag;
        return flags;
    }

    private static void SpawnSeismicWave(IEntitySource source, Vector2 origin, int direction, int damage, float knockback,
        int waveFlags, int charge, int owner) {
        Projectile.NewProjectile(source, origin, Vector2.Zero, ModContent.ProjectileType<ArmodrilloQuakeProjectile>(),
            damage, knockback, owner, direction, waveFlags, charge);
    }

    private static int ScaleDamage(int damage, float multiplier) =>
        Math.Max(1, (int)Math.Round(damage * multiplier));

    private static void SpawnGroundChargeDust(Player player, int direction, int chargeGain) {
        for (int i = 0; i < 8 + chargeGain * 4; i++) {
            Vector2 dustVelocity = new(direction * Main.rand.NextFloat(0.8f, 2.6f), Main.rand.NextFloat(-1.2f, 0.2f));
            Dust dust = Dust.NewDustPerfect(player.Bottom + Main.rand.NextVector2Circular(16f, 5f),
                i % 3 == 0 ? DustID.GemDiamond : DustID.Smoke, dustVelocity, 115, Color.White,
                Main.rand.NextFloat(0.95f, 1.3f));
            dust.noGravity = true;
        }
    }
}
