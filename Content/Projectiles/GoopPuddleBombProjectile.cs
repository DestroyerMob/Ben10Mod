using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GoopPuddleBombProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.hide = true;
    }

    public override void AI() {
        Projectile.velocity.Y += 0.22f;
        Projectile.rotation += Projectile.velocity.X * 0.08f;

        if (Main.rand.NextBool(2)) {
            Dust blob = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.GreenTorch,
                Projectile.velocity * 0.1f, 95, new Color(105, 230, 125), Main.rand.NextFloat(1.05f, 1.35f));
            blob.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Poisoned, 3 * 60);
        Projectile.Kill();
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            int puddleDamage = System.Math.Max(1, (int)(Projectile.damage * 0.5f));
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Bottom + new Vector2(0f, -8f), Vector2.Zero,
                ModContent.ProjectileType<GoopPuddleProjectile>(), puddleDamage, 0f, Projectile.owner);
        }

        for (int i = 0; i < 16; i++) {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-2.6f, 2.6f), Main.rand.NextFloat(-3.2f, -0.4f));
            Dust splash = Dust.NewDustPerfect(Projectile.Bottom + new Vector2(Main.rand.NextFloat(-10f, 10f), -6f),
                i % 3 == 0 ? DustID.GreenTorch : DustID.GreenMoss, velocity, 90, new Color(110, 245, 135), Main.rand.NextFloat(1f, 1.35f));
            splash.noGravity = false;
        }
    }
}
