using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.XLR8;

public sealed class XLR8MomentumPlayer : ModPlayer {
    public const int ReversalWindowTicks = 30;
    public const int MomentumMemoryTicks = 36;
    public const int MaxTimebreakStacks = 6;

    private const float ReversalSpeedThreshold = 3.2f;
    private const float MovingSpeedThreshold = 2.1f;
    private const float MomentumReferenceSpeed = 18f;
    private const int TimebreakFlowDuration = 150;

    private int lastHorizontalDirection;
    private int reversalWindow;
    private int movingWindow;
    private int timebreakStacks;
    private int timebreakStackTime;
    private int enemyPassCooldown;
    private int projectilePassCooldown;
    private float maxRecentHorizontalSpeed;

    public bool ReversalReady => reversalWindow > 0;
    public bool MovingRecently => movingWindow > 0;
    public int TimebreakStacks => timebreakStacks;
    public float MomentumRatio => MathHelper.Clamp(maxRecentHorizontalSpeed / MomentumReferenceSpeed, 0f, 1f);
    public float TimebreakFlowRatio => MathHelper.Clamp(timebreakStacks / (float)MaxTimebreakStacks, 0f, 1f);
    public int MomentumPercent => (int)Math.Round(MomentumRatio * 100f);

    public float StationaryDamageScale => MovingRecently ? 1f : 0.78f;
    public float ReversalDamageScale => ReversalReady ? 1.22f + MomentumRatio * 0.18f : 1f;
    public float TimebreakDamageScale => 1f + TimebreakFlowRatio * 0.22f;

    public void UpdateMomentum(Player player, OmnitrixPlayer omp) {
        if (!IsXLR8Active(omp) || player.dead) {
            ResetMomentum();
            return;
        }

        if (reversalWindow > 0)
            reversalWindow--;
        if (movingWindow > 0)
            movingWindow--;
        if (enemyPassCooldown > 0)
            enemyPassCooldown--;
        if (projectilePassCooldown > 0)
            projectilePassCooldown--;

        UpdateTimebreakFlow();

        float horizontalSpeed = Math.Abs(player.velocity.X);
        maxRecentHorizontalSpeed = Math.Max(horizontalSpeed, maxRecentHorizontalSpeed * 0.94f);
        if (maxRecentHorizontalSpeed < 0.08f)
            maxRecentHorizontalSpeed = 0f;

        if (horizontalSpeed > MovingSpeedThreshold)
            movingWindow = MomentumMemoryTicks;

        int currentDirection = horizontalSpeed > 0.75f ? Math.Sign(player.velocity.X) : 0;
        if (currentDirection != 0) {
            if (lastHorizontalDirection != 0 &&
                currentDirection != lastHorizontalDirection &&
                horizontalSpeed >= ReversalSpeedThreshold) {
                reversalWindow = ReversalWindowTicks;
                movingWindow = MomentumMemoryTicks;
                maxRecentHorizontalSpeed = Math.Max(maxRecentHorizontalSpeed, horizontalSpeed + 4f);
            }

            lastHorizontalDirection = currentDirection;
        }
    }

    public float ResolveVelocityDashPower(Player player) {
        float speedRatio = MathHelper.Clamp(Math.Abs(player.velocity.X) / 22f, 0f, 1f);
        return MathHelper.Clamp(speedRatio * 0.7f + MomentumRatio * 0.3f, 0.08f, 1f);
    }

    public float ResolveVectorDashPower(float requestedDistance, float maxRange) {
        float distanceRatio = maxRange <= 0f ? 0f : MathHelper.Clamp(requestedDistance / maxRange, 0f, 1f);
        return MathHelper.Clamp(distanceRatio * 0.68f + MomentumRatio * 0.32f, 0.08f, 1f);
    }

    public void RegisterTimebreakEnemyPass(Player player, NPC npc) {
        if (enemyPassCooldown > 0 || npc == null || !npc.active)
            return;

        enemyPassCooldown = npc.boss ? 12 : 8;
        AddTimebreakFlow(player, npc.Center, npc.boss ? 2 : 1);
    }

    public void RegisterTimebreakProjectilePass(Player player, Projectile projectile) {
        if (projectilePassCooldown > 0 || projectile == null || !projectile.active || !projectile.hostile)
            return;

        projectilePassCooldown = 5;
        AddTimebreakFlow(player, projectile.Center, 1);
    }

    public static bool IsReversalStrike(Projectile projectile) {
        return projectile?.type == ModContent.ProjectileType<global::Ben10Mod.Content.Projectiles.XLR8StarlightProjectile>() &&
               projectile.ai[0] >= 4f;
    }

    private void AddTimebreakFlow(Player player, Vector2 source, int stacks) {
        timebreakStacks = Math.Min(MaxTimebreakStacks, timebreakStacks + Math.Max(1, stacks));
        timebreakStackTime = TimebreakFlowDuration;
        reversalWindow = Math.Max(reversalWindow, ReversalWindowTicks / 2);
        movingWindow = MomentumMemoryTicks;
        maxRecentHorizontalSpeed = Math.Max(maxRecentHorizontalSpeed, MomentumReferenceSpeed * 0.78f);

        player.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(0.55f + 0.2f * Math.Max(0, stacks - 1));

        if (Main.dedServ)
            return;

        for (int i = 0; i < 8 + stacks * 4; i++) {
            Vector2 velocity = player.DirectionFrom(source).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.55f) *
                Main.rand.NextFloat(1.2f, 4.2f);
            Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(10f, 14f),
                i % 4 == 0 ? DustID.WhiteTorch : DustID.BlueCrystalShard, velocity, 100,
                new Color(128, 226, 255), Main.rand.NextFloat(0.9f, 1.35f));
            dust.noGravity = true;
        }
    }

    private void UpdateTimebreakFlow() {
        if (timebreakStacks <= 0)
            return;

        if (timebreakStackTime > 0) {
            timebreakStackTime--;
            return;
        }

        timebreakStacks--;
        timebreakStackTime = timebreakStacks > 0 ? 20 : 0;
    }

    private void ResetMomentum() {
        lastHorizontalDirection = 0;
        reversalWindow = 0;
        movingWindow = 0;
        timebreakStacks = 0;
        timebreakStackTime = 0;
        enemyPassCooldown = 0;
        projectilePassCooldown = 0;
        maxRecentHorizontalSpeed = 0f;
    }

    private static bool IsXLR8Active(OmnitrixPlayer omp) {
        return omp != null && string.Equals(omp.currentTransformationId, XLR8Transformation.TransformationId, StringComparison.Ordinal);
    }
}
