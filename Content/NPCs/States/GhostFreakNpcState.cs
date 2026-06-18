using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.NPCs;

public struct GhostFreakNpcState {
    public int FearOwner;
    public int FearStacks;
    public int FearTime;
    public int HauntOwner;
    public int HauntTime;

    public bool IsFearedFor(int owner) => FearOwner == owner && FearTime > 0 && FearStacks > 0;

    public bool IsHauntedFor(int owner) => HauntOwner == owner && HauntTime > 0;

    public int GetFearStacks(int owner) => IsFearedFor(owner) ? FearStacks : 0;

    public void ApplyFear(int owner, int stacks, int refreshTime) {
        if (FearOwner != owner) {
            FearOwner = owner;
            FearStacks = 0;
        }

        FearStacks = Utils.Clamp(FearStacks + stacks, 0, 5);
        FearTime = Utils.Clamp(System.Math.Max(FearTime, refreshTime), 1, 420);
    }

    public void ApplyHaunt(int owner, int time) {
        HauntOwner = owner;
        HauntTime = Utils.Clamp(time, 1, 420);
        ApplyFear(owner, 2, time);
    }

    public bool ConsumeHaunt(int owner) {
        if (!IsHauntedFor(owner))
            return false;

        HauntOwner = -1;
        HauntTime = 0;
        return true;
    }

    public void BeforeStatusTick(NPC npc) {
        if (HauntTime == 1)
            TriggerHauntDetonation(npc);
    }

    public void ApplyAI(NPC npc) {
        if (FearTime > 0)
            ApplyFearControl(npc);

        if (HauntTime > 0)
            npc.velocity *= npc.boss ? 0.985f : 0.92f;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (HauntTime > 0) {
            drawColor = Color.Lerp(drawColor, new Color(150, 95, 210), 0.35f);
            return;
        }

        if (FearTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(92, 72, 130), 0.16f + FearStacks * 0.035f);
    }

    public void Tick() {
        if (FearTime > 0) {
            FearTime--;
            if (FearTime % 55 == 0)
                FearStacks = System.Math.Max(0, FearStacks - 1);
        }
        else {
            FearOwner = -1;
            FearStacks = 0;
        }

        if (HauntTime > 0) {
            HauntTime--;
        }
        else {
            HauntOwner = -1;
        }
    }

    private void ApplyFearControl(NPC npc) {
        if (FearOwner < 0 || FearOwner >= Main.maxPlayers)
            return;

        Player owner = Main.player[FearOwner];
        if (!owner.active || owner.dead)
            return;

        float fearRatio = MathHelper.Clamp(FearStacks / 5f, 0f, 1f);
        if (npc.boss) {
            npc.velocity *= MathHelper.Lerp(0.99f, 0.965f, fearRatio);
            return;
        }

        Vector2 away = (npc.Center - owner.Center).SafeNormalize(Vector2.Zero);
        if (away == Vector2.Zero)
            away = new Vector2(npc.direction == 0 ? 1f : npc.direction, 0f);

        Vector2 panicVelocity = npc.velocity + away * MathHelper.Lerp(0.18f, 0.68f, fearRatio);
        npc.velocity = Vector2.Lerp(npc.velocity, panicVelocity, MathHelper.Lerp(0.08f, 0.18f, fearRatio));
        npc.velocity *= MathHelper.Lerp(0.98f, 0.92f, fearRatio);
        npc.velocity.X = MathHelper.Clamp(npc.velocity.X, -8f, 8f);
        npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -6f, 6f);
    }

    private void TriggerHauntDetonation(NPC npc) {
        if (Main.netMode == NetmodeID.MultiplayerClient || HauntOwner < 0 || HauntOwner >= Main.maxPlayers)
            return;

        Player owner = Main.player[HauntOwner];
        if (!owner.active || owner.dead)
            return;

        int fearStacks = GetFearStacks(owner.whoAmI);
        float baseDamage = npc.boss ? 34f : 48f;
        int damage = System.Math.Max(1,
            (int)System.Math.Round(owner.GetDamage<HeroDamage>().ApplyTo(baseDamage + fearStacks * 7f)));
        int hitDirection = npc.Center.X >= owner.Center.X ? 1 : -1;
        npc.SimpleStrikeNPC(damage, hitDirection, false, 0f, ModContent.GetInstance<HeroDamage>());
        npc.AddBuff(BuffID.Confused, npc.boss ? 90 : 210);
        npc.netUpdate = true;
    }
}
