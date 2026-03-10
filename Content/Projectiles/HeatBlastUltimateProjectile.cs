using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace Ben10Mod.Content.Projectiles
{
    public class HeatBlastUltimateProjectile : ModProjectile
    {
        private bool launched = false;

        public override void SetDefaults()
        {
            Projectile.width       = 128;
            Projectile.height      = 128;
            Projectile.scale       = 0.3f;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly    = true;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!launched)
            {
                if (owner.channel && owner.active && !owner.dead)
                {
                    Projectile.Center   = owner.Center + new Vector2(0f, -78f); 
                    Projectile.rotation = 0f;                                   

                    // Grow while holding
                    if (Projectile.scale < 2.2f)
                    {
                        Projectile.scale += 0.038f;
                        Projectile.scale = Math.Min(2.2f, Projectile.scale);
                    }
                    else
                    {
                        owner.channel = false;
                    }

                    // Grow hitbox with scale
                    Projectile.width  = (int)(128 * Projectile.scale);
                    Projectile.height = (int)(128 * Projectile.scale);

                    SpawnChargingDust();
                    return;
                } else {
                    launched = true;

                    Vector2 launchDir = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = launchDir * 5f;
                }
            }
            SpawnFlyingDust();
        }

        private void SpawnChargingDust() {
            float radius = 58f * Projectile.scale;
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());
            for (int i = 0; i < 35; i++) {
                if (Main.rand.NextBool(2)) {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(radius, radius);
                    Dust d = Dust.NewDustPerfect(pos, DustID.InfernoFork,
                        Main.rand.NextVector2Circular(1f, 2.5f), 90,
                        new Color(255, 90, 0), Main.rand.NextFloat(2.1f, 3.4f));
                    d.noGravity = true;
                }
            }
        }

        private void SpawnFlyingDust() {
            float radius = 58f * Projectile.scale;
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());
            for (int i = 0; i < 28; i++) {
                if (Main.rand.NextBool(2)) {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(radius * 0.8f, radius * 0.8f);
                    Dust d = Dust.NewDustPerfect(pos, DustID.InfernoFork,
                        Projectile.velocity * -0.15f, 100,
                        new Color(255, 110, 0), 2.4f);
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            Texture2D tex    = TextureAssets.Projectile[Projectile.type].Value;
            Vector2   origin = tex.Size() / 2f;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor,
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false; // skip default draw
        }
    }
}