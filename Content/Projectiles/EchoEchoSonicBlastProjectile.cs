using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoSonicBlastProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private const int MaxLifetime = 42;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = MaxLifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.95f, 0.85f, 0.7f) * 0.9f);
        SpawnWaveDust();

        if (Main.rand.NextBool(2)) {
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Vector2 dustOffset = perpendicular * Main.rand.NextFloatDirection() * Main.rand.NextFloat(4f, 10f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.GemDiamond,
                Projectile.velocity * 0.05f, 100, new Color(255, 240, 210), 1.05f);
            dust.noGravity = true;
        }
    }

    private void SpawnWaveDust() {
        float progress = 1f - Projectile.timeLeft / (float)MaxLifetime;
        float growthProgress = (float)System.Math.Sqrt(progress);
        float fade = Utils.GetLerpValue(0f, 0.12f, progress, true) *
            Utils.GetLerpValue(0f, 0.24f, Projectile.timeLeft / (float)MaxLifetime, true);
        float radius = MathHelper.Lerp(14f, 34f, growthProgress);
        float arcHalfWidth = MathHelper.Lerp(0.6f, 0.92f, growthProgress);
        int segments = 11;

        for (int i = 0; i < segments; i++) {
            float completion = i / (float)(segments - 1);
            float angle = Projectile.rotation + MathHelper.Lerp(-arcHalfWidth, arcHalfWidth, completion);
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 tangentVelocity = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.18f + Projectile.velocity * 0.02f;

            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GemDiamond, tangentVelocity, 160,
                new Color(255, 242, 210) * fade, 0.95f + progress * 0.2f);
            dust.noGravity = true;
            dust.fadeIn = 0.55f;
            dust.scale *= 0.92f;
            dust.velocity *= 0.7f;
            dust.alpha = 185;

            if (i % 2 == 0) {
                Dust innerDust = Dust.NewDustPerfect(Projectile.Center + offset * 0.92f, DustID.Smoke, tangentVelocity * 0.35f, 175,
                    new Color(255, 255, 245) * fade, 0.7f + progress * 0.12f);
                innerDust.noGravity = true;
                innerDust.velocity *= 0.45f;
                innerDust.alpha = 205;
            }
        }
    }
}
