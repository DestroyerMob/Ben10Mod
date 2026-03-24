using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ClockworkTimeFieldProjectile : ModProjectile {
    private const float Radius = 54f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 150;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 24;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        Lighting.AddLight(Projectile.Center, 0.34f, 0.28f, 0.08f);

        float spin = Main.GlobalTimeWrappedHourly * 2.1f;
        for (int i = 0; i < 5; i++) {
            float angle = spin + MathHelper.TwoPi * i / 5f;
            Vector2 unit = angle.ToRotationVector2();
            Dust dust = Dust.NewDustPerfect(Projectile.Center + unit * Radius,
                i % 2 == 0 ? DustID.GemTopaz : DustID.YellowTorch,
                unit * 0.2f, 100, new Color(245, 220, 125), Main.rand.NextFloat(0.95f, 1.16f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= Radius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 3 * 60);
    }
}
