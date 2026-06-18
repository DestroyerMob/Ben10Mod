using Ben10Mod.Content.Transformations.Rath;
using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct RathNpcState {
    public int PreyOwner;
    public int PreyTime;
    public int RendStacks;

    public bool IsPreyFor(int owner) => PreyOwner == owner && PreyTime > 0 && RendStacks > 0;

    public int GetRendStacks(int owner) => IsPreyFor(owner) ? RendStacks : 0;

    public void ApplyPrey(int owner, int stacks, int time) {
        if (PreyOwner != owner) {
            PreyOwner = owner;
            RendStacks = 0;
        }

        RendStacks = Utils.Clamp(RendStacks + System.Math.Max(0, stacks), 1, RathTransformation.RendMaxStacks);
        PreyTime = Utils.Clamp(System.Math.Max(PreyTime, time), 1, RathTransformation.RendDurationTicks);
    }

    public int ConsumeRend(int owner) {
        int stacks = GetRendStacks(owner);
        if (stacks <= 0)
            return 0;

        ClearPrey(owner);
        return stacks;
    }

    public void ClearPrey(int owner = -1) {
        if (owner >= 0 && PreyOwner != owner)
            return;

        PreyOwner = -1;
        PreyTime = 0;
        RendStacks = 0;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (PreyTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(255, 96, 66), 0.16f + RendStacks * 0.04f);
    }

    public void Tick() {
        if (PreyTime > 0) {
            PreyTime--;
        }
        else {
            ClearPrey();
        }
    }
}
