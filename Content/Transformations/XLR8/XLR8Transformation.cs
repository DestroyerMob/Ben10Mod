using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.XLR8;

public class XLR8Transformation : Transformation {
    private const int VectorDashEnergyCost = 18;
    private const int VectorDashCooldown = 14 * 60;
    private const float BaseAttackUseTimeMultiplier = 0.88f;
    private const float OverdriveAttackUseTimeMultiplier = 0.72f;
    private const float BaseVectorDashRange = 420f;
    private const float OverdriveVectorDashRange = 600f;
    private const int PrimaryStrikeBurstCount = 5;
    private const int PrimaryStrikeSpacing = 2;

    public override string FullID                  => "Ben10Mod:XLR8";
    public override string TransformationName      => "XLR8";
    public override string IconPath                => "Ben10Mod/Content/Interface/XLR8Select";
    public override int    TransformationBuffId    => ModContent.BuffType<XLR8_Buff>();
    public override string Description =>
        "A Kineceleran speedster built to blur across the battlefield, hammer enemies with light-speed jabs, carve through crowds with dashes, and distort the pace of combat.";

    public override List<string> Abilities => new() {
        "Rapid light-strikes fired at close range",
        "Velocity dash that tears through a line",
        "Extreme speed overdrive",
        "Vector Dash to the cursor",
        "Water running at speed",
        "Time-slowing field that freezes the battlefield"
    };

    public override string PrimaryAttackName       => "Speed Strike";
    public override string SecondaryAttackName     => "Velocity Dash";
    public override string PrimaryAbilityName      => "Overdrive";
    public override string SecondaryAbilityAttackName => "Vector Dash";
    public override string UltimateAbilityName     => "Temporal Distortion";
    public override int    PrimaryAbilityDuration  => 10 * 60;
    public override int    PrimaryAbilityCooldown  => 30 * 60;
    public override int    PrimaryAttack           => ModContent.ProjectileType<XLR8StarlightProjectile>();
    public override int    PrimaryAttackSpeed      => 10;
    public override int    PrimaryShootSpeed       => 20;
    public override int    PrimaryUseStyle         => ItemUseStyleID.Shoot;
    public override float  PrimaryAttackModifier   => 0.5f;
    public override int    SecondaryAttack         => ModContent.ProjectileType<XLR8DashProjectile>();
    public override int    SecondaryAttackSpeed    => 82;
    public override int    SecondaryShootSpeed     => 14;
    public override int    SecondaryUseStyle       => ItemUseStyleID.Shoot;
    public override float  SecondaryAttackModifier => 1.35f;
    public override int    SecondaryAbilityAttack => ModContent.ProjectileType<XLR8VectorDashProjectile>();
    public override int    SecondaryAbilityAttackSpeed => 16;
    public override int    SecondaryAbilityAttackShootSpeed => 0;
    public override int    SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float  SecondaryAbilityAttackModifier => 1.18f;
    public override int    SecondaryAbilityAttackEnergyCost => VectorDashEnergyCost;
    public override int    SecondaryAbilityCooldown => VectorDashCooldown;
    public override bool   SecondaryAbilityAttackSingleUse => true;
    public override bool   HasUltimateAbility      => true;
    public override int    UltimateAbilityCost     => 100;
    public override int    UltimateAbilityDuration => 4 * 60;
    public override int    UltimateAbilityCooldown => 60 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        
        player.moveSpeed *= omp.PrimaryAbilityEnabled ? 5.2f : 2.7f;
        player.accRunSpeed *= omp.PrimaryAbilityEnabled ? 4.3f : 2.2f;
        player.runAcceleration *= omp.PrimaryAbilityEnabled ? 2.15f : 1.45f;
        player.maxRunSpeed += omp.PrimaryAbilityEnabled ? 2.2f : 1.1f;
        player.GetAttackSpeed(DamageClass.Generic) += omp.PrimaryAbilityEnabled ? 1.25f : 0.75f;
        player.GetCritChance<HeroDamage>() += omp.PrimaryAbilityEnabled ? 16f : 8f;
        player.pickSpeed *= omp.PrimaryAbilityEnabled ? 0.45f : 0.65f;
        player.tileSpeed *= omp.PrimaryAbilityEnabled ? 0.45f : 0.65f;
        player.wallSpeed *= omp.PrimaryAbilityEnabled ? 0.45f : 0.65f;
        player.jumpSpeedBoost += omp.PrimaryAbilityEnabled ? 3f : 1.6f;
        if (Math.Abs(player.velocity.X) > 2) {
            player.waterWalk =  true;
        }
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        float speedMultiplier = omp.PrimaryAbilityEnabled
            ? OverdriveAttackUseTimeMultiplier
            : BaseAttackUseTimeMultiplier;
        bool firingPrimary = !omp.altAttack && !omp.IsSecondaryAbilityAttackLoaded && !omp.ultimateAttack;
        int minUseTime = firingPrimary ? 7 : 6;

        item.useTime = item.useAnimation = Math.Max(minUseTime, (int)Math.Round(item.useTime * speedMultiplier));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (!omp.altAttack && !omp.IsSecondaryAbilityAttackLoaded) {
            if (HasActiveOwnedProjectile(player, ModContent.ProjectileType<XLR8StarlightProjectile>()))
                return false;

            Vector2 attackDirection = ResolveAimDirection(player, velocity);
            float empoweredFlag = omp.PrimaryAbilityEnabled ? 1f : 0f;

            for (int i = 0; i < PrimaryStrikeBurstCount; i++) {
                Vector2 burstDirection = attackDirection.RotatedBy(Main.rand.NextFloat(-0.085f, 0.085f));
                omp.transformationAttackSerial++;
                int burstProjectileIndex = Projectile.NewProjectile(source,
                    player.MountedCenter + burstDirection * 12f,
                    burstDirection * PrimaryShootSpeed,
                    ModContent.ProjectileType<XLR8StarlightProjectile>(),
                    damage,
                    knockback,
                    player.whoAmI,
                    empoweredFlag,
                    omp.transformationAttackSerial,
                    i * PrimaryStrikeSpacing);

                if (burstProjectileIndex >= 0 && burstProjectileIndex < Main.maxProjectiles)
                    Main.projectile[burstProjectileIndex].netUpdate = true;
            }

            return false;
        }

        if (!omp.IsSecondaryAbilityAttackLoaded)
            return base.Shoot(player, omp, source, position, velocity, damage, knockback);

        if (Main.netMode == NetmodeID.Server ||
            (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
            return false;

        Vector2 destination = Main.MouseWorld;
        Vector2 offset = destination - player.MountedCenter;
        if (offset == Vector2.Zero)
            offset = new Vector2(player.direction, 0f);

        float maxRange = omp.PrimaryAbilityEnabled ? OverdriveVectorDashRange : BaseVectorDashRange;
        float requestedDistance = Math.Min(offset.Length(), maxRange);
        Vector2 direction = offset.SafeNormalize(new Vector2(player.direction, 0f));
        bool empowered = omp.PrimaryAbilityEnabled;
        float dashSpeed = XLR8VectorDashProjectile.GetDashSpeed(empowered);
        int dashFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / dashSpeed),
            XLR8VectorDashProjectile.MinDashFrames, XLR8VectorDashProjectile.MaxDashFrames);
        int dashDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));

        int projectileIndex = Projectile.NewProjectile(source, player.MountedCenter + direction * 18f, direction * dashSpeed,
            SecondaryAbilityAttack, dashDamage, knockback + 1f, player.whoAmI, empowered ? 1f : 0f);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Projectile projectile = Main.projectile[projectileIndex];
            projectile.timeLeft = dashFrames;
            projectile.netUpdate = true;
        }

        if (!Main.dedServ) {
            int dustCount = empowered ? 22 : 16;
            for (int i = 0; i < dustCount; i++) {
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(12f, 18f), DustID.BlueCrystalShard,
                    direction.RotatedByRandom(0.45f) * Main.rand.NextFloat(1.8f, 4.8f), 110,
                    new Color(120, 210, 255), Main.rand.NextFloat(1f, 1.25f));
                dust.noGravity = true;
            }
        }

        SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f, Volume = 0.82f }, player.Center);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<XLR8>();
        player.armorEffectDrawShadow = true;
        player.head                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        if (omp.PrimaryAbilityEnabled)
            player.head = EquipLoader.GetEquipSlot(Mod, "XLR8_alt", EquipType.Head);
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

    private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
                return true;
        }

        return false;
    }
    
    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => [
        new TransformationPaletteChannel(
            "base",
            "Base",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Head",
                "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Head_alt",
                "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Head_alt"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Body",
                "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Body"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Legs",
                "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Legs"),
        new TransformationPaletteOverlay(
            "Ben10Mod/Content/Transformations/XLR8/XLR8_Tail",
            "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Tail")),
        new TransformationPaletteChannel(
            "eye",
            "Eye",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Head",
                "Ben10Mod/Content/Transformations/XLR8/XLR8EyeMask_Head"))
    ];
}
