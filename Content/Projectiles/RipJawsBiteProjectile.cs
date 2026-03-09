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
    public class RipJawsBiteProjectile : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

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
            Projectile.penetrate   = -1;
        }

        public override void AI() {
            var player = Main.player[Projectile.owner];
            Projectile.position = player.Center + (new Vector2(15f, 0) * player.direction);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { 
            target.AddBuff(BuffID.Bleeding, 240);
            int dustNum = Dust.NewDust(target.Center, target.height, target.width, DustID.GemDiamond, Scale: 10);
            Main.dust[dustNum].noGravity = true;
        }
    }
}
