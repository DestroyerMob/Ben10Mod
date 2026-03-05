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
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.Arrow;

            AIType = ProjectileID.Bullet;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft   = 60 * 5;
        }

        public override void AI() {
            for (int i = 0; i < 3; i++) {
                int dustNum = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch,
                    newColor: Main.DiscoColor, Scale: 2);
                Main.dust[dustNum].noGravity = true;
            }
        }

        public override void OnKill(int timeLeft) {
            for (int i = 0; i < 25; i++) {
                int dustNum = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-25f, 25f), 0, Main.DiscoColor, 3f);
                Main.dust[dustNum].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            lightColor = Main.DiscoColor;
            Lighting.AddLight(Projectile.position + Projectile.velocity * 0.5f, lightColor.ToVector3() * 0.5f);
            return base.PreDraw(ref lightColor);
        }
    }
}
