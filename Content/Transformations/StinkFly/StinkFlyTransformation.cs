using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Players;
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
    private const float GroundedPrimaryDamageModifier = 0.72f;
    private const float AirbornePrimaryDamageModifier = 1.18f;
    private const float GroundedSecondaryDamageModifier = 0.9f;
    private const float AirborneSecondaryDamageModifier = 1.24f;
    private const float SlipstreamTrailSpeedThreshold = 5.8f;
    private const int SlipstreamTrailSpawnInterval = 10;
    private const float SlipstreamTrailDamageMultiplier = 0.34f;
    private const int FallbackBaseDamage = 18;

    public override string FullID => "Ben10Mod:StinkFly";
    public override string TransformationName => "Stinkfly";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<StinkFly_Buff>();

    public override string Description =>
        "An airborne skirmisher that scales venom pressure by staying fast, evasive, and off the ground.";

    public override List<string> Abilities => new() {
        "Slime Shot is weak on the ground but sharper while airborne",
        "Poison Spit chains only from slimed targets",
        "Passive flight",
        "Toxic Slipstream echoes attacks and leaves trails while flying fast",
        "Corrosive Barrage rains more toxic droplets from higher flight speed"
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

        player.noFallDmg = true;
        player.wingTimeMax += 60;

        bool airborne = IsAirborne(player);
        float aerialPressure = GetAerialPressure(player);

        if (airborne) {
            player.GetDamage<HeroDamage>() += MathHelper.Lerp(0.04f, 0.16f, aerialPressure);
            player.GetCritChance<HeroDamage>() += MathHelper.Lerp(3f, 10f, aerialPressure);
            player.GetAttackSpeed<HeroDamage>() += MathHelper.Lerp(0.04f, 0.14f, aerialPressure);
            player.moveSpeed += MathHelper.Lerp(0.06f, 0.18f, aerialPressure);
            player.maxRunSpeed += MathHelper.Lerp(0.75f, 1.8f, aerialPressure);
            player.accRunSpeed += MathHelper.Lerp(0.65f, 1.4f, aerialPressure);
            player.wingTimeMax += (int)Math.Round(MathHelper.Lerp(24f, 84f, aerialPressure));
        }

        if (omp.PrimaryAbilityEnabled && airborne) {
            player.GetDamage<HeroDamage>() += MathHelper.Lerp(0.04f, 0.12f, aerialPressure);
            player.GetAttackSpeed<HeroDamage>() += MathHelper.Lerp(0.04f, 0.12f, aerialPressure);
            player.moveSpeed += MathHelper.Lerp(0.04f, 0.12f, aerialPressure);
            player.wingTimeMax += (int)Math.Round(MathHelper.Lerp(18f, 48f, aerialPressure));
            player.armorEffectDrawShadow = true;
            TrySpawnSlipstreamTrail(player, aerialPressure);
        }

        Lighting.AddLight(player.Center, new Vector3(0.08f, 0.16f, 0.05f));
        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<StinkFlyWings>());
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.Center + direction * 18f + new Vector2(0f, -player.height * 0.12f);
        bool airborne = IsAirborne(player);
        float aerialPressure = GetAerialPressure(player);
        float speedPressure = airborne ? GetFlightSpeedPressure(player) : 0f;

        if (omp.ultimateAttack) {
            float ultimateModifier = airborne
                ? UltimateAttackModifier * MathHelper.Lerp(0.95f, 1.25f, aerialPressure)
                : UltimateAttackModifier * 0.82f;
            int finalDamage = Math.Max(1, (int)Math.Round(damage * ultimateModifier));
            Vector2 launchDirection = airborne
                ? Vector2.Lerp(direction, new Vector2(player.direction * 0.25f, 1f).SafeNormalize(Vector2.UnitY), 0.4f)
                    .SafeNormalize(direction)
                : direction;
            float launchSpeed = UltimateShootSpeed + MathHelper.Lerp(0f, 5f, speedPressure);
            Projectile.NewProjectile(source, spawnPosition, launchDirection * launchSpeed,
                ModContent.ProjectileType<StinkFlyProjectile>(), finalDamage, knockback + 1f, player.whoAmI, speedPressure);
            return false;
        }

        if (omp.altAttack) {
            float secondaryModifier = airborne
                ? MathHelper.Lerp(SecondaryAttackModifier, AirborneSecondaryDamageModifier, aerialPressure)
                : GroundedSecondaryDamageModifier;
            int finalDamage = Math.Max(1, (int)Math.Round(damage * secondaryModifier));
            float shootSpeed = airborne
                ? SecondaryShootSpeed + MathHelper.Lerp(0f, 4f, aerialPressure)
                : SecondaryShootSpeed * 0.85f;
            Projectile.NewProjectile(source, spawnPosition, direction * shootSpeed,
                ModContent.ProjectileType<StinkFlyPoisonProjectile>(), finalDamage, knockback + 0.5f, player.whoAmI);

            if (omp.PrimaryAbilityEnabled && airborne) {
                int echoDamage = Math.Max(1, (int)Math.Round(finalDamage * 0.6f));
                Vector2 echoVelocity = direction.RotatedBy(player.direction * -0.14f) * (PrimaryShootSpeed + 1f);
                Projectile.NewProjectile(source, spawnPosition - direction.RotatedBy(MathHelper.PiOver2) * 8f, echoVelocity,
                    ModContent.ProjectileType<StinkFlySlowProjectile>(), echoDamage, knockback, player.whoAmI);
            }

            return false;
        }

        float primaryModifier = airborne
            ? MathHelper.Lerp(PrimaryAttackModifier, AirbornePrimaryDamageModifier, aerialPressure)
            : GroundedPrimaryDamageModifier;
        int slimeDamage = Math.Max(1, (int)Math.Round(damage * primaryModifier));
        float primaryShootSpeed = airborne
            ? PrimaryShootSpeed + MathHelper.Lerp(0f, 4f, aerialPressure)
            : PrimaryShootSpeed * 0.72f;
        Projectile.NewProjectile(source, spawnPosition, direction * primaryShootSpeed,
            ModContent.ProjectileType<StinkFlySlowProjectile>(), slimeDamage, knockback, player.whoAmI);

        if (omp.PrimaryAbilityEnabled && airborne) {
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

        if (IsAirborne(player))
            modifiers.FinalDamage *= 1f + GetAerialPressure(player) * 0.08f;

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

    private static bool IsAirborne(Player player) {
        if (!AlienIdentityPlayer.IsGrounded(player))
            return true;

        return Math.Abs(player.velocity.Y) > 0.35f;
    }

    private static float GetAerialPressure(Player player) {
        if (!IsAirborne(player))
            return 0f;

        float speedPressure = GetFlightSpeedPressure(player);
        float fallPressure = MathHelper.Clamp(player.velocity.Y / 8f, 0f, 1f);
        return MathHelper.Clamp(0.45f + speedPressure * 0.45f + fallPressure * 0.2f, 0f, 1f);
    }

    private static float GetFlightSpeedPressure(Player player) {
        return MathHelper.Clamp(player.velocity.Length() / 11f, 0f, 1f);
    }

    private static void TrySpawnSlipstreamTrail(Player player, float aerialPressure) {
        if (player.whoAmI != Main.myPlayer || player.velocity.Length() < SlipstreamTrailSpeedThreshold)
            return;

        if ((Main.GameUpdateCount + (ulong)player.whoAmI) % SlipstreamTrailSpawnInterval != 0)
            return;

        Vector2 moveDirection = player.velocity.SafeNormalize(new Vector2(player.direction, 0f));
        Vector2 spawnPosition = player.Center - moveDirection * 18f + Main.rand.NextVector2Circular(5f, 5f);
        Vector2 trailVelocity = -moveDirection * 0.35f + new Vector2(0f, 0.25f);
        int damage = ResolveHeroDamage(player, SlipstreamTrailDamageMultiplier * MathHelper.Lerp(0.8f, 1.25f, aerialPressure));
        int projectileIndex = Projectile.NewProjectile(player.GetSource_FromThis(), spawnPosition, trailVelocity,
            ModContent.ProjectileType<StinkFlyToxicTrailProjectile>(), damage, 0.2f, player.whoAmI, aerialPressure);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;
    }

    private static int ResolveHeroDamage(Player player, float multiplier) {
        float baseDamage = ResolveBaseDamage(player) * multiplier;
        return Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(baseDamage)));
    }

    private static int ResolveBaseDamage(Player player) {
        Item heldItem = player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }
}
