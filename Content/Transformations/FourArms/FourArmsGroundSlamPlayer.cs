using System;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.FourArms;

public class FourArmsGroundSlamPlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:FourArms";
    public const float BerserkActivationThresholdRatio = 0.9f;

    private const float MaxRage = 100f;
    private const float InFormRageDecay = 0.18f;
    private const float OutOfFormRageDecay = 5f;
    private const int ComboResetTicks = 34;
    private const int SlamTapLockTicks = 8;
    private const int GroundSlamCooldownTicks = 5 * 60;
    private const float GroundSlamDamageMultiplier = 1.12f;
    private const float BerserkGroundSlamDamageMultiplier = 1.34f;
    private const float BaseGroundSlamKnockback = 7.5f;
    private const int FallbackBaseDamage = 32;
    private const int TransientStateGraceTicks = 3;

    private bool fourArmsActive;
    private int comboStep;
    private int comboResetTimer;
    private int slamTapLockTime;
    private int haymakerArmorTime;
    private int groundSlamStateTime;
    private float haymakerChargeRatio;

    public float Rage { get; private set; }
    public float RageRatio => MathHelper.Clamp(Rage / MaxRage, 0f, 1f);
    public bool HasFullRage => Rage >= MaxRage - 0.01f;
    public bool HasBerserkThreshold => RageRatio >= BerserkActivationThresholdRatio;
    public bool HaymakerCharging => haymakerArmorTime > 0;
    public bool GroundSlamActive => groundSlamStateTime > 0;
    public float HaymakerChargeRatio => haymakerChargeRatio;
    public bool FinisherReady => comboResetTimer > 0 && comboStep >= 2;
    public int NextComboHit => FinisherReady ? 3 : comboResetTimer > 0 ? comboStep + 1 : 1;

    public bool BerserkActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return fourArmsActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public int BerserkTicksRemaining {
        get {
            if (!BerserkActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>().GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public float BerserkProgress {
        get {
            int remaining = BerserkTicksRemaining;
            if (remaining <= 0)
                return 0f;

            return MathHelper.Clamp(remaining / (float)FourArmsTransformation.BerserkDurationTicks, 0f, 1f);
        }
    }

    public override void ResetEffects() {
        fourArmsActive = IsFourArmsActive();
    }

    public override void PostUpdate() {
        UpdateTransientStates();

        if (!fourArmsActive) {
            Rage = Math.Max(0f, Rage - OutOfFormRageDecay);
            comboStep = 0;
            comboResetTimer = 0;
            return;
        }

        if (comboResetTimer > 0) {
            comboResetTimer--;
        }
        else {
            comboStep = 0;
        }

        UpdateRageDecay();
        TryStartDashTriggeredGroundSlam();
    }

    public void AddRage(float amount) {
        if (!fourArmsActive || BerserkActive || amount <= 0f)
            return;

        Rage = MathHelper.Clamp(Rage + amount, 0f, MaxRage);
    }

    public float ConsumeAllRage() {
        float consumed = Rage;
        Rage = 0f;
        return consumed;
    }

    public int ConsumeComboStep() {
        if (comboResetTimer <= 0)
            comboStep = 0;

        int currentStep = comboStep;
        comboResetTimer = ComboResetTicks;
        comboStep = currentStep >= 2 ? 0 : currentStep + 1;
        return currentStep;
    }

    public void RegisterHaymakerCharge(float chargeRatio) {
        haymakerArmorTime = Math.Max(haymakerArmorTime, TransientStateGraceTicks);
        haymakerChargeRatio = MathHelper.Clamp(chargeRatio, 0f, 1f);
    }

    public void RegisterGroundSlamState() {
        groundSlamStateTime = Math.Max(groundSlamStateTime, TransientStateGraceTicks);
    }

    public bool TryStartGroundSlam() {
        if (Player.whoAmI != Main.myPlayer || !CanStartGroundSlam())
            return false;

        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        bool grounded = AlienIdentityPlayer.IsGrounded(Player);
        bool berserk = BerserkActive;
        float damageMultiplier = berserk ? BerserkGroundSlamDamageMultiplier : GroundSlamDamageMultiplier;
        int damage = ResolveHeroDamage(damageMultiplier);
        float knockback = BaseGroundSlamKnockback + (berserk ? 1.5f : 0f);

        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
            ModContent.ProjectileType<FourArmsGroundSlamSequenceProjectile>(), damage, knockback, Player.whoAmI,
            berserk ? 1f : 0f, grounded ? 1f : 0f);

        slamTapLockTime = SlamTapLockTicks;
        groundSlamStateTime = Math.Max(groundSlamStateTime, GroundSlamCooldownTicks / 15);
        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), GroundSlamCooldownTicks);
        return true;
    }

    public bool CanStartGroundSlam() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        return fourArmsActive &&
               !Player.dead &&
               !Player.CCed &&
               !Player.noItems &&
               !Player.mount.Active &&
               !Player.HasBuff<PrimaryAbilityCooldown>() &&
               !GroundSlamActive &&
               !HaymakerCharging &&
               !omp.HasLoadedAbilityAttack;
    }

    private void UpdateTransientStates() {
        if (slamTapLockTime > 0)
            slamTapLockTime--;

        if (haymakerArmorTime > 0) {
            haymakerArmorTime--;
            if (haymakerArmorTime == 0)
                haymakerChargeRatio = 0f;
        }

        if (groundSlamStateTime > 0)
            groundSlamStateTime--;
    }

    private void UpdateRageDecay() {
        if (BerserkActive) {
            Rage = 0f;
            return;
        }

        float decay = InFormRageDecay;
        if (comboResetTimer > 0 || HaymakerCharging || GroundSlamActive)
            decay *= 0.5f;

        Rage = Math.Max(0f, Rage - decay);
    }

    private void TryStartDashTriggeredGroundSlam() {
        if (Player.whoAmI != Main.myPlayer || slamTapLockTime > 0)
            return;

        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        if (omp.DashDir != OmnitrixPlayer.DashDown)
            return;

        slamTapLockTime = SlamTapLockTicks;
        TryStartGroundSlam();
    }

    private int ResolveHeroDamage(float multiplier) {
        float baseDamage = ResolveBaseDamage() * multiplier;
        return Math.Max(1, (int)Math.Round(Player.GetDamage<HeroDamage>().ApplyTo(baseDamage)));
    }

    private int ResolveBaseDamage() {
        Item heldItem = Player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }

    private bool IsFourArmsActive() {
        return Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == TransformationId;
    }
}
