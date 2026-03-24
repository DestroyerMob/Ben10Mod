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

namespace Ben10Mod.Content.Transformations.Cannonbolt;

public class CannonboltTransformation : Transformation {
    public override string FullID => "Ben10Mod:Cannonbolt";
    public override string TransformationName => "Cannonbolt";
    public override int TransformationBuffId => ModContent.BuffType<Cannonbolt_Buff>();
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";

    public override string Description =>
        "An armored Arburian Pelarota that turns momentum into offense with crushing shell rolls, impact bursts, and high-speed crash attacks.";

    public override List<string> Abilities => new() {
        "Armored shell roll primary",
        "Ricochet roll secondary",
        "Fortified curl stance",
        "Point-blank impact burst",
        "Meteor crash ultimate"
    };

    public override string PrimaryAttackName => "Shell Roll";
    public override string SecondaryAttackName => "Ricochet Roll";
    public override string SecondaryAbilityAttackName => "Impact Burst";
    public override string UltimateAttackName => "Meteor Crash";

    public override int PrimaryAttack => ModContent.ProjectileType<CannonboltRollProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 14;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.95f;

    public override int SecondaryAttack => ModContent.ProjectileType<CannonboltRollProjectile>();
    public override int SecondaryAttackSpeed => 42;
    public override int SecondaryShootSpeed => 18;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.45f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 12 * 60;
    public override int PrimaryAbilityCooldown => 38 * 60;
    public override int PrimaryAbilityCost => 20;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<CannonboltImpactBurstProjectile>();
    public override int SecondaryAbilityAttackSpeed => 26;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 1.15f;
    public override int SecondaryAbilityAttackEnergyCost => 30;
    public override int SecondaryAbilityCooldown => 22 * 60;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<CannonboltRollProjectile>();
    public override int UltimateAttackSpeed => 44;
    public override int UltimateShootSpeed => 22;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override float UltimateAttackModifier => 2.35f;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 50 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.1f;
        player.statDefense += 12;
        player.endurance += 0.06f;
        player.GetKnockback<HeroDamage>() += 0.55f;
        player.moveSpeed += 0.08f;
        player.noKnockback = true;
        player.noFallDmg = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.08f;
        player.statDefense += 18;
        player.endurance += 0.1f;
        player.moveSpeed += 0.22f;
        player.maxRunSpeed += 1.4f;
        player.runAcceleration *= 1.25f;
        player.jumpSpeedBoost += 2.2f;
        player.armorEffectDrawShadow = true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        TransformationAttackProfile profile = GetSelectedAttackProfile(omp);
        if (profile == null || profile.ProjectileType <= 0)
            return false;

        int finalDamage = Math.Max(1, (int)Math.Round(damage * profile.DamageMultiplier));
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        bool empowered = omp.PrimaryAbilityEnabled;

        if (omp.IsSecondaryAbilityAttackLoaded) {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                ModContent.ProjectileType<CannonboltImpactBurstProjectile>(), finalDamage, knockback + 2f, player.whoAmI,
                empowered ? 1.18f : 1f);
            return false;
        }

        float variant = 0f;
        if (omp.ultimateAttack)
            variant = 2f;
        else if (omp.altAttack)
            variant = 1f;

        Projectile.NewProjectile(source, player.Center, direction, ModContent.ProjectileType<CannonboltRollProjectile>(),
            finalDamage, knockback + (omp.ultimateAttack ? 3f : 1.5f), player.whoAmI, variant, empowered ? 1f : 0f);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MeteorHelmet;
        player.body = ArmorIDs.Body.MeteorSuit;
        player.legs = ArmorIDs.Legs.MeteorLeggings;
    }
}
