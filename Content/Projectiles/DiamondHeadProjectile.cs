using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class DiamondHeadProjectile : ModProjectile {
        public override void SetDefaults() {
            Projectile.width = 6;
            Projectile.height = 16;
            Projectile.scale = 1.5f;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType             = ProjectileID.Bullet;

            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI() {
            Projectile.spriteDirection = Projectile.direction;
        }
    }
}
