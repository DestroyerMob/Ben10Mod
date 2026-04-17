using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
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
    private const int ToxicSlipstreamDuration = 9 * 60;
    private const int ToxicSlipstreamCooldown = 18 * 60;
    private const int ToxicSlipstreamCost = 14;
    private const int CorrosiveBarrageEnergyCost = 22;
    private const int CorrosiveBarrageCooldown = 24 * 60;

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
        "Toxic Slipstream that echoes attacks with extra venom",
        "Corrosive Barrage that bursts into toxic droplets"
    };

    public override string PrimaryAttackName => "Slime Shot";
    public override string SecondaryAttackName => "Poison Spit";
    public override string PrimaryAbilityName => "Toxic Slipstream";
    public override string UltimateAttackName => "Corrosive Barrage";

    public override int PrimaryAttack => ModContent.ProjectileType<StinkFlySlowProjectile>();
    public override int PrimaryAttackSpeed => 15;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 1f;

    public override int SecondaryAttack => ModContent.ProjectileType<StinkFlyPoisonProjectile>();
    public override int SecondaryAttackSpeed => 18;
    public override int SecondaryShootSpeed => 21;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.18f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => ToxicSlipstreamDuration;
    public override int PrimaryAbilityCooldown => ToxicSlipstreamCooldown;
    public override int PrimaryAbilityCost => ToxicSlipstreamCost;

    public override int UltimateAttack => ModContent.ProjectileType<StinkFlyProjectile>();
    public override int UltimateAttackSpeed => 24;
    public override int UltimateShootSpeed => 15;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override float UltimateAttackModifier => 2.15f;
    public override int UltimateEnergyCost => CorrosiveBarrageEnergyCost;
    public override int UltimateAbilityCooldown => CorrosiveBarrageCooldown;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetAttackSpeed<HeroDamage>() += 0.1f;
        player.moveSpeed += 0.14f;
        player.maxRunSpeed += 1.35f;
        player.accRunSpeed += 1.1f;
        player.noFallDmg = true;
        player.wingTimeMax += 60;

        if (omp.PrimaryAbilityEnabled) {
            player.GetDamage<HeroDamage>() += 0.1f;
            player.GetAttackSpeed<HeroDamage>() += 0.12f;
            player.moveSpeed += 0.1f;
            player.wingTimeMax += 42;
            player.armorEffectDrawShadow = true;
        }

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

            if (omp.PrimaryAbilityEnabled) {
                int echoDamage = Math.Max(1, (int)Math.Round(finalDamage * 0.6f));
                Vector2 echoVelocity = direction.RotatedBy(player.direction * -0.14f) * (PrimaryShootSpeed + 1f);
                Projectile.NewProjectile(source, spawnPosition - direction.RotatedBy(MathHelper.PiOver2) * 8f, echoVelocity,
                    ModContent.ProjectileType<StinkFlySlowProjectile>(), echoDamage, knockback, player.whoAmI);
            }

            return false;
        }

        int slimeDamage = Math.Max(1, (int)Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<StinkFlySlowProjectile>(), slimeDamage, knockback, player.whoAmI);

        if (omp.PrimaryAbilityEnabled) {
            int echoDamage = Math.Max(1, (int)Math.Round(slimeDamage * 0.62f));
            Vector2 echoVelocity = direction.RotatedBy(player.direction * 0.12f) * (SecondaryShootSpeed + 1f);
            Projectile.NewProjectile(source, spawnPosition + direction.RotatedBy(MathHelper.PiOver2) * 8f, echoVelocity,
                ModContent.ProjectileType<StinkFlyPoisonProjectile>(), echoDamage, knockback + 0.2f, player.whoAmI);
        }

        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (projectile.type != PrimaryAttack && projectile.type != SecondaryAttack && projectile.type != UltimateAttack)
            return;

        int afflictedStates = 0;
        if (target.HasBuff(ModContent.BuffType<EnemySlow>()))
            afflictedStates++;
        if (target.HasBuff(BuffID.Poisoned))
            afflictedStates++;

        if (afflictedStates <= 0)
            return;

        modifiers.FinalDamage *= projectile.type == UltimateAttack
            ? 1f + afflictedStates * 0.13f
            : 1f + afflictedStates * 0.09f;

        if (afflictedStates >= 2 && projectile.type != PrimaryAttack)
            modifiers.ArmorPenetration += 10;
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
