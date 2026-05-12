using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoShockwaveProjectile : ModProjectile {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DD2OgreSmash}";

        public override void SetDefaults() {
            Projectile.width = 52;
            Projectile.height = 28;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 75;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI() {
            Projectile.velocity.Y = 0f;
            Projectile.rotation = 0f;
            float age = 75f - Projectile.timeLeft;
            Projectile.scale = 1.18f + age * 0.012f + Projectile.ai[1] * 0.05f;
            Projectile.GetGlobalProjectile<OmnitrixProjectile>().EnableScaleHitboxSync(Projectile);
            Lighting.AddLight(Projectile.Center, 1f, 0.46f, 0.16f);

            if (!Main.dedServ) {
                for (int i = 0; i < 2; i++) {
                    Vector2 dustPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-22f, 22f), Main.rand.NextFloat(-8f, 12f));
                    Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Torch,
                        new Vector2(-Math.Sign(Projectile.velocity.X) * 0.45f, -0.55f),
                        110, new Color(255, 135, 70), 1.2f);
                    dust.noGravity = true;
                }
            }
        }
    }
}
