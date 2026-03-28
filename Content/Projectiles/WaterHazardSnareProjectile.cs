using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WaterHazardSnareProjectile : ModProjectile {
    private const int LifetimeTicks = 5 * 60;
    private const float MaxRadius = 84f;
    private float PressureRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);

    private float CurrentRadius {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 24;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        float lifetimeProgress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float fadeOut = Utils.GetLerpValue(0f, 30f, Projectile.timeLeft, true);
        float pulse = 0.82f + 0.18f * (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f)) * 0.5f;
        CurrentRadius = (MaxRadius + 32f * PressureRatio) * (0.45f + 0.55f * fadeOut) * pulse;

        Lighting.AddLight(Projectile.Center, new Vector3(0.08f, 0.28f, 0.45f));
        PullNearbyEnemies();
        SpawnSnareDust(lifetimeProgress);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().AddWaterHazardSoak(Projectile.owner, 18 + (int)Math.Round(PressureRatio * 14f),
            260);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 14; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.8f, 2.8f);
            Dust splash = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.DungeonWater : DustID.Water,
                velocity, 95, new Color(165, 230, 255), Main.rand.NextFloat(0.95f, 1.3f));
            splash.noGravity = true;
        }
    }

    private void SpawnSnareDust(float lifetimeProgress) {
        if (Main.dedServ)
            return;

        int points = Math.Max(9, (int)Math.Round(CurrentRadius / 10f));
        float rotation = Main.GlobalTimeWrappedHourly * (2f + lifetimeProgress * 1.5f);

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            Vector2 position = Projectile.Center + direction * Main.rand.NextFloat(CurrentRadius * 0.4f, CurrentRadius);
            Vector2 velocity = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.4f, 1.8f);

            Dust dust = Dust.NewDustPerfect(position, i % 4 == 0 ? DustID.DungeonWater : DustID.Water, velocity, 110,
                new Color(120, 210, 255), Main.rand.NextFloat(0.9f, 1.2f));
            dust.noGravity = true;
        }
    }

    private void PullNearbyEnemies() {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance > CurrentRadius || distance <= 8f)
                continue;

            Vector2 inward = (Projectile.Center - npc.Center).SafeNormalize(Vector2.UnitY);
            float pullStrength = MathHelper.Lerp(1.1f, 5.6f + 2.4f * PressureRatio, 1f - distance / CurrentRadius);
            npc.velocity = Vector2.Lerp(npc.velocity, inward * pullStrength, npc.boss ? 0.06f : 0.18f);
            npc.GetGlobalNPC<AlienIdentityGlobalNPC>().AddWaterHazardSoak(Projectile.owner, 1, 45);
            npc.netUpdate = true;
        }
    }
}
