using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GoopGlobProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 75;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.velocity.Y += 0.05f;
        Projectile.rotation = Projectile.velocity.ToRotation();

        Vector2 dustVelocity = Projectile.velocity * 0.12f;
        Dust outer = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.GreenTorch,
            dustVelocity + Main.rand.NextVector2Circular(0.4f, 0.4f), 90, new Color(110, 235, 130), Main.rand.NextFloat(1f, 1.2f));
        outer.noGravity = true;

        if (Main.rand.NextBool(2)) {
            Dust inner = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), DustID.GreenTorch,
                dustVelocity * 0.5f, 110, new Color(180, 255, 145), Main.rand.NextFloat(0.8f, 1f));
            inner.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Poisoned, 4 * 60);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
            Dust splash = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.GreenTorch : DustID.GreenMoss, velocity, 90,
                new Color(120, 240, 135), Main.rand.NextFloat(0.95f, 1.25f));
            splash.noGravity = false;
        }
    }
}
