using System;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.AlienX;

public class AlienXJudgementPlayer : ModPlayer {
    public const int MaxJudgement = 100;
    public const int SingularityCascadeThreshold = 90;

    private const int JudgementGainIntervalTicks = 3;
    private int deliberationTicks;
    private int cascadeRemaining;
    private int cascadeSpawned;
    private int cascadeInterval;
    private int cascadeCount;
    private int cascadeDamage;
    private int cascadeTicks;
    private float cascadeKnockback;
    private float cascadeAmplitude;
    private float cascadeSpeed;
    private float cascadeHalfWaveDistance;
    private Vector2 cascadeOrigin;
    private Vector2 cascadeDirection;

    public int Judgement { get; private set; }
    public float JudgementRatio => Judgement / (float)MaxJudgement;
    public bool HasSingularityCascadeThreshold => Judgement >= SingularityCascadeThreshold;
    public bool SingularityCascadeActive => cascadeCount > 0;
    public int SingularityCascadeRemaining => cascadeRemaining;
    public float SingularityCascadeRatio => cascadeCount <= 0 ? 0f : cascadeRemaining / (float)cascadeCount;

    public void ChannelDeliberation() {
        deliberationTicks++;
        if (Judgement < MaxJudgement && deliberationTicks % JudgementGainIntervalTicks == 0)
            Judgement++;
    }

    public void StopDeliberating() {
        deliberationTicks = 0;
    }

    public bool TryStartSingularityCascade(Player player, Vector2 targetPosition, Vector2 direction, int damage,
        float knockback, int count, int interval, float speed) {
        if (SingularityCascadeActive || !HasSingularityCascadeThreshold)
            return false;

        Judgement = Math.Max(0, Judgement - SingularityCascadeThreshold);
        Vector2 fallbackDirection = direction.SafeNormalize(new Vector2(player.direction, 0f));
        Vector2 travelDirection = (targetPosition - player.Center).SafeNormalize(fallbackDirection);

        cascadeCount = Math.Max(1, count);
        cascadeRemaining = cascadeCount;
        cascadeSpawned = 0;
        cascadeInterval = Math.Max(1, interval);
        cascadeTicks = 0;
        cascadeSpeed = Math.Max(1f, speed);
        cascadeHalfWaveDistance = cascadeSpeed * cascadeInterval;
        cascadeOrigin = player.Center + travelDirection * 72f;
        cascadeDirection = travelDirection;
        cascadeAmplitude = MathHelper.Clamp(cascadeHalfWaveDistance * 0.38f, 54f, 96f);
        cascadeDamage = Math.Max(1, damage);
        cascadeKnockback = knockback;

        SpawnCascadeStartVisuals(player);
        return true;
    }

    public void UpdateSingularityCascade(Player player) {
        if (!SingularityCascadeActive)
            return;

        if (!player.active || player.dead) {
            ClearSingularityCascade();
            return;
        }

        if (Main.netMode == NetmodeID.Server) {
            ClearSingularityCascade();
            return;
        }

        if (player.whoAmI != Main.myPlayer)
            return;

        float previousDistance = cascadeTicks * cascadeSpeed;
        Vector2 previousPosition = ResolveCascadeWavePosition(previousDistance);
        cascadeTicks++;
        float currentDistance = cascadeTicks * cascadeSpeed;
        Vector2 currentPosition = ResolveCascadeWavePosition(currentDistance);
        SpawnCascadeRunnerVisuals(previousPosition, currentPosition);

        while (cascadeSpawned < cascadeCount &&
               currentDistance >= GetCascadeExtremumDistance(cascadeSpawned)) {
            float extremumDistance = GetCascadeExtremumDistance(cascadeSpawned);
            float visualProgress = cascadeCount == 1 ? 1f : cascadeSpawned / (float)(cascadeCount - 1);
            SpawnNextSingularity(player, extremumDistance, visualProgress);
            cascadeSpawned++;
            cascadeRemaining = Math.Max(0, cascadeCount - cascadeSpawned);
        }

        if (currentDistance >= GetCascadeEndDistance()) {
            SpawnCascadeEndVisuals(currentPosition);
            ClearSingularityCascade();
        }
    }

    public void ResetJudgement() {
        Judgement = 0;
        deliberationTicks = 0;
        ClearSingularityCascade();
    }

    private void SpawnNextSingularity(Player player, float distance, float visualProgress) {
        int singularityType = ModContent.ProjectileType<AlienXBlackHoleProjectile>();
        Vector2 spawnPosition = ResolveCascadeWavePosition(distance);
        Vector2 tangent = ResolveCascadeWavePosition(distance + cascadeSpeed) -
            ResolveCascadeWavePosition(Math.Max(0f, distance - cascadeSpeed));
        Vector2 drift = tangent.SafeNormalize(cascadeDirection) * MathHelper.Lerp(3.2f, 1.1f, visualProgress);
        int projectileIndex = Projectile.NewProjectile(player.GetSource_FromThis(), spawnPosition, drift,
            singularityType, cascadeDamage, cascadeKnockback, player.whoAmI);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Projectile projectile = Main.projectile[projectileIndex];
            projectile.timeLeft = Math.Max(projectile.timeLeft, 110);
            projectile.netUpdate = true;
        }

        SpawnCascadeArrivalVisuals(spawnPosition, visualProgress);
    }

    private void ClearSingularityCascade() {
        cascadeRemaining = 0;
        cascadeSpawned = 0;
        cascadeInterval = 0;
        cascadeCount = 0;
        cascadeDamage = 0;
        cascadeTicks = 0;
        cascadeKnockback = 0f;
        cascadeAmplitude = 0f;
        cascadeSpeed = 0f;
        cascadeHalfWaveDistance = 0f;
        cascadeOrigin = Vector2.Zero;
        cascadeDirection = Vector2.Zero;
    }

    private float GetCascadeExtremumDistance(int extremumIndex) {
        return (extremumIndex + 0.5f) * cascadeHalfWaveDistance;
    }

    private float GetCascadeEndDistance() {
        return cascadeCount * cascadeHalfWaveDistance;
    }

    private Vector2 ResolveCascadeWavePosition(float distance) {
        Vector2 centerLine = cascadeOrigin + cascadeDirection * Math.Max(0f, distance);
        Vector2 perpendicular = cascadeDirection.RotatedBy(MathHelper.PiOver2);
        float wave = cascadeHalfWaveDistance <= 0f
            ? 0f
            : (float)Math.Sin(distance / cascadeHalfWaveDistance * MathHelper.Pi);
        return centerLine + perpendicular * wave * cascadeAmplitude;
    }

    private static void SpawnCascadeStartVisuals(Player player) {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.48f, Volume = 0.84f }, player.Center);
        for (int i = 0; i < 20; i++) {
            Vector2 offset = Main.rand.NextVector2CircularEdge(76f, 56f);
            Vector2 position = player.Center + offset;
            Vector2 velocity = (player.Center - position).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2.4f, 4.4f);
            Dust dust = Dust.NewDustPerfect(position,
                Main.rand.NextBool(3) ? DustID.WhiteTorch : DustID.ShadowbeamStaff,
                velocity, 105, new Color(215, 225, 255), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private void SpawnCascadeRunnerVisuals(Vector2 previousPosition, Vector2 currentPosition) {
        if (Main.dedServ)
            return;

        Lighting.AddLight(currentPosition, new Vector3(0.3f, 0.36f, 0.62f));

        Vector2 travel = currentPosition - previousPosition;
        int trailPoints = Math.Max(1, (int)(travel.Length() / 18f));
        for (int i = 0; i <= trailPoints; i++) {
            float progress = trailPoints == 0 ? 1f : i / (float)trailPoints;
            Vector2 position = Vector2.Lerp(previousPosition, currentPosition, progress);
            Vector2 velocity = Main.rand.NextVector2Circular(0.7f, 0.7f);
            int dustType = Main.rand.NextBool(3) ? DustID.WhiteTorch :
                Main.rand.NextBool() ? DustID.GemDiamond : DustID.ShadowbeamStaff;
            Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 110,
                new Color(175, 195, 255), Main.rand.NextFloat(0.8f, 1.15f));
            dust.noGravity = true;
        }
    }

    private static void SpawnCascadeArrivalVisuals(Vector2 center, float progress) {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item122 with {
            Pitch = MathHelper.Lerp(-0.38f, -0.12f, progress),
            Volume = MathHelper.Lerp(0.72f, 0.96f, progress)
        }, center);

        int dustCount = 18 + (int)(progress * 8f);
        for (int i = 0; i < dustCount; i++) {
            float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.08f, 0.08f);
            Vector2 direction = angle.ToRotationVector2();
            Vector2 position = center + direction * Main.rand.NextFloat(14f, 42f);
            Vector2 velocity = direction * Main.rand.NextFloat(1.8f, 4.2f);
            int dustType = i % 3 == 0 ? DustID.WhiteTorch : Main.rand.NextBool() ? DustID.GemDiamond : DustID.BlueTorch;
            Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 95,
                new Color(205, 220, 255), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private static void SpawnCascadeEndVisuals(Vector2 center) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.2f, 2.2f);
            Dust dust = Dust.NewDustPerfect(center, Main.rand.NextBool() ? DustID.GemDiamond : DustID.ShadowbeamStaff,
                velocity, 120, new Color(195, 210, 255), Main.rand.NextFloat(0.8f, 1.2f));
            dust.noGravity = true;
        }
    }
}
