using System;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WayBig;

public class WayBigCombatPlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:WayBig";

    private int commitmentTicks;
    private float commitmentMoveMultiplier = 1f;
    private int rayBraceTicks;

    public int CommitmentTicks => commitmentTicks;
    public bool IsCommitted => commitmentTicks > 0;
    public bool RayBraced => rayBraceTicks > 0;
    public float CommitmentMoveMultiplier => IsCommitted ? commitmentMoveMultiplier : 1f;

    public override void PostUpdate() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != TransformationId) {
            Clear();
            return;
        }

        if (commitmentTicks > 0)
            commitmentTicks--;
        if (rayBraceTicks > 0)
            rayBraceTicks--;

        if (commitmentTicks <= 0)
            commitmentMoveMultiplier = 1f;
    }

    public void RegisterCommitment(int ticks, float moveMultiplier, bool rayBrace = false) {
        if (ticks <= 0)
            return;

        commitmentTicks = Math.Max(commitmentTicks, ticks);
        commitmentMoveMultiplier = Math.Min(commitmentMoveMultiplier, Math.Clamp(moveMultiplier, 0.02f, 1f));

        if (rayBrace)
            rayBraceTicks = Math.Max(rayBraceTicks, ticks);
    }

    private void Clear() {
        commitmentTicks = 0;
        rayBraceTicks = 0;
        commitmentMoveMultiplier = 1f;
    }
}
