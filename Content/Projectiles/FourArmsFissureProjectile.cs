using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsFissureProjectile : ModProjectile {
    private const float MaxVisibleRange = 18f * 16f;
    private const float ShockwaveSpeed = 14f;
    private const float WaveLength = 62f;
    private const float StartWaveHeight = 22f;
    private const float EndWaveHeight = 52f;
    private const float StartCollisionWidth = 18f;
    private const float EndCollisionWidth = 30f;

    private bool PotisInfused => Projectile.ai[1] >= 0.5f;
    private float MaxRange => MaxVisibleRange * (PotisInfused ? 1.35f : 1f);
    private float CurrentShockwaveSpeed => ShockwaveSpeed * (PotisInfused ? 1.12f : 1f);
    private float CurrentWaveLength => WaveLength * (PotisInfused ? 1.22f : 1f);

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
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.localAI[1] = Projectile.Center.X;
            SpawnLaunchBurst();
        }

        Projectile.velocity = new Vector2(ShockwaveDirection * CurrentShockwaveSpeed, 0f);
        Projectile.rotation = 0f;
        Projectile.spriteDirection = ShockwaveDirection > 0f ? 1 : -1;

        float progress = GetTravelProgress();
        Projectile.scale = MathHelper.Lerp(PotisInfused ? 1.08f : 0.9f, PotisInfused ? 1.78f : 1.45f, progress);

        Lighting.AddLight(Projectile.Center,
            (PotisInfused ? new Vector3(0.95f, 0.38f, 0.1f) : new Vector3(0.55f, 0.2f, 0.08f)) *
            (0.28f + (PotisInfused ? 0.28f : 0.18f) * progress));
        SpawnShockwaveDust(progress);

        if (GetTravelDistance() + CurrentWaveLength >= MaxRange)
            Projectile.Kill();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float progress = GetTravelProgress();
        float waveHeight = MathHelper.Lerp(StartWaveHeight, EndWaveHeight, progress) * (PotisInfused ? 1.18f : 1f);
        float collisionWidth = MathHelper.Lerp(StartCollisionWidth, EndCollisionWidth, progress) * (PotisInfused ? 1.18f : 1f);

        Vector2 groundOrigin = GetGroundOrigin();
        Vector2 crestPoint = groundOrigin + new Vector2(ShockwaveDirection * CurrentWaveLength, -waveHeight);
        Vector2 trailingPoint = groundOrigin + new Vector2(ShockwaveDirection * CurrentWaveLength * 0.56f, -waveHeight * 0.34f);
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), groundOrigin, crestPoint,
                   collisionWidth, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), trailingPoint, crestPoint,
                   collisionWidth * 0.82f, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) => false;

    public override void OnKill(int timeLeft) {
        Vector2 groundOrigin = GetGroundOrigin();
        for (int i = 0; i < (PotisInfused ? 18 : 12); i++) {
            Vector2 velocity = new Vector2(ShockwaveDirection * Main.rand.NextFloat(0.8f, PotisInfused ? 3.8f : 3f),
                Main.rand.NextFloat(-2f, -0.35f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(12f, 6f),
                PotisInfused && i % 4 == 0 ? DustID.WhiteTorch : i % 2 == 0 ? DustID.Torch : DustID.Smoke,
                velocity, 105,
                i % 2 == 0 || PotisInfused ? new Color(255, 190, 115) : new Color(220, 210, 205),
                Main.rand.NextFloat(1f, PotisInfused ? 1.65f : 1.35f));
            dust.noGravity = true;
        }
    }

    private float ShockwaveDirection => Projectile.ai[0] == 0f ? 1f : System.MathF.Sign(Projectile.ai[0]);

    private float GetTravelDistance() => System.MathF.Abs(Projectile.Center.X - Projectile.localAI[1]);

    private float GetTravelProgress() => MathHelper.Clamp(GetTravelDistance() / MaxRange, 0f, 1f);

    private Vector2 GetGroundOrigin() => Projectile.Bottom + new Vector2(0f, -4f);

    private void SpawnLaunchBurst() {
        Vector2 groundOrigin = GetGroundOrigin();

        for (int i = 0; i < (PotisInfused ? 24 : 16); i++) {
            Vector2 burstVelocity = new Vector2(ShockwaveDirection * Main.rand.NextFloat(1.4f, PotisInfused ? 5f : 4f),
                Main.rand.NextFloat(-2.5f, -0.45f));
            Dust dust = Dust.NewDustPerfect(groundOrigin + Main.rand.NextVector2Circular(10f, 4f),
                PotisInfused && i % 4 == 0 ? DustID.WhiteTorch : i % 3 == 0 ? DustID.Torch : DustID.Smoke,
                burstVelocity, 100,
                i % 3 == 0 || PotisInfused ? new Color(255, 188, 110) : new Color(225, 215, 208),
                Main.rand.NextFloat(1f, PotisInfused ? 1.72f : 1.45f));
            dust.noGravity = true;
        }
    }

    private void SpawnShockwaveDust(float progress) {
        Vector2 groundOrigin = GetGroundOrigin();
        float waveHeight = MathHelper.Lerp(StartWaveHeight, EndWaveHeight, progress) * (PotisInfused ? 1.18f : 1f);
        int pointCount = 10 + (int)System.MathF.Round(progress * (PotisInfused ? 10f : 6f));

        int layerCount = PotisInfused ? 3 : 2;
        for (int layer = 0; layer < layerCount; layer++) {
            float layerLength = CurrentWaveLength * (layer == 0 ? 1f : layer == 1 ? 0.72f : 0.48f);
            float layerHeight = waveHeight * (layer == 0 ? 1f : 0.66f);
            float layerScale = layer == 0 ? 1.05f : 0.88f;

            for (int i = 0; i < pointCount; i++) {
                float t = pointCount == 1 ? 0f : i / (pointCount - 1f);
                float forward = MathHelper.Lerp(0f, layerLength, t);
                float arcHeight = System.MathF.Sin(t * MathHelper.Pi) * layerHeight;
                float rippleOffset = System.MathF.Sin(Projectile.timeLeft * 0.22f + t * MathHelper.TwoPi * 2f + layer * 0.85f) *
                                     (1.5f + 2.8f * progress);
                Vector2 dustPosition = groundOrigin + new Vector2(ShockwaveDirection * forward, -arcHeight + rippleOffset);
                Vector2 dustVelocity = new Vector2(
                    ShockwaveDirection * MathHelper.Lerp(1.1f, 4f, t),
                    -MathHelper.Lerp(0.15f, 1.15f, t)) + Main.rand.NextVector2Circular(0.2f, 0.2f);

                int dustType = PotisInfused && (i + layer) % 5 == 0 ? DustID.WhiteTorch : (i + layer) % 3 == 0 ? DustID.Torch : DustID.Smoke;
                Color dustColor = dustType == DustID.Torch
                    ? Color.Lerp(new Color(255, 158, 90), PotisInfused ? new Color(255, 230, 155) : new Color(255, 215, 150), t)
                    : dustType == DustID.WhiteTorch
                        ? Color.Lerp(new Color(255, 208, 130), new Color(255, 242, 190), t)
                        : Color.Lerp(new Color(196, 188, 180), new Color(236, 226, 220), t);
                Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 112, dustColor,
                    layerScale + progress * (PotisInfused ? 0.38f : 0.25f));
                dust.noGravity = true;
            }
        }
    }
}
