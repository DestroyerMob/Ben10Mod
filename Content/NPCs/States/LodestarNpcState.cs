using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct LodestarNpcState {
    public int PolarityOwner;
    public int PolarityTime;
    public int PolarityDirection;

    public bool HasPolarityFor(int owner) => PolarityOwner == owner && PolarityTime > 0;

    public int GetPolarityTime(int owner) => HasPolarityFor(owner) ? PolarityTime : 0;

    public int GetPolarityDirection(int owner) => HasPolarityFor(owner) ? PolarityDirection : -1;

    public void ApplyPolarity(int owner, int time, int direction) {
        PolarityOwner = owner;
        PolarityTime = Utils.Clamp(time, 1, 300);
        PolarityDirection = direction >= 0 ? 1 : -1;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (PolarityTime > 0)
            drawColor = Color.Lerp(drawColor, PolarityDirection >= 0 ? new Color(255, 120, 95) : new Color(125, 180, 255), 0.2f);
    }

    public void Tick() {
        if (PolarityTime > 0) {
            PolarityTime--;
        }
        else {
            PolarityOwner = -1;
            PolarityDirection = -1;
        }
    }
}
