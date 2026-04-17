using System;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Humungousaur;

public class UltimateHumungousaurStatePlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:UltimateHumungousaur";
    public const int BreachMaxStacks = 5;
    public const int TitanChargeDurationTicks = 6 * 60;
    public const int TitanChargeCooldownTicks = 18 * 60;
    public const int MeteorStompCooldownTicks = 14 * 60;
    public const int CataclysmDurationTicks = 8 * 60;
    public const int CataclysmCooldownTicks = 60 * 60;

    private bool transformationActive;
    private bool cataclysmWasActive;
    private int comboResetTime;
    private int comboStep;

    public bool TitanChargeActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return transformationActive &&
                   omp.IsPrimaryAbilityActive &&
                   string.Equals(omp.primaryAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public bool CataclysmActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return transformationActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public int ComboStep => transformationActive ? comboStep : 0;

    public int TitanChargeTicksRemaining {
        get {
            if (!TitanChargeActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>()
                .GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.PrimaryAbility);
        }
    }

    public int CataclysmTicksRemaining {
        get {
            if (!CataclysmActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>()
                .GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public int ConsumeComboStep() {
        int step = comboStep;
        comboStep = (comboStep + 1) % 3;
        comboResetTime = 42;
        return step;
    }

    public override void ResetEffects() {
        transformationActive = string.Equals(Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId, TransformationId,
            StringComparison.Ordinal);
        if (transformationActive)
            return;

        cataclysmWasActive = false;
        comboResetTime = 0;
        comboStep = 0;
    }

    public override void PostUpdate() {
        if (!transformationActive)
            return;

        if (comboResetTime > 0) {
            comboResetTime--;
            if (comboResetTime <= 0)
                comboStep = 0;
        }

        bool cataclysmActive = CataclysmActive;
        if (Player.whoAmI == Main.myPlayer &&
            !Player.dead &&
            !cataclysmActive &&
            cataclysmWasActive) {
            UltimateHumungousaurTransformation.TriggerCataclysmShutdownPulse(Player);
        }

        cataclysmWasActive = cataclysmActive;
    }
}
