using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class PlumberBlasterBoltProjectile : ModProjectile {
    private bool StrongVariant => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PurpleLaser}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 72;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.scale = StrongVariant ? 1.04f : 0.9f;
            if (StrongVariant) {
                Projectile.penetrate = 2;
                Projectile.timeLeft = System.Math.Max(Projectile.timeLeft, 90);
            }
        }

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, StrongVariant ? new Vector3(0.2f, 0.9f, 1f) : new Vector3(0.12f, 0.68f, 0.82f));

        if (Main.dedServ || !Main.rand.NextBool(3))
            return;

        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.Electric : DustID.BlueTorch,
            -Projectile.velocity * 0.08f, 95, StrongVariant ? new Color(165, 255, 255) : new Color(110, 220, 255),
            StrongVariant ? Main.rand.NextFloat(0.95f, 1.18f) : Main.rand.NextFloat(0.82f, 1.02f));
        dust.noGravity = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (StrongVariant)
            target.AddBuff(BuffID.Electrified, 75);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(2.3f, 2.3f), 100,
                StrongVariant ? new Color(165, 255, 255) : new Color(110, 220, 255),
                Main.rand.NextFloat(0.9f, 1.12f));
            dust.noGravity = true;
        }
    }
}
