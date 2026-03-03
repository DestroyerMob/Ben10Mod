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
    public class ChromaStoneProjectile : ModProjectile {

        

        public override void SetDefaults() {
            Projectile.width = 32;
            Projectile.height = 64;
            Projectile.aiStyle = ProjAIStyleID.Arrow;

            AIType = ProjectileID.Bullet;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft   = 60 * 5;

        }

        public override bool PreDraw(ref Color lightColor) {
            lightColor = Main.DiscoColor;
            Lighting.AddLight(Projectile.position + Projectile.velocity * 0.5f, lightColor.ToVector3() * 0.5f);
            return base.PreDraw(ref lightColor);
        }
    }
}
