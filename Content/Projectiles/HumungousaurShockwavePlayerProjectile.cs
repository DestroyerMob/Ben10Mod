using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Humungousaur;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HumungousaurShockwavePlayerProjectile : ModProjectile {
    private const float MaxVisibleRange = 24f * 16f;
    private const float BaseShockwaveSpeed = 12f;
    private const float BaseWaveLength = 62f;
    private const float BaseStartWaveHeight = 16f;
    private const float BaseEndWaveHeight = 56f;
    private const float BaseStartCollisionWidth = 18f;
    private const float BaseEndCollisionWidth = 32f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 34;
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
            Projectile.ai[2] = MathF.Max(MathF.Abs(Projectile.velocity.X), BaseShockwaveSpeed);
            SpawnLaunchBurst();
        }

        float direction = ShockwaveDirection;
        float speed = MathF.Max(Projectile.ai[2], BaseShockwaveSpeed) * GetSpeedScale();
        float progress = GetTravelProgress();

        Projectile.velocity = new Vector2(direction * speed, 0f);
        Projectile.rotation = 0f;
        Projectile.direction = direction > 0f ? 1 : -1;
        Projectile.spriteDirection = Projectile.direction;
        Projectile.scale = MathHelper.Lerp(0.9f, Variant >= 2f ? 1.7f : 1.48f, progress) * GetWaveScale();
        Projectile.GetGlobalProjectile<OmnitrixProjectile>().EnableScaleHitboxSync(Projectile);

        Color lightColor = Variant >= 2f ? new Color(255, 192, 120) : new Color(255, 132, 82);
        Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * (0.28f + 0.12f * progress));
        SpawnShockwaveDust(progress);

        GetWaveShape(progress, out float waveLength, out _, out _);
        if (GetTravelDistance() + waveLength >= MaxVisibleRange * GetRangeScale())
            Projectile.Kill();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float progress = GetTravelProgress();
        GetWaveShape(progress, out float waveLength, out float waveHeight, out float collisionWidth);

        Vector2 groundOrigin = GetGroundOrigin();
        Vector2 crestPoint = groundOrigin + new Vector2(ShockwaveDirection * waveLength, -waveHeight);
        Vector2 trailingPoint = groundOrigin + new Vector2(ShockwaveDirection * waveLength * 0.58f, -waveHeight * 0.36f);
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), groundOrigin, crestPoint,
                   collisionWidth, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), trailingPoint, crestPoint,
                   collisionWidth * 0.82f, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        float growthRatio = GetGrowthRatio();
        float pushStrength = 5.8f + growthRatio * 2.3f + (Variant >= 2f ? 1.2f : Variant >= 1f ? 0.6f : 0f);
        float liftStrength = 1.4f + growthRatio * 1.1f + (Variant >= 2f ? 0.8f : 0f);

        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + ShockwaveDirection * pushStrength, -16f, 16f),
            MathHelper.Clamp(target.velocity.Y - liftStrength, -10f, 10f));
        target.netUpdate = true;
    }

    public override void OnKill(int timeLeft) {
        Vector2 groundOrigin = GetGroundOrigin();
        for (int i = 0; i < 14; i++) {
            Vector2 burstVelocity = new(ShockwaveDirection * Main.rand.NextFloat(0.9f, 3.6f), Main.rand.NextFloat(-2.3f, -0.35f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(14f, 7f),
                i % 2 == 0 ? DustID.Torch : DustID.Smoke, burstVelocity, 118, GetDustColor(i / 14f),
                Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private float ShockwaveDirection {
        get {
            if (MathF.Abs(Projectile.velocity.X) > 0.01f)
                return MathF.Sign(Projectile.velocity.X);

            if (Projectile.ai[0] != 0f)
                return MathF.Sign(Projectile.ai[0]);

            return 1f;
        }
    }

    private float OwnerScale => Projectile.ai[0] == 0f
        ? 1f
        : MathHelper.Clamp(MathF.Abs(Projectile.ai[0]), 0.9f, HumungousaurTransformation.UltimateGrownScale);

    private float Variant => Projectile.ai[1];

    private float GetGrowthRatio() {
        return MathHelper.Clamp((OwnerScale - 1f) / (HumungousaurTransformation.UltimateGrownScale - 1f), 0f, 1f);
    }

    private float GetSpeedScale() {
        return MathHelper.Lerp(1f, 1.16f, GetGrowthRatio()) * (Variant >= 2f ? 1.08f : 1f);
    }

    private float GetWaveScale() {
        return MathHelper.Lerp(1f, 1.52f, GetGrowthRatio()) * (Variant >= 2f ? 1.14f : Variant >= 1f ? 1.06f : 1f);
    }

    private float GetRangeScale() {
        return MathHelper.Lerp(1f, 1.32f, GetGrowthRatio()) * (Variant >= 2f ? 1.12f : 1f);
    }

    private float GetTravelDistance() {
        return MathF.Abs(Projectile.Center.X - Projectile.localAI[1]);
    }

    private float GetTravelProgress() {
        return MathHelper.Clamp(GetTravelDistance() / (MaxVisibleRange * GetRangeScale()), 0f, 1f);
    }

    private Vector2 GetGroundOrigin() {
        return Projectile.Bottom + new Vector2(0f, -6f * GetWaveScale());
    }

    private void GetWaveShape(float progress, out float waveLength, out float waveHeight, out float collisionWidth) {
        float waveScale = GetWaveScale();
        waveLength = BaseWaveLength * waveScale;
        waveHeight = MathHelper.Lerp(BaseStartWaveHeight, BaseEndWaveHeight, progress) * waveScale;
        collisionWidth = MathHelper.Lerp(BaseStartCollisionWidth, BaseEndCollisionWidth, progress) * waveScale;
    }

    private void SpawnLaunchBurst() {
        Vector2 groundOrigin = GetGroundOrigin();
        for (int i = 0; i < 18; i++) {
            Vector2 burstVelocity = new(ShockwaveDirection * Main.rand.NextFloat(1.8f, 4.8f), Main.rand.NextFloat(-3.1f, -0.5f));
            int dustType = i % 3 == 0 ? DustID.Torch : DustID.Smoke;
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(12f, 5f), dustType,
                burstVelocity, 108, GetDustColor(i / 18f), Main.rand.NextFloat(1.05f, 1.55f));
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
                float rippleOffset = MathF.Sin(frameWave + t * MathHelper.TwoPi * 2.2f + layer * 0.86f) *
                                     (1.6f + 3.2f * progress);
                Vector2 dustPosition = groundOrigin + new Vector2(ShockwaveDirection * forward, -arcHeight + rippleOffset);
                Vector2 dustVelocity = new(
                    ShockwaveDirection * MathHelper.Lerp(1.2f, 4.3f, t),
                    -MathHelper.Lerp(0.1f, 1.2f, t));
                dustVelocity += Main.rand.NextVector2Circular(0.22f, 0.22f);

                int dustType = (i + layer) % 3 == 0 ? DustID.Torch : DustID.Smoke;
                Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 112, GetDustColor(t),
                    layerScale + progress * 0.32f);
                dust.noGravity = true;
            }
        }

        if (Main.rand.NextBool()) {
            Vector2 baseDustPosition = groundOrigin +
                                       new Vector2(ShockwaveDirection * Main.rand.NextFloat(0f, waveLength * 0.55f),
                                           Main.rand.NextFloat(-4f, 4f));
            Dust baseDust = Dust.NewDustPerfect(baseDustPosition, DustID.Smoke,
                new Vector2(ShockwaveDirection * Main.rand.NextFloat(0.7f, 2f), Main.rand.NextFloat(-0.45f, 0.15f)),
                130, new Color(218, 202, 190), Main.rand.NextFloat(1f, 1.35f));
            baseDust.noGravity = true;
        }
    }

    private Color GetDustColor(float t) {
        if (Variant >= 2f)
            return Color.Lerp(new Color(255, 146, 70), new Color(255, 234, 178), MathHelper.Clamp(t, 0f, 1f));

        return Color.Lerp(new Color(192, 158, 132), new Color(255, 172, 104), MathHelper.Clamp(t, 0f, 1f));
    }
}
