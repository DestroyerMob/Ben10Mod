using System;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Cannonbolt;

public class CannonboltStatePlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:Cannonbolt";
    public const int MaxBounceCount = 6;
    private const int FallbackBaseDamage = 30;

    private bool cannonboltActive;
    private int groundSwipeVariant;
    private float visibleSpeedRatio;
    private float visibleImpactCharge;
    private int visibleBounceCount;
    private int grazeFlashTime;

    public bool IsRolled => FindRollProjectile() != null;
    public float RollSpeedRatio => visibleSpeedRatio;
    public float ImpactChargeRatio => visibleImpactCharge;
    public int BounceCount => visibleBounceCount;
    public bool GrazeFlashActive => grazeFlashTime > 0;

    public bool RicochetActive => cannonboltActive && Player.GetModPlayer<OmnitrixPlayer>().IsPrimaryAbilityActive;
    public bool GyroShellActive => cannonboltActive && Player.GetModPlayer<OmnitrixPlayer>().IsTertiaryAbilityActive;

    public bool SiegeActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return cannonboltActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public int SiegeTicksRemaining {
        get {
            if (!SiegeActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>().GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public float SiegeProgress {
        get {
            if (!SiegeActive)
                return 0f;

            return MathHelper.Clamp(SiegeTicksRemaining / (float)CannonboltTransformation.SiegeRollDurationTicks, 0f, 1f);
        }
    }

    public int GyroTicksRemaining {
        get {
            if (!GyroShellActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>().GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.TertiaryAbility);
        }
    }

    public float GyroProgress {
        get {
            if (!GyroShellActive)
                return 0f;

            return MathHelper.Clamp(GyroTicksRemaining / (float)CannonboltTransformation.GyroShellDurationTicks, 0f, 1f);
        }
    }

    public string RollStateLabel {
        get {
            CannonboltRollProjectile roll = FindRollProjectile();
            if (roll == null)
                return "Unrolled";

            if (SiegeActive)
                return "Siege";

            if (roll.IsVaultingVisible)
                return "Vault";

            if (RicochetActive)
                return "Ricochet";

            return "Rolling";
        }
    }

    public override void ResetEffects() {
        cannonboltActive = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == TransformationId;
    }

    public override void PostUpdate() {
        if (grazeFlashTime > 0)
            grazeFlashTime--;

        CannonboltRollProjectile roll = FindRollProjectile();
        if (roll != null) {
            visibleSpeedRatio = roll.VisibleSpeedRatio;
            visibleImpactCharge = roll.VisibleImpactChargeRatio;
            visibleBounceCount = roll.VisibleBounceCount;
        }
        else if (!cannonboltActive) {
            ClearRollTelemetry();
            groundSwipeVariant = 0;
            return;
        }
        else {
            visibleSpeedRatio = Math.Max(0f, visibleSpeedRatio - 0.08f);
            visibleImpactCharge = Math.Max(0f, visibleImpactCharge - 0.12f);
            if (visibleImpactCharge <= 0.01f)
                visibleBounceCount = 0;
        }

        if (!cannonboltActive)
            return;

        if (Player.whoAmI != Main.myPlayer)
            return;

        if (SiegeActive && roll == null && !Player.dead && !Player.noItems && !Player.CCed && !Player.mount.Active) {
            TryEnterRoll(Player.GetSource_FromThis(), ResolveRollEntryDirection(), ResolveRollBaseDamage(true), 7.5f);
            roll = FindRollProjectile();
        }

        if (roll == null)
            return;

        if (Player.controlJump && Player.releaseJump) {
            TryActivateVaultLaunch();
            Player.controlJump = false;
        }
        else {
            Player.controlJump = false;
        }
    }

    public override bool CanBeHitByProjectile(Projectile proj) {
        if (!ShouldGrazeProjectile(proj, out float speedRatio))
            return true;

        int grazeThreshold = (int)Math.Round(MathHelper.Lerp(25f, SiegeActive ? 68f : 52f, speedRatio));
        int grazeHash = Math.Abs(proj.identity * 31 + proj.type * 17 + Player.whoAmI * 13) % 100;
        if (grazeHash >= grazeThreshold)
            return true;

        DeflectProjectile(proj);
        grazeFlashTime = Math.Max(grazeFlashTime, 10);
        return false;
    }

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
        if (!ShouldGrazeProjectile(proj, out float speedRatio))
            return;

        float reduction = MathHelper.Lerp(0.14f, SiegeActive ? 0.48f : 0.36f, speedRatio);
        modifiers.FinalDamage *= 1f - reduction;
        modifiers.Knockback *= 0.45f;
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
        if (ShouldGrazeProjectile(proj, out _))
            grazeFlashTime = Math.Max(grazeFlashTime, 6);
    }

    public int ConsumeGroundSwipeVariant() {
        int variant = groundSwipeVariant;
        groundSwipeVariant = (groundSwipeVariant + 1) % 2;
        return variant;
    }

    public bool TryToggleRoll(IEntitySource source, Vector2 direction, int damage, float knockback) {
        CannonboltRollProjectile roll = FindRollProjectile();
        if (roll != null) {
            if (SiegeActive)
                return false;

            roll.RequestEndRoll();
            return true;
        }

        return TryEnterRoll(source, direction, damage, knockback);
    }

    public bool TryEnterRoll(IEntitySource source, Vector2 direction, int damage, float knockback) {
        if (Player.whoAmI != Main.myPlayer ||
            !cannonboltActive ||
            Player.dead ||
            Player.CCed ||
            Player.noItems ||
            Player.mount.Active) {
            return false;
        }

        if (FindRollProjectile() != null)
            return true;

        Vector2 rollDirection = direction;
        if (Math.Abs(rollDirection.X) <= 0.05f)
            rollDirection = ResolveRollEntryDirection();

        rollDirection.Y = 0f;
        rollDirection = rollDirection.SafeNormalize(new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f));

        int projectileIndex = Projectile.NewProjectile(source, Player.Center, rollDirection,
            ModContent.ProjectileType<CannonboltRollProjectile>(), damage, knockback, Player.whoAmI);
        if (projectileIndex < 0 || projectileIndex >= Main.maxProjectiles)
            return false;

        Projectile projectile = Main.projectile[projectileIndex];
        projectile.originalDamage = damage;
        projectile.netUpdate = true;
        return true;
    }

    public bool TryActivateVaultLaunch() {
        if (Player.whoAmI != Main.myPlayer ||
            !cannonboltActive ||
            Player.dead ||
            Player.CCed ||
            Player.mount.Active ||
            Player.HasBuff<SecondaryAbilityCooldown>()) {
            return false;
        }

        CannonboltRollProjectile roll = FindRollProjectile();
        if (roll == null || !roll.TryTriggerVaultLaunch())
            return false;

        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        int cooldown = omp.CurrentTransformation?.GetSecondaryAbilityCooldown(omp) ?? CannonboltTransformation.VaultSlamCooldownTicks;
        if (cooldown > 0)
            Player.AddBuff(ModContent.BuffType<SecondaryAbilityCooldown>(), cooldown);

        return true;
    }

    public void ClearRollTelemetry() {
        visibleSpeedRatio = 0f;
        visibleImpactCharge = 0f;
        visibleBounceCount = 0;
    }

    private bool ShouldGrazeProjectile(Projectile proj, out float speedRatio) {
        speedRatio = visibleSpeedRatio;
        return cannonboltActive &&
               IsRolled &&
               GyroShellActive &&
               proj != null &&
               proj.active &&
               proj.hostile &&
               proj.damage > 0 &&
               speedRatio >= 0.55f;
    }

    private void DeflectProjectile(Projectile proj) {
        Vector2 away = (proj.Center - Player.Center).SafeNormalize(new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f));
        int spinDirection = ((proj.identity + proj.type + Player.whoAmI) & 1) == 0 ? 1 : -1;
        Vector2 tangent = away.RotatedBy(spinDirection * MathHelper.PiOver2);
        float speed = Math.Max(6f, proj.velocity.Length());

        proj.velocity = tangent * speed * 0.86f;
        proj.position += tangent * 18f;
        proj.netUpdate = true;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Vector2 dustVelocity = tangent.RotatedByRandom(0.42f) * Main.rand.NextFloat(1.8f, 4.8f);
            Dust dust = Dust.NewDustPerfect(Player.Center + away * 18f, i % 3 == 0 ? Terraria.ID.DustID.GemTopaz : Terraria.ID.DustID.Smoke,
                dustVelocity, 110, i % 3 == 0 ? new Color(255, 214, 135) : new Color(200, 190, 170), Main.rand.NextFloat(0.9f, 1.25f));
            dust.noGravity = true;
        }
    }

    private CannonboltRollProjectile FindRollProjectile() {
        int projectileType = ModContent.ProjectileType<CannonboltRollProjectile>();

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active || projectile.owner != Player.whoAmI || projectile.type != projectileType)
                continue;

            return projectile.ModProjectile as CannonboltRollProjectile;
        }

        return null;
    }

    private Vector2 ResolveRollEntryDirection() {
        int inputX = (Player.controlRight ? 1 : 0) - (Player.controlLeft ? 1 : 0);
        if (inputX != 0)
            return new Vector2(inputX, 0f);

        if (Math.Abs(Player.velocity.X) > 0.15f)
            return new Vector2(MathF.Sign(Player.velocity.X), 0f);

        return new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f);
    }

    private int ResolveRollBaseDamage(bool siege) {
        float baseDamage = ResolveHeroBaseDamage();
        float damageMultiplier = CannonboltTransformation.RollContactDamageMultiplier * (siege ? 1.18f : 1f);
        return Math.Max(1, (int)Math.Round(Player.GetDamage<HeroDamage>().ApplyTo(baseDamage * damageMultiplier)));
    }

    private int ResolveHeroBaseDamage() {
        Item heldItem = Player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }
}
