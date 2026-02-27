using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class GhostFreakProjectile : ModProjectile {

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        private int oddEven = 0;

        public override void SetDefaults() {
            
            Projectile.width      = (int)(Projectile.width  * 0.6f);
            Projectile.height     = (int)(Projectile.height * 0.6f);
            Projectile.scale      = 0.6f;
            Projectile.friendly   = true;
            Projectile.hostile    = false;
            Projectile.penetrate  = -1;
            Projectile.timeLeft   = 35;
            Projectile.DamageType = DamageClass.Magic;

        }

        public override void AI() {
            // Rotate velocity a tiny random amount each tick -> tentacle-like curve
            float maxCurve = 0.15f; // radians; tweak for more/less wiggle
            Projectile.velocity = Projectile.velocity.RotatedByRandom(maxCurve);

            // Slow down over time so it doesn’t go forever
            // Projectile.velocity *= 0.97f;

            // Face along direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            oddEven++;
            Random random  = new Random();
            int dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.WhiteTorch, random.Next(0, 0), random.Next(0, 0), 1, oddEven % 2 == 0 ? Color.White : Color.Black,  Projectile.timeLeft / 10);
            Main.dust[dustNum].noGravity = true;
        }
    }
}
