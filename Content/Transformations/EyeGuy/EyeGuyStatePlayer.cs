using System;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EyeGuy;

public class EyeGuyStatePlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:EyeGuy";
    public const int WatcherArrayDurationTicks = 8 * 60;
    public const int WatcherArrayCooldownTicks = 26 * 60;
    public const int AllEyesOpenDurationTicks = 8 * 60;
    public const int AllEyesOpenCooldownTicks = 60 * 60;

    private bool eyeGuyActive;
    private bool allEyesOpenWasActive;
    private int nextBurstElementIndex;
    private int nextWatcherEyeIndex;

    public bool WatcherArrayActive => eyeGuyActive && Player.GetModPlayer<OmnitrixPlayer>().IsSecondaryAbilityActive;

    public bool AllEyesOpenActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return eyeGuyActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public bool HasWatcherEyes => WatcherArrayActive || AllEyesOpenActive;

    public int WatcherTicksRemaining {
        get {
            if (!WatcherArrayActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>().GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.SecondaryAbility);
        }
    }

    public int AllEyesOpenTicksRemaining {
        get {
            if (!AllEyesOpenActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>().GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public EyeGuyElement PeekBurstElement(int offset = 0) {
        return (EyeGuyElement)((nextBurstElementIndex + offset) % 3);
    }

    public EyeGuyElement ConsumeBurstElement() {
        EyeGuyElement element = PeekBurstElement();
        nextBurstElementIndex = (nextBurstElementIndex + 1) % 3;
        return element;
    }

    public int ConsumeWatcherEyeIndex() {
        int eyeIndex = nextWatcherEyeIndex;
        nextWatcherEyeIndex = (nextWatcherEyeIndex + 1) % 4;
        return eyeIndex;
    }

    public override void ResetEffects() {
        eyeGuyActive = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == TransformationId;
        if (!eyeGuyActive) {
            allEyesOpenWasActive = false;
            nextBurstElementIndex = 0;
            nextWatcherEyeIndex = 0;
        }
    }

    public override void PostUpdate() {
        if (!eyeGuyActive)
            return;

        bool allEyesOpenActive = AllEyesOpenActive;
        if (Player.whoAmI == Main.myPlayer &&
            !Player.dead &&
            !allEyesOpenActive &&
            allEyesOpenWasActive) {
            EyeGuyTransformation.TriggerAllEyesOpenShutdownPulse(Player);
        }

        allEyesOpenWasActive = allEyesOpenActive;
    }
}
