using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArmodrilloQuakeProjectile : ModProjectile {
    public const int GroundedWaveFlag = 1;
    public const int SiegeWaveFlag = 2;
    public const int FaultLineWaveFlag = 4;

    private const int MaxChargeStacks = 3;
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
        Projectile.velocity *= GetSpeedScale();
        Projectile.rotation = 0f;
        Projectile.spriteDirection = ShockwaveDirection > 0f ? 1 : -1;

        float progress = GetTravelProgress();
        GetWaveShape(progress, out float waveLength, out _, out _);
        Projectile.scale = MathHelper.Lerp(GroundedWave ? 0.85f : 0.58f, GroundedWave ? 1.65f : 0.95f, progress) *
                           (1f + ChargeStacks * 0.07f + (SiegeWave ? 0.08f : 0f) + (FaultLineWave ? 0.14f : 0f));

        Lighting.AddLight(Projectile.Center, new Vector3(0.46f, 0.46f, 0.46f) *
                                              (0.18f + 0.24f * progress + (FaultLineWave ? 0.12f : 0f)));
        SpawnShockwaveDust(progress);

        if (GetTravelDistance() + waveLength >= EffectiveMaxRange)
            Projectile.Kill();
    }

    private float ShockwaveDirection => Projectile.ai[0] == 0f ? 1f : MathF.Sign(Projectile.ai[0]);

    private int WaveFlags => (int)MathF.Round(Projectile.ai[1]);

    private bool GroundedWave => (WaveFlags & GroundedWaveFlag) != 0;

    private bool SiegeWave => (WaveFlags & SiegeWaveFlag) != 0;

    private bool FaultLineWave => (WaveFlags & FaultLineWaveFlag) != 0;

    private int ChargeStacks => Math.Clamp((int)MathF.Round(Projectile.ai[2]), 0, MaxChargeStacks);

    private float EffectiveMaxRange {
        get {
            float range = MaxVisibleRange * (GroundedWave ? 1f : 0.52f);
            range *= 1f + ChargeStacks * 0.12f;
            if (GroundedWave && SiegeWave)
                range *= 1.16f;
            if (FaultLineWave)
                range *= 1.45f;

            return range;
        }
    }

    private float GetTravelDistance() => MathF.Abs(Projectile.Center.X - Projectile.localAI[1]);

    private float GetTravelProgress() => MathHelper.Clamp(GetTravelDistance() / EffectiveMaxRange, 0f, 1f);

    private Vector2 GetGroundOrigin() => Projectile.Bottom + new Vector2(0f, -4f);

    private void GetWaveShape(float progress, out float waveLength, out float waveHeight, out float collisionWidth) {
        float connectionScale = GroundedWave ? 1f : 0.54f;
        float setupScale = 1f + ChargeStacks * 0.1f;
        if (GroundedWave && SiegeWave)
            setupScale += 0.18f;
        if (FaultLineWave)
            setupScale += 0.36f;

        waveLength = WaveLength * connectionScale * setupScale;
        waveHeight = MathHelper.Lerp(StartWaveHeight, EndWaveHeight, progress) *
                     (GroundedWave ? 1f : 0.56f) *
                     (1f + ChargeStacks * 0.06f + (FaultLineWave ? 0.18f : 0f));
        collisionWidth = MathHelper.Lerp(StartCollisionWidth, EndCollisionWidth, progress) *
                         (GroundedWave ? 1f : 0.58f) *
                         (1f + ChargeStacks * 0.05f + (GroundedWave && SiegeWave ? 0.12f : 0f) +
                          (FaultLineWave ? 0.18f : 0f));
    }

    private float GetSpeedScale() {
        float speedScale = GroundedWave ? 1f : 0.72f;
        if (FaultLineWave)
            speedScale *= 0.9f;
        if (ChargeStacks > 0)
            speedScale *= 1f + ChargeStacks * 0.035f;

        return speedScale;
    }

    private void SpawnLaunchBurst() {
        Vector2 groundOrigin = GetGroundOrigin();
        int dustCount = GroundedWave ? 14 : 7;
        if (FaultLineWave)
            dustCount += 10;
        dustCount += ChargeStacks * 3;

        for (int i = 0; i < dustCount; i++) {
            Vector2 burstVelocity = new Vector2(ShockwaveDirection * Main.rand.NextFloat(1.5f, FaultLineWave ? 5.4f : 4f),
                Main.rand.NextFloat(FaultLineWave ? -3.4f : -2.6f, -0.5f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(10f, 4f),
                i % 3 == 0 ? DustID.GemDiamond : DustID.Smoke, burstVelocity, 110,
                FaultLineWave ? new Color(235, 225, 185) : Color.White,
                Main.rand.NextFloat(1.1f, FaultLineWave ? 1.85f : 1.55f));
            dust.noGravity = true;
        }
    }

    private void SpawnShockwaveDust(float progress) {
        GetWaveShape(progress, out float waveLength, out float waveHeight, out _);

        Vector2 groundOrigin = GetGroundOrigin();
        int pointCount = (GroundedWave ? 11 : 7) + ChargeStacks + (int)MathF.Round(progress * (FaultLineWave ? 11f : 7f));
        float frameWave = Projectile.timeLeft * 0.22f;

        for (int layer = 0; layer < 2; layer++) {
            if (!GroundedWave && layer == 1)
                continue;

            float layerLength = waveLength * (layer == 0 ? 1f : 0.72f);
            float layerHeight = waveHeight * (layer == 0 ? 1f : 0.68f);
            float layerScale = (layer == 0 ? 1.12f : 0.9f) + ChargeStacks * 0.04f + (FaultLineWave ? 0.18f : 0f);

            for (int i = 0; i < pointCount; i++) {
                float t = pointCount == 1 ? 0f : i / (pointCount - 1f);
                float forward = MathHelper.Lerp(0f, layerLength, t);
                float arcHeight = MathF.Sin(t * MathHelper.Pi) * layerHeight;
                float rippleOffset = MathF.Sin(frameWave + t * MathHelper.TwoPi * 2.2f + layer * 0.9f) *
                                     (1.2f + 3.2f * progress + ChargeStacks * 0.45f);
                Vector2 dustPosition = groundOrigin + new Vector2(ShockwaveDirection * forward, -arcHeight + rippleOffset);
                Vector2 dustVelocity = new Vector2(
                    ShockwaveDirection * MathHelper.Lerp(1.2f, 4.2f, t),
                    -MathHelper.Lerp(0.1f, 1.25f, t))
                    + Main.rand.NextVector2Circular(0.22f, 0.22f);

                int dustType = (i + layer) % 3 == 0 ? DustID.GemDiamond : DustID.Smoke;
                Color dustColor = FaultLineWave
                    ? Color.Lerp(new Color(170, 150, 95), new Color(245, 235, 190), 0.45f + 0.55f * t)
                    : Color.Lerp(new Color(190, 190, 190), Color.White, 0.45f + 0.55f * t);
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

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (!GroundedWave) {
            target.velocity.X += ShockwaveDirection * 0.9f;
            return;
        }

        int fractureTime = 100 + ChargeStacks * 35 + (SiegeWave ? 55 : 0) + (FaultLineWave ? 90 : 0);
        target.AddBuff(BuffID.BrokenArmor, fractureTime);
        if (SiegeWave || FaultLineWave)
            target.AddBuff(BuffID.Slow, Math.Max(50, fractureTime / 2));

        target.velocity.X += ShockwaveDirection *
                             (2.4f + ChargeStacks * 0.5f + (SiegeWave ? 0.9f : 0f) + (FaultLineWave ? 1.2f : 0f));
        if (!target.noGravity)
            target.velocity.Y -= 1.4f + ChargeStacks * 0.25f + (FaultLineWave ? 0.7f : 0f);
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
        int dustCount = GroundedWave ? 12 : 6;
        if (FaultLineWave)
            dustCount += 10;
        dustCount += ChargeStacks * 2;

        for (int i = 0; i < dustCount; i++) {
            Vector2 burstVelocity = new Vector2(ShockwaveDirection * Main.rand.NextFloat(0.8f, 3.2f), Main.rand.NextFloat(-2.1f, -0.35f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(12f, 6f),
                i % 2 == 0 ? DustID.GemDiamond : DustID.Smoke, burstVelocity, 120,
                FaultLineWave ? new Color(235, 225, 185) : Color.White, Main.rand.NextFloat(1f, FaultLineWave ? 1.75f : 1.4f));
            dust.noGravity = true;
        }
    }
}
