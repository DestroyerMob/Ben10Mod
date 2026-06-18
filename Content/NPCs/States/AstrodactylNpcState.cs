using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct AstrodactylNpcState {
    public int SkyMarkOwner;
    public int SkyMarkTime;

    public bool IsSkyMarkedFor(int owner) => SkyMarkOwner == owner && SkyMarkTime > 0;

    public int GetSkyMarkTime(int owner) => IsSkyMarkedFor(owner) ? SkyMarkTime : 0;

    public void ApplySkyMark(int owner, int time) {
        SkyMarkOwner = owner;
        SkyMarkTime = Utils.Clamp(time, 1, 360);
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (SkyMarkTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(140, 255, 210), 0.28f);
    }

    public void Tick() {
        if (SkyMarkTime > 0) {
            SkyMarkTime--;
        }
        else {
            SkyMarkOwner = -1;
        }
    }
}
