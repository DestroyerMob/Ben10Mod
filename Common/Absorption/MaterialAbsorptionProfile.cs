using Microsoft.Xna.Framework;

namespace Ben10Mod.Common.Absorption;

public sealed class MaterialAbsorptionProfile {
    public int SourceItemType { get; init; }
    public string DisplayName { get; init; } = "";
    public Color TintColor { get; init; } = Color.White;
    public int ConsumeAmount { get; init; }
    public int DurationTicks { get; init; }
    public float GenericDamageBonus { get; init; }
    public int DefenseBonus { get; init; }
    public float EnduranceBonus { get; init; }
    public float MeleeKnockbackBonus { get; init; }
}
