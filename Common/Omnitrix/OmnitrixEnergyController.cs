using System;

namespace Ben10Mod.Common.Omnitrix;

public sealed class OmnitrixEnergyController {
    private float _current;
    private float _max;
    private float _regen;

    public float Current {
        get => _current;
        set {
            _current = Math.Max(0f, value);
            if (Max > 0f)
                ClampToMax();
        }
    }

    public float Max {
        get => _max;
        set {
            _max = Math.Max(0f, value);
            if (Max > 0f)
                ClampToMax();
        }
    }

    public float Regen {
        get => _regen;
        set => _regen = Math.Max(0f, value);
    }

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
