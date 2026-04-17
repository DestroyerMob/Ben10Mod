using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateHumungousaurRocketPlayerProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.RocketI}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 1;
        Projectile.timeLeft = 150;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.scale = Projectile.ai[0] >= 2f ? 1.22f : Projectile.ai[0] >= 1f ? 1.12f : 1f;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, Projectile.ai[0] >= 2f ? 1.15f : 0.95f, 0.25f, 0.15f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                Projectile.ai[0] >= 2f ? DustID.Torch : DustID.Smoke, 0f, 0f, 105, new Color(255, 175, 115),
                Main.rand.NextFloat(0.95f, 1.25f));
            dust.velocity *= 0.35f;
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center, i % 3 == 0 ? DustID.Torch : DustID.Smoke,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 110, new Color(255, 170, 115), Main.rand.NextFloat(1f, 1.3f));
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 18; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                DustID.Firework_Red, Scale: 1.3f);
            dust.velocity *= 1.8f;
            dust.noGravity = true;
        }
    }
}
