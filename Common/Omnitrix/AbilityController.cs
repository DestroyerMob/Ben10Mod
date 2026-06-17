namespace Ben10Mod.Common.Omnitrix;

public sealed class AbilityController {
    public bool PrimaryEnabled { get; set; }
    public bool PrimaryWasEnabled { get; set; }
    public bool SecondaryEnabled { get; set; }
    public bool SecondaryWasEnabled { get; set; }
    public bool TertiaryEnabled { get; set; }
    public bool TertiaryWasEnabled { get; set; }
    public bool UltimateEnabled { get; set; }
    public bool UltimateWasEnabled { get; set; }

    public string PrimaryTransformationId { get; set; } = "";
    public string SecondaryTransformationId { get; set; } = "";
    public string TertiaryTransformationId { get; set; } = "";
    public string UltimateTransformationId { get; set; } = "";

    public void ResetActiveFlags() {
        PrimaryEnabled = false;
        SecondaryEnabled = false;
        TertiaryEnabled = false;
        UltimateEnabled = false;
    }

    public string GetTransformationId(OmnitrixPlayer.AttackSelection selection) {
        return selection switch {
            OmnitrixPlayer.AttackSelection.PrimaryAbility => PrimaryTransformationId,
            OmnitrixPlayer.AttackSelection.SecondaryAbility => SecondaryTransformationId,
            OmnitrixPlayer.AttackSelection.TertiaryAbility => TertiaryTransformationId,
            OmnitrixPlayer.AttackSelection.Ultimate => UltimateTransformationId,
            _ => ""
        };
    }

    public void SetTransformationId(OmnitrixPlayer.AttackSelection selection, string transformationId) {
        transformationId ??= "";

        switch (selection) {
            case OmnitrixPlayer.AttackSelection.PrimaryAbility:
                PrimaryTransformationId = transformationId;
                break;
            case OmnitrixPlayer.AttackSelection.SecondaryAbility:
                SecondaryTransformationId = transformationId;
                break;
            case OmnitrixPlayer.AttackSelection.TertiaryAbility:
                TertiaryTransformationId = transformationId;
                break;
            case OmnitrixPlayer.AttackSelection.Ultimate:
                UltimateTransformationId = transformationId;
                break;
        }
    }
}
