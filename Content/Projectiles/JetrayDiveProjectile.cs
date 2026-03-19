using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class JetrayDiveProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PhantasmalBolt}";

    public override void SetDefaults() {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = owner.velocity.SafeNormalize(Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f)));
        Projectile.Center = owner.Center + direction * 18f;
        Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneTime = 6;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueCrystalShard,
                -direction.X * 1.2f, -direction.Y * 1.2f, 100, Color.Cyan, 1.15f);
            dust.noGravity = true;
        }
    }
}
