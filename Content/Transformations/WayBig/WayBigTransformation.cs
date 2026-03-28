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

namespace Ben10Mod.Content.Transformations.WayBig;

public class WayBigTransformation : Transformation {
    public const float BaseScale = 6f;
    public const float EmpoweredScale = 7f;

    private const int ScaleRampDuration = 240;
    private const int PrimaryAbilityDurationTicks = 15 * 60;
    private const int PrimaryAbilityCooldownTicks = 45 * 60;
    private const int CosmicRayActivationCost = 100;
    private const int CosmicRaySustainCost = 15;
    private const int CosmicRaySustainInterval = 15;
    private const int CosmicRayCooldownTicks = 90 * 60;

    public override string FullID => "Ben10Mod:WayBig";
    public override string TransformationName => "Way Big";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<WayBig_Buff>();

    public override string Description =>
        "A colossal cosmic juggernaut that hammers the battlefield with planet-cracking blows and an overwhelming stellar beam.";

    public override List<string> Abilities => new() {
        "Titan-scale melee punch",
        "Ground-ripping stomp shockwave",
        "Cosmic surge that boosts size and strength",
        "Cosmic Ray, a sustained stellar beam"
    };

    public override string PrimaryAttackName => "Cosmic Punch";
    public override string SecondaryAttackName => "Planetary Stomp";
    public override string PrimaryAbilityName => "Cosmic Surge";
    public override string UltimateAttackName => "Cosmic Ray";
    public override int PrimaryAttack => ModContent.ProjectileType<WayBigPunchProjectile>();
    public override int PrimaryAttackSpeed => 30;
    public override int PrimaryShootSpeed => 10;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 1.35f;
    public override int SecondaryAttack => ModContent.ProjectileType<WayBigShockwaveProjectile>();
    public override int SecondaryAttackSpeed => 40;
    public override int SecondaryShootSpeed => 12;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.7f;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => PrimaryAbilityDurationTicks;
    public override int PrimaryAbilityCooldown => PrimaryAbilityCooldownTicks;
    public override int UltimateAttack => ModContent.ProjectileType<WayBigCosmicRayProjectile>();
    public override int UltimateAttackSpeed => CosmicRaySustainInterval;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override bool UltimateChannel => true;
    public override bool UltimateNoMelee => true;
    public override int UltimateEnergyCost => CosmicRaySustainCost;
    public override int UltimateAttackSustainEnergyCost => CosmicRaySustainCost;
    public override int UltimateAttackSustainInterval => CosmicRaySustainInterval;
    public override int UltimateAbilityCost => CosmicRayActivationCost;
    public override int UltimateAbilityCooldown => CosmicRayCooldownTicks;

    public override string GetDisplayName(OmnitrixPlayer omp) {
        return omp.IsPrimaryAbilityActive ? "Way Big (Cosmic Surge)" : base.GetDisplayName(omp);
    }

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        float targetScale = omp.IsPrimaryAbilityActive ? EmpoweredScale : BaseScale;
        omp.SetTransformationScale(targetScale, ScaleRampDuration, 1f, targetScale);

        player.GetDamage<HeroDamage>() += omp.IsPrimaryAbilityActive ? 0.34f : 0.24f;
        player.statDefense += omp.IsPrimaryAbilityActive ? 30 : 22;
        player.endurance += omp.IsPrimaryAbilityActive ? 0.12f : 0.08f;
        player.GetKnockback<HeroDamage>() += omp.IsPrimaryAbilityActive ? 1.25f : 0.85f;
        player.GetArmorPenetration<HeroDamage>() += omp.IsPrimaryAbilityActive ? 12 : 6;
        player.noKnockback = true;
        player.moveSpeed *= omp.IsPrimaryAbilityActive ? 0.92f : 0.84f;
        player.maxRunSpeed *= 0.88f;

        if (omp.ultimateAttack || omp.IsPrimaryAbilityActive)
            Lighting.AddLight(player.Center, 0.15f, 0.55f, 0.65f);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 aimDirection = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        float combatScale = Math.Max(1f, omp.CurrentTransformationScale);

        if (omp.ultimateAttack) {
            Projectile.NewProjectile(source, player.Center, aimDirection,
                ModContent.ProjectileType<WayBigCosmicRayProjectile>(), damage, knockback + 2f, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            Vector2 stompOrigin = player.Bottom + new Vector2(aimDirection.X * (10f + combatScale * 5f), -16f);
            Projectile.NewProjectile(source, stompOrigin, new Vector2(aimDirection.X, 0f),
                ModContent.ProjectileType<WayBigShockwaveProjectile>(), damage, knockback + 3f, player.whoAmI,
                aimDirection.X, combatScale);
            return false;
        }

        Projectile.NewProjectile(source, player.Center, aimDirection * 10f,
            ModContent.ProjectileType<WayBigPunchProjectile>(), damage, knockback + 4f, player.whoAmI, combatScale);
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
        if (!omp.IsPrimaryAbilityActive && !omp.ultimateAttack)
            return;

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GemSapphire,
                Scale: omp.IsPrimaryAbilityActive ? 1.6f : 1.3f);
            dust.velocity *= 0.18f;
            dust.noGravity = true;
        }
    }
}
