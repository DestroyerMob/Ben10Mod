using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class NRGBurstProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DD2ExplosiveTrapT3Explosion}";

    public override void SetDefaults() {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        Projectile.Center = owner.Center;
        Projectile.scale = 0.5f + (18f - Projectile.timeLeft) * 0.06f;
        Projectile.alpha = (int)MathHelper.Clamp((18f - Projectile.timeLeft) * 10f, 0f, 140f);
        Lighting.AddLight(Projectile.Center, 1f, 0.55f, 0.2f);

        if (Main.rand.NextBool()) {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(1f, 2.4f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), DustID.Torch,
                velocity, 100, new Color(255, 170, 80), 1.25f);
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 240);
    }
}
