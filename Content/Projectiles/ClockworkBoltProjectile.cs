using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ClockworkBoltProjectile : ModProjectile {
    private const float NormalScrambleDistance = 72f;
    private const float BossScrambleDistance = 40f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GoldenBullet}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.45f, 0.36f, 0.08f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.GemTopaz : DustID.YellowTorch,
                -Projectile.velocity * 0.1f, 100, new Color(244, 220, 120), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 originalCenter = target.Center;
        ScrambleTargetThroughTime(target);
        EmitScrambleBurst(originalCenter, target.Center);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GemTopaz : DustID.YellowTorch,
                Main.rand.NextVector2Circular(1.8f, 1.8f), 100, new Color(245, 220, 120), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }

    private void ScrambleTargetThroughTime(NPC target) {
        float scrambleDistance = target.boss ? BossScrambleDistance : NormalScrambleDistance;
        Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);

        for (int i = 0; i < 10; i++) {
            Vector2 offset = baseDirection.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(scrambleDistance * 0.45f, scrambleDistance);
            Vector2 candidateCenter = target.Center + offset;
            Vector2 candidatePosition = candidateCenter - target.Size * 0.5f;

            if (target.noTileCollide || !Collision.SolidCollision(candidatePosition, target.width, target.height)) {
                target.position = candidatePosition;
                target.velocity = Vector2.Zero;
                target.netUpdate = true;
                return;
            }
        }
    }

    private static void EmitScrambleBurst(Vector2 fromCenter, Vector2 toCenter) {
        for (int i = 0; i < 10; i++) {
            Vector2 fromVelocity = Main.rand.NextVector2Circular(2.4f, 2.4f);
            Dust fromDust = Dust.NewDustPerfect(fromCenter, i % 2 == 0 ? DustID.GemTopaz : DustID.YellowTorch,
                fromVelocity, 95, new Color(245, 220, 130), Main.rand.NextFloat(0.95f, 1.2f));
            fromDust.noGravity = true;

            Vector2 toVelocity = Main.rand.NextVector2Circular(2.4f, 2.4f);
            Dust toDust = Dust.NewDustPerfect(toCenter, i % 2 == 0 ? DustID.GemTopaz : DustID.YellowTorch,
                toVelocity, 95, new Color(255, 235, 150), Main.rand.NextFloat(0.95f, 1.25f));
            toDust.noGravity = true;
        }
    }
}
