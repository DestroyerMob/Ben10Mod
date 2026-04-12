using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.HeatBlast;

public class HeatBlastStatePlayer : ModPlayer {
    public const int FocusDurationTicks = 4 * 60;
    private const int HaloChargeThreshold = 4;

    private bool heatBlastActive;
    private int focusedTargetIndex = -1;
    private int focusedTargetTime;
    private int haloCharge;
    private int queuedHaloShots;

    public bool HeatBlastActive => heatBlastActive;
    public int QueuedHaloShots => heatBlastActive ? queuedHaloShots : 0;

    public override void ResetEffects() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        heatBlastActive = omp.currentTransformationId == "Ben10Mod:HeatBlast";
        if (heatBlastActive)
            return;

        focusedTargetIndex = -1;
        focusedTargetTime = 0;
        haloCharge = 0;
        queuedHaloShots = 0;
    }

    public override void PostUpdate() {
        if (!heatBlastActive)
            return;

        if (focusedTargetTime > 0)
            focusedTargetTime--;

        if (focusedTargetTime <= 0)
            focusedTargetIndex = -1;
    }

    public void RegisterDirectHit(NPC target, bool flameJetHit, bool superheatActive, int queuedShots = 0) {
        if (!heatBlastActive || target == null || !target.active)
            return;

        focusedTargetIndex = target.whoAmI;
        focusedTargetTime = FocusDurationTicks;
        if (queuedShots > 0)
            queuedHaloShots = Utils.Clamp(queuedHaloShots + queuedShots, 0, 8);

        if (!flameJetHit)
            return;

        haloCharge += superheatActive ? 2 : 1;
        while (haloCharge >= HaloChargeThreshold) {
            haloCharge -= HaloChargeThreshold;
            queuedHaloShots = Utils.Clamp(queuedHaloShots + 1, 0, 8);
        }
    }

    public bool TryConsumeHaloQueuedShot() {
        if (!heatBlastActive || queuedHaloShots <= 0)
            return false;

        queuedHaloShots--;
        return true;
    }

    public bool TryGetFocusedTarget(out NPC target) {
        target = null;
        if (!heatBlastActive || focusedTargetTime <= 0 || focusedTargetIndex < 0 || focusedTargetIndex >= Main.maxNPCs)
            return false;

        NPC candidate = Main.npc[focusedTargetIndex];
        if (!candidate.active || !candidate.CanBeChasedBy()) {
            focusedTargetIndex = -1;
            focusedTargetTime = 0;
            return false;
        }

        target = candidate;
        return true;
    }
}
