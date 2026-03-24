using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastSupernovaProjectile : ModProjectile {
    private const int LifetimeTicks = 24;
    private const float StartRadius = 28f;
    private const float MaxRadius = 240f;
    private const int BaseDustPoints = 16;
    private const int MaxDustPoints = 36;

    private float CurrentRadius {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private float PreviousRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;

        if (Projectile.ai[0] == 0f) {
            Projectile.ai[0] = 1f;
            SpawnIgnitionBurst(owner);
        }

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.8f);
        float radius = MathHelper.Lerp(StartRadius, MaxRadius, easedProgress);

        SpawnHeatWaveDust(owner, radius, PreviousRadius);
        PreviousRadius = radius;
        CurrentRadius = radius;

        Vector3 light = owner.GetModPlayer<OmnitrixPlayer>().snowflake
            ? new Vector3(0.45f, 0.8f, 1.1f)
            : new Vector3(1.35f, 0.48f, 0.1f);
        Lighting.AddLight(Projectile.Center, light);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();

        if (omp.snowflake)
            target.AddBuff(BuffID.Frostburn2, 300);
        else
            target.AddBuff(BuffID.OnFire3, 300);
    }

    private void SpawnIgnitionBurst(Player owner) {
        if (Main.dedServ)
            return;

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        int primaryDust = omp.snowflake ? DustID.IceTorch : DustID.Flare;
        int secondaryDust = omp.snowflake ? DustID.SnowflakeIce : DustID.Smoke;
        Color coreColor = omp.snowflake ? new Color(170, 235, 255) : new Color(255, 125, 55);

        for (int i = 0; i < 28; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(4.4f, 4.4f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f), primaryDust,
                velocity, 95, coreColor, Main.rand.NextFloat(1.25f, 1.85f));
            dust.noGravity = true;
        }

        for (int i = 0; i < 14; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(3.1f, 3.1f);
            Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), secondaryDust,
                velocity, 115, coreColor, Main.rand.NextFloat(0.95f, 1.35f));
            smoke.noGravity = true;
        }
    }

    private void SpawnHeatWaveDust(Player owner, float radius, float previousRadius) {
        if (Main.dedServ)
            return;

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        int primaryDust = omp.snowflake ? DustID.IceTorch : DustID.Torch;
        int secondaryDust = omp.snowflake ? DustID.SnowflakeIce : DustID.RedTorch;
        Color startColor = omp.snowflake ? new Color(170, 235, 255) : new Color(255, 110, 55);
        Color endColor = omp.snowflake ? new Color(230, 250, 255) : new Color(255, 205, 120);

        float shellThickness = 28f;
        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - shellThickness));
        float radiusProgress = MathHelper.Clamp(radius / MaxRadius, 0f, 1f);
        int points = Math.Max(BaseDustPoints, (int)Math.Round(MathHelper.Lerp(BaseDustPoints, MaxDustPoints, radiusProgress)));
        float rotation = Main.GlobalTimeWrappedHourly * 2.8f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction * Main.rand.NextFloat(0.9f, 3.2f);

            Dust dust = Dust.NewDustPerfect(position, i % 4 == 0 ? secondaryDust : primaryDust, velocity, 105,
                Color.Lerp(startColor, endColor, Main.rand.NextFloat()), Main.rand.NextFloat(1.05f, 1.55f));
            dust.noGravity = true;
        }
    }
}
