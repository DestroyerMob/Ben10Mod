using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct BlitzwolferNpcState {
    public int ResonanceOwner;
    public int ResonanceStacks;
    public int ResonanceTime;

    public bool IsResonantFor(int owner) => ResonanceOwner == owner && ResonanceTime > 0 && ResonanceStacks > 0;

    public int GetResonanceStacks(int owner) => IsResonantFor(owner) ? ResonanceStacks : 0;

    public void ApplyResonance(int owner, int stacks, int time) {
        if (ResonanceOwner != owner) {
            ResonanceOwner = owner;
            ResonanceStacks = 0;
        }

        ResonanceStacks = Utils.Clamp(ResonanceStacks + stacks, 0, 8);
        ResonanceTime = Utils.Clamp(time, 1, 360);
    }

    public int ConsumeResonance(int owner) {
        int stacks = GetResonanceStacks(owner);
        if (stacks <= 0)
            return 0;

        ResonanceStacks = 0;
        ResonanceTime = 0;
        ResonanceOwner = -1;
        return stacks;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (ResonanceTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(115, 255, 145), 0.16f + ResonanceStacks * 0.03f);
    }

    public void Tick() {
        if (ResonanceTime > 0) {
            ResonanceTime--;
        }
        else {
            ResonanceOwner = -1;
            ResonanceStacks = 0;
        }
    }
}
