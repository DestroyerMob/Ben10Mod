using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct EchoEchoNpcState {
    public int ResonanceOwner;
    public int ResonanceStacks;
    public int ResonanceTime;
    public int ResonanceLastSource;
    public bool ResonancePrimed;
    public int FractureOwner;
    public int FractureTime;
    public int FocusedOwner;
    public int FocusedTime;

    public bool IsResonantFor(int owner) => ResonanceOwner == owner && ResonanceTime > 0 && ResonanceStacks > 0;

    public bool IsResonancePrimedFor(int owner) => IsResonantFor(owner) && ResonancePrimed;

    public bool IsFracturedFor(int owner) => FractureOwner == owner && FractureTime > 0;

    public bool IsFocusedFor(int owner) => FocusedOwner == owner && FocusedTime > 0;

    public int GetResonanceStacks(int owner) => IsResonantFor(owner) ? ResonanceStacks : 0;

    public int GetFractureTime(int owner) => IsFracturedFor(owner) ? FractureTime : 0;

    public int GetFocusTime(int owner) => IsFocusedFor(owner) ? FocusedTime : 0;

    public void ApplyResonance(int owner, int sourceId, int stacks, int time) {
        if (ResonanceOwner != owner || ResonanceTime <= 0) {
            ResonanceOwner = owner;
            ResonanceStacks = 0;
            ResonancePrimed = false;
            ResonanceLastSource = -1;
        }

        int gain = Utils.Clamp(stacks, 1, 3);
        if (ResonanceLastSource >= 0 && ResonanceLastSource != sourceId)
            gain++;

        ResonanceStacks = Utils.Clamp(ResonanceStacks + gain, 0, 8);
        ResonancePrimed = ResonanceStacks >= 8;
        ResonanceLastSource = sourceId;
        ResonanceTime = Utils.Clamp(time, 1, 360);
    }

    public int ConsumeResonance(int owner) {
        int stacks = GetResonanceStacks(owner);
        if (stacks <= 0)
            return 0;

        ResonanceOwner = -1;
        ResonanceStacks = 0;
        ResonanceTime = 0;
        ResonanceLastSource = -1;
        ResonancePrimed = false;
        return stacks;
    }

    public void ApplyFracture(int owner, int time) {
        FractureOwner = owner;
        FractureTime = Utils.Clamp(time, 1, 240);
    }

    public void ApplyFocus(int owner, int time) {
        FocusedOwner = owner;
        FocusedTime = Utils.Clamp(System.Math.Max(FocusedTime, time), 1, 300);
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (ResonanceTime > 0)
            drawColor = Color.Lerp(drawColor, ResonancePrimed ? new Color(178, 238, 255) : new Color(150, 205, 255),
                0.12f + ResonanceStacks * 0.025f);

        if (FractureTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(205, 235, 255), 0.18f);

        if (FocusedTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(165, 228, 255), 0.28f);
    }

    public void Tick() {
        if (ResonanceTime > 0) {
            ResonanceTime--;
        }
        else {
            ResonanceOwner = -1;
            ResonanceStacks = 0;
            ResonanceLastSource = -1;
            ResonancePrimed = false;
        }

        if (FractureTime > 0) {
            FractureTime--;
        }
        else {
            FractureOwner = -1;
        }

        if (FocusedTime > 0) {
            FocusedTime--;
        }
        else {
            FocusedOwner = -1;
        }
    }
}
