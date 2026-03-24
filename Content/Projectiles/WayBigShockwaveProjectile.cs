using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.WayBig;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WayBigShockwaveProjectile : ModProjectile {
    private const float MaxVisibleRange = 52f * 16f;
    private const float BaseShockwaveSpeed = 18f;
    private const float BaseWaveLength = 120f;
    private const float BaseStartWaveHeight = 26f;
    private const float BaseEndWaveHeight = 124f;
    private const float BaseStartCollisionWidth = 28f;
    private const float BaseEndCollisionWidth = 54f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = 100;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.localAI[1] = Projectile.Center.X;
            SpawnLaunchBurst();
        }

        float scaleFactor = GetScaleFactor();
        Projectile.velocity = new Vector2(ShockwaveDirection * BaseShockwaveSpeed * scaleFactor, 0f);
        Projectile.rotation = 0f;
        Projectile.spriteDirection = ShockwaveDirection > 0f ? 1 : -1;
        Projectile.scale = MathHelper.Lerp(0.95f, 1.95f, GetTravelProgress()) * scaleFactor;
        Projectile.GetGlobalProjectile<OmnitrixProjectile>().EnableScaleHitboxSync(Projectile);

        Lighting.AddLight(Projectile.Center, 0.18f, 0.7f, 0.78f);
        SpawnShockwaveDust(GetTravelProgress());

        GetWaveShape(GetTravelProgress(), out float waveLength, out _, out _);
        if (GetTravelDistance() + waveLength >= MaxVisibleRange * scaleFactor)
            Projectile.Kill();
    }

    private float ShockwaveDirection => Projectile.ai[0] == 0f ? 1f : MathF.Sign(Projectile.ai[0]);

    private float OwnerScale => Projectile.ai[1] <= 0f ? WayBigTransformation.BaseScale : Projectile.ai[1];

    private float GetScaleFactor() {
        return MathHelper.Clamp(OwnerScale / WayBigTransformation.BaseScale, 0.8f, 1.2f);
    }

    private float GetTravelDistance() => MathF.Abs(Projectile.Center.X - Projectile.localAI[1]);

    private float GetTravelProgress() {
        return MathHelper.Clamp(GetTravelDistance() / (MaxVisibleRange * GetScaleFactor()), 0f, 1f);
    }

    private Vector2 GetGroundOrigin() => Projectile.Bottom + new Vector2(0f, -6f);

    private void GetWaveShape(float progress, out float waveLength, out float waveHeight, out float collisionWidth) {
        float scaleFactor = GetScaleFactor();
        waveLength = BaseWaveLength * scaleFactor;
        waveHeight = MathHelper.Lerp(BaseStartWaveHeight, BaseEndWaveHeight, progress) * scaleFactor;
        collisionWidth = MathHelper.Lerp(BaseStartCollisionWidth, BaseEndCollisionWidth, progress) * scaleFactor;
    }

    private void SpawnLaunchBurst() {
        Vector2 groundOrigin = GetGroundOrigin();

        for (int i = 0; i < 18; i++) {
            Vector2 burstVelocity = new Vector2(ShockwaveDirection * Main.rand.NextFloat(1.8f, 4.8f),
                Main.rand.NextFloat(-3.6f, -0.8f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(14f, 6f),
                i % 3 == 0 ? DustID.GemSapphire : DustID.Smoke, burstVelocity, 110,
                new Color(180, 245, 255), Main.rand.NextFloat(1.15f, 1.65f));
            dust.noGravity = true;
        }
    }

    private void SpawnShockwaveDust(float progress) {
        GetWaveShape(progress, out float waveLength, out float waveHeight, out _);

        Vector2 groundOrigin = GetGroundOrigin();
        int pointCount = 14 + (int)MathF.Round(progress * 8f);
        float frameWave = Projectile.timeLeft * 0.2f;

        for (int layer = 0; layer < 2; layer++) {
            float layerLength = waveLength * (layer == 0 ? 1f : 0.72f);
            float layerHeight = waveHeight * (layer == 0 ? 1f : 0.7f);
            float layerScale = layer == 0 ? 1.2f : 0.95f;

            for (int i = 0; i < pointCount; i++) {
                float t = pointCount == 1 ? 0f : i / (pointCount - 1f);
                float forward = MathHelper.Lerp(0f, layerLength, t);
                float arcHeight = MathF.Sin(t * MathHelper.Pi) * layerHeight;
                float rippleOffset = MathF.Sin(frameWave + t * MathHelper.TwoPi * 2f + layer * 0.9f)
                    * (2f + 4f * progress);
                Vector2 dustPosition = groundOrigin + new Vector2(ShockwaveDirection * forward, -arcHeight + rippleOffset);
                Vector2 dustVelocity = new Vector2(
                    ShockwaveDirection * MathHelper.Lerp(1.4f, 4.8f, t),
                    -MathHelper.Lerp(0.1f, 1.5f, t)) + Main.rand.NextVector2Circular(0.25f, 0.25f);

                int dustType = (i + layer) % 4 == 0 ? DustID.GemDiamond : DustID.GemSapphire;
                Color dustColor = Color.Lerp(new Color(110, 225, 255), Color.White, 0.45f + 0.55f * t);
                Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 118, dustColor,
                    layerScale + progress * 0.4f);
                dust.noGravity = true;
            }
        }

        if (Main.rand.NextBool()) {
            Vector2 baseDustPosition = groundOrigin +
                                       new Vector2(ShockwaveDirection * Main.rand.NextFloat(0f, waveLength * 0.55f),
                                           Main.rand.NextFloat(-4f, 4f));
            Dust baseDust = Dust.NewDustPerfect(baseDustPosition, DustID.Smoke,
                new Vector2(ShockwaveDirection * Main.rand.NextFloat(0.7f, 2.2f), Main.rand.NextFloat(-0.55f, 0.15f)),
                130, new Color(210, 245, 255), Main.rand.NextFloat(1.05f, 1.45f));
            baseDust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float progress = GetTravelProgress();
        GetWaveShape(progress, out float waveLength, out float waveHeight, out float collisionWidth);

        Vector2 groundOrigin = GetGroundOrigin();
        Vector2 crestPoint = groundOrigin + new Vector2(ShockwaveDirection * waveLength, -waveHeight);
        Vector2 trailingPoint = groundOrigin + new Vector2(ShockwaveDirection * waveLength * 0.58f, -waveHeight * 0.35f);
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), groundOrigin, crestPoint,
                   collisionWidth, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), trailingPoint, crestPoint,
                   collisionWidth * 0.82f, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnKill(int timeLeft) {
        Vector2 groundOrigin = GetGroundOrigin();

        for (int i = 0; i < 14; i++) {
            Vector2 burstVelocity = new Vector2(ShockwaveDirection * Main.rand.NextFloat(1f, 3.8f),
                Main.rand.NextFloat(-2.4f, -0.35f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(16f, 8f),
                i % 2 == 0 ? DustID.GemDiamond : DustID.GemSapphire, burstVelocity, 120,
                new Color(200, 255, 255), Main.rand.NextFloat(1.05f, 1.45f));
            dust.noGravity = true;
        }
    }
}
