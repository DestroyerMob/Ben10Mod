using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class NRGBurstProjectile : ModProjectile {
    private const int LifetimeTicks = 18;
    private const float StartRadius = 14f;
    private const float MaxRadius = 176f;
    private const int MaxDustPoints = 30;

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
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        if (Projectile.ai[0] == 0f) {
            Projectile.ai[0] = 1f;
            SpawnIgnitionDust();
        }

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 3f);
        float radius = MathHelper.Lerp(StartRadius, MaxRadius, easedProgress);

        SpawnExpandingBurstDust(radius, PreviousRadius);
        PreviousRadius = radius;
        CurrentRadius = radius;
        Lighting.AddLight(Projectile.Center, 1.25f, 0.22f, 0.06f);
    }

    private void SpawnIgnitionDust() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 22; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.8f, 2.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.RedTorch,
                velocity, 95, new Color(255, 90, 50), Main.rand.NextFloat(1.3f, 1.8f));
            dust.noGravity = true;
        }

        for (int i = 0; i < 10; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.4f, 2.4f);
            Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), DustID.Smoke,
                velocity, 115, new Color(255, 120, 80), Main.rand.NextFloat(0.9f, 1.3f));
            smoke.noGravity = true;
        }
    }

    private void SpawnExpandingBurstDust(float radius, float previousRadius) {
        if (Main.dedServ)
            return;

        float shellThickness = 18f;
        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - shellThickness));
        float radiusProgress = MathHelper.Clamp(radius / MaxRadius, 0f, 1f);
        int points = Math.Max(6, (int)Math.Round(radiusProgress * MaxDustPoints));
        float rotation = Main.GlobalTimeWrappedHourly * 2.4f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction * Main.rand.NextFloat(0.6f, 2.1f);

            Dust dust = Dust.NewDustPerfect(position, i % 4 == 0 ? DustID.Torch : DustID.RedTorch, velocity, 110,
                Color.Lerp(new Color(255, 80, 45), new Color(255, 170, 90), Main.rand.NextFloat()),
                Main.rand.NextFloat(1.05f, 1.45f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 300);
    }
}
