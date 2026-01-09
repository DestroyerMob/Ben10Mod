using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Ben10Mod.Content.Projectiles {
    public class HeatBlastFireSlam : ModProjectile {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        public override void SetDefaults() {
            Projectile.width       = 64;
            Projectile.height      = 64;
            Projectile.aiStyle     = ProjAIStyleID.Arrow;
            AIType                 = ProjectileID.Bullet;
            Projectile.friendly    = true;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft    = 20;
            Projectile.DamageType  = DamageClass.Ranged;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.OnFire3, 10 * 60);
        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            Random random = new Random();
            for (int i = 0; i < 25; i++) {
                int dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.Flare, 0, 1, 1, Color.White, random.Next(4));
            }
        }
    }
}
