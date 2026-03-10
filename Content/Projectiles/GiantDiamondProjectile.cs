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
    public class GiantDiamondProjectile : ModProjectile {
        private int timeAlive = 0;
        
        public override void SetDefaults() {
            Projectile.width = 64;
            Projectile.height = 128;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType  = DamageClass.Ranged;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source) {
            if (Projectile.ai[0] == 0) {
                Projectile.NewProjectile(source, Projectile.position + new Vector2(Projectile.width + Main.rand.Next(15, 25), 0), Projectile.velocity, this.Type, Projectile.damage, 0, Projectile.owner, 1);
                Projectile.NewProjectile(source, Projectile.position - new Vector2(Projectile.width - Main.rand.Next(15, 25), 0), Projectile.velocity, this.Type, Projectile.damage, 0, Projectile.owner, 1);
            }
        }

        public override void AI() {
            timeAlive++;
            Projectile.velocity.Y = (float)Math.Pow(timeAlive / 10f, 2);
        }

        public override void OnKill(int timeLeft) {
            for (int i = 0; i < 10; i++) {
                int dustNum = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond,
                    1, 1, 0, Color.White, 2);
                Main.dust[dustNum].noGravity = true;
            }
        }
    }
}
