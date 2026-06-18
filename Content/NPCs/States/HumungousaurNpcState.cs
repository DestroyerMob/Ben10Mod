using Ben10Mod.Content.Transformations.Humungousaur;
using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct HumungousaurNpcState {
    public int BreachOwner;
    public int BreachStacks;
    public int BreachTime;
    public int ShatteredOwner;
    public int ShatteredTime;

    public bool IsBreachedFor(int owner) => BreachOwner == owner && BreachTime > 0 && BreachStacks > 0;

    public bool IsShatteredFor(int owner) => ShatteredOwner == owner && ShatteredTime > 0;

    public int GetBreachStacks(int owner) => IsBreachedFor(owner) ? BreachStacks : 0;

    public int GetShatteredTime(int owner) => IsShatteredFor(owner) ? ShatteredTime : 0;

    public void ApplyBreach(int owner, int stacks, int time, int shatteredTime) {
        if (BreachOwner != owner && ShatteredOwner != owner)
            Clear();

        BreachOwner = owner;
        BreachStacks = Utils.Clamp(BreachStacks + stacks, 0, UltimateHumungousaurStatePlayer.BreachMaxStacks);
        BreachTime = Utils.Clamp(System.Math.Max(BreachTime, time), 1, 420);

        if (ShatteredOwner == owner && ShatteredTime > 0)
            ShatteredTime = Utils.Clamp(System.Math.Max(ShatteredTime, time), 1, 420);

        if (BreachStacks >= UltimateHumungousaurStatePlayer.BreachMaxStacks)
            PromoteShattered(owner, shatteredTime);
    }

    public int ConsumeShattered(int owner, int residualStacks = 0) {
        if (!IsShatteredFor(owner))
            return 0;

        int consumed = BreachStacks;
        ShatteredOwner = -1;
        ShatteredTime = 0;

        if (residualStacks > 0) {
            BreachOwner = owner;
            BreachStacks = Utils.Clamp(residualStacks, 0, UltimateHumungousaurStatePlayer.BreachMaxStacks - 1);
            BreachTime = 180;
        }
        else {
            BreachOwner = -1;
            BreachStacks = 0;
            BreachTime = 0;
        }

        return consumed;
    }

    public void ApplyAI(NPC npc) {
        if (ShatteredTime > 0) {
            float dampening = npc.boss ? 0.95f : 0.78f;
            npc.velocity *= dampening;
            if (!npc.boss)
                npc.velocity = Vector2.Clamp(npc.velocity, new Vector2(-3.2f, -3.2f), new Vector2(3.2f, 3.2f));
        }
        else if (BreachTime > 0) {
            float dampening = npc.boss ? 0.985f : 0.92f;
            npc.velocity *= dampening;
        }
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (BreachTime > 0) {
            float breachRatio = BreachStacks / (float)UltimateHumungousaurStatePlayer.BreachMaxStacks;
            Color breachColor = Color.Lerp(new Color(208, 112, 72), new Color(255, 208, 148), breachRatio);
            drawColor = Color.Lerp(drawColor, breachColor, 0.14f + breachRatio * 0.18f);
        }

        if (ShatteredTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(255, 232, 188), 0.24f);
    }

    public void Tick() {
        if (ShatteredTime > 0) {
            ShatteredTime--;
            BreachTime = System.Math.Max(BreachTime - 1, 0);
            if (ShatteredTime <= 0)
                Clear();
        }
        else if (BreachTime > 0) {
            BreachTime--;
            if (BreachTime <= 0 || BreachStacks <= 0)
                Clear();
        }
        else {
            Clear();
        }
    }

    private void PromoteShattered(int owner, int time) {
        BreachOwner = owner;
        BreachStacks = UltimateHumungousaurStatePlayer.BreachMaxStacks;
        BreachTime = Utils.Clamp(time, 1, 420);
        ShatteredOwner = owner;
        ShatteredTime = Utils.Clamp(time, 1, 420);
    }

    private void Clear() {
        BreachOwner = -1;
        BreachStacks = 0;
        BreachTime = 0;
        ShatteredOwner = -1;
        ShatteredTime = 0;
    }
}
