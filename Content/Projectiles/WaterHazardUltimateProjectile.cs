using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WaterHazardUltimateProjectile : ModProjectile {
    private const int LifetimeTicks = 28;
    private const float StartRadius = 34f;
    private const float MaxRadius = 252f;

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
        Projectile.width = 22;
        Projectile.height = 22;
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

        Vector2 aimDirection = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        if (aimDirection == Vector2.Zero)
            aimDirection = new Vector2(owner.direction, 0f);

        Projectile.Center = owner.Center + aimDirection * 42f;

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.7f);
        float radius = MathHelper.Lerp(StartRadius, MaxRadius, easedProgress);
        SpawnWaveDust(radius, PreviousRadius, aimDirection);
        PreviousRadius = radius;
        CurrentRadius = radius;
        Lighting.AddLight(Projectile.Center, new Vector3(0.16f, 0.46f, 0.72f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 180);
    }

    private void SpawnWaveDust(float radius, float previousRadius, Vector2 aimDirection) {
        if (Main.dedServ)
            return;

        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - 28f));
        int points = Math.Max(16, (int)Math.Round(MathHelper.Lerp(16f, 34f, radius / MaxRadius)));
        float baseRotation = aimDirection.ToRotation();

        for (int i = 0; i < points; i++) {
            float angle = baseRotation + MathHelper.Lerp(-1.7f, 1.7f, i / (float)(points - 1));
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction * Main.rand.NextFloat(1.2f, 4.1f);

            Dust dust = Dust.NewDustPerfect(position, i % 5 == 0 ? DustID.DungeonWater : DustID.Water, velocity, 100,
                Color.Lerp(new Color(110, 205, 255), new Color(220, 250, 255), Main.rand.NextFloat()),
                Main.rand.NextFloat(1.05f, 1.5f));
            dust.noGravity = true;
        }
    }
}
