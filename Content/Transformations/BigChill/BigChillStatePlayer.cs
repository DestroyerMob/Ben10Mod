using System;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.BigChill;

public class BigChillStatePlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:BigChill";
    public const string UltimateTransformationId = "Ben10Mod:UltimateBigChill";
    public const int PhaseDriftEmpowerDurationTicks = 2 * 60;
    public const int PhaseDriftIntangibleTicks = 18;
    public const int PhaseDriftCooldownTicks = 14 * 60;
    public const int GraveMistCooldownTicks = 18 * 60;
    public const int AbsoluteZeroDurationTicks = 8 * 60;
    public const int AbsoluteZeroCooldownTicks = 60 * 60;
    public const int HungerBoostDurationTicks = 90;

    private bool bigChillActive;
    private bool absoluteZeroWasActive;
    private int phaseDriftIntangibleTime;
    private int hungerBoostTime;
    private int nextSideLanceDirection = 1;

    public bool PhaseDriftEmpowered => bigChillActive && Player.GetModPlayer<OmnitrixPlayer>().IsPrimaryAbilityActive;

    public bool AbsoluteZeroActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return bigChillActive &&
                   omp.IsUltimateAbilityActive &&
                   IsBigChillTransformationId(omp.ultimateAbilityTransformationId);
        }
    }

    public bool PhaseDriftIntangibleActive => bigChillActive && phaseDriftIntangibleTime > 0;
    public bool HungerBoostActive => bigChillActive && hungerBoostTime > 0;

    public int PhaseDriftTicksRemaining {
        get {
            if (!PhaseDriftEmpowered)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>()
                .GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.PrimaryAbility);
        }
    }

    public int AbsoluteZeroTicksRemaining {
        get {
            if (!AbsoluteZeroActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>()
                .GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public int HungerBoostTicksRemaining => HungerBoostActive ? hungerBoostTime : 0;

    public static bool IsBigChillTransformationId(string transformationId) {
        return string.Equals(transformationId, TransformationId, StringComparison.Ordinal) ||
               string.Equals(transformationId, UltimateTransformationId, StringComparison.Ordinal);
    }

    public void StartPhaseDrift() {
        phaseDriftIntangibleTime = Math.Max(phaseDriftIntangibleTime, PhaseDriftIntangibleTicks);
    }

    public void ApplyHungerSurge() {
        hungerBoostTime = Math.Max(hungerBoostTime, HungerBoostDurationTicks);
    }

    public int ConsumeSideLanceDirection() {
        int direction = nextSideLanceDirection;
        nextSideLanceDirection *= -1;
        return direction;
    }

    public override void ResetEffects() {
        bigChillActive = IsBigChillTransformationId(Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId);
        if (bigChillActive)
            return;

        absoluteZeroWasActive = false;
        phaseDriftIntangibleTime = 0;
        hungerBoostTime = 0;
        nextSideLanceDirection = 1;
    }

    public override void PostUpdate() {
        if (!bigChillActive)
            return;

        if (phaseDriftIntangibleTime > 0)
            phaseDriftIntangibleTime--;

        if (hungerBoostTime > 0)
            hungerBoostTime--;

        bool absoluteZeroActive = AbsoluteZeroActive;
        if (Player.whoAmI == Main.myPlayer && !Player.dead) {
            if (absoluteZeroActive && !absoluteZeroWasActive)
                BigChillTransformation.HandleAbsoluteZeroActivated(Player);

            if (!absoluteZeroActive && absoluteZeroWasActive)
                BigChillTransformation.TriggerAbsoluteZeroShutdownPulse(Player);
        }

        absoluteZeroWasActive = absoluteZeroActive;
    }
}
