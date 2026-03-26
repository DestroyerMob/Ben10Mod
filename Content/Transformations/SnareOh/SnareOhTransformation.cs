using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.SnareOh;

public class SnareOhTransformation : Transformation {
    private const int BurialBindEnergyCost = 30;
    private const int BurialBindCooldown = 18 * 60;

    public override string FullID => "Ben10Mod:SnareOh";
    public override string TransformationName => "Snare-Oh";
    public override int TransformationBuffId => ModContent.BuffType<SnareOh_Buff>();

    public override string Description =>
        "A Thep Khufan who dominates fights with living bandages, locking enemies in place before exposing his cursed core and flooding the area around him with weakening radiation.";

    public override List<string> Abilities => new() {
        "Bandage lash primary",
        "Constricting wrap secondary",
        "Expose core weaken stance",
        "Burial bind prison",
        "Irradiated core aura ultimate"
    };

    public override string PrimaryAttackName => "Bandage Lash";
    public override string SecondaryAttackName => "Constricting Wrap";
    public override string SecondaryAbilityAttackName => "Burial Bind";
    public override string UltimateAttackName => "Irradiated Core";

    public override int PrimaryAttack => ModContent.ProjectileType<SnareOhBandageProjectile>();
    public override int PrimaryAttackSpeed => 16;
    public override int PrimaryShootSpeed => 16;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.86f;

    public override int SecondaryAttack => ModContent.ProjectileType<SnareOhWrapProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 11;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.18f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 9 * 60;
    public override int PrimaryAbilityCooldown => 28 * 60;
    public override int PrimaryAbilityCost => 20;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<SnareOhBindFieldProjectile>();
    public override int SecondaryAbilityAttackSpeed => 20;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 0.95f;
    public override int SecondaryAbilityAttackEnergyCost => BurialBindEnergyCost;
    public override int SecondaryAbilityCooldown => BurialBindCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityCost => 60;
    public override int UltimateAbilityDuration => 10 * 60;
    public override int UltimateAbilityCooldown => 55 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.statDefense += 6;
        player.endurance += 0.04f;
        player.moveSpeed += 0.08f;
        player.maxRunSpeed += 0.6f;
        player.noFallDmg = true;

        if (omp.PrimaryAbilityEnabled) {
            player.GetDamage<HeroDamage>() += 0.14f;
            player.GetAttackSpeed<HeroDamage>() += 0.1f;
            player.GetCritChance<HeroDamage>() += 4f;
            player.statDefense -= 6;
            player.moveSpeed += 0.08f;
            player.maxRunSpeed += 0.4f;
            player.armorEffectDrawShadow = true;
            Lighting.AddLight(player.Center, new Vector3(0.62f, 0.46f, 0.18f));
        }

        if (!omp.IsUltimateAbilityActive)
            return;

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetArmorPenetration<HeroDamage>() += 10;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.statDefense -= 4;
        player.moveSpeed += 0.12f;
        player.maxRunSpeed += 0.8f;
        player.endurance += 0.03f;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.38f, 0.72f, 0.18f));
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.IsUltimateAbilityActive || player.whoAmI != Main.myPlayer)
            return;

        int projectileType = ModContent.ProjectileType<SnareOhUltimateProjectile>();
        int auraDamage = System.Math.Max(1,
            (int)System.Math.Round(player.GetDamage<HeroDamage>().ApplyTo(34)));
        int existingAura = FindOwnedProjectile(player.whoAmI, projectileType);

        if (existingAura >= 0) {
            Projectile aura = Main.projectile[existingAura];
            aura.ai[1] = omp.PrimaryAbilityEnabled ? 1f : 0f;
            aura.damage = auraDamage;
            aura.originalDamage = auraDamage;
            aura.Center = player.Center;
            aura.timeLeft = 2;
            aura.netUpdate = true;
            return;
        }

        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
            projectileType, auraDamage, 1.5f, player.whoAmI, 0f, omp.PrimaryAbilityEnabled ? 1f : 0f);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.IsSecondaryAbilityAttackLoaded) {
            Vector2 bindCenter = player.Center + direction * 94f;
            Projectile.NewProjectile(source, bindCenter, Vector2.Zero,
                ModContent.ProjectileType<SnareOhBindFieldProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            int wrapDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, player.Center + direction * 12f, direction * SecondaryShootSpeed,
                ModContent.ProjectileType<SnareOhWrapProjectile>(), wrapDamage, knockback + 1.2f, player.whoAmI);
            return false;
        }

        Projectile.NewProjectile(source, player.Center + direction * 10f, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<SnareOhBandageProjectile>(), damage, knockback, player.whoAmI);
        return false;
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

    private static int FindOwnedProjectile(int owner, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == owner && projectile.type == projectileType)
                return i;
        }

        return -1;
    }
}
