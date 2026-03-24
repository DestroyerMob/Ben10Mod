using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class TerraspinBurstProjectile : ModProjectile {
    private const int LifetimeTicks = 24;
    private const float StartRadius = 34f;
    private const float MaxRadius = 94f;

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
        owner.noKnockback = true;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, 2);
        owner.velocity.X *= 0.9f;
        owner.fallStart = (int)(owner.position.Y / 16f);

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.1f);
        float radius = MathHelper.Lerp(StartRadius, MaxRadius, easedProgress);
        SpawnBurstDust(radius, PreviousRadius);
        PreviousRadius = radius;
        CurrentRadius = radius;
        Lighting.AddLight(Projectile.Center, new Vector3(0.22f, 0.25f, 0.28f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 105);
        Vector2 pushDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
        target.velocity = Vector2.Lerp(target.velocity, pushDirection * 5.2f + new Vector2(0f, -2.4f), 0.7f);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active)
            owner.noKnockback = false;
    }

    private void SpawnBurstDust(float radius, float previousRadius) {
        if (Main.dedServ)
            return;

        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - 16f));
        int points = Math.Max(12, (int)Math.Round(radius / 7f));
        float rotation = Main.GlobalTimeWrappedHourly * 5.2f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(1.2f, 4f);

            Dust dust = Dust.NewDustPerfect(position, i % 3 == 0 ? DustID.SilverCoin : DustID.Smoke, velocity, 100,
                Color.Lerp(new Color(200, 230, 235), Color.White, Main.rand.NextFloat()), Main.rand.NextFloat(0.95f, 1.28f));
            dust.noGravity = true;
        }
    }
}
