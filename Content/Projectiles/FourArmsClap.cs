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
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = ProjAIStyleID.Arrow;

            AIType                 = ProjectileID.Bullet;
            Projectile.friendly    = true;
            Projectile.timeLeft    = 360;
            Projectile.tileCollide = false;
            Projectile.DamageType  = DamageClass.Ranged;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            for (int i = 0; i < 64; i++) {
                int dustNum = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch,  Main.rand.NextFloat(-85, 85), Main.rand.NextFloat(-85, 85), Scale: 3);
                Main.dust[dustNum].noGravity = true;
            }
        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            Random random = new Random();
            for (int i = 0; i < 16; i++) {
                int dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.WhiteTorch);
                Main.dust[dustNum].noGravity = true;
            }
        }

    }
}
