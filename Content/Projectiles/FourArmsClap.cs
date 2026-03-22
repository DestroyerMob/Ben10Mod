using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsClap : ModProjectile {
    private const int ImpactDustCount = 56;
    private const float ImpactSpread = 1.08f;
    private const float ImpactSurfaceInset = 4f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.aiStyle = ProjAIStyleID.Arrow;

        AIType = ProjectileID.Bullet;
        Projectile.friendly = true;
        Projectile.timeLeft = 360;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Ranged;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 impactDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 burstDirection = -impactDirection;
        Vector2 impactPoint = GetImpactPoint(target, impactDirection);

        SpawnImpactBurst(impactPoint, burstDirection);
    }

    public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
        for (int i = 0; i < 16; i++) {
            int dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.WhiteTorch);
            Main.dust[dustNum].noGravity = true;
        }
    }

    private void SpawnImpactBurst(Vector2 impactPoint, Vector2 burstDirection) {
        for (int i = 0; i < ImpactDustCount; i++) {
            float spread = Main.rand.NextFloat(-ImpactSpread, ImpactSpread);
            Vector2 direction = burstDirection.RotatedBy(spread).SafeNormalize(burstDirection);
            float speed = Main.rand.NextFloat(5.5f, 11.5f);
            Vector2 velocity = direction * speed;
            Vector2 position = impactPoint + direction * Main.rand.NextFloat(0f, 8f);

            Dust dust = Dust.NewDustPerfect(position, DustID.WhiteTorch, velocity, 80, Color.White,
                Main.rand.NextFloat(1.6f, 2.7f));
            dust.noGravity = true;

            if (Main.rand.NextBool(3)) {
                dust.velocity += burstDirection * Main.rand.NextFloat(0.8f, 1.6f);
            }
        }

        for (int i = 0; i < 12; i++) {
            Vector2 direction = burstDirection.RotatedBy(Main.rand.NextFloat(-0.42f, 0.42f)).SafeNormalize(burstDirection);
            Dust coreDust = Dust.NewDustPerfect(impactPoint, DustID.Smoke, direction * Main.rand.NextFloat(1.2f, 3.2f), 110,
                new Color(240, 240, 240), Main.rand.NextFloat(1.1f, 1.6f));
            coreDust.noGravity = true;
        }
    }

    private Vector2 GetImpactPoint(NPC target, Vector2 impactDirection) {
        Rectangle hitbox = target.Hitbox;
        Vector2 surfacePoint = new(
            MathHelper.Clamp(Projectile.Center.X, hitbox.Left, hitbox.Right),
            MathHelper.Clamp(Projectile.Center.Y, hitbox.Top, hitbox.Bottom));
        return surfacePoint - impactDirection * ImpactSurfaceInset;
    }
}
