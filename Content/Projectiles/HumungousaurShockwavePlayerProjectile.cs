using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HumungousaurShockwavePlayerProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DD2OgreSmash}";

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 60;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.velocity.Y = 0f;
        Projectile.rotation = 0f;
        Projectile.scale = 1f + (60f - Projectile.timeLeft) * 0.015f;
        Lighting.AddLight(Projectile.Center, 0.8f, 0.25f, 0.1f);
    }
}
