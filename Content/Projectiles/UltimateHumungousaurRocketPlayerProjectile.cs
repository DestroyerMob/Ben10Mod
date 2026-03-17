using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateHumungousaurRocketPlayerProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.RocketI}";

    public override void SetDefaults() {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.9f, 0.25f, 0.15f);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 18; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                DustID.Firework_Red, Scale: 1.3f);
            dust.velocity *= 1.8f;
            dust.noGravity = true;
        }
    }
}
