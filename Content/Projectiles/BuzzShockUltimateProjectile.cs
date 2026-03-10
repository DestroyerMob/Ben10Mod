using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class BuzzShockUltimateProjectile : ModProjectile {

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        private int target = -1;

        public override void SetDefaults() {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.Arrow;

            AIType = ProjectileID.Bullet;
            Projectile.friendly = true;
            Projectile.penetrate = 10;
            
            Projectile.DamageType = DamageClass.Ranged;

        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            int    dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.UltraBrightTorch, Scale: 3);
            Main.dust[dustNum].noGravity = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            FindTarget();
            for (int i = 0; i < 5; i++) {
                int    dustNum = Dust.NewDust(target.position, target.width, target.height, DustID.UltraBrightTorch);
                Main.dust[dustNum].noGravity = true;
            }
        }

        public override void AI() {
            if (target == -1 || !Main.npc[target].active || !Main.npc[target].CanBeChasedBy(this)) {
                target = -1;
                FindTarget();
            }

            if (target != -1) {
                NPC     npc             = Main.npc[target];
                Vector2 desiredVelocity = Projectile.DirectionTo(npc.Center) * 24f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.2f);
            }
        }

        private void FindTarget() {
            float smallestDistance = 250f;
            target = -1;

            foreach (NPC npc in Main.npc) {
                if (npc.CanBeChasedBy(this)) {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < smallestDistance) {
                        smallestDistance = distance;
                        target           = npc.whoAmI;
                    }
                }
            }
        }
    }
}
