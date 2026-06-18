using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct AlienXNpcState {
    public int JudgementOwner;
    public int JudgementStacks;
    public int JudgementTime;
    public int StasisTime;

    public bool IsJudgedFor(int owner) => JudgementOwner == owner && JudgementTime > 0 && JudgementStacks > 0;

    public int GetJudgementStacks(int owner) => IsJudgedFor(owner) ? JudgementStacks : 0;

    public void ApplyJudgement(int owner, int stacks, int time) {
        if (JudgementOwner != owner) {
            JudgementOwner = owner;
            JudgementStacks = 0;
        }

        JudgementStacks = Utils.Clamp(JudgementStacks + stacks, 0, 6);
        JudgementTime = Utils.Clamp(time, 1, 420);
    }

    public int ConsumeJudgement(int owner, int amount = 99) {
        int available = GetJudgementStacks(owner);
        if (available <= 0)
            return 0;

        int consumed = System.Math.Min(available, amount);
        JudgementStacks -= consumed;
        if (JudgementStacks <= 0) {
            JudgementOwner = -1;
            JudgementTime = 0;
        }

        return consumed;
    }

    public void ApplyStasis(int owner, int time, int judgementStacks = 0) {
        StasisTime = Utils.Clamp(time, 1, 180);
        if (judgementStacks > 0)
            ApplyJudgement(owner, judgementStacks, time + 90);
    }

    public void ApplyAI(NPC npc) {
        if (StasisTime <= 0)
            return;

        float dampening = npc.boss ? 0.88f : 0.22f;
        npc.velocity *= dampening;
        if (!npc.boss)
            npc.velocity = Vector2.Clamp(npc.velocity, new Vector2(-0.45f, -0.45f), new Vector2(0.45f, 0.45f));
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (JudgementTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(190, 200, 255), 0.16f + JudgementStacks * 0.04f);

        if (StasisTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(240, 245, 255), 0.28f);
    }

    public void Tick() {
        if (JudgementTime > 0) {
            JudgementTime--;
        }
        else {
            JudgementOwner = -1;
            JudgementStacks = 0;
        }

        if (StasisTime > 0)
            StasisTime--;
    }
}
