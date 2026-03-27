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

    public override string FullID                  => "Ben10Mod:XLR8";
    public override string TransformationName      => "XLR8";
    public override string IconPath                => "Ben10Mod/Content/Interface/XLR8Select";
    public override int    TransformationBuffId    => ModContent.BuffType<XLR8_Buff>();
    public override string Description =>
        "A Kineceleran speedster built to blur across the battlefield, chain rapid strikes, slash through crowds with dashing attacks, and distort the pace of combat.";

    public override List<string> Abilities => new() {
        "Rapid strike rush",
        "Piercing velocity dash",
        "Extreme speed boost",
        "Targeted vector dash",
        "Water running at speed",
        "Time-slowing ultimate field"
    };

    public override string PrimaryAttackName       => "Speed Strike";
    public override string SecondaryAttackName     => "Velocity Dash";
    public override string SecondaryAbilityAttackName => "Vector Dash";
    public override int    PrimaryAbilityDuration  => 10 * 60;
    public override int    PrimaryAbilityCooldown  => 30 * 60;
    public override int    PrimaryAttack           => ModContent.ProjectileType<XLR8PunchProjectile>();
    public override int    PrimaryAttackSpeed      => 11;
    public override int    PrimaryShootSpeed       => 30;
    public override float  PrimaryAttackModifier   => 0.75f;
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
        player.GetAttackSpeed<HeroDamage>() += omp.PrimaryAbilityEnabled ? 0.44f : 0.26f;
        player.GetCritChance<HeroDamage>() += omp.PrimaryAbilityEnabled ? 16f : 8f;
        player.pickSpeed *= omp.PrimaryAbilityEnabled ? 0.45f : 0.65f;
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

        item.useTime = item.useAnimation = Math.Max(6, (int)Math.Round(item.useTime * speedMultiplier));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
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
}
