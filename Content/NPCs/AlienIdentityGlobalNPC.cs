using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.NPCs;

public class AlienIdentityGlobalNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public int FasttrackComboOwner = -1;
    public int FasttrackComboStacks;
    public int FasttrackComboTime;

    public int AstrodactylSkyMarkOwner = -1;
    public int AstrodactylSkyMarkTime;

    public int WhampirePreyOwner = -1;
    public int WhampirePreyTime;
    public int WhampireHypnosisTime;

    public int PeskyDustOwner = -1;
    public int PeskyDustDrowsy;
    public int PeskyDustDrowsyTime;
    public int PeskyDustDreamTime;

    public bool IsFasttrackComboActiveFor(int owner) => FasttrackComboOwner == owner && FasttrackComboTime > 0 && FasttrackComboStacks > 0;
    public bool IsSkyMarkedFor(int owner) => AstrodactylSkyMarkOwner == owner && AstrodactylSkyMarkTime > 0;
    public bool IsWhampirePreyFor(int owner) => WhampirePreyOwner == owner && WhampirePreyTime > 0;
    public bool IsDreamboundFor(int owner) => PeskyDustOwner == owner && PeskyDustDreamTime > 0;

    public void ApplyFasttrackCombo(int owner, int stacks, int time) {
        if (FasttrackComboOwner != owner) {
            FasttrackComboOwner = owner;
            FasttrackComboStacks = 0;
        }

        FasttrackComboStacks = Utils.Clamp(FasttrackComboStacks + stacks, 0, 6);
        FasttrackComboTime = Utils.Clamp(time, 1, 240);
    }

    public void ApplySkyMark(int owner, int time) {
        AstrodactylSkyMarkOwner = owner;
        AstrodactylSkyMarkTime = Utils.Clamp(time, 1, 360);
    }

    public void ApplyWhampirePrey(int owner, int time) {
        WhampirePreyOwner = owner;
        WhampirePreyTime = Utils.Clamp(time, 1, 420);
    }

    public void ApplyWhampireHypnosis(int time) {
        WhampireHypnosisTime = Utils.Clamp(time, 1, 240);
    }

    public void AddPeskyDrowsy(int owner, int amount, int refreshTime, int dreamThreshold, int dreamTime) {
        PeskyDustOwner = owner;
        PeskyDustDrowsy = Utils.Clamp(PeskyDustDrowsy + amount, 0, dreamThreshold);
        PeskyDustDrowsyTime = Utils.Clamp(refreshTime, 1, 300);

        if (PeskyDustDrowsy >= dreamThreshold)
            ApplyDreambound(owner, dreamTime, dreamThreshold / 3);
    }

    public void ApplyDreambound(int owner, int dreamTime, int residualDrowsy = 0) {
        PeskyDustOwner = owner;
        PeskyDustDreamTime = Utils.Clamp(dreamTime, 1, 360);
        PeskyDustDrowsy = Utils.Clamp(residualDrowsy, 0, 99);
        PeskyDustDrowsyTime = Utils.Clamp(dreamTime / 2, 1, 180);
    }

    public override void AI(NPC npc) {
        TickStatuses();

        if (WhampireHypnosisTime > 0) {
            float dampening = npc.boss ? 0.92f : 0.55f;
            npc.velocity *= dampening;
            if (!npc.boss)
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -0.8f, 0.8f);
        }

        if (PeskyDustDreamTime > 0) {
            float driftDampening = npc.boss ? 0.96f : 0.78f;
            npc.velocity *= driftDampening;
            if (!npc.boss)
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y - 0.05f, -1.6f, 1.2f);
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (FasttrackComboTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(110, 255, 210), 0.2f + FasttrackComboStacks * 0.04f);

        if (AstrodactylSkyMarkTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(140, 255, 210), 0.28f);

        if (WhampirePreyTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(170, 45, 60), 0.3f);

        if (WhampireHypnosisTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(255, 170, 185), 0.36f);

        if (PeskyDustDreamTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(255, 228, 180), 0.34f);
        else if (PeskyDustDrowsy > 0)
            drawColor = Color.Lerp(drawColor, new Color(255, 210, 235), 0.12f + 0.18f * (PeskyDustDrowsy / 100f));
    }

    private void TickStatuses() {
        if (FasttrackComboTime > 0) {
            FasttrackComboTime--;
        }
        else {
            FasttrackComboOwner = -1;
            FasttrackComboStacks = 0;
        }

        if (AstrodactylSkyMarkTime > 0) {
            AstrodactylSkyMarkTime--;
        }
        else {
            AstrodactylSkyMarkOwner = -1;
        }

        if (WhampirePreyTime > 0) {
            WhampirePreyTime--;
        }
        else {
            WhampirePreyOwner = -1;
        }

        if (WhampireHypnosisTime > 0)
            WhampireHypnosisTime--;

        if (PeskyDustDrowsyTime > 0) {
            PeskyDustDrowsyTime--;
            if (PeskyDustDrowsyTime % 40 == 0 && PeskyDustDreamTime <= 0)
                PeskyDustDrowsy = System.Math.Max(0, PeskyDustDrowsy - 8);
        }
        else {
            PeskyDustDrowsy = System.Math.Max(0, PeskyDustDrowsy - 12);
        }

        if (PeskyDustDreamTime > 0) {
            PeskyDustDreamTime--;
        }
        else if (PeskyDustOwner != -1 && PeskyDustDrowsy <= 0) {
            PeskyDustOwner = -1;
        }
    }
}
