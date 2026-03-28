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
    private const int ContainmentBurstEnergyCost = 50;
    private const int ContainmentBurstCooldown = 18 * 60;
    private const float ContainmentBurstDamageMultiplier = 1.9f;
    private const int UnboundCoreDuration = 60 * 60;
    private const int UnboundCoreCooldown = 90 * 60;
    private const int UnboundCoreCost = 100;
    private const int UnboundPrimaryAttackSpeed = 18;
    private const float UnboundPrimaryShootSpeed = 22f;
    private const int UnboundSecondaryAttackSpeed = 20;
    private const float UnboundSecondaryShootSpeed = 11f;
    private const float UnboundSecondaryDamageMultiplier = 0.9f;

    public override string FullID => "Ben10Mod:NRG";
    public override string TransformationName => "NRG";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<NRG_Buff>();

    public override string Description =>
        "A living reactor sealed in armor that batters foes with radioactive blasts and vented energy bursts.";

    public override List<string> Abilities => new() {
        "Reactor laser shot",
        "Containment heat burst",
        "Unbound reactor form",
        "Homing energy spheres"
    };

    public override string PrimaryAttackName => "Containment Beam";
    public override string SecondaryAttackName => "Radiant Seeker";
    public override string PrimaryAbilityAttackName => "Containment Burst";
    public override string UltimateAbilityName => "Unbound Core";
    public override int PrimaryAttack => ModContent.ProjectileType<NRGLaserProjectile>();
    public override int PrimaryAttackSpeed => 28;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<NRGHomingEnergyBallProjectile>();
    public override int SecondaryAttackSpeed => UnboundSecondaryAttackSpeed;
    public override int SecondaryShootSpeed => (int)UnboundSecondaryShootSpeed;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => UnboundSecondaryDamageMultiplier;
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
        player.GetDamage<HeroDamage>() += 0.14f;
        player.statDefense += 16;
        player.endurance += 0.08f;
        player.GetKnockback<HeroDamage>() += 0.4f;
        player.fireWalk = true;
        player.lavaImmune = true;
        player.noKnockback = true;
        if (omp.IsUltimateAbilityActive) {
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
            CreateDisabledAttackProfile(SecondaryAttackDisplayName),
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
}
