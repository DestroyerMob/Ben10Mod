using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoRocketProjectile : ModProjectile {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.RocketI}";

        public override void SetDefaults() {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1.15f;
        }

        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 1f, 0.22f, 0.12f);

            for (int i = 0; i < 2; i++) {
                Vector2 offset = -Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4f, 18f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Torch,
                    -Projectile.velocity * 0.18f + Main.rand.NextVector2Circular(0.55f, 0.55f),
                    110, new Color(255, 120, 70), 1.15f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft) {
            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Firework_Red, Scale: 1.3f);
                dust.velocity *= 1.8f;
                dust.noGravity = true;
            }
        }
    }
}
