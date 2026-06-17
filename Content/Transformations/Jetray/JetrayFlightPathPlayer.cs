using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Jetray;

public sealed class JetrayFlightPathPlayer : ModPlayer {
    private const float GoodPathSpeed = 9f;
    private const int PathMemoryTicks = 36;

    private float pathDiscipline;
    private int pathMemory;

    public float PathDisciplineRatio => MathHelper.Clamp(pathDiscipline, 0f, 1f);
    public int PathDisciplinePercent => (int)Math.Round(PathDisciplineRatio * 100f);
    public bool HasCleanPath => pathMemory > 0 && PathDisciplineRatio >= 0.5f;

    public void UpdateFlightPath(Player player, OmnitrixPlayer omp) {
        if (!IsJetrayActive(omp) || player.dead) {
            pathDiscipline = 0f;
            pathMemory = 0;
            return;
        }

        if (pathMemory > 0)
            pathMemory--;

        float speed = player.velocity.Length();
        float horizontalSpeed = Math.Abs(player.velocity.X);
        float verticalSpeed = Math.Abs(player.velocity.Y);
        float speedRatio = MathHelper.Clamp(speed / GoodPathSpeed, 0f, 1f);
        float horizontalRatio = speed <= 0.05f ? 0f : MathHelper.Clamp(horizontalSpeed / speed, 0f, 1f);
        float diagonalRatio = verticalSpeed > 1f && horizontalSpeed > 2.4f
            ? MathHelper.Clamp(verticalSpeed / (horizontalSpeed + verticalSpeed), 0f, 0.35f)
            : 0f;

        float target = speedRatio * MathHelper.Clamp(horizontalRatio + diagonalRatio, 0f, 1f);
        if (target > 0.28f)
            pathMemory = PathMemoryTicks;

        float smoothing = target > pathDiscipline ? 0.24f : 0.08f;
        pathDiscipline = MathHelper.Lerp(pathDiscipline, target, smoothing);
        if (pathMemory <= 0)
            pathDiscipline *= 0.92f;
    }

    public float ResolveShotQuality(Player player, Vector2 aimDirection) {
        float aimHorizontalRatio = Math.Abs(aimDirection.X);
        float pathRatio = PathDisciplineRatio;
        float forwardStrafeBonus = player.velocity.LengthSquared() > 4f &&
                                  Math.Sign(player.velocity.X) == Math.Sign(aimDirection.X)
            ? 0.12f
            : 0f;

        return MathHelper.Clamp(pathRatio * 0.82f + aimHorizontalRatio * 0.12f + forwardStrafeBonus, 0f, 1f);
    }

    private static bool IsJetrayActive(OmnitrixPlayer omp) {
        return omp != null && string.Equals(omp.currentTransformationId, JetrayTransformation.TransformationId,
            StringComparison.Ordinal);
    }
}
