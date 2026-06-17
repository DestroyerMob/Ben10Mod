using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Humungousaur;

public class HumungousaurCombatPlayer : ModPlayer {
    private const int BossImpactGuardTicks = 54;
    private const int NormalImpactGuardTicks = 30;
    private const int BraceWarmupTicks = 8;
    private const int BraceMaxTicks = 24;

    private int guardTime;
    private int braceTime;
    private int bracePulseTimer;
    private int rampagePunchCounter;
    private float guardStrength;
    private bool braceUpdatedThisTick;

    public bool GuardActive => guardTime > 0;
    public float GuardStrength => GuardActive ? guardStrength : 0f;
    public bool BraceActive => braceTime >= BraceWarmupTicks;
    public float BraceRatio => MathHelper.Clamp(braceTime / (float)BraceMaxTicks, 0f, 1f);

    public bool RampageActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return omp.currentTransformationId == HumungousaurTransformation.TransformationId &&
                   omp.IsUltimateAbilityActive &&
                   omp.ultimateAbilityTransformationId == HumungousaurTransformation.TransformationId;
        }
    }

    public override void ResetEffects() {
        braceUpdatedThisTick = false;
    }

    public override void PostUpdate() {
        if (!IsHumungousaurActive()) {
            guardTime = 0;
            braceTime = 0;
            bracePulseTimer = 0;
            rampagePunchCounter = 0;
            guardStrength = 0f;
            return;
        }

        if (!braceUpdatedThisTick)
            braceTime = Math.Max(0, braceTime - 3);

        if (!RampageActive)
            rampagePunchCounter = 0;

        if (!BraceActive)
            bracePulseTimer = 0;

        if (guardTime <= 0) {
            guardStrength = 0f;
            return;
        }

        guardTime--;
        if (guardTime <= 0)
            guardStrength = 0f;
    }

    public void RegisterAttackGuard(int duration, float strength) {
        RegisterGuard(duration, strength);
    }

    public void UpdateBraceState(float growthScale, bool rampageActive) {
        braceUpdatedThisTick = true;
        bool canBrace = (growthScale > 1.08f || rampageActive) &&
                        Player.controlDown &&
                        Math.Abs(Player.velocity.Y) < 0.08f &&
                        !Player.mount.Active &&
                        !Player.CCed;

        if (canBrace) {
            braceTime = Math.Min(BraceMaxTicks, braceTime + 2);
        }
        else {
            braceTime = Math.Max(0, braceTime - 3);
        }
    }

    public bool ConsumeBracePulse(int pulseInterval) {
        if (!BraceActive || !RampageActive || pulseInterval <= 0)
            return false;

        bracePulseTimer++;
        if (bracePulseTimer < pulseInterval)
            return false;

        bracePulseTimer = 0;
        return true;
    }

    public bool RegisterRampagePunch(int pulseInterval) {
        if (!RampageActive || pulseInterval <= 0)
            return false;

        rampagePunchCounter++;
        if (rampagePunchCounter < pulseInterval)
            return false;

        rampagePunchCounter = 0;
        return true;
    }

    public void RegisterImpactGuard(NPC target, float growthScale, bool shockwave, bool heavyHit) {
        if (target == null || !target.active)
            return;

        bool bossHit = target.boss || target.lifeMax >= 3000;
        float growthRatio = MathHelper.Clamp(
            (growthScale - 1f) / (HumungousaurTransformation.UltimateGrownScale - 1f), 0f, 1f);
        float strength = shockwave ? 0.1f : 0.12f;

        strength += growthRatio * 0.08f;
        if (bossHit)
            strength += 0.06f;
        if (heavyHit)
            strength += 0.03f;
        if (BraceActive)
            strength += 0.05f * BraceRatio;

        RegisterGuard(bossHit ? BossImpactGuardTicks : NormalImpactGuardTicks, strength);
    }

    private void RegisterGuard(int duration, float strength) {
        if (!IsHumungousaurActive() || duration <= 0 || strength <= 0f)
            return;

        guardTime = Math.Max(guardTime, duration);
        float maxGuard = BraceActive ? 0.35f : 0.28f;
        guardStrength = MathHelper.Clamp(Math.Max(guardStrength, strength), 0f, maxGuard);
    }

    private bool IsHumungousaurActive() {
        string transformationId = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId;
        return transformationId == HumungousaurTransformation.TransformationId ||
               transformationId == HumungousaurTransformation.UltimateTransformationId;
    }
}
