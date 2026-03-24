using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ClockworkTimeTrapProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{Terraria.ID.ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override void AI() {
        Projectile.rotation += 0.22f * Projectile.direction;
        Projectile.velocity.Y += 0.16f;
        Lighting.AddLight(Projectile.Center, 0.38f, 0.3f, 0.08f);

        for (int i = 0; i < 2; i++) {
            Vector2 orbitOffset = Main.rand.NextVector2Circular(9f, 9f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + orbitOffset,
                Main.rand.NextBool() ? Terraria.ID.DustID.GemTopaz : Terraria.ID.DustID.YellowTorch,
                -Projectile.velocity * 0.08f, 100, new Color(244, 220, 120), Main.rand.NextFloat(0.85f, 1.08f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Projectile.Kill();
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<ClockworkTimeFieldProjectile>(), Projectile.damage, 0f, Projectile.owner);
        }

        for (int i = 0; i < 14; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.4f, 2.4f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center,
                i % 2 == 0 ? Terraria.ID.DustID.GemTopaz : Terraria.ID.DustID.YellowTorch,
                velocity, 95, new Color(245, 220, 125), Main.rand.NextFloat(0.95f, 1.22f));
            dust.noGravity = true;
        }
    }
}
