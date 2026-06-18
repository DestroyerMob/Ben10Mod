using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct SnareOhNpcState {
    public int CurseOwner;
    public int CurseStacks;
    public int CurseTime;

    public bool IsCursedFor(int owner) => CurseOwner == owner && CurseTime > 0 && CurseStacks > 0;

    public int GetCurseStacks(int owner) => IsCursedFor(owner) ? CurseStacks : 0;

    public void ApplyCurse(int owner, int stacks, int time) {
        if (CurseOwner != owner) {
            CurseOwner = owner;
            CurseStacks = 0;
        }

        CurseStacks = Utils.Clamp(CurseStacks + stacks, 0, 7);
        CurseTime = Utils.Clamp(time, 1, 360);
    }

    public int ConsumeCurse(int owner, int amount) {
        int available = GetCurseStacks(owner);
        if (available <= 0)
            return 0;

        int consumed = System.Math.Min(available, amount);
        CurseStacks -= consumed;
        if (CurseStacks <= 0) {
            CurseOwner = -1;
            CurseTime = 0;
        }

        return consumed;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (CurseTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(220, 190, 120), 0.14f + CurseStacks * 0.03f);
    }

    public void Tick() {
        if (CurseTime > 0) {
            CurseTime--;
        }
        else {
            CurseOwner = -1;
            CurseStacks = 0;
        }
    }
}
