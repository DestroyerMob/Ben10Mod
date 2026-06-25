using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.NPCs;

public struct UpgradeNpcState {
    public const int MaxInfectionStacks = 5;

    public int InfectionOwner;
    public int InfectionStacks;
    public int InfectionTime;
    private int infectionPulseCooldown;

    public bool IsInfectedFor(int owner) => InfectionOwner == owner && InfectionTime > 0 && InfectionStacks > 0;

    public int GetInfectionStacks(int owner) => IsInfectedFor(owner) ? InfectionStacks : 0;

    public int GetInfectionTime(int owner) => IsInfectedFor(owner) ? InfectionTime : 0;

    public bool HasMaxInfectionFor(int owner) => IsInfectedFor(owner) && InfectionStacks >= MaxInfectionStacks;

    public void ApplyInfection(int owner, int stacks, int time, bool mechanicalTarget) {
        if (InfectionOwner != owner) {
            Clear();
            InfectionOwner = owner;
        }

        int stackGain = mechanicalTarget ? stacks + 1 : stacks;
        InfectionStacks = Utils.Clamp(InfectionStacks + stackGain, 0, MaxInfectionStacks);
        InfectionTime = Utils.Clamp(System.Math.Max(InfectionTime, mechanicalTarget ? time + 120 : time), 1, mechanicalTarget ? 540 : 420);
    }

    public int ConsumeInfection(int owner, out bool wasFullyInfected) {
        wasFullyInfected = false;
        if (!IsInfectedFor(owner))
            return 0;

        int consumed = InfectionStacks;
        wasFullyInfected = InfectionStacks >= MaxInfectionStacks;
        Clear();
        return consumed;
    }

    public void HandleOverclockPulse(NPC npc) {
        if (InfectionTime <= 0 || InfectionStacks <= 0)
            return;

        if (infectionPulseCooldown > 0) {
            infectionPulseCooldown--;
            return;
        }

        if (InfectionOwner < 0 || InfectionOwner >= Main.maxPlayers)
            return;

        Player owner = Main.player[InfectionOwner];
        if (!owner.active || owner.dead)
            return;

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsTransformed || omp.currentTransformationId != "Ben10Mod:Upgrade" || !omp.PrimaryAbilityEnabled)
            return;

        infectionPulseCooldown = npc.boss ? 58 : 42;
        int badgeDamage = owner.GetModPlayer<UpgradeTechPlayer>().ResolveBadgeDamage();
        int pulseDamage = System.Math.Max(2, (int)System.Math.Round(badgeDamage * (npc.boss ? 0.08f : 0.14f)) + InfectionStacks);

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            npc.SimpleStrikeNPC(pulseDamage, owner.direction, false, 0f, ModContent.GetInstance<HeroDamage>());
            npc.AddBuff(BuffID.Electrified, npc.boss ? 45 : 75);
            npc.netUpdate = true;
        }

        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Vector2 offset = Main.rand.NextVector2CircularEdge(npc.width * 0.5f + 8f, npc.height * 0.5f + 8f);
            Dust dust = Dust.NewDustPerfect(npc.Center + offset, Main.rand.NextBool() ? DustID.Electric : DustID.GreenTorch,
                -offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.7f, 1.9f), 95,
                new Color(110, 255, 185), Main.rand.NextFloat(0.9f, 1.2f));
            dust.noGravity = true;
        }
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (InfectionTime <= 0 || InfectionStacks <= 0)
            return;

        float infectionRatio = InfectionStacks / (float)MaxInfectionStacks;
        drawColor = Color.Lerp(drawColor, Color.Lerp(new Color(90, 255, 155), new Color(210, 255, 235), infectionRatio),
            0.12f + infectionRatio * 0.22f);
    }

    public void Tick() {
        if (InfectionTime > 0) {
            InfectionTime--;
            if (InfectionTime <= 0)
                Clear();
        }
        else {
            Clear();
        }
    }

    private void Clear() {
        InfectionOwner = -1;
        InfectionStacks = 0;
        InfectionTime = 0;
        infectionPulseCooldown = 0;
    }
}
