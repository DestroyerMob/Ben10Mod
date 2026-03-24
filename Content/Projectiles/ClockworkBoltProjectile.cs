using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ClockworkBoltProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GoldenBullet}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.45f, 0.36f, 0.08f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.GemTopaz : DustID.YellowTorch,
                -Projectile.velocity * 0.1f, 100, new Color(244, 220, 120), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 3 * 60);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GemTopaz : DustID.YellowTorch,
                Main.rand.NextVector2Circular(1.8f, 1.8f), 100, new Color(245, 220, 120), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }
}
