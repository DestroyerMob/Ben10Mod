using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class CannonboltImpactBurstProjectile : ModProjectile {
    private const int LifetimeTicks = 18;
    private const float BaseStartRadius = 24f;
    private const float BaseMaxRadius = 164f;

    private float RadiusScale => Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0];

    private float CurrentRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float PreviousRadius {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
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
        if (Projectile.ai[1] == 0f) {
            Projectile.ai[1] = 1f;
            SpawnLaunchBurst();
        }

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.5f);
        float startRadius = BaseStartRadius * RadiusScale;
        float maxRadius = BaseMaxRadius * RadiusScale;
        float radius = MathHelper.Lerp(startRadius, maxRadius, easedProgress);

        SpawnWaveDust(radius, PreviousRadius, maxRadius);
        PreviousRadius = radius;
        CurrentRadius = radius;
        Lighting.AddLight(Projectile.Center, new Vector3(0.55f, 0.52f, 0.42f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 90);
        if (RadiusScale > 1.05f)
            target.AddBuff(BuffID.BrokenArmor, 180);
    }

    private void SpawnLaunchBurst() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 18; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(3.6f, 3.6f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                i % 3 == 0 ? DustID.Smoke : DustID.GemTopaz, velocity, 105, Color.White, Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private void SpawnWaveDust(float radius, float previousRadius, float maxRadius) {
        if (Main.dedServ)
            return;

        float shellThickness = 22f * RadiusScale;
        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - shellThickness));
        float radiusProgress = MathHelper.Clamp(radius / maxRadius, 0f, 1f);
        int points = Math.Max(10, (int)Math.Round(MathHelper.Lerp(10f, 24f, radiusProgress)));
        float rotation = Main.GlobalTimeWrappedHourly * 2.2f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction * Main.rand.NextFloat(0.8f, 2.8f);

            int dustType = i % 4 == 0 ? DustID.Smoke : DustID.GemTopaz;
            Color dustColor = Color.Lerp(new Color(215, 205, 170), Color.White, Main.rand.NextFloat());
            Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 110, dustColor, Main.rand.NextFloat(0.95f, 1.35f));
            dust.noGravity = true;
        }
    }
}
