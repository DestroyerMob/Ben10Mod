using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
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
            Projectile.timeLeft    = 30;
            Projectile.tileCollide = false;
            AIType                 = ProjectileID.Bullet;
            Projectile.DamageType  = ModContent.GetInstance<HeroDamage>();
            Projectile.penetrate   = -1;
        }

        public override void OnSpawn(IEntitySource source) {
            maxTime = Projectile.timeLeft;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            Projectile.damage /= 2;
        }

        public override void AI() {
            base.AI();
            Projectile.alpha = 255 - (int)(255 * ((float)Projectile.timeLeft / (float)maxTime));
        }

    }
}
