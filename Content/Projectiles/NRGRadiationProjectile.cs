using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class NRGRadiationProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.InfernoFriendlyBlast}";

    public override void SetDefaults() {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 90;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation += 0.18f * Projectile.direction;
        Projectile.scale = 0.95f + (90f - Projectile.timeLeft) * 0.0035f;
        Lighting.AddLight(Projectile.Center, 1f, 0.45f, 0.1f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch,
                0f, 0f, 110, new Color(255, 160, 60), 1.15f);
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 180);
    }
}
