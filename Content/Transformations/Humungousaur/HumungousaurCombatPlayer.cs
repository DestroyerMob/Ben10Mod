using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Humungousaur;

public class HumungousaurCombatPlayer : ModPlayer {
    private const int BossImpactGuardTicks = 54;
    private const int NormalImpactGuardTicks = 30;

    private int guardTime;
    private int rampagePunchCounter;
    private float guardStrength;

    public bool GuardActive => guardTime > 0;
    public float GuardStrength => GuardActive ? guardStrength : 0f;
    public bool RampageActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return omp.currentTransformationId == HumungousaurTransformation.TransformationId &&
                   omp.IsUltimateAbilityActive &&
                   omp.ultimateAbilityTransformationId == HumungousaurTransformation.TransformationId;
        }
    }

    public override void PostUpdate() {
        if (!IsHumungousaurActive()) {
            guardTime = 0;
            rampagePunchCounter = 0;
            guardStrength = 0f;
            return;
        }

        if (!RampageActive)
            rampagePunchCounter = 0;

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

        RegisterGuard(bossHit ? BossImpactGuardTicks : NormalImpactGuardTicks, strength);
    }

    private void RegisterGuard(int duration, float strength) {
        if (!IsHumungousaurActive() || duration <= 0 || strength <= 0f)
            return;

        guardTime = Math.Max(guardTime, duration);
        guardStrength = MathHelper.Clamp(Math.Max(guardStrength, strength), 0f, 0.28f);
    }

    private bool IsHumungousaurActive() {
        string transformationId = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId;
        return transformationId == HumungousaurTransformation.TransformationId ||
               transformationId == HumungousaurTransformation.UltimateTransformationId;
    }
}
