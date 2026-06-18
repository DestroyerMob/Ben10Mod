using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Frankenstrike;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.NPCs;

public struct FrankenstrikeNpcState {
    public int ConductiveOwner;
    public int ConductiveStacks;
    public int ConductiveTime;
    public int OverchargedOwner;
    public int OverchargedTime;
    public int OverchargedArcTimer;

    public bool IsConductiveFor(int owner) => ConductiveOwner == owner && ConductiveTime > 0 && ConductiveStacks > 0;

    public bool IsOverchargedFor(int owner) => OverchargedOwner == owner && OverchargedTime > 0;

    public int GetConductiveStacks(int owner) => ConductiveOwner == owner ? ConductiveStacks : 0;

    public int GetOverchargedTime(int owner) => IsOverchargedFor(owner) ? OverchargedTime : 0;

    public void ApplyConductive(int owner, int stacks, int time) {
        if (ConductiveOwner != owner) {
            Clear();
            ConductiveOwner = owner;
        }

        ConductiveStacks = Utils.Clamp(ConductiveStacks + stacks, 0, FrankenstrikeStatePlayer.ConductiveMaxStacks);
        ConductiveTime = Utils.Clamp(time, 1, 300);
        if (OverchargedOwner == owner)
            OverchargedTime = System.Math.Max(OverchargedTime, Utils.Clamp(time, 1, 300));

        if (ConductiveStacks >= FrankenstrikeStatePlayer.ConductiveMaxStacks)
            PromoteOvercharged(owner, FrankenstrikeStatePlayer.OverchargedDurationTicks);
    }

    public int ConsumeConductive(int owner, int amount) {
        int available = GetConductiveStacks(owner);
        if (available <= 0)
            return 0;

        int consumed = System.Math.Min(available, amount);
        ConductiveStacks -= consumed;
        if (ConductiveStacks <= 0)
            Clear();

        return consumed;
    }

    public int ConsumeOvercharged(int owner, int residualConductiveStacks = 0) {
        if (!IsOverchargedFor(owner))
            return 0;

        int consumed = ConductiveStacks;
        OverchargedOwner = -1;
        OverchargedTime = 0;
        OverchargedArcTimer = 0;

        if (residualConductiveStacks > 0) {
            ConductiveOwner = owner;
            ConductiveStacks = Utils.Clamp(residualConductiveStacks, 0, FrankenstrikeStatePlayer.ConductiveMaxStacks - 1);
            ConductiveTime = 180;
        }
        else {
            ConductiveOwner = -1;
            ConductiveStacks = 0;
            ConductiveTime = 0;
        }

        return consumed;
    }

    public void HandleOverchargedArcs(NPC npc) {
        if (Main.netMode == NetmodeID.MultiplayerClient || OverchargedTime <= 0)
            return;

        if (OverchargedArcTimer > 0) {
            OverchargedArcTimer--;
            return;
        }

        if (OverchargedOwner < 0 || OverchargedOwner >= Main.maxPlayers)
            return;

        Player owner = Main.player[OverchargedOwner];
        if (!owner.active || owner.dead)
            return;

        OverchargedArcTimer = npc.boss ? 34 : 24;
        int remainingTargets = npc.boss ? 1 : 2;
        int arcDamage = FrankenstrikeTransformation.ResolveHeroDamage(owner, npc.boss ? 0.18f : 0.24f);

        foreach (NPC other in Main.ActiveNPCs) {
            if (remainingTargets <= 0 || other.whoAmI == npc.whoAmI || !other.CanBeChasedBy())
                continue;

            if (other.Center.Distance(npc.Center) > 176f)
                continue;

            other.SimpleStrikeNPC(arcDamage, owner.direction, false, 0f, ModContent.GetInstance<HeroDamage>());
            other.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyFrankenstrikeConductive(owner.whoAmI, 1 + (other.wet ? 1 : 0), 150);
            other.AddBuff(BuffID.Electrified, 120);
            other.netUpdate = true;
            remainingTargets--;

            if (Main.dedServ)
                continue;

            Vector2 arcDirection = (other.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 8; i++) {
                float progress = i / 7f;
                Dust dust = Dust.NewDustPerfect(Vector2.Lerp(npc.Center, other.Center, progress), DustID.Electric,
                    arcDirection.RotatedByRandom(0.42f) * Main.rand.NextFloat(0.15f, 0.85f), 105,
                    new Color(185, 228, 255), Main.rand.NextFloat(0.88f, 1.1f));
                dust.noGravity = true;
            }
        }
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (ConductiveTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(120, 205, 255), 0.18f + ConductiveStacks * 0.04f);

        if (OverchargedTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(225, 242, 255), 0.24f);
    }

    public void Tick() {
        if (OverchargedTime > 0) {
            OverchargedTime--;
            ConductiveTime = System.Math.Max(ConductiveTime - 1, 0);
            if (OverchargedTime <= 0)
                Clear();
        }
        else if (ConductiveTime > 0) {
            ConductiveTime--;
        }
        else {
            Clear();
        }
    }

    private void PromoteOvercharged(int owner, int time) {
        ConductiveOwner = owner;
        ConductiveStacks = FrankenstrikeStatePlayer.ConductiveMaxStacks;
        ConductiveTime = Utils.Clamp(time, 1, 300);
        OverchargedOwner = owner;
        OverchargedTime = Utils.Clamp(time, 1, 300);
        OverchargedArcTimer = 10;
    }

    private void Clear() {
        ConductiveOwner = -1;
        ConductiveStacks = 0;
        ConductiveTime = 0;
        OverchargedOwner = -1;
        OverchargedTime = 0;
        OverchargedArcTimer = 0;
    }
}
