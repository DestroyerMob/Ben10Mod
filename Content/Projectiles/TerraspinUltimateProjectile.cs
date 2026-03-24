using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class TerraspinUltimateProjectile : ModProjectile {
    private const int LifetimeTicks = 40;
    private const float StartRadius = 56f;
    private const float MaxRadius = 168f;

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

        Projectile.Center = owner.Center;
        owner.noKnockback = true;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, 2);
        owner.velocity *= 0.88f;
        owner.fallStart = (int)(owner.position.Y / 16f);

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 1.9f);
        float radius = MathHelper.Lerp(StartRadius, MaxRadius, easedProgress);
        SpawnStormDust(radius, PreviousRadius, aimDirection);
        PreviousRadius = radius;
        CurrentRadius = radius;
        Lighting.AddLight(Projectile.Center, new Vector3(0.26f, 0.32f, 0.36f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 180);
        Vector2 launchDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
        target.velocity = Vector2.Lerp(target.velocity, launchDirection * 6.2f + new Vector2(0f, -3.2f), 0.72f);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active)
            owner.noKnockback = false;
    }

    private void SpawnStormDust(float radius, float previousRadius, Vector2 aimDirection) {
        if (Main.dedServ)
            return;

        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - 26f));
        int points = Math.Max(18, (int)Math.Round(MathHelper.Lerp(18f, 38f, radius / MaxRadius)));
        float baseRotation = aimDirection.ToRotation() + Main.GlobalTimeWrappedHourly * 6.2f;

        for (int i = 0; i < points; i++) {
            float angle = baseRotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(1.6f, 4.8f);

            Dust dust = Dust.NewDustPerfect(position, i % 4 == 0 ? DustID.SilverCoin : DustID.Smoke, velocity, 100,
                Color.Lerp(new Color(210, 235, 235), Color.White, Main.rand.NextFloat()), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }
}
