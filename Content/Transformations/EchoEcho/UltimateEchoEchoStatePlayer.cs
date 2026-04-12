using System;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EchoEcho;

public class UltimateEchoEchoStatePlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:UltimateEchoEcho";
    public const int OverclockDurationTicks = 6 * 60;
    public const int OverclockCooldownTicks = 24 * 60;
    public const int FeedbackPulseCooldownTicks = 18 * 60;
    public const int ResonantRelayCooldownTicks = 12 * 60;
    public const int ResonantRelayCataclysmCooldownTicks = 4 * 60;
    public const int HarmonicCataclysmDurationTicks = 8 * 60;
    public const int HarmonicCataclysmCooldownTicks = 60 * 60;

    private bool ultimateEchoEchoActive;
    private bool cataclysmWasActive;
    private int immediateVolleyCooldown;

    public bool OverclockActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return ultimateEchoEchoActive &&
                   omp.IsPrimaryAbilityActive &&
                   string.Equals(omp.primaryAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public bool CataclysmActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return ultimateEchoEchoActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public bool EffectiveOverclockActive => OverclockActive || CataclysmActive;
    public bool CanTriggerImmediateVolley => immediateVolleyCooldown <= 0;

    public int OverclockTicksRemaining {
        get {
            if (!OverclockActive)
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

    public void ConsumeImmediateVolleyCooldown(int cooldownTicks) {
        immediateVolleyCooldown = Math.Max(immediateVolleyCooldown, Math.Max(1, cooldownTicks));
    }

    public override void ResetEffects() {
        ultimateEchoEchoActive = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == TransformationId;
        if (!ultimateEchoEchoActive) {
            cataclysmWasActive = false;
            immediateVolleyCooldown = 0;
        }
    }

    public override void PostUpdate() {
        if (!ultimateEchoEchoActive)
            return;

        if (immediateVolleyCooldown > 0)
            immediateVolleyCooldown--;

        bool cataclysmActive = CataclysmActive;
        if ((Player.whoAmI == Main.myPlayer || Main.netMode == Terraria.ID.NetmodeID.Server) &&
            !Player.dead &&
            !cataclysmActive &&
            cataclysmWasActive) {
            UltimateEchoEchoTransformation.TriggerCataclysmShutdown(Player);
        }

        cataclysmWasActive = cataclysmActive;
    }
}
