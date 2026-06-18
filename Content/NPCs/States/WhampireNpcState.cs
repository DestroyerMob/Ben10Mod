using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct WhampireNpcState {
    public int PreyOwner;
    public int PreyTime;
    public int HypnosisTime;

    public bool IsPreyFor(int owner) => PreyOwner == owner && PreyTime > 0;

    public int GetPreyTime(int owner) => IsPreyFor(owner) ? PreyTime : 0;

    public void ApplyPrey(int owner, int time) {
        PreyOwner = owner;
        PreyTime = Utils.Clamp(time, 1, 420);
    }

    public void ApplyHypnosis(int time) {
        HypnosisTime = Utils.Clamp(time, 1, 240);
    }

    public void ApplyAI(NPC npc) {
        if (HypnosisTime <= 0)
            return;

        float dampening = npc.boss ? 0.92f : 0.55f;
        npc.velocity *= dampening;
        if (!npc.boss)
            npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -0.8f, 0.8f);
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (PreyTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(170, 45, 60), 0.3f);

        if (HypnosisTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(255, 170, 185), 0.36f);
    }

    public void Tick() {
        if (PreyTime > 0) {
            PreyTime--;
        }
        else {
            PreyOwner = -1;
        }

        if (HypnosisTime > 0)
            HypnosisTime--;
    }
}
