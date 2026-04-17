using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Armodrillo;

public class ArmodrilloTransformation : Transformation {
    private const float SeismicWaveDamageMultiplier = 1.24f;

    public override string FullID => "Ben10Mod:Armodrillo";
    public override string TransformationName => "Armodrillo";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Armodrillo_Buff>();

    public override string Description =>
        "A heavily-armored powerhouse that pulverizes enemies with piston drills and seismic ground-shattering force.";

    public override List<string> Abilities => new() {
        "Piston drill strike",
        "Seismic wave that travels through the ground",
        "Siege plating",
        "Seismic Slam that shatters the ground"
    };

    public override string PrimaryAttackName => "Piston Drill";
    public override string SecondaryAttackName => "Seismic Wave";
    public override string PrimaryAbilityName => "Siege Plating";
    public override string UltimateAttackName => "Seismic Slam";
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
        player.GetDamage<HeroDamage>() += 0.16f;
        player.GetAttackSpeed<HeroDamage>() += 0.1f;
        player.statDefense += 12;
        player.endurance += 0.06f;
        player.GetKnockback<HeroDamage>() += 0.6f;
        player.GetArmorPenetration<HeroDamage>() += 12;
        player.moveSpeed += 0.05f;
        player.noKnockback = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.2f;
        player.GetAttackSpeed<HeroDamage>() += 0.14f;
        player.statDefense += 16;
        player.endurance += 0.1f;
        player.GetArmorPenetration<HeroDamage>() += 14;
        player.moveSpeed -= 0.01f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.ultimateAttack) {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, UltimateAttack,
                (int)(damage * UltimateAttackModifier), knockback + 2f, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            int waveDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAttackModifier));
            Vector2 quakeOrigin = player.Bottom + new Vector2(direction.X * 12f, -8f);
            Projectile.NewProjectile(source, quakeOrigin, Vector2.Zero, SecondaryAttack, waveDamage,
                knockback + 1.2f, player.whoAmI, direction.X == 0f ? player.direction : System.Math.Sign(direction.X));
            return false;
        }

        Projectile.NewProjectile(source, player.Center + direction * 18f, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<ArmodrilloDrillProjectile>(), damage, knockback + 1f, player.whoAmI);
        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (projectile.type != PrimaryAttack && projectile.type != SecondaryAttack && projectile.type != UltimateAttack)
            return;

        if (projectile.type == SecondaryAttack)
            modifiers.ArmorPenetration += 10;

        if (!omp.PrimaryAbilityEnabled)
            return;

        modifiers.FinalDamage *= projectile.type == SecondaryAttack ? 1.18f : 1.12f;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MeteorHelmet;
        player.body = ArmorIDs.Body.MeteorSuit;
        player.legs = ArmorIDs.Legs.MeteorLeggings;
    }
}
