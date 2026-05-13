using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Rath;

public class RathStatePlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:Rath";

    private const int ComboResetTicks = 38;
    private const int BossImpactGuardTicks = 42;
    private const int NormalImpactGuardTicks = 24;

    private bool rathActive;
    private int comboStep;
    private int comboResetTimer;
    private int guardTime;
    private float guardStrength;

    public bool GuardActive => guardTime > 0;
    public float GuardStrength => GuardActive ? guardStrength : 0f;

    public bool RageActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return rathActive && omp.PrimaryAbilityEnabled;
        }
    }

    public override void ResetEffects() {
        rathActive = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == TransformationId;
    }

    public override void PostUpdate() {
        if (!rathActive) {
            comboStep = 0;
            comboResetTimer = 0;
            guardTime = 0;
            guardStrength = 0f;
            return;
        }

        if (comboResetTimer > 0) {
            comboResetTimer--;
        }
        else {
            comboStep = 0;
        }

        if (guardTime <= 0) {
            guardStrength = 0f;
            return;
        }

        guardTime--;
        if (guardTime <= 0)
            guardStrength = 0f;
    }

    public int ConsumeComboStep() {
        if (comboResetTimer <= 0)
            comboStep = 0;

        int currentStep = comboStep;
        comboResetTimer = ComboResetTicks;
        comboStep = currentStep >= 2 ? 0 : currentStep + 1;
        return currentStep;
    }

    public void RegisterRathGuard(int duration, float strength) {
        if (!rathActive || duration <= 0 || strength <= 0f)
            return;

        guardTime = Math.Max(guardTime, duration);
        guardStrength = MathHelper.Clamp(Math.Max(guardStrength, strength), 0f, RageActive ? 0.25f : 0.21f);
    }

    public void RegisterRathImpact(NPC target, bool pounce, bool finisher) {
        if (target == null || !target.active)
            return;

        bool bossHit = target.boss || target.lifeMax >= 3000;
        float strength = bossHit ? 0.13f : 0.08f;
        if (pounce)
            strength += bossHit ? 0.05f : 0.03f;
        if (finisher)
            strength += 0.03f;
        if (RageActive)
            strength += 0.03f;

        RegisterRathGuard(bossHit ? BossImpactGuardTicks : NormalImpactGuardTicks, strength);
    }
}
