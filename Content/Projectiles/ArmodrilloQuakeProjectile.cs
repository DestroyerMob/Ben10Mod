using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArmodrilloQuakeProjectile : ModProjectile {
    private const float MaxVisibleRange = 28f * 16f;
    private const float ShockwaveSpeed = 16.5f;
    private const float WaveLength = 56f;
    private const float StartWaveHeight = 18f;
    private const float EndWaveHeight = 68f;
    private const float StartCollisionWidth = 16f;
    private const float EndCollisionWidth = 28f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 30;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 9;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.localAI[1] = Projectile.Center.X;
            SpawnLaunchBurst();
        }

        Projectile.velocity = new Vector2(ShockwaveDirection * ShockwaveSpeed, 0f);
        Projectile.rotation = 0f;
        Projectile.spriteDirection = ShockwaveDirection > 0f ? 1 : -1;

        float progress = GetTravelProgress();
        GetWaveShape(progress, out float waveLength, out _, out _);
        Projectile.scale = MathHelper.Lerp(0.85f, 1.65f, progress);

        Lighting.AddLight(Projectile.Center, new Vector3(0.46f, 0.46f, 0.46f) * (0.25f + 0.2f * progress));
        SpawnShockwaveDust(progress);

        if (GetTravelDistance() + waveLength >= MaxVisibleRange)
            Projectile.Kill();
    }

    private float ShockwaveDirection => Projectile.ai[0] == 0f ? 1f : MathF.Sign(Projectile.ai[0]);

    private float GetTravelDistance() => MathF.Abs(Projectile.Center.X - Projectile.localAI[1]);

    private float GetTravelProgress() => MathHelper.Clamp(GetTravelDistance() / MaxVisibleRange, 0f, 1f);

    private Vector2 GetGroundOrigin() => Projectile.Bottom + new Vector2(0f, -4f);

    private void GetWaveShape(float progress, out float waveLength, out float waveHeight, out float collisionWidth) {
        waveLength = WaveLength;
        waveHeight = MathHelper.Lerp(StartWaveHeight, EndWaveHeight, progress);
        collisionWidth = MathHelper.Lerp(StartCollisionWidth, EndCollisionWidth, progress);
    }

    private void SpawnLaunchBurst() {
        Vector2 groundOrigin = GetGroundOrigin();

        for (int i = 0; i < 14; i++) {
            Vector2 burstVelocity = new Vector2(ShockwaveDirection * Main.rand.NextFloat(1.5f, 4f), Main.rand.NextFloat(-2.6f, -0.5f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(10f, 4f),
                i % 3 == 0 ? DustID.GemDiamond : DustID.Smoke, burstVelocity, 110, Color.White, Main.rand.NextFloat(1.1f, 1.55f));
            dust.noGravity = true;
        }
    }

    private void SpawnShockwaveDust(float progress) {
        GetWaveShape(progress, out float waveLength, out float waveHeight, out _);

        Vector2 groundOrigin = GetGroundOrigin();
        int pointCount = 11 + (int)MathF.Round(progress * 7f);
        float frameWave = Projectile.timeLeft * 0.22f;

        for (int layer = 0; layer < 2; layer++) {
            float layerLength = waveLength * (layer == 0 ? 1f : 0.72f);
            float layerHeight = waveHeight * (layer == 0 ? 1f : 0.68f);
            float layerScale = layer == 0 ? 1.12f : 0.9f;

            for (int i = 0; i < pointCount; i++) {
                float t = pointCount == 1 ? 0f : i / (pointCount - 1f);
                float forward = MathHelper.Lerp(0f, layerLength, t);
                float arcHeight = MathF.Sin(t * MathHelper.Pi) * layerHeight;
                float rippleOffset = MathF.Sin(frameWave + t * MathHelper.TwoPi * 2.2f + layer * 0.9f) * (1.6f + 3.2f * progress);
                Vector2 dustPosition = groundOrigin + new Vector2(ShockwaveDirection * forward, -arcHeight + rippleOffset);
                Vector2 dustVelocity = new Vector2(
                    ShockwaveDirection * MathHelper.Lerp(1.2f, 4.2f, t),
                    -MathHelper.Lerp(0.1f, 1.25f, t))
                    + Main.rand.NextVector2Circular(0.22f, 0.22f);

                int dustType = (i + layer) % 3 == 0 ? DustID.GemDiamond : DustID.Smoke;
                Color dustColor = Color.Lerp(new Color(190, 190, 190), Color.White, 0.45f + 0.55f * t);
                Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 115, dustColor, layerScale + progress * 0.35f);
                dust.noGravity = true;
            }
        }

        if (Main.rand.NextBool()) {
            Vector2 baseDustPosition = groundOrigin + new Vector2(ShockwaveDirection * Main.rand.NextFloat(0f, waveLength * 0.55f), Main.rand.NextFloat(-4f, 4f));
            Dust baseDust = Dust.NewDustPerfect(baseDustPosition, DustID.Smoke,
                new Vector2(ShockwaveDirection * Main.rand.NextFloat(0.6f, 1.8f), Main.rand.NextFloat(-0.45f, 0.15f)),
                130, new Color(220, 220, 220), Main.rand.NextFloat(1f, 1.35f));
            baseDust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float progress = GetTravelProgress();
        GetWaveShape(progress, out float waveLength, out float waveHeight, out float collisionWidth);

        Vector2 groundOrigin = GetGroundOrigin();
        Vector2 crestPoint = groundOrigin + new Vector2(ShockwaveDirection * waveLength, -waveHeight);
        Vector2 trailingPoint = groundOrigin + new Vector2(ShockwaveDirection * waveLength * 0.58f, -waveHeight * 0.38f);
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), groundOrigin, crestPoint, collisionWidth, ref collisionPoint)
            || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), trailingPoint, crestPoint, collisionWidth * 0.8f, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnKill(int timeLeft) {
        Vector2 groundOrigin = GetGroundOrigin();

        for (int i = 0; i < 12; i++) {
            Vector2 burstVelocity = new Vector2(ShockwaveDirection * Main.rand.NextFloat(0.8f, 3.2f), Main.rand.NextFloat(-2.1f, -0.35f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(12f, 6f),
                i % 2 == 0 ? DustID.GemDiamond : DustID.Smoke, burstVelocity, 120, Color.White, Main.rand.NextFloat(1f, 1.4f));
            dust.noGravity = true;
        }
    }
}
