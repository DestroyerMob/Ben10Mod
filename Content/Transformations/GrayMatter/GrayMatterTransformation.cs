using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.GrayMatter;

public class GrayMatterTransformation : Transformation {
    private const float VisualScale = 24f / 42f;
    private const float WidthScale = 12f / 20f;
    private const int ScaleRampTicks = 12;
    private const int HyperfocusDurationTicks = 12 * 60;
    private const int HyperfocusCooldownTicks = 30 * 60;
    private const int HyperfocusCost = 12;
    private const int CerebralCascadeEnergyCost = 34;
    private const int CerebralCascadeCooldownTicks = 38 * 60;

    public override string FullID => "Ben10Mod:GrayMatter";
    public override string TransformationName => "Gray Matter";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<GrayMatter_Buff>();

    public override string Description =>
        "A tiny Galvan prodigy who trades raw power for speed, precision, Omnitrix efficiency, and gadget-like neuro attacks.";

    public override List<string> Abilities => new() {
        "Rapid neuro-darts that punish enemies with precise, high-penetration shots",
        "Logic orbs that ricochet, home in, and scramble targets",
        "Hyperfocus to boost OE recovery, mobility, and shot quality",
        "Cerebral Cascade to flood the screen with guided darts"
    };

    public override string PrimaryAttackName => "Neuro Dart";
    public override string SecondaryAttackName => "Logic Orb";
    public override string PrimaryAbilityName => "Hyperfocus";
    public override string UltimateAttackName => "Cerebral Cascade";

    public override int PrimaryAttack => ModContent.ProjectileType<GrayMatterNeuronProjectile>();
    public override float PrimaryAttackModifier => 0.72f;
    public override int PrimaryAttackSpeed => 11;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int PrimaryArmorPenetration => 18;

    public override int SecondaryAttack => ModContent.ProjectileType<GrayMatterLogicOrbProjectile>();
    public override float SecondaryAttackModifier => 1.08f;
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 10;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryArmorPenetration => 8;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => HyperfocusDurationTicks;
    public override int PrimaryAbilityCooldown => HyperfocusCooldownTicks;
    public override int PrimaryAbilityCost => HyperfocusCost;

    public override int UltimateAttack => ModContent.ProjectileType<GrayMatterNeuronProjectile>();
    public override float UltimateAttackModifier => 0.78f;
    public override int UltimateAttackSpeed => 24;
    public override int UltimateShootSpeed => 19;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override int UltimateEnergyCost => CerebralCascadeEnergyCost;
    public override int UltimateAbilityCooldown => CerebralCascadeCooldownTicks;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        omp.SetTransformationScale(VisualScale, ScaleRampTicks, WidthScale, VisualScale);
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 12f;
        player.GetArmorPenetration<HeroDamage>() += 8;
        player.moveSpeed += 0.2f;
        player.runAcceleration += 0.22f;
        player.jumpSpeedBoost += 1.2f;
        player.noFallDmg = true;
        omp.omnitrixEnergyRegenBonus += 1;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.16f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.GetArmorPenetration<HeroDamage>() += 10;
        player.moveSpeed += 0.12f;
        player.runAcceleration += 0.16f;
        omp.omnitrixEnergyRegenBonus += 1;
        Lighting.AddLight(player.Center, 0.08f, 0.28f, 0.08f);
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        player.gravity *= 0.82f;
        player.maxFallSpeed *= 0.9f;

        if (player.controlJump && player.velocity.Y > -4.8f)
            player.velocity.Y -= 0.08f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        bool hyperfocused = omp.IsPrimaryAbilityActive;

        if (omp.ultimateAttack) {
            float[] spreads = { -0.28f, -0.14f, 0f, 0.14f, 0.28f };
            int finalDamage = System.Math.Max(1, (int)System.Math.Round(damage * UltimateAttackModifier));

            for (int i = 0; i < spreads.Length; i++) {
                Vector2 shotDirection = direction.RotatedBy(spreads[i]).SafeNormalize(direction);
                Projectile.NewProjectile(source, player.MountedCenter + shotDirection * 10f,
                    shotDirection * UltimateShootSpeed, ModContent.ProjectileType<GrayMatterNeuronProjectile>(),
                    finalDamage, knockback, player.whoAmI, 1f);
            }

            return false;
        }

        if (omp.altAttack) {
            int finalDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAttackModifier));
            Vector2 spawnPosition = player.MountedCenter + direction * 10f + new Vector2(0f, -player.height * 0.12f);
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed,
                ModContent.ProjectileType<GrayMatterLogicOrbProjectile>(), finalDamage, knockback + 0.5f,
                player.whoAmI, hyperfocused ? 1f : 0f);
            return false;
        }

        int dartDamage = System.Math.Max(1, (int)System.Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, player.MountedCenter + direction * 8f, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<GrayMatterNeuronProjectile>(), dartDamage, knockback,
            player.whoAmI, hyperfocused ? 1f : 0f);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.TinHelmet;
        player.body = ArmorIDs.Body.TinChainmail;
        player.legs = ArmorIDs.Legs.TinGreaves;
    }

    public override void ModifyDrawInfo(Player player, OmnitrixPlayer omp, ref PlayerDrawSet drawInfo) {
        Color tintColor = omp.IsPrimaryAbilityActive ? new Color(185, 230, 175) : new Color(155, 200, 150);
        byte targetAlpha = omp.IsPrimaryAbilityActive ? (byte)165 : (byte)185;
        float tintStrength = omp.IsPrimaryAbilityActive ? 0.45f : 0.32f;

        drawInfo.colorArmorHead = TintDrawColor(drawInfo.colorArmorHead, tintColor, tintStrength, targetAlpha);
        drawInfo.colorArmorBody = TintDrawColor(drawInfo.colorArmorBody, tintColor, tintStrength, targetAlpha);
        drawInfo.colorArmorLegs = TintDrawColor(drawInfo.colorArmorLegs, tintColor, tintStrength, targetAlpha);
        drawInfo.colorEyeWhites = TintDrawColor(drawInfo.colorEyeWhites, Color.White, 0.18f, targetAlpha);
        drawInfo.colorEyes = TintDrawColor(drawInfo.colorEyes, new Color(115, 255, 115), 0.55f, targetAlpha);
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();

        if (!Main.rand.NextBool(2)) {
            Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.5f, player.height * 0.55f);
            Dust dust = Dust.NewDustPerfect(player.Center + offset,
                Main.rand.NextBool(3) ? DustID.GreenTorch : DustID.Electric,
                player.velocity * 0.06f + Main.rand.NextVector2Circular(0.25f, 0.25f), 95,
                omp.IsPrimaryAbilityActive ? new Color(190, 255, 180) : new Color(145, 225, 155),
                Main.rand.NextFloat(0.82f, 1.06f));
            dust.noGravity = true;
        }
    }

    private static Color TintDrawColor(Color baseColor, Color tint, float tintStrength, byte maxAlpha) {
        return new Color(
            (byte)MathHelper.Lerp(baseColor.R, tint.R, tintStrength),
            (byte)MathHelper.Lerp(baseColor.G, tint.G, tintStrength),
            (byte)MathHelper.Lerp(baseColor.B, tint.B, tintStrength),
            (byte)System.Math.Min(baseColor.A, maxAlpha)
        );
    }
}
