using Microsoft.Xna.Framework;
using Ben10Mod.Content.DamageClasses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsLandingShockwaveProjectile : ModProjectile {
    private const float DustRadius = 42f;
    private const float GroundDustLift = 6f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 128;
        Projectile.height = 52;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 8;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override void AI() {
        if (Projectile.localAI[0] > 0f)
            return;

        Projectile.localAI[0] = 1f;
        SpawnImpactDust();
    }

    private void SpawnImpactDust() {
        Vector2 impactLineCenter = new Vector2(Projectile.Center.X, Projectile.Bottom.Y - GroundDustLift);

        for (int i = 0; i < 24; i++) {
            float completion = i / 23f;
            float direction = MathHelper.Lerp(-1f, 1f, completion);
            Vector2 position = impactLineCenter + new Vector2(direction * DustRadius, Main.rand.NextFloat(-3f, 3f));
            Vector2 velocity = new Vector2(direction * Main.rand.NextFloat(1.1f, 3.2f), Main.rand.NextFloat(-1.8f, -0.4f));

            Dust dust = Dust.NewDustPerfect(position, DustID.Smoke, velocity, 95, new Color(215, 215, 215),
                Main.rand.NextFloat(0.95f, 1.3f));
            dust.noGravity = true;
        }

        for (int i = 0; i < 16; i++) {
            float direction = Main.rand.NextBool() ? -1f : 1f;
            Vector2 velocity = new Vector2(direction * Main.rand.NextFloat(0.9f, 2.4f), Main.rand.NextFloat(-1.4f, -0.2f));
            Dust dust = Dust.NewDustPerfect(impactLineCenter + Main.rand.NextVector2Circular(10f, 4f), DustID.Stone, velocity, 90,
                Color.White, Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }
}
