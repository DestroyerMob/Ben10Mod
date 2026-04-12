using System;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Frankenstrike;

public class FrankenstrikeStatePlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:Frankenstrike";
    public const int ConductiveMaxStacks = 6;
    public const int OverchargedDurationTicks = 4 * 60;
    public const int GalvanizedDurationTicks = 3 * 60;
    public const int ThunderLeapCooldownTicks = 14 * 60;
    public const int StormheartDurationTicks = 8 * 60;
    public const int StormheartCooldownTicks = 60 * 60;

    private bool frankenstrikeActive;
    private bool stormheartWasActive;
    private int galvanizedTime;
    private int comboResetTime;
    private int comboStep;

    public bool GalvanizedActive => frankenstrikeActive && galvanizedTime > 0;

    public bool StormheartActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return frankenstrikeActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public int GalvanizedTicksRemaining => GalvanizedActive ? galvanizedTime : 0;

    public int StormheartTicksRemaining {
        get {
            if (!StormheartActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>()
                .GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public int ActiveSpireCount => FrankenstrikeCapacitorSpireProjectile.GetOwnedSpires(Player).Count;

    public int ConsumeComboStep() {
        int step = comboStep;
        comboStep = (comboStep + 1) % 3;
        comboResetTime = 42;
        return step;
    }

    public void ApplyGalvanized() {
        galvanizedTime = Math.Max(galvanizedTime, GalvanizedDurationTicks);
    }

    public override void ResetEffects() {
        frankenstrikeActive =
            string.Equals(Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId, TransformationId, StringComparison.Ordinal);
        if (frankenstrikeActive)
            return;

        stormheartWasActive = false;
        galvanizedTime = 0;
        comboResetTime = 0;
        comboStep = 0;
    }

    public override void PostUpdate() {
        if (!frankenstrikeActive)
            return;

        if (galvanizedTime > 0)
            galvanizedTime--;

        if (comboResetTime > 0) {
            comboResetTime--;
            if (comboResetTime <= 0)
                comboStep = 0;
        }

        bool stormheartActive = StormheartActive;
        if (Player.whoAmI == Main.myPlayer &&
            !Player.dead &&
            !stormheartActive &&
            stormheartWasActive) {
            FrankenstrikeTransformation.TriggerStormheartShutdown(Player);
        }

        stormheartWasActive = stormheartActive;
    }
}
