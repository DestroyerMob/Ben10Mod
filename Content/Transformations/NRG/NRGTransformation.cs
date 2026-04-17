using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.NRG;

public class NRGTransformation : Transformation {
    private const int BaseSecondaryAttackSpeed = 24;
    private const float BaseSecondaryShootSpeed = 10.5f;
    private const float BaseSecondaryDamageMultiplier = 0.9f;
    private const int ContainmentBurstEnergyCost = 30;
    private const int ContainmentBurstCooldown = 12 * 60;
    private const float ContainmentBurstDamageMultiplier = 2.25f;
    private const int UnboundCoreDuration = 45 * 60;
    private const int UnboundCoreCooldown = 70 * 60;
    private const int UnboundCoreCost = 72;
    private const int UnboundPrimaryAttackSpeed = 15;
    private const float UnboundPrimaryShootSpeed = 24f;
    private const int UnboundSecondaryAttackSpeed = 16;
    private const float UnboundSecondaryShootSpeed = 12.5f;
    private const float UnboundSecondaryDamageMultiplier = 1.08f;

    public override string FullID => "Ben10Mod:NRG";
    public override string TransformationName => "NRG";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<NRG_Buff>();

    public override string Description =>
        "A living reactor sealed in armor that burns targets with reactor fire, then detonates that heat into radiation blooms and unbound plasma pressure.";

    public override List<string> Abilities => new() {
        "Containment beam that sets targets ablaze",
        "Radiant seeker that blooms off burning enemies",
        "Containment heat burst",
        "Unbound reactor form",
        "Radiation fallout pressure"
    };

    public override string PrimaryAttackName => "Containment Beam";
    public override string SecondaryAttackName => "Radiant Seeker";
    public override string PrimaryAbilityAttackName => "Containment Burst";
    public override string UltimateAbilityName => "Unbound Core";
    public override int PrimaryAttack => ModContent.ProjectileType<NRGLaserProjectile>();
    public override int PrimaryAttackSpeed => 22;
    public override int PrimaryShootSpeed => 20;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<NRGHomingEnergyBallProjectile>();
    public override int SecondaryAttackSpeed => BaseSecondaryAttackSpeed;
    public override int SecondaryShootSpeed => (int)BaseSecondaryShootSpeed;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => BaseSecondaryDamageMultiplier;
    public override bool HasPrimaryAbility => false;
    public override int PrimaryAbilityCooldown => ContainmentBurstCooldown;
    public override int PrimaryAbilityAttack => ModContent.ProjectileType<NRGBurstProjectile>();
    public override int PrimaryAbilityAttackSpeed => 34;
    public override int PrimaryAbilityAttackShootSpeed => 0;
    public override int PrimaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float PrimaryAbilityAttackModifier => ContainmentBurstDamageMultiplier;
    public override int PrimaryAbilityAttackEnergyCost => ContainmentBurstEnergyCost;
    public override bool PrimaryAbilityAttackSingleUse => true;
    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityCost => UnboundCoreCost;
    public override int UltimateAbilityDuration => UnboundCoreDuration;
    public override int UltimateAbilityCooldown => UnboundCoreCooldown;
    public override int GetMoveSetIndex(OmnitrixPlayer omp) => omp.IsUltimateAbilityActive ? 1 : 0;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.16f;
        player.GetAttackSpeed<HeroDamage>() += 0.1f;
        player.statDefense += 18;
        player.endurance += 0.08f;
        player.GetKnockback<HeroDamage>() += 0.4f;
        player.GetArmorPenetration<HeroDamage>() += 8;
        player.moveSpeed += 0.05f;
        player.fireWalk = true;
        player.lavaImmune = true;
        player.noKnockback = true;
        if (omp.IsUltimateAbilityActive) {
            player.GetDamage<HeroDamage>() += 0.14f;
            player.GetAttackSpeed<HeroDamage>() += 0.12f;
            player.moveSpeed += 0.16f;
            player.maxRunSpeed += 1.8f;
            player.accRunSpeed += 1.35f;
            player.jumpSpeedBoost += 1.8f;
            player.noFallDmg = true;
            player.ignoreWater = true;
            Lighting.AddLight(player.Center, 1.25f, 0.28f, 0.08f);
        }

        if (omp.IsPrimaryAbilityAttackLoaded)
            Lighting.AddLight(player.Center, 1.15f, 0.35f, 0.08f);
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.IsUltimateAbilityActive)
            return;

        ApplyUnboundFlight(player);
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetPrimaryAttackProfiles() {
        return CreateMoveSetProfiles(
            CreatePrimaryAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = "Radiant Beam",
                ProjectileType = PrimaryAttack,
                DamageMultiplier = PrimaryAttackModifier,
                UseTime = UnboundPrimaryAttackSpeed,
                ShootSpeed = UnboundPrimaryShootSpeed,
                UseStyle = PrimaryUseStyle,
                Channel = PrimaryChannel,
                NoMelee = PrimaryNoMelee,
                ArmorPenetration = PrimaryArmorPenetration,
                EnergyCost = PrimaryEnergyCost
            }
        );
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetSecondaryAttackProfiles() {
        return CreateMoveSetProfiles(
            CreateSecondaryAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = SecondaryAttackDisplayName,
                ProjectileType = SecondaryAttack,
                DamageMultiplier = SecondaryAttackModifier,
                UseTime = UnboundSecondaryAttackSpeed,
                ShootSpeed = UnboundSecondaryShootSpeed,
                UseStyle = SecondaryUseStyle,
                Channel = SecondaryChannel,
                NoMelee = SecondaryNoMelee,
                ArmorPenetration = SecondaryArmorPenetration,
                EnergyCost = SecondaryEnergyCost
            }
        );
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        TransformationAttackProfile profile = GetSelectedAttackProfile(omp);
        if (profile == null || profile.ProjectileType <= 0)
            return false;

        if (omp.IsPrimaryAbilityAttackLoaded) {
            int burstDamage = System.Math.Max(1, (int)System.Math.Round(damage * ContainmentBurstDamageMultiplier));
            Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                ModContent.ProjectileType<NRGBurstProjectile>(), burstDamage,
                knockback + 2.5f, player.whoAmI);
            return false;
        }

        if (profile.ProjectileType == ModContent.ProjectileType<NRGHomingEnergyBallProjectile>()) {
            Projectile.NewProjectile(source, player.MountedCenter + direction * 14f, direction * profile.ShootSpeed,
                profile.ProjectileType,
                System.Math.Max(1, (int)System.Math.Round(damage * profile.DamageMultiplier)),
                knockback + 0.75f, player.whoAmI);
            return false;
        }

        Projectile.NewProjectile(source, player.Center + direction * 18f, direction * profile.ShootSpeed,
            profile.ProjectileType, damage, knockback + 0.5f, player.whoAmI);
        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (!IsNRGProjectile(projectile.type))
            return;

        if (!IsBurningTarget(target))
            return;

        modifiers.ArmorPenetration += projectile.type == PrimaryAbilityAttack || projectile.type == SecondaryAttack ? 12 : 6;
        modifiers.FinalDamage *= projectile.type switch {
            _ when projectile.type == PrimaryAbilityAttack => 1.25f,
            _ when projectile.type == SecondaryAttack => 1.18f,
            _ when projectile.type == ModContent.ProjectileType<NRGRadiationProjectile>() => 1.12f,
            _ => 1.1f
        };
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsNRGProjectile(projectile.type))
            return;

        bool wasBurning = IsBurningTarget(target);
        if (projectile.type != ModContent.ProjectileType<NRGRadiationProjectile>())
            target.AddBuff(BuffID.OnFire3, projectile.type == PrimaryAbilityAttack ? 300 : 240);

        if (!wasBurning || projectile.type == ModContent.ProjectileType<NRGRadiationProjectile>())
            return;

        int bloomCount = projectile.type == PrimaryAbilityAttack ? 4 : projectile.type == SecondaryAttack ? 2 : 1;
        float bloomRatio = projectile.type == PrimaryAbilityAttack ? 0.34f : projectile.type == SecondaryAttack ? 0.26f : 0.2f;
        SpawnRadiationBloom(player, projectile, target, bloomCount, bloomRatio);
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MoltenHelmet;
        player.body = ArmorIDs.Body.MoltenBreastplate;
        player.legs = ArmorIDs.Legs.MoltenGreaves;
    }

    private static void ApplyUnboundFlight(Player player) {
        if (player.controlJump || player.controlUp) {
            float ascentAcceleration = player.controlUp ? 0.65f : 0.46f;
            float maxRiseSpeed = player.controlUp ? -7.2f : -5.5f;
            player.velocity.Y = System.Math.Max(maxRiseSpeed, player.velocity.Y - ascentAcceleration);
        }
        else if (player.velocity.Y > -1.2f) {
            player.velocity.Y = System.Math.Min(player.velocity.Y, 3.2f);
        }

        if (player.controlDown)
            player.velocity.Y = System.Math.Min(player.velocity.Y + 0.36f, 9.5f);
        else if (player.velocity.Y > 0f)
            player.velocity.Y *= 0.9f;

        player.fallStart = (int)(player.position.Y / 16f);
        player.maxFallSpeed = 9.5f;
    }

    private static bool IsBurningTarget(NPC target) {
        return target.HasBuff(BuffID.OnFire) || target.HasBuff(BuffID.OnFire3) || target.HasBuff(BuffID.Burning);
    }

    private bool IsNRGProjectile(int projectileType) {
        return projectileType == PrimaryAttack
               || projectileType == SecondaryAttack
               || projectileType == PrimaryAbilityAttack
               || projectileType == ModContent.ProjectileType<NRGRadiationProjectile>();
    }

    private static void SpawnRadiationBloom(Player player, Projectile projectile, NPC target, int bloomCount, float damageRatio) {
        if (player.whoAmI != Main.myPlayer)
            return;

        int bloomDamage = System.Math.Max(1, (int)System.Math.Round(projectile.damage * damageRatio));
        for (int i = 0; i < bloomCount; i++) {
            Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4.5f, 8f);
            Projectile.NewProjectile(projectile.GetSource_FromThis(), target.Center, velocity,
                ModContent.ProjectileType<NRGRadiationProjectile>(), bloomDamage, projectile.knockBack * 0.7f,
                player.whoAmI);
        }
    }
}
