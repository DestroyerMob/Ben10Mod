using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.FourArms;

public class FourArmsGroundSlamPlayer : ModPlayer {
    private const float FastFallAcceleration = 0.72f;
    private const float FastFallSpeedCap = 17.5f;
    private const float MinimumImpactSpeed = 8f;
    private const float ShockwaveDamageMultiplier = 0.8f;
    private const int FallbackBaseDamage = 28;

    private bool fourArmsActive;
    private bool slamArmed;
    private bool wasGrounded;
    private float lastFallSpeed;

    public override void Initialize() {
        wasGrounded = true;
    }

    public override void ResetEffects() {
        fourArmsActive = false;
    }

    public override void PostUpdate() {
        if (fourArmsActive)
            return;

        slamArmed = false;
        lastFallSpeed = 0f;
        wasGrounded = IsGrounded(Player);
    }

    public void UpdateFourArmsMovement() {
        fourArmsActive = true;

        bool grounded = IsGrounded(Player);
        bool falling = Player.velocity.Y > 0f;

        if (!grounded && falling)
            lastFallSpeed = Math.Max(lastFallSpeed, Player.velocity.Y);

        if (!grounded && Player.controlDown && falling) {
            Player.velocity.Y = Math.Min(Player.velocity.Y + FastFallAcceleration, FastFallSpeedCap);
            Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, FastFallSpeedCap);
            Player.fallStart = (int)(Player.position.Y / 16f);
            slamArmed = true;
        }
        else if (!grounded && !Player.controlDown) {
            slamArmed = false;
        }

        if (slamArmed && !wasGrounded && grounded && lastFallSpeed >= MinimumImpactSpeed) {
            SpawnLandingShockwave();
            slamArmed = false;
            lastFallSpeed = 0f;
        }
        else if (grounded && !Player.controlDown) {
            slamArmed = false;
            lastFallSpeed = 0f;
        }

        wasGrounded = grounded;
    }

    private void SpawnLandingShockwave() {
        if (Player.whoAmI != Main.myPlayer)
            return;

        int baseDamage = ResolveBaseDamage();
        int damage = Math.Max(1, (int)Math.Round(Player.GetDamage<HeroDamage>().ApplyTo(baseDamage * ShockwaveDamageMultiplier)));
        Vector2 spawnPosition = Player.Bottom + new Vector2(0f, -10f);
        Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPosition, Vector2.Zero,
            ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>(), damage, 6f, Player.whoAmI);
    }

    private int ResolveBaseDamage() {
        Item heldItem = Player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }

    private static bool IsGrounded(Player player) {
        return player.velocity.Y == 0f;
    }
}
