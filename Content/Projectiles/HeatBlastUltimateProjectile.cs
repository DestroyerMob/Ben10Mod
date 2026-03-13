using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Ben10Mod.Content.Projectiles.UltimateAttacks;

namespace Ben10Mod.Content.Projectiles
{
    public class HeatBlastUltimateProjectile : ChargedThrownUltimateProjectile
    {
        protected override Vector2 ChargeOffset => new(0f, -78f);
        protected override float InitialScale => 0.3f;
        protected override float MaxChargeScale => 2.2f;
        protected override float ChargeStep => 0.038f;
        protected override float LaunchSpeed => 5f;
        protected override int MaxLifetime => 600;

        protected override void UpdateCharging(Player owner) {
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

        protected override void UpdateReleased(Player owner) {
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
