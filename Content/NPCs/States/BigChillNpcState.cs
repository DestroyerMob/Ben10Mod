using Ben10Mod.Content.Transformations.BigChill;
using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct BigChillNpcState {
    public int Owner;
    public int FrostbiteStacks;
    public int FrostbiteTime;
    public int DeepFreezeTime;
    public int DeepFreezePressure;
    public int DeepFreezeArmorPenetration;
    public int ShiverburstCooldown;
    public int FrigidFractureOwner;
    public int FrigidFractureTime;

    public bool HasFrostbiteFor(int owner) => Owner == owner && FrostbiteTime > 0 && FrostbiteStacks > 0;

    public bool IsDeepFrozenFor(int owner) => Owner == owner && DeepFreezeTime > 0;

    public bool IsFrigidFracturedFor(int owner) => FrigidFractureOwner == owner && FrigidFractureTime > 0;

    public int GetFrostbiteStacks(int owner) {
        if (Owner != owner)
            return 0;

        return FrostbiteTime > 0 ? System.Math.Max(1, FrostbiteStacks) : 0;
    }

    public int GetFrostbiteTime(int owner) => HasFrostbiteFor(owner) ? FrostbiteTime : 0;

    public int GetFrigidFractureTime(int owner) => IsFrigidFracturedFor(owner) ? FrigidFractureTime : 0;

    public int GetArmorPenetration(int owner) => FrostbiteTime > 0 && Owner == owner ? DeepFreezeArmorPenetration : 0;

    public void ApplyHoarfrost(int owner, int time, int armorPenetrationBonus = 4) {
        if (Owner != owner || FrostbiteTime <= 0) {
            Clear();
            Owner = owner;
        }

        FrostbiteStacks = 1;
        FrostbiteTime = Utils.Clamp(System.Math.Max(FrostbiteTime, time), 1, 360);
        DeepFreezeTime = 0;
        DeepFreezePressure = 0;
        DeepFreezeArmorPenetration = System.Math.Max(0, armorPenetrationBonus);
    }

    public bool ConsumeHoarfrost(int owner) {
        if (!HasFrostbiteFor(owner))
            return false;

        Clear();
        return true;
    }

    public bool CanTriggerShiverburst(int owner) {
        return Owner == owner && ShiverburstCooldown <= 0;
    }

    public void TriggerShiverburstCooldown(int cooldown = 6) {
        ShiverburstCooldown = Utils.Clamp(cooldown, 1, 60);
    }

    public bool ApplyFrostbite(int owner, int stacks, int refreshTime, int deepFreezeTime, int armorPenetrationBonus = 8) {
        if (Owner != owner) {
            Clear();
            Owner = owner;
        }

        FrostbiteTime = Utils.Clamp(System.Math.Max(FrostbiteTime, refreshTime), 1, 420);
        if (DeepFreezeTime > 0)
            return false;

        FrostbiteStacks = Utils.Clamp(FrostbiteStacks + stacks, 0, BigChillTransformation.FrostbiteThreshold);
        if (FrostbiteStacks < BigChillTransformation.FrostbiteThreshold)
            return false;

        DeepFreezeTime = Utils.Clamp(deepFreezeTime, 1, 360);
        FrostbiteTime = DeepFreezeTime;
        DeepFreezePressure = 0;
        DeepFreezeArmorPenetration = System.Math.Max(0, armorPenetrationBonus);
        return true;
    }

    public bool RefreshDeepFreeze(int owner, int amount) {
        if (!IsDeepFrozenFor(owner))
            return false;

        DeepFreezeTime = Utils.Clamp(DeepFreezeTime + System.Math.Max(1, amount), 1, 360);
        FrostbiteTime = DeepFreezeTime;
        return true;
    }

    public bool AddDeepFreezePressure(int owner, int amount, int threshold) {
        if (!IsDeepFrozenFor(owner))
            return false;

        DeepFreezePressure = Utils.Clamp(DeepFreezePressure + amount, 0, System.Math.Max(1, threshold));
        return DeepFreezePressure >= threshold;
    }

    public bool ConsumeDeepFreeze(int owner) {
        if (!IsDeepFrozenFor(owner))
            return false;

        Clear();
        return true;
    }

    public void ApplyFrigidFracture(int owner, int time) {
        FrigidFractureOwner = owner;
        FrigidFractureTime = Utils.Clamp(time, 1, 420);
    }

    public void ApplyAI(NPC npc) {
        if (DeepFreezeTime > 0) {
            float dampening = npc.boss ? 0.94f : 0.72f;
            npc.velocity *= dampening;
            if (!npc.boss)
                npc.velocity = Vector2.Clamp(npc.velocity, new Vector2(-2.1f, -2.1f), new Vector2(2.1f, 2.1f));
        }
        else if (FrostbiteTime > 0) {
            float dampening = npc.boss ? 0.97f : 0.84f;
            npc.velocity *= dampening;
        }
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (FrostbiteTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(178, 232, 255), 0.22f);

        if (FrigidFractureTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(236, 248, 255), 0.16f);
    }

    public void Tick() {
        if (Owner != -1) {
            FrostbiteTime = System.Math.Max(FrostbiteTime - 1, 0);
            DeepFreezeTime = 0;
            DeepFreezePressure = 0;
            if (FrostbiteTime <= 0 || FrostbiteStacks <= 0)
                Clear();
        }

        if (ShiverburstCooldown > 0)
            ShiverburstCooldown--;

        if (FrigidFractureTime > 0) {
            FrigidFractureTime--;
        }
        else {
            FrigidFractureOwner = -1;
        }
    }

    private void Clear() {
        Owner = -1;
        FrostbiteStacks = 0;
        FrostbiteTime = 0;
        DeepFreezeTime = 0;
        DeepFreezePressure = 0;
        DeepFreezeArmorPenetration = 8;
        ShiverburstCooldown = 0;
    }
}
