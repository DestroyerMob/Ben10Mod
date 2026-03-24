using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SnareOhUltimateProjectile : ModProjectile {
    private const int LifetimeTicks = 42;
    private const float StartRadius = 48f;
    private const float MaxRadius = 178f;

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
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.1f);
        float radius = MathHelper.Lerp(StartRadius, MaxRadius, easedProgress);

        HoldNearbyEnemies(radius);
        SpawnShroudDust(radius, PreviousRadius);
        PreviousRadius = radius;
        CurrentRadius = radius;
        Lighting.AddLight(Projectile.Center, new Vector3(0.45f, 0.34f, 0.14f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool exposedCore = OwnerExposedCore();
        target.AddBuff(ModContent.BuffType<EnemySlow>(), exposedCore ? 270 : 210);

        Vector2 toCenter = (Projectile.Center - target.Center).SafeNormalize(Vector2.Zero);
        target.velocity = Vector2.Lerp(target.velocity, toCenter * 2.2f, 0.7f);
        target.velocity *= 0.18f;

        if (exposedCore)
            target.AddBuff(BuffID.BrokenArmor, 300);

        target.netUpdate = true;
    }

    private void HoldNearbyEnemies(float radius) {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance > radius || distance <= 6f)
                continue;

            Vector2 toCenter = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
            float pullStrength = MathHelper.Lerp(0.28f, 0.1f, distance / radius);
            Vector2 desiredVelocity = toCenter * MathHelper.Lerp(2.6f, 0.8f, distance / radius);
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, pullStrength);
        }
    }

    private void SpawnShroudDust(float radius, float previousRadius) {
        if (Main.dedServ)
            return;

        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - 24f));
        int points = Math.Max(18, (int)Math.Round(MathHelper.Lerp(18f, 34f, radius / MaxRadius)));
        float rotation = Main.GlobalTimeWrappedHourly * 5.8f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(1.2f, 3.4f);

            Dust dust = Dust.NewDustPerfect(position, i % 5 == 0 ? DustID.GoldFlame : DustID.Sand, velocity, 105,
                new Color(240, 215, 155), Main.rand.NextFloat(0.95f, 1.28f));
            dust.noGravity = true;
        }
    }

    private bool OwnerExposedCore() {
        Player owner = Main.player[Projectile.owner];
        return owner.active && !owner.dead && owner.GetModPlayer<OmnitrixPlayer>().PrimaryAbilityEnabled;
    }
}
