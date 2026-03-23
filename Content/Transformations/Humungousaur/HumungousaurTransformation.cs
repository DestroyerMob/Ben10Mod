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

namespace Ben10Mod.Content.Transformations.Humungousaur;

public class HumungousaurTransformation : Transformation {
    public const float GrownScale = 1.65f;
    public const float UltimateGrownScale = 1f + (GrownScale - 1f) * 2f;
    public const int GrowthRampDuration = 3 * 60;
    public const int UltimateAbilityDurationTicks = 20 * 60;
    public const int UltimateAbilityCooldownTicks = 90 * 60;

    public override string FullID => "Ben10Mod:Humungousaur";
    public override string TransformationName => "Humungousaur";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Humungousaur_Buff>();
    public override Transformation ChildTransformation => ModContent.GetInstance<UltimateHumungousaurTransformation>();

    public override string Description =>
        "A towering Vaxasaurian bruiser that can grow stronger mid-battle and smash enemies apart with raw force.";

    public override List<string> Abilities => new() {
        "Close-range power punch",
        "Forward shockwave slam",
        "Growth surge that boosts strength and toughness",
        "Titan growth that doubles the surge",
        "Ultimate evolution"
    };

    public override string PrimaryAttackName => "Power Punch";
    public override string SecondaryAttackName => "Ground Shockwave";
    public override int PrimaryAttack => ModContent.ProjectileType<HumungousaurPunchProjectile>();
    public override int PrimaryAttackSpeed => 34;
    public override int PrimaryShootSpeed => 10;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>();
    public override int SecondaryAttackSpeed => 34;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 60 * 60;
    public override int PrimaryAbilityCooldown => 50 * 60;
    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityCost => 75;
    public override int UltimateAbilityDuration => UltimateAbilityDurationTicks;
    public override int UltimateAbilityCooldown => UltimateAbilityCooldownTicks;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        float growthScale = GetActiveGrowthScale(omp);
        float growthBonusMultiplier = GetGrowthBonusMultiplier(growthScale);
        bool growthActive = growthBonusMultiplier > 0f;

        omp.SetTransformationScale(growthScale, GrowthRampDuration, 1f, growthScale);

        player.statDefense += 8;
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetKnockback<HeroDamage>() += 0.25f;
        player.endurance += 0.04f;

        if (!growthActive)
            return;

        player.statDefense += (int)Math.Round(14f * growthBonusMultiplier);
        player.GetDamage<HeroDamage>() += 0.2f * growthBonusMultiplier;
        player.GetKnockback<HeroDamage>() += 0.5f * growthBonusMultiplier;
        player.endurance += 0.05f * growthBonusMultiplier;
        player.moveSpeed *= Math.Max(0.65f, 1f - 0.1f * growthBonusMultiplier);
    }

    public override string GetDisplayName(OmnitrixPlayer omp) {
        if (omp.IsUltimateAbilityActive)
            return "Humungousaur (Titan Growth)";

        return omp.IsPrimaryAbilityActive ? "Humungousaur (Grown)" : base.GetDisplayName(omp);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        float growthScale = GetCurrentCombatScale(omp);
        bool growthActive = growthScale > 1f;

        if (omp.altAttack) {
            Vector2 shockwaveVelocity = new(player.direction * (growthActive ? 9f : 7.5f), 0f);
            Vector2 shockwaveSpawn = player.Bottom + new Vector2(player.direction * 12f * growthScale, -10f * growthScale);
            Projectile.NewProjectile(source, shockwaveSpawn, shockwaveVelocity,
                ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), (int)(damage * 1.05f * growthScale),
                knockback, player.whoAmI, growthScale);
            return false;
        }

        Vector2 punchVelocity = velocity.SafeNormalize(new Vector2(player.direction, 0f)) * 10f;
        Projectile.NewProjectile(source, player.Center + punchVelocity * (2f * growthScale), punchVelocity,
            ModContent.ProjectileType<HumungousaurPunchProjectile>(), (int)(damage * growthScale), knockback,
            player.whoAmI, growthScale);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MoltenHelmet;
        player.body = ArmorIDs.Body.MoltenBreastplate;
        player.legs = ArmorIDs.Legs.MoltenGreaves;
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsPrimaryAbilityActive && !omp.IsUltimateAbilityActive)
            return;

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch, Scale: 1.2f);
            dust.velocity *= 0.2f;
            dust.noGravity = true;
        }
    }

    private static float GetActiveGrowthScale(OmnitrixPlayer omp) {
        if (omp.IsUltimateAbilityActive)
            return UltimateGrownScale;

        return omp.IsPrimaryAbilityActive ? GrownScale : 1f;
    }

    private static float GetCurrentCombatScale(OmnitrixPlayer omp) {
        return Math.Max(1f, omp.CurrentTransformationScale);
    }

    private static float GetGrowthBonusMultiplier(float growthScale) {
        if (growthScale <= 1f || GrownScale <= 1f)
            return 0f;

        return (growthScale - 1f) / (GrownScale - 1f);
    }
}
