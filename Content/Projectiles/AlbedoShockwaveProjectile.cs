using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoShockwaveProjectile : ModProjectile {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DD2OgreSmash}";

        public override void SetDefaults() {
            Projectile.width = 40;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 75;
            Projectile.tileCollide = false;
        }

        public override void AI() {
            Projectile.velocity.Y = 0f;
            Projectile.rotation = 0f;
            Projectile.scale = 1f + (75f - Projectile.timeLeft) * 0.01f;
            Lighting.AddLight(Projectile.Center, 0.7f, 0.1f, 0.1f);
        }
    }
}
