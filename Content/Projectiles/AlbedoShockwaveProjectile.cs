using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoShockwaveProjectile : ModProjectile {
        public override string Texture => "Ben10Mod/Content/Projectiles/Projectile_464";

        public override void SetDefaults() {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
        }

        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.scale += 0.01f;
            Lighting.AddLight(Projectile.Center, 0.7f, 0.1f, 0.1f);
        }
    }
}
