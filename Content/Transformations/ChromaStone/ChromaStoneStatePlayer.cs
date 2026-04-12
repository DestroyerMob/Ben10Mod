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
    public const int AbsorptionGuardCooldownTicks = 9 * 60;
    public const int PrismaticLanceCooldownTicks = 8 * 60;
    public const int FullSpectrumDischargeDurationTicks = 8 * 60;
    public const int FullSpectrumDischargeCooldownTicks = 48 * 60;
    public const int GuardMaxHoldTicks = 90;
    public const int MaxFacets = 3;

    private const float FacetProgressThreshold = 100f;
    private const int PrismBoltInterval = 4;
    private const int BeamFacetConsumeIntervalTicks = 24;
    private const int UltimateAbsorbExtensionCap = FullSpectrumDischargeDurationTicks + 120;
    private const int FallbackBaseDamage = 28;

    private int storedFacets;
    private int volleyShotCounter;
    private int guardGraceTime;
    private int dischargeFacetPower;
    private float guardHoldRatio;
    private float guardStoredEnergy;
    private bool chromastoneActive;

    public float PrismCharge => Player.GetModPlayer<AlienIdentityPlayer>().ChromaStonePrismCharge;
    public float PrismChargeRatio => Player.GetModPlayer<AlienIdentityPlayer>().ChromaStonePrismChargeRatio;
    public int StoredFacets => Math.Clamp(storedFacets, 0, MaxFacets);
    public float NextFacetProgressRatio => MathHelper.Clamp(PrismChargeRatio, 0f, 1f);
    public float FacetPowerRatio => MathHelper.Clamp((StoredFacets + NextFacetProgressRatio) / MaxFacets, 0f, 1f);
    public bool Guarding => guardGraceTime > 0;
    public float GuardHoldRatio => MathHelper.Clamp(guardHoldRatio, 0f, 1f);
    public float GuardStoredEnergy => guardStoredEnergy;
    public float GuardStoredRatio => MathHelper.Clamp(guardStoredEnergy / 90f, 0f, 1f);
    public bool DischargeActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return chromastoneActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public bool OverloadActive => DischargeActive;

    public int DischargeTicksRemaining {
        get {
            if (!DischargeActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>().GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public int OverloadTicksRemaining => DischargeTicksRemaining;

    public float DischargeProgress {
        get {
            if (!DischargeActive)
                return 0f;

            return MathHelper.Clamp(DischargeTicksRemaining / (float)FullSpectrumDischargeDurationTicks, 0f, 1f);
        }
    }

    public float OverloadProgress => DischargeProgress;
    public int VisibleFacetCount => DischargeActive ? 0 : StoredFacets;
    public int ActiveDischargeFacetPower => Math.Clamp(dischargeFacetPower, 0, MaxFacets);
    public bool HasAnyFacets => StoredFacets > 0;
    public bool HasFullCharge => StoredFacets >= MaxFacets;

    public override void ResetEffects() {
        chromastoneActive = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == TransformationId;
    }

    public override void PostUpdate() {
        if (guardGraceTime > 0)
            guardGraceTime--;
        else {
            guardHoldRatio = 0f;
            guardStoredEnergy = 0f;
        }

        if (!chromastoneActive) {
            if (storedFacets > 0 || PrismCharge > 0f || dischargeFacetPower > 0)
                ClearStoredFacets(clearPartialCharge: true);
            volleyShotCounter = 0;
            dischargeFacetPower = 0;
            return;
        }

        if (!DischargeActive)
            dischargeFacetPower = 0;

        EnsureFacetProjectiles();
    }

    public override bool CanBeHitByProjectile(Projectile proj) {
        if (!ShouldHandleHostileProjectile(proj))
            return true;

        if (DischargeActive && TryAbsorbDischargeProjectile(proj))
            return false;

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

        if (Guarding) {
            modifiers.FinalDamage *= IsGuardAbsorbableProjectile(proj) ? 0.5f : 0.7f;
            modifiers.Knockback *= 0.2f;
            return;
        }

        if (DischargeActive) {
            modifiers.FinalDamage *= 0.78f;
            modifiers.Knockback *= 0.5f;
        }
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
        if (!ShouldHandleHostileProjectile(proj))
            return;

        if (Guarding && !IsGuardAbsorbableProjectile(proj)) {
            float absorbedStrength = 8f + Math.Min(26f, hurtInfo.Damage * 0.42f);
            float refund = 2f + Math.Min(6f, hurtInfo.Damage * 0.08f);
            AddFacetProgress(absorbedStrength);
            guardStoredEnergy = MathHelper.Clamp(guardStoredEnergy + absorbedStrength * 1.4f, 0f, 100f);
            Player.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(refund);
            return;
        }

        if (DischargeActive && IsWeakProjectile(proj))
            RegisterDischargeAbsorption(proj.Center, ResolveAimDirection(), proj.damage);
    }

    public void ApplyHoverControl() {
        if (!chromastoneActive || Player.dead || Player.mount.Active || Guarding)
            return;

        bool grounded = AlienIdentityPlayer.IsGrounded(Player);
        if (grounded)
            return;

        if (Player.controlJump) {
            if (Player.velocity.Y > 1.6f)
                Player.velocity.Y = 1.6f;
            else if (Player.velocity.Y > 0f)
                Player.velocity.Y *= 0.92f;
            else
                Player.velocity.Y *= 0.985f;
        }

        if (!DischargeActive)
            return;

        Player.noKnockback = true;
        if (Player.velocity.Y > 0.4f)
            Player.velocity.Y = 0.4f;
        else
            Player.velocity.Y = Math.Max(Player.velocity.Y - 0.05f, -1.2f);
    }

    public bool TryStartAbsorptionGuard() {
        if (!CanStartAbsorptionGuard())
            return false;

        if (FindOwnedProjectile(ModContent.ProjectileType<ChromaStoneGuardProjectile>()) != -1)
            return true;

        guardStoredEnergy = 0f;
        guardHoldRatio = 0f;
        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
            ModContent.ProjectileType<ChromaStoneGuardProjectile>(), 0, 0f, Player.whoAmI);

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.32f, Volume = 0.6f }, Player.Center);

        return true;
    }

    public void RegisterGuardFrame(float holdRatio) {
        guardGraceTime = 2;
        guardHoldRatio = MathHelper.Clamp(holdRatio, 0f, 1f);
    }

    public void ReleaseGuardBurst(Vector2 direction) {
        guardGraceTime = 0;
        guardHoldRatio = 0f;

        if (Player.HasBuff<PrimaryAbilityCooldown>())
            return;

        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), AbsorptionGuardCooldownTicks);
        if (Player.whoAmI != Main.myPlayer)
            return;

        direction = direction.SafeNormalize(new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f));
        int shardCount = Math.Max(3, 4 + (int)Math.Round(guardStoredEnergy / 26f));
        int burstDamage = ResolveHeroDamage(0.48f + MathHelper.Clamp(guardStoredEnergy / 90f, 0f, 0.44f));
        float burstRatio = MathHelper.Clamp(guardStoredEnergy / 90f, 0f, 1f);

        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + direction * 54f, Vector2.Zero,
            ModContent.ProjectileType<ChromaStoneRadianceBurstProjectile>(), burstDamage, 3.2f, Player.whoAmI,
            burstRatio, 0f);

        for (int i = 0; i < shardCount; i++) {
            float progress = shardCount <= 1 ? 0.5f : i / (float)(shardCount - 1);
            float angleOffset = MathHelper.Lerp(-0.34f, 0.34f, progress);
            Vector2 velocity = direction.RotatedBy(angleOffset) * Main.rand.NextFloat(11.5f, 15.5f);
            int shardDamage = Math.Max(1, (int)Math.Round(burstDamage * 0.5f));
            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + direction * 20f, velocity,
                ModContent.ProjectileType<ChromaStoneProjectile>(), shardDamage, 2.5f, Player.whoAmI,
                ChromaStoneProjectile.ModeBurstShard, FacetPowerRatio);
        }

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.42f, Volume = 0.7f }, Player.Center);
            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustPerfect(Player.Center + direction * 24f + Main.rand.NextVector2Circular(14f, 14f),
                    DustID.WhiteTorch, direction.RotatedByRandom(0.85f) * Main.rand.NextFloat(1.6f, 5.6f), 95,
                    ChromaStonePrismHelper.GetSpectrumColor(i * 0.22f), Main.rand.NextFloat(0.95f, 1.3f));
                dust.noGravity = true;
            }
        }

        guardStoredEnergy = 0f;
    }

    public bool ConsumeFacetForBeam() {
        if (StoredFacets <= 0)
            return false;

        storedFacets--;
        if (!Main.dedServ) {
            for (int i = 0; i < 8; i++) {
                Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(18f, 18f), DustID.GemDiamond,
                    Main.rand.NextVector2Circular(2.2f, 2.2f), 95,
                    ChromaStonePrismHelper.GetSpectrumColor(i * 0.35f + storedFacets), Main.rand.NextFloat(0.85f, 1.1f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    public bool TryConsumeFacetForBeam(ref int timer) {
        if (StoredFacets <= 0)
            return false;

        timer++;
        if (timer < BeamFacetConsumeIntervalTicks)
            return false;

        timer = 0;
        return ConsumeFacetForBeam();
    }

    public int ConsumeAllFacets(bool clearPartialCharge = false) {
        int consumed = StoredFacets;
        storedFacets = 0;

        if (clearPartialCharge)
            Player.GetModPlayer<AlienIdentityPlayer>().SetChromaStonePrismCharge(0f);

        return consumed;
    }

    public int ConsumeAllFacetsForDischarge() {
        int consumed = ConsumeAllFacets(clearPartialCharge: false);
        dischargeFacetPower = consumed;
        return consumed;
    }

    public void StartFullSpectrumDischarge(int consumedFacets) {
        dischargeFacetPower = Math.Clamp(consumedFacets, 0, MaxFacets);
    }

    public void ExtendDischargeDuration(int ticks) {
        if (!DischargeActive || ticks <= 0)
            return;

        int ultimateBuffType = ModContent.BuffType<UltimateAbility>();
        for (int i = 0; i < Player.buffType.Length; i++) {
            if (Player.buffType[i] != ultimateBuffType)
                continue;

            Player.buffTime[i] = Math.Min(UltimateAbsorbExtensionCap, Player.buffTime[i] + ticks);
            break;
        }
    }

    public void RegisterDischargeAbsorption(Vector2 origin, Vector2 direction, int projectileDamage) {
        if (!DischargeActive)
            return;

        ExtendDischargeDuration(4 + Math.Min(12, projectileDamage / 8));
        Player.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(1f + Math.Min(4f, projectileDamage * 0.03f));

        if (Player.whoAmI != Main.myPlayer)
            return;

        int burstDamage = ResolveHeroDamage(0.2f + ActiveDischargeFacetPower * 0.05f);
        int burstCount = 2 + Math.Max(0, ActiveDischargeFacetPower - 1);
        direction = direction.SafeNormalize(new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f));

        for (int i = 0; i < burstCount; i++) {
            float angleOffset = burstCount <= 1 ? 0f : MathHelper.Lerp(-0.3f, 0.3f, i / (float)(burstCount - 1));
            Vector2 velocity = direction.RotatedBy(angleOffset) * Main.rand.NextFloat(12f, 16f);
            Projectile.NewProjectile(Player.GetSource_FromThis(), origin, velocity,
                ModContent.ProjectileType<ChromaStoneProjectile>(), burstDamage, 2.1f, Player.whoAmI,
                ChromaStoneProjectile.ModeBurstShard, Math.Max(FacetPowerRatio, ActiveDischargeFacetPower / (float)MaxFacets));
        }
    }

    public bool TryAdvanceVolleyToPrismBolt() {
        volleyShotCounter++;
        if (volleyShotCounter < PrismBoltInterval)
            return false;

        volleyShotCounter = 0;
        return true;
    }

    public void AddPrimaryAttackCharge(float amount) {
        AddFacetProgress(amount);
    }

    public bool AddFacetProgress(float amount) {
        if (!chromastoneActive || amount <= 0f)
            return false;

        if (StoredFacets >= MaxFacets) {
            Player.GetModPlayer<AlienIdentityPlayer>().SetChromaStonePrismCharge(0f);
            return false;
        }

        AlienIdentityPlayer identity = Player.GetModPlayer<AlienIdentityPlayer>();
        float progress = identity.ChromaStonePrismCharge + amount;
        bool gainedFacet = false;

        while (progress >= FacetProgressThreshold && storedFacets < MaxFacets) {
            progress -= FacetProgressThreshold;
            storedFacets++;
            gainedFacet = true;
        }

        if (storedFacets >= MaxFacets)
            progress = 0f;

        identity.SetChromaStonePrismCharge(progress);

        if (gainedFacet && !Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.18f, Volume = 0.56f }, Player.Center);
            for (int i = 0; i < 12; i++) {
                Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(18f, 24f), DustID.GemDiamond,
                    Main.rand.NextVector2Circular(2.8f, 2.8f), 95,
                    ChromaStonePrismHelper.GetSpectrumColor(i * 0.24f + storedFacets), Main.rand.NextFloat(0.9f, 1.2f));
                dust.noGravity = true;
            }
        }

        return gainedFacet;
    }

    public bool IsFacetVisible(int slotIndex) {
        return slotIndex >= 0 && slotIndex < StoredFacets && !DischargeActive;
    }

    public Vector2 GetFacetWorldOffset(int slotIndex) {
        float orbitSpeed = DischargeActive ? 3.6f : 2.3f;
        float angle = Main.GlobalTimeWrappedHourly * orbitSpeed + slotIndex * MathHelper.TwoPi / MaxFacets;
        float radius = 30f + slotIndex * 4f;
        return angle.ToRotationVector2() * radius + new Vector2(0f, -8f);
    }

    public int ResolveHeroDamage(float multiplier) {
        float scaledDamage = ResolveBaseHeroDamage() * multiplier;
        return Math.Max(1, (int)Math.Round(Player.GetDamage<HeroDamage>().ApplyTo(scaledDamage)));
    }

    public void ResetTransientState(bool clearStoredFacets = true) {
        guardGraceTime = 0;
        guardHoldRatio = 0f;
        guardStoredEnergy = 0f;
        volleyShotCounter = 0;
        dischargeFacetPower = 0;

        if (clearStoredFacets)
            ClearStoredFacets(clearPartialCharge: true);
    }

    private void ClearStoredFacets(bool clearPartialCharge) {
        storedFacets = 0;
        if (clearPartialCharge)
            Player.GetModPlayer<AlienIdentityPlayer>().SetChromaStonePrismCharge(0f);
    }

    private bool TryAbsorbGuardProjectile(Projectile proj) {
        if (!IsGuardAbsorbableProjectile(proj))
            return false;

        float facetGain = 34f + Math.Min(38f, proj.damage * 0.6f);
        float refund = 3f + Math.Min(5f, proj.damage * 0.08f);
        AddFacetProgress(facetGain);
        guardStoredEnergy = MathHelper.Clamp(guardStoredEnergy + 16f + proj.damage * 0.3f, 0f, 100f);
        Player.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(refund);

        AbsorbProjectile(proj);
        return true;
    }

    private bool TryAbsorbDischargeProjectile(Projectile proj) {
        if (!IsWeakProjectile(proj))
            return false;

        RegisterDischargeAbsorption(proj.Center, ResolveAimDirection(), proj.damage);
        AbsorbProjectile(proj);
        return true;
    }

    private bool TryBlockFacetProjectile(Projectile proj) {
        if (!IsFacetInterceptableProjectile(proj) || StoredFacets <= 0 || DischargeActive)
            return false;

        storedFacets--;
        AbsorbProjectile(proj);

        if (!Main.dedServ) {
            for (int i = 0; i < 8; i++) {
                Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.GemDiamond,
                    Main.rand.NextVector2Circular(2.2f, 2.2f), 95,
                    ChromaStonePrismHelper.GetSpectrumColor(i * 0.28f), Main.rand.NextFloat(0.85f, 1.12f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    private void AbsorbProjectile(Projectile proj) {
        Vector2 center = proj.Center;
        if (proj.active)
            proj.Kill();

        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(8f, 8f), DustID.GemDiamond,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 95,
                ChromaStonePrismHelper.GetSpectrumColor(i * 0.35f), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }

    private bool CanStartAbsorptionGuard() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        return chromastoneActive &&
               Player.whoAmI == Main.myPlayer &&
               !Player.dead &&
               !Player.CCed &&
               !Player.noItems &&
               !Player.mount.Active &&
               !Guarding &&
               !DischargeActive &&
               !Player.HasBuff<PrimaryAbilityCooldown>() &&
               !omp.HasLoadedAbilityAttack;
    }

    private int ResolveBaseHeroDamage() {
        Item heldItem = Player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
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
        return proj.damage <= 65 &&
               proj.width <= 42 &&
               proj.height <= 42;
    }

    private static bool IsGuardAbsorbableProjectile(Projectile proj) {
        return proj.damage <= 95 &&
               proj.width <= 54 &&
               proj.height <= 54;
    }

    private static bool IsFacetInterceptableProjectile(Projectile proj) {
        return proj.damage <= 40 &&
               proj.width <= 28 &&
               proj.height <= 28;
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
