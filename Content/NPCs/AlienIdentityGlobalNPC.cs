using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.BigChill;
using Ben10Mod.Content.Transformations.EyeGuy;
using Ben10Mod.Content.Transformations.Frankenstrike;
using Ben10Mod.Content.Transformations.HeatBlast;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.NPCs;

public class AlienIdentityGlobalNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public int FasttrackComboOwner = -1;
    public int FasttrackComboStacks;
    public int FasttrackComboTime;

    public int AstrodactylSkyMarkOwner = -1;
    public int AstrodactylSkyMarkTime;

    public int BlitzwolferResonanceOwner = -1;
    public int BlitzwolferResonanceStacks;
    public int BlitzwolferResonanceTime;

    public int EchoEchoResonanceOwner = -1;
    public int EchoEchoResonanceStacks;
    public int EchoEchoResonanceTime;
    public int EchoEchoResonanceLastSource = -1;
    public bool EchoEchoResonancePrimed;
    public int EchoEchoFractureOwner = -1;
    public int EchoEchoFractureTime;
    public int UltimateEchoEchoFocusedOwner = -1;
    public int UltimateEchoEchoFocusedTime;

    public int FrankenstrikeConductiveOwner = -1;
    public int FrankenstrikeConductiveStacks;
    public int FrankenstrikeConductiveTime;
    public int FrankenstrikeOverchargedOwner = -1;
    public int FrankenstrikeOverchargedTime;
    public int FrankenstrikeOverchargedArcTimer;

    public int LodestarPolarityOwner = -1;
    public int LodestarPolarityTime;
    public int LodestarPolarityDirection = -1;

    public int WaterHazardSoakOwner = -1;
    public int WaterHazardSoak;
    public int WaterHazardSoakTime;

    public int JetrayLockOwner = -1;
    public int JetrayLockTime;

    public int BigChillOwner = -1;
    public int BigChillFrostbiteStacks;
    public int BigChillFrostbiteTime;
    public int BigChillDeepFreezeTime;
    public int BigChillDeepFreezePressure;
    public int BigChillDeepFreezeArmorPenetration = 8;
    public int BigChillShiverburstCooldown;
    public int BigChillFrigidFractureOwner = -1;
    public int BigChillFrigidFractureTime;

    public int HeatBlastOwner = -1;
    public int HeatBlastFlashpointStacks;
    public int HeatBlastFlashpointProgress;
    public int HeatBlastFlashpointTime;
    public int HeatBlastFlarePopCooldown;

    public int EyeGuyOwner = -1;
    public int EyeGuyFireMarkTime;
    public int EyeGuyFrostMarkTime;
    public int EyeGuyShockMarkTime;
    public int EyeGuyExposedTime;

    public int WhampirePreyOwner = -1;
    public int WhampirePreyTime;
    public int WhampireHypnosisTime;

    public int SnareOhCurseOwner = -1;
    public int SnareOhCurseStacks;
    public int SnareOhCurseTime;

    public int AlienXJudgementOwner = -1;
    public int AlienXJudgementStacks;
    public int AlienXJudgementTime;
    public int AlienXStasisTime;

    public int PeskyDustOwner = -1;
    public int PeskyDustDrowsy;
    public int PeskyDustDrowsyTime;
    public int PeskyDustDreamTime;

    public bool IsFasttrackComboActiveFor(int owner) => FasttrackComboOwner == owner && FasttrackComboTime > 0 && FasttrackComboStacks > 0;
    public bool IsSkyMarkedFor(int owner) => AstrodactylSkyMarkOwner == owner && AstrodactylSkyMarkTime > 0;
    public bool IsBlitzwolferResonantFor(int owner) => BlitzwolferResonanceOwner == owner && BlitzwolferResonanceTime > 0 && BlitzwolferResonanceStacks > 0;
    public bool IsEchoEchoResonantFor(int owner) => EchoEchoResonanceOwner == owner && EchoEchoResonanceTime > 0 && EchoEchoResonanceStacks > 0;
    public bool IsEchoEchoResonancePrimedFor(int owner) => IsEchoEchoResonantFor(owner) && EchoEchoResonancePrimed;
    public bool IsEchoEchoFracturedFor(int owner) => EchoEchoFractureOwner == owner && EchoEchoFractureTime > 0;
    public bool IsUltimateEchoEchoFocusedFor(int owner) => UltimateEchoEchoFocusedOwner == owner && UltimateEchoEchoFocusedTime > 0;
    public bool IsFrankenstrikeConductiveFor(int owner) => FrankenstrikeConductiveOwner == owner && FrankenstrikeConductiveTime > 0 && FrankenstrikeConductiveStacks > 0;
    public bool IsFrankenstrikeOverchargedFor(int owner) => FrankenstrikeOverchargedOwner == owner && FrankenstrikeOverchargedTime > 0;
    public bool HasLodestarPolarityFor(int owner) => LodestarPolarityOwner == owner && LodestarPolarityTime > 0;
    public bool IsWaterHazardSoakedFor(int owner) => WaterHazardSoakOwner == owner && WaterHazardSoakTime > 0 && WaterHazardSoak > 0;
    public bool IsJetrayLockedFor(int owner) => JetrayLockOwner == owner && JetrayLockTime > 0;
    public bool HasBigChillFrostbiteFor(int owner) => BigChillOwner == owner && BigChillFrostbiteTime > 0 && BigChillFrostbiteStacks > 0;
    public bool IsBigChillDeepFrozenFor(int owner) => BigChillOwner == owner && BigChillDeepFreezeTime > 0;
    public bool IsBigChillFrigidFracturedFor(int owner) => BigChillFrigidFractureOwner == owner && BigChillFrigidFractureTime > 0;
    public bool HasHeatBlastFlashpointFor(int owner) => HeatBlastOwner == owner && HeatBlastFlashpointTime > 0 && HeatBlastFlashpointStacks > 0;
    public bool IsEyeGuyExposedFor(int owner) => EyeGuyOwner == owner && EyeGuyExposedTime > 0;
    public bool IsWhampirePreyFor(int owner) => WhampirePreyOwner == owner && WhampirePreyTime > 0;
    public bool IsSnareOhCursedFor(int owner) => SnareOhCurseOwner == owner && SnareOhCurseTime > 0 && SnareOhCurseStacks > 0;
    public bool IsAlienXJudgedFor(int owner) => AlienXJudgementOwner == owner && AlienXJudgementTime > 0 && AlienXJudgementStacks > 0;
    public bool IsDreamboundFor(int owner) => PeskyDustOwner == owner && PeskyDustDreamTime > 0;

    public int GetBlitzwolferResonanceStacks(int owner) => IsBlitzwolferResonantFor(owner) ? BlitzwolferResonanceStacks : 0;
    public int GetEchoEchoResonanceStacks(int owner) => IsEchoEchoResonantFor(owner) ? EchoEchoResonanceStacks : 0;
    public int GetFrankenstrikeConductiveStacks(int owner) => FrankenstrikeConductiveOwner == owner ? FrankenstrikeConductiveStacks : 0;
    public int GetWaterHazardSoak(int owner) => IsWaterHazardSoakedFor(owner) ? WaterHazardSoak : 0;
    public int GetSnareOhCurseStacks(int owner) => IsSnareOhCursedFor(owner) ? SnareOhCurseStacks : 0;
    public int GetAlienXJudgementStacks(int owner) => IsAlienXJudgedFor(owner) ? AlienXJudgementStacks : 0;
    public int GetHeatBlastFlashpointStacks(int owner) => HasHeatBlastFlashpointFor(owner) ? HeatBlastFlashpointStacks : 0;
    public int GetBigChillFrostbiteStacks(int owner) {
        if (BigChillOwner != owner)
            return 0;

        return BigChillFrostbiteTime > 0 ? System.Math.Max(1, BigChillFrostbiteStacks) : 0;
    }

    public void ApplyBigChillHoarfrost(int owner, int time, int armorPenetrationBonus = 4) {
        if (BigChillOwner != owner) {
            ClearBigChillState();
            BigChillOwner = owner;
        }

        BigChillFrostbiteStacks = 1;
        BigChillFrostbiteTime = Utils.Clamp(System.Math.Max(BigChillFrostbiteTime, time), 1, 360);
        BigChillDeepFreezeTime = 0;
        BigChillDeepFreezePressure = 0;
        BigChillDeepFreezeArmorPenetration = System.Math.Max(0, armorPenetrationBonus);
    }

    public bool ConsumeBigChillHoarfrost(int owner) {
        if (!HasBigChillFrostbiteFor(owner))
            return false;

        ClearBigChillState();
        return true;
    }

    public bool CanTriggerBigChillShiverburst(int owner) {
        return BigChillOwner == owner && BigChillShiverburstCooldown <= 0;
    }

    public void TriggerBigChillShiverburstCooldown(int cooldown = 6) {
        BigChillShiverburstCooldown = Utils.Clamp(cooldown, 1, 60);
    }

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

    public void ApplyBlitzwolferResonance(int owner, int stacks, int time) {
        if (BlitzwolferResonanceOwner != owner) {
            BlitzwolferResonanceOwner = owner;
            BlitzwolferResonanceStacks = 0;
        }

        BlitzwolferResonanceStacks = Utils.Clamp(BlitzwolferResonanceStacks + stacks, 0, 8);
        BlitzwolferResonanceTime = Utils.Clamp(time, 1, 360);
    }

    public int ConsumeBlitzwolferResonance(int owner) {
        int stacks = GetBlitzwolferResonanceStacks(owner);
        if (stacks <= 0)
            return 0;

        BlitzwolferResonanceStacks = 0;
        BlitzwolferResonanceTime = 0;
        BlitzwolferResonanceOwner = -1;
        return stacks;
    }

    public void ApplyEchoEchoResonance(int owner, int sourceId, int stacks, int time) {
        if (EchoEchoResonanceOwner != owner) {
            EchoEchoResonanceOwner = owner;
            EchoEchoResonanceStacks = 0;
            EchoEchoResonancePrimed = false;
            EchoEchoResonanceLastSource = -1;
        }

        int gain = Utils.Clamp(stacks, 1, 3);
        if (EchoEchoResonanceLastSource >= 0 && EchoEchoResonanceLastSource != sourceId)
            gain++;

        EchoEchoResonanceStacks = Utils.Clamp(EchoEchoResonanceStacks + gain, 0, 8);
        EchoEchoResonancePrimed = EchoEchoResonanceStacks >= 8;
        EchoEchoResonanceLastSource = sourceId;
        EchoEchoResonanceTime = Utils.Clamp(time, 1, 360);
    }

    public int ConsumeEchoEchoResonance(int owner) {
        int stacks = GetEchoEchoResonanceStacks(owner);
        if (stacks <= 0)
            return 0;

        EchoEchoResonanceOwner = -1;
        EchoEchoResonanceStacks = 0;
        EchoEchoResonanceTime = 0;
        EchoEchoResonanceLastSource = -1;
        EchoEchoResonancePrimed = false;
        return stacks;
    }

    public void ApplyEchoEchoFracture(int owner, int time) {
        EchoEchoFractureOwner = owner;
        EchoEchoFractureTime = Utils.Clamp(time, 1, 240);
    }

    public void ApplyUltimateEchoEchoFocus(int owner, int time) {
        UltimateEchoEchoFocusedOwner = owner;
        UltimateEchoEchoFocusedTime = Utils.Clamp(System.Math.Max(UltimateEchoEchoFocusedTime, time), 1, 300);
    }

    public void ApplyFrankenstrikeConductive(int owner, int stacks, int time) {
        if (FrankenstrikeConductiveOwner != owner) {
            ClearFrankenstrikeState();
            FrankenstrikeConductiveOwner = owner;
        }

        FrankenstrikeConductiveStacks = Utils.Clamp(FrankenstrikeConductiveStacks + stacks, 0, FrankenstrikeStatePlayer.ConductiveMaxStacks);
        FrankenstrikeConductiveTime = Utils.Clamp(time, 1, 300);
        if (FrankenstrikeOverchargedOwner == owner)
            FrankenstrikeOverchargedTime = System.Math.Max(FrankenstrikeOverchargedTime, Utils.Clamp(time, 1, 300));

        if (FrankenstrikeConductiveStacks >= FrankenstrikeStatePlayer.ConductiveMaxStacks)
            PromoteFrankenstrikeOvercharged(owner, FrankenstrikeStatePlayer.OverchargedDurationTicks);
    }

    public int ConsumeFrankenstrikeConductive(int owner, int amount) {
        int available = GetFrankenstrikeConductiveStacks(owner);
        if (available <= 0)
            return 0;

        int consumed = System.Math.Min(available, amount);
        FrankenstrikeConductiveStacks -= consumed;
        if (FrankenstrikeConductiveStacks <= 0) {
            FrankenstrikeConductiveOwner = -1;
            FrankenstrikeConductiveTime = 0;
            FrankenstrikeOverchargedOwner = -1;
            FrankenstrikeOverchargedTime = 0;
            FrankenstrikeOverchargedArcTimer = 0;
        }

        return consumed;
    }

    public int ConsumeFrankenstrikeOvercharged(int owner, int residualConductiveStacks = 0) {
        if (!IsFrankenstrikeOverchargedFor(owner))
            return 0;

        int consumed = FrankenstrikeConductiveStacks;
        FrankenstrikeOverchargedOwner = -1;
        FrankenstrikeOverchargedTime = 0;
        FrankenstrikeOverchargedArcTimer = 0;

        if (residualConductiveStacks > 0) {
            FrankenstrikeConductiveOwner = owner;
            FrankenstrikeConductiveStacks = Utils.Clamp(residualConductiveStacks, 0, FrankenstrikeStatePlayer.ConductiveMaxStacks - 1);
            FrankenstrikeConductiveTime = 180;
        }
        else {
            FrankenstrikeConductiveOwner = -1;
            FrankenstrikeConductiveStacks = 0;
            FrankenstrikeConductiveTime = 0;
        }

        return consumed;
    }

    private void PromoteFrankenstrikeOvercharged(int owner, int time) {
        FrankenstrikeConductiveOwner = owner;
        FrankenstrikeConductiveStacks = FrankenstrikeStatePlayer.ConductiveMaxStacks;
        FrankenstrikeConductiveTime = Utils.Clamp(time, 1, 300);
        FrankenstrikeOverchargedOwner = owner;
        FrankenstrikeOverchargedTime = Utils.Clamp(time, 1, 300);
        FrankenstrikeOverchargedArcTimer = 10;
    }

    private void ClearFrankenstrikeState() {
        FrankenstrikeConductiveOwner = -1;
        FrankenstrikeConductiveStacks = 0;
        FrankenstrikeConductiveTime = 0;
        FrankenstrikeOverchargedOwner = -1;
        FrankenstrikeOverchargedTime = 0;
        FrankenstrikeOverchargedArcTimer = 0;
    }

    public void ApplyLodestarPolarity(int owner, int time, int direction) {
        LodestarPolarityOwner = owner;
        LodestarPolarityTime = Utils.Clamp(time, 1, 300);
        LodestarPolarityDirection = direction >= 0 ? 1 : -1;
    }

    public void AddWaterHazardSoak(int owner, int amount, int refreshTime) {
        WaterHazardSoakOwner = owner;
        WaterHazardSoak = Utils.Clamp(WaterHazardSoak + amount, 0, 100);
        WaterHazardSoakTime = Utils.Clamp(refreshTime, 1, 360);
    }

    public int ConsumeWaterHazardSoak(int owner, int amount) {
        int soaked = GetWaterHazardSoak(owner);
        if (soaked <= 0)
            return 0;

        int consumed = System.Math.Min(soaked, amount);
        WaterHazardSoak -= consumed;
        if (WaterHazardSoak <= 0) {
            WaterHazardSoakOwner = -1;
            WaterHazardSoakTime = 0;
        }

        return consumed;
    }

    public void ApplyJetrayLock(int owner, int time) {
        JetrayLockOwner = owner;
        JetrayLockTime = Utils.Clamp(time, 1, 420);
    }

    public bool ApplyBigChillFrostbite(int owner, int stacks, int refreshTime, int deepFreezeTime,
        int armorPenetrationBonus = 8) {
        if (BigChillOwner != owner) {
            ClearBigChillState();
            BigChillOwner = owner;
        }

        BigChillFrostbiteTime = Utils.Clamp(System.Math.Max(BigChillFrostbiteTime, refreshTime), 1, 420);
        if (BigChillDeepFreezeTime > 0)
            return false;

        BigChillFrostbiteStacks = Utils.Clamp(BigChillFrostbiteStacks + stacks, 0, BigChillTransformation.FrostbiteThreshold);
        if (BigChillFrostbiteStacks < BigChillTransformation.FrostbiteThreshold)
            return false;

        BigChillDeepFreezeTime = Utils.Clamp(deepFreezeTime, 1, 360);
        BigChillFrostbiteTime = BigChillDeepFreezeTime;
        BigChillDeepFreezePressure = 0;
        BigChillDeepFreezeArmorPenetration = System.Math.Max(0, armorPenetrationBonus);
        return true;
    }

    public bool RefreshBigChillDeepFreeze(int owner, int amount) {
        if (!IsBigChillDeepFrozenFor(owner))
            return false;

        BigChillDeepFreezeTime = Utils.Clamp(BigChillDeepFreezeTime + System.Math.Max(1, amount), 1, 360);
        BigChillFrostbiteTime = BigChillDeepFreezeTime;
        return true;
    }

    public bool AddBigChillDeepFreezePressure(int owner, int amount, int threshold) {
        if (!IsBigChillDeepFrozenFor(owner))
            return false;

        BigChillDeepFreezePressure = Utils.Clamp(BigChillDeepFreezePressure + amount, 0, System.Math.Max(1, threshold));
        return BigChillDeepFreezePressure >= threshold;
    }

    public bool ConsumeBigChillDeepFreeze(int owner) {
        if (!IsBigChillDeepFrozenFor(owner))
            return false;

        ClearBigChillState();
        return true;
    }

    public void ApplyBigChillFrigidFracture(int owner, int time) {
        BigChillFrigidFractureOwner = owner;
        BigChillFrigidFractureTime = Utils.Clamp(time, 1, 420);
    }

    public int AddHeatBlastFlashpointProgress(int owner, int progress, int refreshTime, int threshold = 3, int maxStacks = 5) {
        if (HeatBlastOwner != owner) {
            ClearHeatBlastState();
            HeatBlastOwner = owner;
        }

        HeatBlastFlashpointTime = Utils.Clamp(System.Math.Max(HeatBlastFlashpointTime, refreshTime), 1, 420);
        if (HeatBlastFlashpointStacks >= maxStacks)
            return 0;

        HeatBlastFlashpointProgress += System.Math.Max(1, progress);
        int gained = 0;
        int clampedThreshold = System.Math.Max(1, threshold);
        while (HeatBlastFlashpointProgress >= clampedThreshold && HeatBlastFlashpointStacks < maxStacks) {
            HeatBlastFlashpointProgress -= clampedThreshold;
            HeatBlastFlashpointStacks++;
            gained++;
        }

        if (HeatBlastFlashpointStacks >= maxStacks)
            HeatBlastFlashpointProgress = 0;

        return gained;
    }

    public int ConsumeHeatBlastFlashpoint(int owner) {
        if (HeatBlastOwner != owner || HeatBlastFlashpointStacks <= 0 || HeatBlastFlashpointTime <= 0)
            return 0;

        int consumed = HeatBlastFlashpointStacks;
        ClearHeatBlastState();
        return consumed;
    }

    public bool TryTriggerHeatBlastFlarePop(int owner, int cooldown) {
        if (!HasHeatBlastFlashpointFor(owner) || HeatBlastFlashpointStacks < 5 || HeatBlastFlarePopCooldown > 0)
            return false;

        HeatBlastFlarePopCooldown = Utils.Clamp(cooldown, 1, 180);
        return true;
    }

    public bool HasEyeGuyMark(int owner, EyeGuyElement element) {
        if (EyeGuyOwner != owner)
            return false;

        return element switch {
            EyeGuyElement.Fire => EyeGuyFireMarkTime > 0,
            EyeGuyElement.Frost => EyeGuyFrostMarkTime > 0,
            _ => EyeGuyShockMarkTime > 0
        };
    }

    public int GetEyeGuyMarkCount(int owner) {
        if (EyeGuyOwner != owner)
            return 0;

        int count = 0;
        if (EyeGuyFireMarkTime > 0)
            count++;
        if (EyeGuyFrostMarkTime > 0)
            count++;
        if (EyeGuyShockMarkTime > 0)
            count++;
        return count;
    }

    public EyeGuyElement GetPreferredEyeGuyMark(int owner, EyeGuyElement fallback) {
        if (EyeGuyOwner != owner || EyeGuyExposedTime > 0)
            return fallback;

        if (EyeGuyFireMarkTime <= 0)
            return EyeGuyElement.Fire;
        if (EyeGuyFrostMarkTime <= 0)
            return EyeGuyElement.Frost;
        if (EyeGuyShockMarkTime <= 0)
            return EyeGuyElement.Shock;

        return fallback;
    }

    public bool ApplyEyeGuyMark(int owner, EyeGuyElement element, int time, int exposedTime) {
        if (EyeGuyOwner != owner) {
            ClearEyeGuyState();
            EyeGuyOwner = owner;
        }

        if (EyeGuyExposedTime > 0)
            return false;

        int clampedTime = Utils.Clamp(time, 1, 420);
        switch (element) {
            case EyeGuyElement.Fire:
                EyeGuyFireMarkTime = System.Math.Max(EyeGuyFireMarkTime, clampedTime);
                break;
            case EyeGuyElement.Frost:
                EyeGuyFrostMarkTime = System.Math.Max(EyeGuyFrostMarkTime, clampedTime);
                break;
            default:
                EyeGuyShockMarkTime = System.Math.Max(EyeGuyShockMarkTime, clampedTime);
                break;
        }

        if (EyeGuyFireMarkTime <= 0 || EyeGuyFrostMarkTime <= 0 || EyeGuyShockMarkTime <= 0)
            return false;

        EyeGuyExposedTime = Utils.Clamp(exposedTime, 1, 600);
        EyeGuyFireMarkTime = EyeGuyExposedTime;
        EyeGuyFrostMarkTime = EyeGuyExposedTime;
        EyeGuyShockMarkTime = EyeGuyExposedTime;
        return true;
    }

    public bool ConsumeEyeGuyExposed(int owner) {
        if (!IsEyeGuyExposedFor(owner))
            return false;

        ClearEyeGuyState();
        return true;
    }

    public void ApplyWhampirePrey(int owner, int time) {
        WhampirePreyOwner = owner;
        WhampirePreyTime = Utils.Clamp(time, 1, 420);
    }

    public void ApplyWhampireHypnosis(int time) {
        WhampireHypnosisTime = Utils.Clamp(time, 1, 240);
    }

    public void ApplySnareOhCurse(int owner, int stacks, int time) {
        if (SnareOhCurseOwner != owner) {
            SnareOhCurseOwner = owner;
            SnareOhCurseStacks = 0;
        }

        SnareOhCurseStacks = Utils.Clamp(SnareOhCurseStacks + stacks, 0, 7);
        SnareOhCurseTime = Utils.Clamp(time, 1, 360);
    }

    public int ConsumeSnareOhCurse(int owner, int amount) {
        int available = GetSnareOhCurseStacks(owner);
        if (available <= 0)
            return 0;

        int consumed = System.Math.Min(available, amount);
        SnareOhCurseStacks -= consumed;
        if (SnareOhCurseStacks <= 0) {
            SnareOhCurseOwner = -1;
            SnareOhCurseTime = 0;
        }

        return consumed;
    }

    public void ApplyAlienXJudgement(int owner, int stacks, int time) {
        if (AlienXJudgementOwner != owner) {
            AlienXJudgementOwner = owner;
            AlienXJudgementStacks = 0;
        }

        AlienXJudgementStacks = Utils.Clamp(AlienXJudgementStacks + stacks, 0, 6);
        AlienXJudgementTime = Utils.Clamp(time, 1, 420);
    }

    public int ConsumeAlienXJudgement(int owner, int amount = 99) {
        int available = GetAlienXJudgementStacks(owner);
        if (available <= 0)
            return 0;

        int consumed = System.Math.Min(available, amount);
        AlienXJudgementStacks -= consumed;
        if (AlienXJudgementStacks <= 0) {
            AlienXJudgementOwner = -1;
            AlienXJudgementTime = 0;
        }

        return consumed;
    }

    public void ApplyAlienXStasis(int owner, int time, int judgementStacks = 0) {
        AlienXStasisTime = Utils.Clamp(time, 1, 180);
        if (judgementStacks > 0)
            ApplyAlienXJudgement(owner, judgementStacks, time + 90);
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

        if (FrankenstrikeOverchargedTime > 0)
            HandleFrankenstrikeOvercharged(npc);

        if (WhampireHypnosisTime > 0) {
            float dampening = npc.boss ? 0.92f : 0.55f;
            npc.velocity *= dampening;
            if (!npc.boss)
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -0.8f, 0.8f);
        }

        if (AlienXStasisTime > 0) {
            float dampening = npc.boss ? 0.88f : 0.22f;
            npc.velocity *= dampening;
            if (!npc.boss)
                npc.velocity = Vector2.Clamp(npc.velocity, new Vector2(-0.45f, -0.45f), new Vector2(0.45f, 0.45f));
        }

        if (BigChillDeepFreezeTime > 0) {
            float dampening = npc.boss ? 0.94f : 0.72f;
            npc.velocity *= dampening;
            if (!npc.boss)
                npc.velocity = Vector2.Clamp(npc.velocity, new Vector2(-2.1f, -2.1f), new Vector2(2.1f, 2.1f));
        }
        else if (BigChillFrostbiteTime > 0) {
            float dampening = npc.boss ? 0.97f : 0.84f;
            npc.velocity *= dampening;
        }

        if (PeskyDustDreamTime > 0) {
            float driftDampening = npc.boss ? 0.96f : 0.78f;
            npc.velocity *= driftDampening;
            if (!npc.boss)
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y - 0.05f, -1.6f, 1.2f);
        }
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) {
        if (BigChillFrostbiteTime > 0 && player.whoAmI == BigChillOwner)
            modifiers.ArmorPenetration += BigChillDeepFreezeArmorPenetration;

        if (IsFrankenstrikeOverchargedFor(player.whoAmI))
            modifiers.ArmorPenetration += 8;
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        if (BigChillFrostbiteTime > 0 && projectile.owner == BigChillOwner)
            modifiers.ArmorPenetration += BigChillDeepFreezeArmorPenetration;

        if (IsFrankenstrikeOverchargedFor(projectile.owner))
            modifiers.ArmorPenetration += 8;
    }

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (npc.HasBuff(ModContent.BuffType<AlienXSupernovaBurn>())) {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            npc.lifeRegen -= AlienXSupernovaBurn.LifeRegenPenalty;
            damage = System.Math.Max(damage, AlienXSupernovaBurn.CombatTextDamage);
        }
    }

    public override void OnKill(NPC npc) {
        if (EyeGuyExposedTime <= 0 || EyeGuyOwner < 0 || EyeGuyOwner >= Main.maxPlayers)
            return;

        Player owner = Main.player[EyeGuyOwner];
        if (!owner.active)
            return;

        owner.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(npc.boss ? 1.5f : 3f);
    }

    private void HandleFrankenstrikeOvercharged(NPC npc) {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (FrankenstrikeOverchargedArcTimer > 0) {
            FrankenstrikeOverchargedArcTimer--;
            return;
        }

        if (FrankenstrikeOverchargedOwner < 0 || FrankenstrikeOverchargedOwner >= Main.maxPlayers)
            return;

        Player owner = Main.player[FrankenstrikeOverchargedOwner];
        if (!owner.active || owner.dead)
            return;

        FrankenstrikeOverchargedArcTimer = npc.boss ? 34 : 24;
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

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (npc.HasBuff(ModContent.BuffType<AlienXSupernovaBurn>()))
            drawColor = Color.Lerp(drawColor, new Color(255, 188, 118), 0.34f);

        if (FasttrackComboTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(110, 255, 210), 0.2f + FasttrackComboStacks * 0.04f);

        if (AstrodactylSkyMarkTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(140, 255, 210), 0.28f);

        if (BlitzwolferResonanceTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(115, 255, 145), 0.16f + BlitzwolferResonanceStacks * 0.03f);

        if (EchoEchoResonanceTime > 0)
            drawColor = Color.Lerp(drawColor, EchoEchoResonancePrimed ? new Color(178, 238, 255) : new Color(150, 205, 255),
                0.12f + EchoEchoResonanceStacks * 0.025f);

        if (EchoEchoFractureTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(205, 235, 255), 0.18f);

        if (UltimateEchoEchoFocusedTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(165, 228, 255), 0.28f);

        if (FrankenstrikeConductiveTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(120, 205, 255), 0.18f + FrankenstrikeConductiveStacks * 0.04f);

        if (FrankenstrikeOverchargedTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(225, 242, 255), 0.24f);

        if (LodestarPolarityTime > 0)
            drawColor = Color.Lerp(drawColor, LodestarPolarityDirection >= 0 ? new Color(255, 120, 95) : new Color(125, 180, 255), 0.2f);

        if (WaterHazardSoakTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(120, 215, 255), 0.08f + 0.18f * (WaterHazardSoak / 100f));

        if (JetrayLockTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(100, 255, 225), 0.24f);

        if (BigChillFrostbiteTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(178, 232, 255), 0.22f);

        if (BigChillFrigidFractureTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(236, 248, 255), 0.16f);

        if (HeatBlastFlashpointTime > 0) {
            float flashpointRatio = HeatBlastFlashpointStacks / 5f;
            Color flashpointColor = Color.Lerp(new Color(255, 138, 62), new Color(255, 242, 210), flashpointRatio);
            drawColor = Color.Lerp(drawColor, flashpointColor, 0.12f + flashpointRatio * 0.22f);
        }

        if (EyeGuyExposedTime > 0) {
            drawColor = Color.Lerp(drawColor, new Color(255, 228, 170), 0.36f);
        }
        else if (EyeGuyFireMarkTime > 0 || EyeGuyFrostMarkTime > 0 || EyeGuyShockMarkTime > 0) {
            int red = 180;
            int green = 180;
            int blue = 180;
            int markCount = 0;
            if (EyeGuyFireMarkTime > 0) {
                red += 75;
                green += 10;
                markCount++;
            }
            if (EyeGuyFrostMarkTime > 0) {
                green += 55;
                blue += 75;
                markCount++;
            }
            if (EyeGuyShockMarkTime > 0) {
                green += 20;
                blue += 75;
                markCount++;
            }

            Color markColor = new Color((byte)Utils.Clamp(red, 0, 255), (byte)Utils.Clamp(green, 0, 255),
                (byte)Utils.Clamp(blue, 0, 255));
            drawColor = Color.Lerp(drawColor, markColor, 0.12f + markCount * 0.06f);
        }

        if (WhampirePreyTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(170, 45, 60), 0.3f);

        if (WhampireHypnosisTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(255, 170, 185), 0.36f);

        if (SnareOhCurseTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(220, 190, 120), 0.14f + SnareOhCurseStacks * 0.03f);

        if (AlienXJudgementTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(190, 200, 255), 0.16f + AlienXJudgementStacks * 0.04f);

        if (AlienXStasisTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(240, 245, 255), 0.28f);

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

        if (BlitzwolferResonanceTime > 0) {
            BlitzwolferResonanceTime--;
        }
        else {
            BlitzwolferResonanceOwner = -1;
            BlitzwolferResonanceStacks = 0;
        }

        if (EchoEchoResonanceTime > 0) {
            EchoEchoResonanceTime--;
        }
        else {
            EchoEchoResonanceOwner = -1;
            EchoEchoResonanceStacks = 0;
            EchoEchoResonanceLastSource = -1;
            EchoEchoResonancePrimed = false;
        }

        if (EchoEchoFractureTime > 0) {
            EchoEchoFractureTime--;
        }
        else {
            EchoEchoFractureOwner = -1;
        }

        if (UltimateEchoEchoFocusedTime > 0) {
            UltimateEchoEchoFocusedTime--;
        }
        else {
            UltimateEchoEchoFocusedOwner = -1;
        }

        if (FrankenstrikeOverchargedTime > 0) {
            FrankenstrikeOverchargedTime--;
            FrankenstrikeConductiveTime = System.Math.Max(FrankenstrikeConductiveTime - 1, 0);
            if (FrankenstrikeOverchargedTime <= 0)
                ClearFrankenstrikeState();
        }
        else if (FrankenstrikeConductiveTime > 0) {
            FrankenstrikeConductiveTime--;
        }
        else {
            ClearFrankenstrikeState();
        }

        if (LodestarPolarityTime > 0) {
            LodestarPolarityTime--;
        }
        else {
            LodestarPolarityOwner = -1;
            LodestarPolarityDirection = -1;
        }

        if (WaterHazardSoakTime > 0) {
            WaterHazardSoakTime--;
            if (WaterHazardSoakTime % 45 == 0)
                WaterHazardSoak = System.Math.Max(0, WaterHazardSoak - 6);
        }
        else {
            WaterHazardSoakOwner = -1;
            WaterHazardSoak = 0;
        }

        if (JetrayLockTime > 0) {
            JetrayLockTime--;
        }
        else {
            JetrayLockOwner = -1;
        }

        if (BigChillOwner != -1) {
            BigChillFrostbiteTime = System.Math.Max(BigChillFrostbiteTime - 1, 0);
            BigChillDeepFreezeTime = 0;
            BigChillDeepFreezePressure = 0;
            if (BigChillFrostbiteTime <= 0 || BigChillFrostbiteStacks <= 0)
                ClearBigChillState();
        }

        if (BigChillShiverburstCooldown > 0)
            BigChillShiverburstCooldown--;

        if (BigChillFrigidFractureTime > 0) {
            BigChillFrigidFractureTime--;
        }
        else {
            BigChillFrigidFractureOwner = -1;
        }

        if (HeatBlastFlashpointTime > 0) {
            HeatBlastFlashpointTime--;
            if (HeatBlastFlashpointTime <= 0)
                ClearHeatBlastState();
        }
        else {
            HeatBlastOwner = -1;
            HeatBlastFlashpointStacks = 0;
            HeatBlastFlashpointProgress = 0;
        }

        if (HeatBlastFlarePopCooldown > 0)
            HeatBlastFlarePopCooldown--;

        if (EyeGuyExposedTime > 0) {
            EyeGuyExposedTime--;
            EyeGuyFireMarkTime = System.Math.Max(EyeGuyFireMarkTime - 1, 0);
            EyeGuyFrostMarkTime = System.Math.Max(EyeGuyFrostMarkTime - 1, 0);
            EyeGuyShockMarkTime = System.Math.Max(EyeGuyShockMarkTime - 1, 0);
            if (EyeGuyExposedTime <= 0)
                ClearEyeGuyState();
        }
        else if (EyeGuyOwner != -1) {
            EyeGuyFireMarkTime = System.Math.Max(EyeGuyFireMarkTime - 1, 0);
            EyeGuyFrostMarkTime = System.Math.Max(EyeGuyFrostMarkTime - 1, 0);
            EyeGuyShockMarkTime = System.Math.Max(EyeGuyShockMarkTime - 1, 0);
            if (EyeGuyFireMarkTime <= 0 && EyeGuyFrostMarkTime <= 0 && EyeGuyShockMarkTime <= 0)
                ClearEyeGuyState();
        }

        if (WhampirePreyTime > 0) {
            WhampirePreyTime--;
        }
        else {
            WhampirePreyOwner = -1;
        }

        if (WhampireHypnosisTime > 0)
            WhampireHypnosisTime--;

        if (SnareOhCurseTime > 0) {
            SnareOhCurseTime--;
        }
        else {
            SnareOhCurseOwner = -1;
            SnareOhCurseStacks = 0;
        }

        if (AlienXJudgementTime > 0) {
            AlienXJudgementTime--;
        }
        else {
            AlienXJudgementOwner = -1;
            AlienXJudgementStacks = 0;
        }

        if (AlienXStasisTime > 0)
            AlienXStasisTime--;

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

    private void ClearEyeGuyState() {
        EyeGuyOwner = -1;
        EyeGuyFireMarkTime = 0;
        EyeGuyFrostMarkTime = 0;
        EyeGuyShockMarkTime = 0;
        EyeGuyExposedTime = 0;
    }

    private void ClearBigChillState() {
        BigChillOwner = -1;
        BigChillFrostbiteStacks = 0;
        BigChillFrostbiteTime = 0;
        BigChillDeepFreezeTime = 0;
        BigChillDeepFreezePressure = 0;
        BigChillDeepFreezeArmorPenetration = 8;
        BigChillShiverburstCooldown = 0;
    }

    private void ClearHeatBlastState() {
        HeatBlastOwner = -1;
        HeatBlastFlashpointStacks = 0;
        HeatBlastFlashpointProgress = 0;
        HeatBlastFlashpointTime = 0;
    }
}
