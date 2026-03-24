using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WaterHazardBurstProjectile : ModProjectile {
    private const int LifetimeTicks = 18;
    private const float StartRadius = 20f;
    private const float MaxRadius = 112f;

    private float CurrentRadius {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
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

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.3f);
        float radius = MathHelper.Lerp(StartRadius, MaxRadius, easedProgress);
        SpawnBurstDust(radius, PreviousRadius);
        PreviousRadius = radius;
        CurrentRadius = radius;
        Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.32f, 0.52f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 90);
    }

    private void SpawnBurstDust(float radius, float previousRadius) {
        if (Main.dedServ)
            return;

        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - 20f));
        int points = Math.Max(10, (int)Math.Round(radius / 8f));
        float rotation = Main.GlobalTimeWrappedHourly * 2.5f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction * Main.rand.NextFloat(0.8f, 3.2f);

            Dust dust = Dust.NewDustPerfect(position, i % 3 == 0 ? DustID.DungeonWater : DustID.Water, velocity, 100,
                Color.Lerp(new Color(95, 190, 255), new Color(205, 245, 255), Main.rand.NextFloat()),
                Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }
}
