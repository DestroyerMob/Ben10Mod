namespace Ben10Mod.Common.Omnitrix;

public sealed class AttackSelectionController {
    public OmnitrixPlayer.AttackSelection Current { get; set; } = OmnitrixPlayer.AttackSelection.Primary;
    public OmnitrixPlayer.AttackSelection Base { get; set; } = OmnitrixPlayer.AttackSelection.Primary;
    public bool LoadedAbilityAttackUsed { get; set; }
    public int AttackSerial { get; set; }
    public int AttackDamage { get; set; }
    public int UltimateEchoEchoSpeakerSpawnSerial { get; set; }
    public int PulseTime { get; set; }
    public int EnergyGainLockTime { get; set; }
    public int MarkProjectilesNoEnergyGainTime { get; set; }

    public bool HasLoadedAbilityAttack => IsAbilityAttackSelection(Current);

    public bool HasLoadedBadgeAttack => Current is not OmnitrixPlayer.AttackSelection.Primary
        and not OmnitrixPlayer.AttackSelection.Secondary;

    public float GetPulseProgress(int pulseDuration) {
        return pulseDuration > 0 ? PulseTime / (float)pulseDuration : 0f;
    }

    public bool Select(OmnitrixPlayer.AttackSelection selection, int pulseDuration) {
        bool changed = Current != selection;
        Current = selection;
        if (selection is OmnitrixPlayer.AttackSelection.Primary or OmnitrixPlayer.AttackSelection.Secondary)
            Base = selection;

        if (changed)
            TriggerPulse(pulseDuration);

        return changed;
    }

    public bool ResetToBase(int pulseDuration) {
        return Select(Base, pulseDuration);
    }

    public bool ToggleBaseSelection(int pulseDuration) {
        Base = Base == OmnitrixPlayer.AttackSelection.Primary
            ? OmnitrixPlayer.AttackSelection.Secondary
            : OmnitrixPlayer.AttackSelection.Primary;
        return Select(Base, pulseDuration);
    }

    public void MarkLoadedAbilityAttackUsed() {
        LoadedAbilityAttackUsed = true;
    }

    public void ClearLoadedAbilityAttackUsed() {
        LoadedAbilityAttackUsed = false;
    }

    public bool ShouldApplyLoadedAttackCooldown(bool addCooldownIfUsed) {
        return addCooldownIfUsed && LoadedAbilityAttackUsed;
    }

    public void NotifyAttackSpentEnergy(int energyCost, int sustainEnergyCost, int attackLockFrames) {
        if (energyCost <= 0 && sustainEnergyCost <= 0)
            return;

        EnergyGainLockTime = System.Math.Max(EnergyGainLockTime, System.Math.Max(8, attackLockFrames));
        MarkProjectilesNoEnergyGainTime = System.Math.Max(MarkProjectilesNoEnergyGainTime, 3);
    }

    public bool ShouldMarkSpawnedAttackProjectilesAsNoEnergyGain() {
        return MarkProjectilesNoEnergyGainTime > 0;
    }

    public bool BlocksOmnitrixEnergyGain() {
        return EnergyGainLockTime > 0;
    }

    public void TriggerPulse(int pulseDuration) {
        PulseTime = System.Math.Max(0, pulseDuration);
    }

    public void Tick() {
        if (PulseTime > 0)
            PulseTime--;
        if (EnergyGainLockTime > 0)
            EnergyGainLockTime--;
        if (MarkProjectilesNoEnergyGainTime > 0)
            MarkProjectilesNoEnergyGainTime--;
    }

    public static bool IsAbilityAttackSelection(OmnitrixPlayer.AttackSelection selection) {
        return selection is OmnitrixPlayer.AttackSelection.PrimaryAbility
            or OmnitrixPlayer.AttackSelection.SecondaryAbility
            or OmnitrixPlayer.AttackSelection.TertiaryAbility;
    }
}
