using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct HeatBlastNpcState {
    public int Owner;
    public int FlashpointStacks;
    public int FlashpointProgress;
    public int FlashpointTime;
    public int FlarePopCooldown;

    public bool HasFlashpointFor(int owner) => Owner == owner && FlashpointTime > 0 && FlashpointStacks > 0;

    public int GetFlashpointStacks(int owner) => HasFlashpointFor(owner) ? FlashpointStacks : 0;

    public int AddFlashpointProgress(int owner, int progress, int refreshTime, int threshold = 3, int maxStacks = 5) {
        if (Owner != owner) {
            ClearFlashpoint();
            Owner = owner;
        }

        FlashpointTime = Utils.Clamp(System.Math.Max(FlashpointTime, refreshTime), 1, 420);
        if (FlashpointStacks >= maxStacks)
            return 0;

        FlashpointProgress += System.Math.Max(1, progress);
        int gained = 0;
        int clampedThreshold = System.Math.Max(1, threshold);
        while (FlashpointProgress >= clampedThreshold && FlashpointStacks < maxStacks) {
            FlashpointProgress -= clampedThreshold;
            FlashpointStacks++;
            gained++;
        }

        if (FlashpointStacks >= maxStacks)
            FlashpointProgress = 0;

        return gained;
    }

    public int ConsumeFlashpoint(int owner) {
        if (Owner != owner || FlashpointStacks <= 0 || FlashpointTime <= 0)
            return 0;

        int consumed = FlashpointStacks;
        ClearFlashpoint();
        return consumed;
    }

    public bool TryTriggerFlarePop(int owner, int cooldown) {
        if (!HasFlashpointFor(owner) || FlashpointStacks < 5 || FlarePopCooldown > 0)
            return false;

        FlarePopCooldown = Utils.Clamp(cooldown, 1, 180);
        return true;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (FlashpointTime <= 0)
            return;

        float flashpointRatio = FlashpointStacks / 5f;
        Color flashpointColor = Color.Lerp(new Color(255, 138, 62), new Color(255, 242, 210), flashpointRatio);
        drawColor = Color.Lerp(drawColor, flashpointColor, 0.12f + flashpointRatio * 0.22f);
    }

    public void Tick() {
        if (FlashpointTime > 0) {
            FlashpointTime--;
            if (FlashpointTime <= 0)
                ClearFlashpoint();
        }
        else {
            ClearFlashpoint();
        }

        if (FlarePopCooldown > 0)
            FlarePopCooldown--;
    }

    private void ClearFlashpoint() {
        Owner = -1;
        FlashpointStacks = 0;
        FlashpointProgress = 0;
        FlashpointTime = 0;
    }
}
