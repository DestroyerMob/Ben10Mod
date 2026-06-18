using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct PeskyDustNpcState {
    public int Owner;
    public int Drowsy;
    public int DrowsyTime;
    public int DreamTime;

    public bool IsDreamboundFor(int owner) => Owner == owner && DreamTime > 0;

    public int GetDreamTime(int owner) => IsDreamboundFor(owner) ? DreamTime : 0;

    public void AddDrowsy(int owner, int amount, int refreshTime, int dreamThreshold, int dreamTime) {
        Owner = owner;
        Drowsy = Utils.Clamp(Drowsy + amount, 0, dreamThreshold);
        DrowsyTime = Utils.Clamp(refreshTime, 1, 300);

        if (Drowsy >= dreamThreshold)
            ApplyDreambound(owner, dreamTime, dreamThreshold / 3);
    }

    public void ApplyDreambound(int owner, int dreamTime, int residualDrowsy = 0) {
        Owner = owner;
        DreamTime = Utils.Clamp(dreamTime, 1, 360);
        Drowsy = Utils.Clamp(residualDrowsy, 0, 99);
        DrowsyTime = Utils.Clamp(dreamTime / 2, 1, 180);
    }

    public void ApplyAI(NPC npc) {
        if (DreamTime <= 0)
            return;

        float driftDampening = npc.boss ? 0.96f : 0.78f;
        npc.velocity *= driftDampening;
        if (!npc.boss)
            npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y - 0.05f, -1.6f, 1.2f);
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (DreamTime > 0) {
            drawColor = Color.Lerp(drawColor, new Color(255, 228, 180), 0.34f);
        }
        else if (Drowsy > 0) {
            drawColor = Color.Lerp(drawColor, new Color(255, 210, 235), 0.12f + 0.18f * (Drowsy / 100f));
        }
    }

    public void Tick() {
        if (DrowsyTime > 0) {
            DrowsyTime--;
            if (DrowsyTime % 40 == 0 && DreamTime <= 0)
                Drowsy = System.Math.Max(0, Drowsy - 8);
        }
        else {
            Drowsy = System.Math.Max(0, Drowsy - 12);
        }

        if (DreamTime > 0) {
            DreamTime--;
        }
        else if (Owner != -1 && Drowsy <= 0) {
            Owner = -1;
        }
    }
}
