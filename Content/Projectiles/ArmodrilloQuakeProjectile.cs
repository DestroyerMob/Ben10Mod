using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArmodrilloQuakeProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DD2OgreSmash}";

    public override void SetDefaults() {
        Projectile.width = 48;
        Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = 4;
        Projectile.timeLeft = 50;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.velocity.Y = 0f;
        Projectile.rotation = 0f;
        Projectile.scale = 0.95f + (50f - Projectile.timeLeft) * 0.02f;
        Lighting.AddLight(Projectile.Center, 0.75f, 0.4f, 0.1f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand,
                Projectile.velocity.X * 0.4f, -0.6f, 120, default, 1.15f);
            dust.noGravity = true;
        }
    }
}
