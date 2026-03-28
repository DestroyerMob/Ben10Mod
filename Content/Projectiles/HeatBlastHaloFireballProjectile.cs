using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastHaloFireballProjectile : ModProjectile {
    private bool Snowflake => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.ImpFireball}";

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = Main.projFrames[ProjectileID.ImpFireball] > 0 ? Main.projFrames[ProjectileID.ImpFireball] : 1;
        ProjectileID.Sets.TrailCacheLength[Type] = 6;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.ImpFireball);
        AIType = ProjectileID.ImpFireball;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 180;
        Projectile.ignoreWater = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Main.projFrames[Type] > 1) {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5) {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }
        }

        Lighting.AddLight(Projectile.Center, Snowflake
            ? new Vector3(0.1f, 0.35f, 0.52f)
            : new Vector3(0.6f, 0.25f, 0.06f));

        if (Main.rand.NextBool(2)) {
            int dustType = Snowflake
                ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce)
                : (Main.rand.NextBool(4) ? DustID.Flare : DustID.Torch);
            Color dustColor = Snowflake
                ? Color.Lerp(new Color(155, 228, 255), new Color(240, 250, 255), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 150, 60), new Color(255, 224, 130), Main.rand.NextFloat());

            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), dustType,
                Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 96, dustColor, Main.rand.NextFloat(0.9f, 1.25f));
            dust.noGravity = true;
        }
    }

    public override bool PreKill(int timeLeft) {
        SpawnImpactDust(6);
        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(Snowflake ? BuffID.Frostburn2 : BuffID.OnFire3, Snowflake ? 240 : 300);
        SpawnImpactDust(10);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SpawnImpactDust(10);
        return true;
    }

    private void SpawnImpactDust(int dustCount) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < dustCount; i++) {
            int dustType = Snowflake
                ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce)
                : (Main.rand.NextBool(4) ? DustID.Flare : DustID.Torch);
            Color dustColor = Snowflake
                ? Color.Lerp(new Color(165, 228, 255), new Color(248, 252, 255), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 162, 70), new Color(255, 236, 155), Main.rand.NextFloat());

            Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, Main.rand.NextVector2Circular(2.6f, 2.6f),
                100, dustColor, Main.rand.NextFloat(0.95f, 1.35f));
            dust.noGravity = true;
        }
    }
}
