using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoSonicBlastProjectile : ModProjectile {
        public override string Texture => "Ben10Mod/Content/Projectiles/EyeGuyLaserbeam";

        public override void SetDefaults() {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI() {
            Projectile.rotation += 0.3f;
            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.2f);
        }
    }
}
