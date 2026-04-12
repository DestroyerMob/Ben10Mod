using System;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.ChromaStone;

public class ChromaStoneStatePlayer : ModPlayer {
    public const string TransformationId = AlienIdentityPlayer.ChromaStoneTransformationId;
    public const int FacetDashCooldownTicks = 5 * 60;
    public const int RefractionGuardCooldownTicks = 10 * 60;
    public const int FullSpectrumOverloadDurationTicks = 8 * 60;
    public const int FullSpectrumOverloadCooldownTicks = 45 * 60;
    public const int GuardMaxHoldTicks = 90;
    public const int MaxFacets = 3;

    private const float FacetThresholdOne = 33f;
    private const float FacetThresholdTwo = 66f;
    private const float FacetThresholdThree = 99.9f;
    private const int FacetRestoreDelayTicks = 90;
    private const int DashDurationTicks = 12;
    private const float DashSpeed = 18.5f;
    private const int FallbackBaseDamage = 28;

    private readonly int[] facetCooldowns = new int[MaxFacets];

    private bool chromastoneActive;
    private int dashTime;
    private Vector2 dashVelocity;
    private int guardGraceTime;
    private float guardHoldRatio;
    private float guardStoredEnergy;

    public float PrismCharge => Player.GetModPlayer<AlienIdentityPlayer>().ChromaStonePrismCharge;
    public float PrismChargeRatio => Player.GetModPlayer<AlienIdentityPlayer>().ChromaStonePrismChargeRatio;
    public bool HasFullCharge => PrismCharge >= FacetThresholdThree;
    public bool DashActive => dashTime > 0;
    public bool Guarding => guardGraceTime > 0;
    public float GuardHoldRatio => MathHelper.Clamp(guardHoldRatio, 0f, 1f);
    public float GuardStoredEnergy => guardStoredEnergy;
    public float GuardStoredRatio => MathHelper.Clamp(guardStoredEnergy / 80f, 0f, 1f);

    public bool OverloadActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return chromastoneActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public int OverloadTicksRemaining {
        get {
            if (!OverloadActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>().GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public float OverloadProgress {
        get {
            if (!OverloadActive)
                return 0f;

            return MathHelper.Clamp(OverloadTicksRemaining / (float)FullSpectrumOverloadDurationTicks, 0f, 1f);
        }
    }

    public int VisibleFacetCount {
        get {
            if (OverloadActive)
                return MaxFacets;

            int count = 0;
            for (int i = 0; i < MaxFacets; i++) {
                if (IsFacetVisible(i))
                    count++;
            }

            return count;
        }
    }

    public override void ResetEffects() {
        chromastoneActive = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == TransformationId;
    }

    public override void PostUpdate() {
        for (int i = 0; i < MaxFacets; i++) {
            if (facetCooldowns[i] > 0)
                facetCooldowns[i]--;
        }

        if (dashTime > 0)
            dashTime--;

        if (guardGraceTime > 0)
            guardGraceTime--;
        else {
            guardHoldRatio = 0f;
            guardStoredEnergy = 0f;
        }

        if (!chromastoneActive) {
            dashVelocity = Vector2.Zero;
            return;
        }

        EnsureFacetProjectiles();
    }

    public override bool CanBeHitByProjectile(Projectile proj) {
        if (!ShouldHandleHostileProjectile(proj))
            return true;

        if (Guarding && TryAbsorbGuardProjectile(proj))
            return false;

        if (TryBlockFacetProjectile(proj))
            return false;

        return true;
    }

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
        if (!ShouldHandleHostileProjectile(proj))
            return;

        modifiers.FinalDamage *= 0.8f;
        modifiers.Knockback *= 0.85f;

        if (!Guarding)
            return;

        modifiers.FinalDamage *= OverloadActive ? 0.45f : 0.6f;
        modifiers.Knockback *= 0.2f;
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
        if (!ShouldHandleHostileProjectile(proj) || !Guarding)
            return;

        float chargeGain = Math.Min(20f, 4f + hurtInfo.Damage * 0.28f);
        float storedGain = Math.Min(24f, 5f + hurtInfo.Damage * 0.36f);
        AddPrismCharge(chargeGain);
        guardStoredEnergy = MathHelper.Clamp(guardStoredEnergy + storedGain, 0f, 100f);
    }

    public void UpdateDashMovement() {
        if (!DashActive)
            return;

        Player.velocity = dashVelocity;
        Player.fallStart = (int)(Player.position.Y / 16f);
        Player.noKnockback = true;
        Player.immune = true;
        Player.immuneNoBlink = true;
        Player.immuneTime = Math.Max(Player.immuneTime, 2);

        if (Math.Abs(dashVelocity.X) > 0.05f)
            Player.ChangeDir(dashVelocity.X >= 0f ? 1 : -1);
    }

    public bool TryStartFacetDash(Vector2 direction) {
        if (!CanStartFacetDash())
            return false;

        direction = direction.SafeNormalize(new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f));
        dashVelocity = direction * (OverloadActive ? DashSpeed + 2.5f : DashSpeed);
        dashTime = DashDurationTicks + (OverloadActive ? 2 : 0);
        Player.velocity = dashVelocity;
        Player.fallStart = (int)(Player.position.Y / 16f);
        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), FacetDashCooldownTicks);

        int dashDamage = ResolveHeroDamage(OverloadActive ? 1.06f : 0.92f);
        float dashKnockback = OverloadActive ? 5.6f : 4.2f;
        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
            ModContent.ProjectileType<ChromaStoneDashHitboxProjectile>(), dashDamage, dashKnockback, Player.whoAmI,
            OverloadActive ? 1f : 0f);

        if (OverloadActive) {
            SpawnFacetBurst(direction, dashDamage, 5, false, true);
        }
        else if (VisibleFacetCount > 0) {
            int shatterCount = VisibleFacetCount;
            ConsumeFacets(shatterCount, FacetRestoreDelayTicks);
            SpawnFacetBurst(direction, dashDamage, shatterCount * 2, true, false);
        }

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item68 with { Pitch = -0.18f, Volume = 0.72f }, Player.Center);
            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(18f, 18f), DustID.GemDiamond,
                    direction.RotatedByRandom(0.9f) * Main.rand.NextFloat(1.8f, 7.4f), 95,
                    ChromaStonePrismHelper.GetSpectrumColor(i * 0.26f), Main.rand.NextFloat(1f, 1.45f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    public bool TryStartRefractionGuard() {
        if (!CanStartRefractionGuard())
            return false;

        if (FindOwnedProjectile(ModContent.ProjectileType<ChromaStoneGuardProjectile>()) != -1)
            return true;

        guardStoredEnergy = 0f;
        guardHoldRatio = 0f;
        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
            ModContent.ProjectileType<ChromaStoneGuardProjectile>(), 0, 0f, Player.whoAmI);

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.32f, Volume = 0.58f }, Player.Center);

        return true;
    }

    public void RegisterGuardFrame(float holdRatio) {
        guardGraceTime = 2;
        guardHoldRatio = MathHelper.Clamp(holdRatio, 0f, 1f);
    }

    public void ReleaseGuardBurst(Vector2 direction) {
        guardGraceTime = 0;
        guardHoldRatio = 0f;

        if (Player.HasBuff<SecondaryAbilityCooldown>())
            return;

        Player.AddBuff(ModContent.BuffType<SecondaryAbilityCooldown>(), RefractionGuardCooldownTicks);
        if (Player.whoAmI != Main.myPlayer)
            return;

        direction = direction.SafeNormalize(new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f));
        int shardCount = Math.Max(3, 4 + (int)Math.Round(guardStoredEnergy / 18f) + (OverloadActive ? 2 : 0));
        int burstDamage = ResolveHeroDamage(0.55f + MathHelper.Clamp(guardStoredEnergy / 72f, 0f, 0.78f));
        float burstSpread = MathHelper.Lerp(0.48f, 0.18f, MathHelper.Clamp(guardStoredEnergy / 80f, 0f, 1f));
        float shardSpeed = OverloadActive ? 17.5f : 15.5f;

        for (int i = 0; i < shardCount; i++) {
            float progress = shardCount <= 1 ? 0.5f : i / (float)(shardCount - 1);
            float angleOffset = MathHelper.Lerp(-burstSpread, burstSpread, progress);
            Vector2 velocity = direction.RotatedBy(angleOffset) * Main.rand.NextFloat(shardSpeed - 1.6f, shardSpeed + 1.6f);
            int shardDamage = Math.Max(1, (int)Math.Round(burstDamage * (i % 2 == 0 ? 0.58f : 0.5f)));
            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + direction * 18f, velocity,
                ModContent.ProjectileType<ChromaStoneProjectile>(), shardDamage, 2.4f, Player.whoAmI,
                PrismChargeRatio, OverloadActive ? 2f : 1f);
        }

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 0.64f }, Player.Center);
            for (int i = 0; i < 20; i++) {
                Dust dust = Dust.NewDustPerfect(Player.Center + direction * 18f + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.WhiteTorch, direction.RotatedByRandom(0.8f) * Main.rand.NextFloat(1.8f, 5.8f), 95,
                    ChromaStonePrismHelper.GetSpectrumColor(i * 0.21f), Main.rand.NextFloat(0.95f, 1.3f));
                dust.noGravity = true;
            }
        }

        guardStoredEnergy = 0f;
    }

    public bool IsFacetVisible(int slotIndex) {
        if (slotIndex < 0 || slotIndex >= MaxFacets)
            return false;

        if (OverloadActive)
            return true;

        return GetFacetTierCount() > slotIndex && facetCooldowns[slotIndex] <= 0;
    }

    public Vector2 GetFacetWorldOffset(int slotIndex) {
        float angle = Main.GlobalTimeWrappedHourly * 2.2f + slotIndex * MathHelper.TwoPi / MaxFacets;
        float radius = 26f + slotIndex * 4f;
        return angle.ToRotationVector2() * radius + new Vector2(0f, -8f);
    }

    public void RestoreAllFacets() {
        for (int i = 0; i < MaxFacets; i++)
            facetCooldowns[i] = 0;
    }

    public void ResetTransientState() {
        dashTime = 0;
        dashVelocity = Vector2.Zero;
        guardGraceTime = 0;
        guardHoldRatio = 0f;
        guardStoredEnergy = 0f;
    }

    private bool TryAbsorbGuardProjectile(Projectile proj) {
        if (!IsWeakProjectile(proj))
            return false;

        float chargeGain = 7f + Math.Min(12f, proj.damage * 0.12f);
        float storedGain = 8f + Math.Min(22f, proj.damage * 0.24f);
        AddPrismCharge(chargeGain);
        guardStoredEnergy = MathHelper.Clamp(guardStoredEnergy + storedGain, 0f, 100f);

        if (OverloadActive)
            SpawnReflectedSplinters(proj);

        AbsorbProjectile(proj);
        return true;
    }

    private bool TryBlockFacetProjectile(Projectile proj) {
        if (!IsWeakProjectile(proj) || VisibleFacetCount <= 0)
            return false;

        if (!OverloadActive && ConsumeOneFacet(FacetRestoreDelayTicks) < 0)
            return false;

        AddPrismCharge(5f + Math.Min(7f, proj.damage * 0.08f));
        AbsorbProjectile(proj);
        return true;
    }

    private void SpawnReflectedSplinters(Projectile proj) {
        if (Player.whoAmI != Main.myPlayer)
            return;

        Vector2 direction = (-proj.velocity).SafeNormalize(ResolveAimDirection());
        int shardDamage = ResolveHeroDamage(0.38f + MathHelper.Clamp(proj.damage / 80f, 0f, 0.3f));
        for (int i = 0; i < 3; i++) {
            Vector2 shardVelocity = direction.RotatedBy(MathHelper.Lerp(-0.16f, 0.16f, i / 2f)) *
                Main.rand.NextFloat(14f, 18f);
            Projectile.NewProjectile(Player.GetSource_FromThis(), proj.Center, shardVelocity,
                ModContent.ProjectileType<ChromaStoneProjectile>(), shardDamage, 2f, Player.whoAmI,
                PrismChargeRatio, 1f);
        }
    }

    private void AbsorbProjectile(Projectile proj) {
        if (proj.active)
            proj.Kill();

        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.GemDiamond,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 95,
                ChromaStonePrismHelper.GetSpectrumColor(i * 0.35f), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }

    private bool CanStartFacetDash() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        return chromastoneActive &&
               Player.whoAmI == Main.myPlayer &&
               !Player.dead &&
               !Player.CCed &&
               !Player.noItems &&
               !Player.mount.Active &&
               !Guarding &&
               !DashActive &&
               !Player.HasBuff<PrimaryAbilityCooldown>() &&
               !omp.HasLoadedAbilityAttack;
    }

    private bool CanStartRefractionGuard() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        return chromastoneActive &&
               Player.whoAmI == Main.myPlayer &&
               !Player.dead &&
               !Player.CCed &&
               !Player.noItems &&
               !Player.mount.Active &&
               !Guarding &&
               !DashActive &&
               !Player.HasBuff<SecondaryAbilityCooldown>() &&
               !omp.HasLoadedAbilityAttack;
    }

    private int ResolveHeroDamage(float multiplier) {
        float scaledDamage = ResolveBaseHeroDamage() * multiplier;
        return Math.Max(1, (int)Math.Round(Player.GetDamage<HeroDamage>().ApplyTo(scaledDamage)));
    }

    private int ResolveBaseHeroDamage() {
        Item heldItem = Player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }

    private void AddPrismCharge(float amount) {
        if (!chromastoneActive || amount <= 0f || OverloadActive)
            return;

        Player.GetModPlayer<AlienIdentityPlayer>().AddChromaStonePrismCharge(amount);
    }

    private int GetFacetTierCount() {
        if (PrismCharge >= FacetThresholdThree)
            return 3;
        if (PrismCharge >= FacetThresholdTwo)
            return 2;
        if (PrismCharge >= FacetThresholdOne)
            return 1;
        return 0;
    }

    private int ConsumeOneFacet(int cooldown) {
        if (OverloadActive)
            return 0;

        for (int i = MaxFacets - 1; i >= 0; i--) {
            if (!IsFacetVisible(i))
                continue;

            facetCooldowns[i] = Math.Max(facetCooldowns[i], cooldown);
            return i;
        }

        return -1;
    }

    private void ConsumeFacets(int count, int cooldown) {
        for (int i = 0; i < count; i++) {
            if (ConsumeOneFacet(cooldown) < 0)
                break;
        }
    }

    private void SpawnFacetBurst(Vector2 direction, int baseDamage, int shardCount, bool focusedFan, bool overloadBurst) {
        if (Player.whoAmI != Main.myPlayer || shardCount <= 0)
            return;

        float spread = focusedFan ? 0.74f : 1.18f;
        float speed = overloadBurst ? 17.5f : 15.2f;
        int shardDamage = Math.Max(1, (int)Math.Round(baseDamage * (overloadBurst ? 0.46f : 0.34f)));
        for (int i = 0; i < shardCount; i++) {
            float progress = shardCount <= 1 ? 0.5f : i / (float)(shardCount - 1);
            float angleOffset = MathHelper.Lerp(-spread, spread, progress);
            Vector2 shardDirection = focusedFan
                ? direction.RotatedBy(angleOffset)
                : (direction.RotatedBy(angleOffset) + Main.rand.NextVector2Circular(0.18f, 0.18f)).SafeNormalize(direction);
            Vector2 velocity = shardDirection * Main.rand.NextFloat(speed - 1.5f, speed + 1.5f);
            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + shardDirection * 14f, velocity,
                ModContent.ProjectileType<ChromaStoneProjectile>(), shardDamage, 2.2f, Player.whoAmI,
                PrismChargeRatio, overloadBurst ? 2f : 1f);
        }
    }

    private void EnsureFacetProjectiles() {
        if (Main.dedServ || Player.whoAmI != Main.myPlayer)
            return;

        int projectileType = ModContent.ProjectileType<ChromaStoneFacetProjectile>();
        for (int slot = 0; slot < MaxFacets; slot++) {
            if (FindOwnedProjectile(projectileType, slot) != -1)
                continue;

            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, projectileType, 0, 0f,
                Player.whoAmI, slot);
        }
    }

    private int FindOwnedProjectile(int projectileType, int slot = -1) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active || projectile.owner != Player.whoAmI || projectile.type != projectileType)
                continue;

            if (slot >= 0 && (int)Math.Round(projectile.ai[0]) != slot)
                continue;

            return i;
        }

        return -1;
    }

    private bool ShouldHandleHostileProjectile(Projectile proj) {
        return chromastoneActive &&
               proj != null &&
               proj.active &&
               proj.hostile &&
               proj.damage > 0 &&
               !proj.friendly;
    }

    private static bool IsWeakProjectile(Projectile proj) {
        return proj.damage <= 60 &&
               proj.width <= 44 &&
               proj.height <= 44;
    }

    private Vector2 ResolveAimDirection() {
        Vector2 direction = new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f);
        if (Main.netMode == NetmodeID.SinglePlayer || Player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = Main.MouseWorld - Player.Center;
            if (mouseDirection.LengthSquared() > 0.0001f)
                direction = mouseDirection;
        }

        return direction.SafeNormalize(new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f));
    }
}
