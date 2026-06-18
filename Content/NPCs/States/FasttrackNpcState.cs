using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct FasttrackNpcState {
    public int ComboOwner;
    public int ComboStacks;
    public int ComboTime;

    public bool IsComboActiveFor(int owner) => ComboOwner == owner && ComboTime > 0 && ComboStacks > 0;

    public int GetComboStacks(int owner) => IsComboActiveFor(owner) ? ComboStacks : 0;

    public void ApplyCombo(int owner, int stacks, int time) {
        if (ComboOwner != owner) {
            ComboOwner = owner;
            ComboStacks = 0;
        }

        ComboStacks = Utils.Clamp(ComboStacks + stacks, 0, 6);
        ComboTime = Utils.Clamp(time, 1, 240);
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (ComboTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(110, 255, 210), 0.2f + ComboStacks * 0.04f);
    }

    public void Tick() {
        if (ComboTime > 0) {
            ComboTime--;
        }
        else {
            ComboOwner = -1;
            ComboStacks = 0;
        }
    }
}
