using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoRocketProjectile : ModProjectile {
        public override string Texture => "Ben10Mod/Content/Projectiles/HeatBlastUltimateProjectile";

        public override void SetDefaults() {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
        }

        public override void AI() {
            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            if (target.active && !target.dead) {
                Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * 10f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.03f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.8f, 0.15f, 0.15f);
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
