using Terraria.ModLoader;

namespace Ben10Mod.Common.Absorption;

public class MaterialAbsorptionPlayer : ModPlayer {
    public bool OsmosianEquipped { get; set; }
    public bool AnoditeCatalystEquipped { get; set; }
    public float DurationMultiplier { get; set; } = 1f;
    public float StrengthMultiplier { get; set; } = 1f;
    public float CostMultiplier { get; set; } = 1f;
    public float DebuffDurationMultiplier { get; set; } = 1f;
    public int CritChanceBonus { get; set; }
    public int ArmorPenBonus { get; set; }
    public float MeleeSpeedBonus { get; set; }
    public float MeleeKnockbackBonus { get; set; }
    public float MoveSpeedBonus { get; set; }
    public int LifeRegenBonus { get; set; }
    public int MaxLifeBonus { get; set; }
    public int FlatDefenseBonus { get; set; }
    public int AbsorbedMaterialItemType { get; set; }
    public int AbsorbedMaterialTime { get; set; }

    public override void ResetEffects() {
        ResetAccessoryEffects();
    }

    public void ResetAccessoryEffects() {
        OsmosianEquipped = false;
        AnoditeCatalystEquipped = false;
        DurationMultiplier = 1f;
        StrengthMultiplier = 1f;
        CostMultiplier = 1f;
        DebuffDurationMultiplier = 1f;
        CritChanceBonus = 0;
        ArmorPenBonus = 0;
        MeleeSpeedBonus = 0f;
        MeleeKnockbackBonus = 0f;
        MoveSpeedBonus = 0f;
        LifeRegenBonus = 0;
        MaxLifeBonus = 0;
        FlatDefenseBonus = 0;
    }

    public bool TryGetActiveProfile(out MaterialAbsorptionProfile profile) {
        if (AbsorbedMaterialTime > 0 && AbsorbedMaterialItemType > 0)
            return MaterialAbsorptionRegistry.TryGetProfile(AbsorbedMaterialItemType, out profile);

        profile = null;
        return false;
    }
}
