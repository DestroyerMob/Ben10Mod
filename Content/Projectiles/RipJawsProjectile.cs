using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class RipJawsProjectile : ModProjectile {

        int maxTime = 0;

        public override void SetDefaults() {
            Projectile.height      = 25;
            Projectile.width       = 50;
            Projectile.aiStyle     = ProjAIStyleID.Arrow;
            Projectile.friendly    = true;
            Projectile.hostile     = false;
            Projectile.timeLeft    = 25;
            Projectile.tileCollide = false;
            AIType                 = ProjectileID.Bullet;
            Projectile.DamageType  = DamageClass.MeleeNoSpeed;
        }

        public override void OnSpawn(IEntitySource source) {
            maxTime = Projectile.timeLeft;
        }

  

        public override void AI() {
            base.AI();
            Projectile.alpha = 255 - (int)(255 * ((float)Projectile.timeLeft / (float)maxTime));
        }

    }
}
