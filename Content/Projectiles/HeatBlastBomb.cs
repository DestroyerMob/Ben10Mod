using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class HeatBlastBomb : ModProjectile {

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        public override void SetDefaults() {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow;

            AIType = ProjectileID.Grenade;
            Projectile.friendly = true;
        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            Random random = new Random();
            for (int i = 0; i < 6; i++) {
                int dustNum = Dust.NewDust(boxPosition, 1, 1, DustID.Torch, 0, 0, 1, Color.White, 2);
                Main.dust[dustNum].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), Projectile.damage, 0, -1, 100);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), Projectile.damage, 0, -1, 100);
            return base.OnTileCollide(oldVelocity);
        }
    }
}
