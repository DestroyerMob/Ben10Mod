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
    public class FourArmsClap : ModProjectile {

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        public override void SetDefaults() {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.aiStyle = ProjAIStyleID.Arrow;

            AIType = ProjectileID.Bullet;
            Projectile.friendly = true;
            Projectile.timeLeft = 5;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            Random random = new Random();
            for (int i = 0; i < 30; i++) {
                int dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.WhiteTorch, 0, 0, 1, Color.White, 1);
                Main.dust[dustNum].noGravity = true;
            }
        }

    }
}
