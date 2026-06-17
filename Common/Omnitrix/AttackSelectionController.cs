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

    public static bool IsAbilityAttackSelection(OmnitrixPlayer.AttackSelection selection) {
        return selection is OmnitrixPlayer.AttackSelection.PrimaryAbility
            or OmnitrixPlayer.AttackSelection.SecondaryAbility
            or OmnitrixPlayer.AttackSelection.TertiaryAbility;
    }
}
