using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles
{
    public class EyeGuyLaserbeam : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;          // Square hitbox = no more culling
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;      // Goes through enemies
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;    // Much smoother laser feel
            Projectile.timeLeft = 240;
            Projectile.alpha = 40;
        }

        public override void AI()
        {
            // Always face movement direction (this is the correct way)
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Eye Guy green glow
            Lighting.AddLight(Projectile.Center, 0f, 0.95f, 0.25f);

            // Optional green dust trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenFairy, Projectile.velocity * 0.2f, 100, default, 1.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,      // position
                null,                                         // source rectangle
                lightColor * Projectile.Opacity,              // color
                Projectile.rotation + MathHelper.PiOver2,     // ← THIS IS THE FIX (90° offset)
                new Vector2(texture.Width / 2f, texture.Height / 2f), // centered origin
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false; // skip default drawing
        }
    }
}