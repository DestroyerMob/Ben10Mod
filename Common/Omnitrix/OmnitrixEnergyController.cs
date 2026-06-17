using System;

namespace Ben10Mod.Common.Omnitrix;

public sealed class OmnitrixEnergyController {
    public float Current { get; set; }
    public float Max { get; set; }
    public float Regen { get; set; }
    public int MaxBonus { get; set; }
    public int RegenBonus { get; set; }

    public void ResetEffectiveStats() {
        Max = 0f;
        Regen = 0f;
        MaxBonus = 0;
        RegenBonus = 0;
    }

    public void RegeneratePerTick() {
        Current += Regen / 120f;
        ClampToMax();
    }

    public void ClampToMax() {
        if (Current > Max)
            Current = Max;
    }

    public bool CanRestore(float amount = 1f) {
        return amount > 0f && Max > 0f && Current < Max;
    }

    public float Restore(float amount) {
        if (!CanRestore(amount))
            return 0f;

        float previousEnergy = Current;
        Current = Math.Min(Max, Current + amount);
        return Current - previousEnergy;
    }

    public bool CanSpend(float amount) {
        return amount <= 0f || Current >= amount;
    }

    public bool TrySpend(float amount) {
        if (!CanSpend(amount))
            return false;

        Current -= Math.Max(0f, amount);
        return true;
    }
}
