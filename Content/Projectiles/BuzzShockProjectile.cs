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
    public class BuzzShockProjectile : ModProjectile {

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        public override void SetDefaults() {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.Arrow;

            AIType = ProjectileID.Bullet;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            
            Projectile.DamageType = DamageClass.Ranged;

        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            Random random = new Random();
            for (int i = 0; i < 5; i++) {
                int dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.UltraBrightTorch, random.Next(-2, 3), random.Next(-2, 3), 1, Color.White, 1);
                Main.dust[dustNum].noGravity = true;
            }
        }
    }
}
