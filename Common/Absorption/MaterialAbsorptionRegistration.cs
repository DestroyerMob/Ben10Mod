using System.Collections.Generic;

namespace Ben10Mod.Common.Absorption;

public sealed class MaterialAbsorptionRegistration {
    public int SourceItemType { get; init; }
    public int SwordItemType { get; init; }
    public int HelmetItemType { get; init; }
    public int BodyItemType { get; init; }
    public int LegItemType { get; init; }

    public int? ConsumeAmountOverride { get; set; }
    public int? DurationTicksOverride { get; set; }
    public float? GenericDamageBonusOverride { get; set; }
    public int? DefenseBonusOverride { get; set; }
    public float? EnduranceBonusOverride { get; set; }
    public float? MeleeKnockbackBonusOverride { get; set; }

    public List<MaterialAbsorptionHitEffect> HitEffects { get; } = new();

    public MaterialAbsorptionRegistration(int sourceItemType, int swordItemType, int helmetItemType, int bodyItemType, int legItemType) {
        SourceItemType = sourceItemType;
        SwordItemType = swordItemType;
        HelmetItemType = helmetItemType;
        BodyItemType = bodyItemType;
        LegItemType = legItemType;
    }

    public MaterialAbsorptionRegistration AddHitBuff(int buffType, int buffTime) {
        HitEffects.Add(new MaterialAbsorptionHitEffect {
            BuffType = buffType,
            BuffTime = buffTime
        });
        return this;
    }
}
