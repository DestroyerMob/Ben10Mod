using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.StinkFly;

public class StinkFlyTransformation : Transformation {
    private const int CorrosiveBarrageEnergyCost = 25;
    private const int CorrosiveBarrageCooldown = 26 * 60;

    public override string FullID => "Ben10Mod:StinkFly";
    public override string TransformationName => "Stinkfly";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<StinkFly_Buff>();

    public override string Description =>
        "A fast flier that controls space with sticky slime, corrosive spit, and a toxic finishing burst from the air.";

    public override List<string> Abilities => new() {
        "Sticky slime glob that gums enemies up",
        "Corrosive spit that poisons and punches through targets",
        "Passive flight",
        "Corrosive Barrage that bursts into toxic droplets"
    };

    public override string PrimaryAttackName => "Slime Shot";
    public override string SecondaryAttackName => "Poison Spit";
    public override string UltimateAttackName => "Corrosive Barrage";

    public override int PrimaryAttack => ModContent.ProjectileType<StinkFlySlowProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 17;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.9f;

    public override int SecondaryAttack => ModContent.ProjectileType<StinkFlyPoisonProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 20;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.05f;

    public override int UltimateAttack => ModContent.ProjectileType<StinkFlyProjectile>();
    public override int UltimateAttackSpeed => 32;
    public override int UltimateShootSpeed => 14;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override float UltimateAttackModifier => 1.8f;
    public override int UltimateEnergyCost => CorrosiveBarrageEnergyCost;
    public override int UltimateAbilityCooldown => CorrosiveBarrageCooldown;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.moveSpeed += 0.12f;
        player.maxRunSpeed += 1.2f;
        player.accRunSpeed += 1f;
        player.noFallDmg = true;
        player.wingTimeMax += 45;
        Lighting.AddLight(player.Center, new Vector3(0.08f, 0.16f, 0.05f));
        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<StinkFlyWings>());
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.Center + direction * 18f + new Vector2(0f, -player.height * 0.12f);

        if (omp.ultimateAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * UltimateShootSpeed,
                ModContent.ProjectileType<StinkFlyProjectile>(), finalDamage, knockback + 1f, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed,
                ModContent.ProjectileType<StinkFlyPoisonProjectile>(), finalDamage, knockback + 0.5f, player.whoAmI);
            return false;
        }

        int slimeDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<StinkFlySlowProjectile>(), slimeDamage, knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<StinkFly>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.waist = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Waist);
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
}
