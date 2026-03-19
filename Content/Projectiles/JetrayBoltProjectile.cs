using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class JetrayBoltProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MartianTurretBolt}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 80;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.2f, 0.8f, 1f);

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch,
                0f, 0f, 100, Color.Cyan, 1.1f);
            dust.noGravity = true;
            dust.velocity *= 0.2f;
        }
    }
}
