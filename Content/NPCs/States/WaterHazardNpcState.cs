using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct WaterHazardNpcState {
    public int SoakOwner;
    public int Soak;
    public int SoakTime;

    public bool IsSoakedFor(int owner) => SoakOwner == owner && SoakTime > 0 && Soak > 0;

    public int GetSoak(int owner) => IsSoakedFor(owner) ? Soak : 0;

    public void AddSoak(int owner, int amount, int refreshTime) {
        SoakOwner = owner;
        Soak = Utils.Clamp(Soak + amount, 0, 100);
        SoakTime = Utils.Clamp(refreshTime, 1, 360);
    }

    public int ConsumeSoak(int owner, int amount) {
        int soaked = GetSoak(owner);
        if (soaked <= 0)
            return 0;

        int consumed = System.Math.Min(soaked, amount);
        Soak -= consumed;
        if (Soak <= 0) {
            SoakOwner = -1;
            SoakTime = 0;
        }

        return consumed;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (SoakTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(120, 215, 255), 0.08f + 0.18f * (Soak / 100f));
    }

    public void Tick() {
        if (SoakTime > 0) {
            SoakTime--;
            if (SoakTime % 45 == 0)
                Soak = System.Math.Max(0, Soak - 6);
        }
        else {
            SoakOwner = -1;
            Soak = 0;
        }
    }
}
