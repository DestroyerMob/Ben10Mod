using Ben10Mod.Content.Buffs.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

internal static class BuzzShockTargeting {
    public static int TagBuffType => ModContent.BuffType<BuzzShockTagBuff>();

    public static bool IsTagged(NPC npc) => npc.active && npc.HasBuff(TagBuffType);

    public static int CountTagged(Vector2 origin, float maxRange) {
        float maxRangeSq = maxRange * maxRange;
        int count = 0;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy() || !IsTagged(npc))
                continue;

            if (Vector2.DistanceSquared(origin, npc.Center) <= maxRangeSq)
                count++;
        }

        return count;
    }

    public static NPC FindTarget(Vector2 origin, float maxRange, bool preferTagged = true, bool preferUntagged = false,
        int excludedWhoAmI = -1, int secondExcludedWhoAmI = -1) {
        float maxRangeSq = maxRange * maxRange;
        float bestScore = maxRangeSq;
        NPC selectedTarget = null;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc.whoAmI == excludedWhoAmI || npc.whoAmI == secondExcludedWhoAmI || !npc.CanBeChasedBy())
                continue;

            float distanceSq = Vector2.DistanceSquared(origin, npc.Center);
            if (distanceSq > maxRangeSq)
                continue;

            bool tagged = IsTagged(npc);
            float score = distanceSq;
            if (preferTagged && tagged)
                score *= 0.22f;
            if (preferUntagged && !tagged)
                score *= 0.35f;
            if (preferUntagged && tagged)
                score *= 1.4f;

            if (score >= bestScore)
                continue;

            bestScore = score;
            selectedTarget = npc;
        }

        return selectedTarget;
    }
}
