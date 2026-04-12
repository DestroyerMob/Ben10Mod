using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.BigChill;

public class BigChillTransformation : Transformation {
    internal const int FrostbiteThreshold = 8;
    internal const int FrostbiteRefreshTicks = 4 * 60;
    internal const int DeepFreezeDurationTicks = 4 * 60;
    internal const int HoarfrostDurationTicks = 4 * 60;

    private const int FallbackBaseDamage = 24;
    private const int PhaseDriftCost = 16;
    private const int GraveMistCost = 18;
    private const int AbsoluteZeroCost = 60;
    private const int WailingWakeDurationTicks = BigChillStatePlayer.WailingWakeDurationTicks;
    private const int CryoLanceUseTime = 19;
    private const int EctoBreathUseTime = 7;
    private const int BaseDeepFreezeArmorPenetration = 4;
    private const int UltimateDeepFreezeArmorPenetration = 10;
    private const int UltimateBreathRefreshTicks = 8;
    private const int UltimateFrigidFractureDurationTicks = 4 * 60;
    private const float EctoBreathDamageMultiplier = 0.34f;
    private const float CryoLanceDamageMultiplier = 0.42f;
    private const float GraveMistDamageMultiplier = 0.52f;
    private const float PhaseDriftDamageMultiplier = 0.38f;
    private const float ShatterDamageMultiplier = 0.76f;
    private const float FinalPulseDamageMultiplier = 0.92f;
    private const float FrostShardDamageMultiplier = 0.34f;

    public override string FullID => BigChillStatePlayer.TransformationId;
    public override string TransformationName => "Bigchill";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<BigChill_Buff>();
    public override Transformation ChildTransformation => ModContent.GetInstance<UltimateBigChillTransformation>();

    public override string Description =>
        "A spectral Necrofriggian skirmisher who paints targets with Hoarfrost, phases through danger and enemies, then cashes that mark out into explosive Shiverbursts while staying airborne.";

    public override List<string> Abilities => new() {
        "Direct hits apply short Hoarfrost, slowing normal enemies, shaving defense, and priming a burst window.",
        "Coldfire Breath is the rapid marking stream you hold while circling above the fight.",
        "Black Ice Barrage pops Hoarfrost into Shiverburst, splinter damage, and a brief frost patch.",
        "Spectral Phase dashes intangible through enemies and projectiles, leaving a freezing trail behind.",
        "Wailing Wake leaves drifting frost clouds behind your movement and lightly slows hostile projectiles.",
        "Predator from the Veil boosts damage right after phasing and refunds OE when you trigger Shiverburst well.",
        "Dead Winter overclocks flight, auto-enables Wailing Wake, and lets Shiverburst spread Hoarfrost back through the arena."
    };

    public override string PrimaryAttackName => "Coldfire Breath";
    public override string SecondaryAttackName => "Black Ice Barrage";
    public override string PrimaryAbilityName => "Spectral Phase";
    public override string SecondaryAbilityName => "Wailing Wake";
    public override string UltimateAbilityName => "Dead Winter";

    public override int PrimaryAttack => ModContent.ProjectileType<BigChillFrostBreathProjectile>();
    public override int PrimaryAttackSpeed => EctoBreathUseTime;
    public override int PrimaryShootSpeed => 1;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => EctoBreathDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<BigChillProjectile>();
    public override int SecondaryAttackSpeed => CryoLanceUseTime;
    public override int SecondaryShootSpeed => 19;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => CryoLanceDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => BigChillStatePlayer.PhaseDriftEmpowerDurationTicks;
    public override int PrimaryAbilityCooldown => BigChillStatePlayer.PhaseDriftCooldownTicks;
    public override int PrimaryAbilityCost => PhaseDriftCost;

    public override bool HasSecondaryAbility => true;
    public override int SecondaryAbilityDuration => WailingWakeDurationTicks;
    public override int SecondaryAbilityCooldown => BigChillStatePlayer.GraveMistCooldownTicks;
    public override int SecondaryAbilityCost => GraveMistCost;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => BigChillStatePlayer.AbsoluteZeroDurationTicks;
    public override int UltimateAbilityCooldown => BigChillStatePlayer.AbsoluteZeroCooldownTicks;
    public override int UltimateAbilityCost => AbsoluteZeroCost;

    public override int GetPrimaryAbilityCost(OmnitrixPlayer omp) {
        return omp.Player.GetModPlayer<BigChillStatePlayer>().AbsoluteZeroActive ? 10 : PhaseDriftCost;
    }

    public override int GetPrimaryAbilityCooldown(OmnitrixPlayer omp) {
        int baseCooldown = omp.Player.GetModPlayer<BigChillStatePlayer>().AbsoluteZeroActive
            ? 8 * 60
            : BigChillStatePlayer.PhaseDriftCooldownTicks;
        return ApplyAbilityCooldownMultiplier(baseCooldown, omp.primaryAbilityCooldownMultiplier);
    }

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<BigChillFrostBreathProjectile>(),
            ModContent.ProjectileType<BigChillProjectile>(),
            ModContent.ProjectileType<BigChillGraveMistProjectile>(),
            ModContent.ProjectileType<BigChillFrostShardProjectile>(),
            ModContent.ProjectileType<BigChillAbsoluteZeroPulseProjectile>(),
            ModContent.ProjectileType<BigChillPhaseStrikeProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        BigChillStatePlayer state = player.GetModPlayer<BigChillStatePlayer>();
        player.GetDamage<HeroDamage>() += 0.08f;
        player.moveSpeed += 0.12f;
        player.runAcceleration += 0.08f;
        player.maxRunSpeed += 0.85f;
        player.noFallDmg = true;
        player.ignoreWater = true;
        player.aggro -= state.PhaseDriftEmpowered ? 560 : 340;
        player.buffImmune[BuffID.Chilled] = true;
        player.buffImmune[BuffID.Frozen] = true;
        player.buffImmune[BuffID.Frostburn] = true;
        player.buffImmune[BuffID.Frostburn2] = true;
        player.wingTimeMax += state.AbsoluteZeroActive ? 95 : 58;
        player.wingTime = Math.Max(player.wingTime, state.AbsoluteZeroActive ? 28f : 16f);

        if (state.PhaseDriftEmpowered) {
            player.GetDamage<HeroDamage>() += 0.05f;
            player.moveSpeed += 0.12f;
            player.maxRunSpeed += 0.9f;
            player.runAcceleration += 0.12f;
        }

        if (state.HungerBoostActive) {
            player.moveSpeed += 0.08f;
            player.maxRunSpeed += 0.6f;
            player.runAcceleration += 0.12f;
            player.wingTime = Math.Max(player.wingTime, 22f);
        }

        if (state.AbsoluteZeroActive) {
            player.GetDamage<HeroDamage>() += 0.05f;
            player.GetAttackSpeed<HeroDamage>() += 0.08f;
            player.moveSpeed += 0.12f;
            player.maxRunSpeed += 1.15f;
            player.runAcceleration += 0.14f;
            player.wingTimeMax += 90;
            player.wingTime = Math.Max(player.wingTime, 32f);
            player.armorEffectDrawShadow = true;
            Lighting.AddLight(player.Center, new Vector3(0.28f, 0.5f, 0.78f));
        }

        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<BigChillWings>());
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        BigChillStatePlayer state = omp.Player.GetModPlayer<BigChillStatePlayer>();
        if (omp.setAttack == OmnitrixPlayer.AttackSelection.Primary && state.AbsoluteZeroActive) {
            item.useTime = item.useAnimation = Math.Max(5, (int)Math.Round(item.useTime * 0.78f));
        }
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        ApplyNecroflight(player, player.GetModPlayer<BigChillStatePlayer>());
    }

    public override bool? CanBeHitByNPC(Player player, OmnitrixPlayer omp, NPC npc, ref int cooldownSlot) {
        if (player.GetModPlayer<BigChillStatePlayer>().PhaseDriftIntangibleActive)
            return false;

        return base.CanBeHitByNPC(player, omp, npc, ref cooldownSlot);
    }

    public override bool? CanBeHitByProjectile(Player player, OmnitrixPlayer omp, Projectile projectile) {
        if (player.GetModPlayer<BigChillStatePlayer>().PhaseDriftIntangibleActive && projectile.hostile)
            return false;

        return base.CanBeHitByProjectile(player, omp, projectile);
    }

    public override void ModifyDrawInfo(Player player, OmnitrixPlayer omp, ref PlayerDrawSet drawInfo) {
        BigChillStatePlayer state = player.GetModPlayer<BigChillStatePlayer>();
        if (state.PhaseDriftIntangibleActive) {
            drawInfo.colorArmorHead.A /= 3;
            drawInfo.colorArmorBody.A /= 3;
            drawInfo.colorArmorLegs.A /= 3;
        }
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        BigChillStatePlayer state = player.GetModPlayer<BigChillStatePlayer>();
        TrySpawnWailingWakeCloud(player);

        if (Main.dedServ)
            return;

        int spawnRate = state.AbsoluteZeroActive ? 1 : state.PhaseDriftEmpowered ? 2 : state.HungerBoostActive ? 2 : 4;
        if (!Main.rand.NextBool(spawnRate))
            return;

        Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.46f, player.height * 0.62f);
        Dust dust = Dust.NewDustPerfect(player.Center + offset,
            Main.rand.NextBool() ? DustID.Frost : DustID.IceTorch,
            Main.rand.NextVector2Circular(0.35f, 0.35f), 100,
            state.AbsoluteZeroActive ? new Color(205, 245, 255) : new Color(180, 230, 255),
            Main.rand.NextFloat(0.84f, state.AbsoluteZeroActive ? 1.2f : 1.02f));
        dust.noGravity = true;
    }

    public override bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<PrimaryAbilityCooldown>() || player.HasBuff<PrimaryAbility>() || player.dead || player.CCed)
            return true;

        if (omp.omnitrixEnergy < GetPrimaryAbilityCost(omp)) {
            omp.ShowTransformFailureFeedback($"Need {GetPrimaryAbilityCost(omp)} OE for {PrimaryAbilityName}.");
            return true;
        }

        omp.omnitrixEnergy -= GetPrimaryAbilityCost(omp);
        omp.primaryAbilityTransformationId = omp.currentTransformationId;
        player.AddBuff(ModContent.BuffType<PrimaryAbility>(), GetPrimaryAbilityDuration(omp));
        player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), GetPrimaryAbilityCooldown(omp));
        ExecutePhaseDrift(player);
        return true;
    }

    public override bool TryActivateSecondaryAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<SecondaryAbilityCooldown>() || player.HasBuff<SecondaryAbility>() ||
            player.GetModPlayer<BigChillStatePlayer>().AbsoluteZeroActive || player.dead || player.CCed)
            return true;

        if (omp.omnitrixEnergy < GetSecondaryAbilityCost(omp)) {
            omp.ShowTransformFailureFeedback($"Need {GetSecondaryAbilityCost(omp)} OE for {SecondaryAbilityName}.");
            return true;
        }

        omp.omnitrixEnergy -= GetSecondaryAbilityCost(omp);
        omp.secondaryAbilityTransformationId = omp.currentTransformationId;
        player.AddBuff(ModContent.BuffType<SecondaryAbility>(), GetSecondaryAbilityDuration(omp));
        player.AddBuff(ModContent.BuffType<SecondaryAbilityCooldown>(), GetSecondaryAbilityCooldown(omp));

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item30 with { Pitch = -0.3f, Volume = 0.4f }, player.Center);

        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        BigChillStatePlayer state = player.GetModPlayer<BigChillStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        bool ultimateForm = state.UltimateBigChillActive;

        if (omp.altAttack) {
            int shardCount = state.PhaseDriftEmpowered ? 4 : 3;
            if (state.AbsoluteZeroActive)
                shardCount += ultimateForm ? 2 : 1;

            float spread = state.PhaseDriftEmpowered ? 0.12f : 0.2f;
            if (state.AbsoluteZeroActive)
                spread += ultimateForm ? 0.04f : 0.02f;

            float shardSpeed = SecondaryShootSpeed + (state.AbsoluteZeroActive ? 1.6f : 0f) + (state.PhaseDriftEmpowered ? 1f : 0f);
            Vector2 spawnPosition = player.MountedCenter + direction * 18f;
            for (int i = 0; i < shardCount; i++) {
                float offset = shardCount == 1 ? 0f : MathHelper.Lerp(-spread, spread, i / (float)(shardCount - 1));
                int shardDamage = ScaleDamage(damage, SecondaryAttackModifier * (state.AbsoluteZeroActive ? 1.04f : 1f));
                Projectile.NewProjectile(source, spawnPosition, direction.RotatedBy(offset) * shardSpeed, SecondaryAttack,
                    shardDamage, knockback + 0.75f, player.whoAmI, state.AbsoluteZeroActive ? 1f : 0f,
                    state.PhaseDriftEmpowered ? 1f : 0f);
            }

            return false;
        }

        int breathDamage = ScaleDamage(damage, PrimaryAttackModifier);
        Projectile.NewProjectile(source, player.Center, direction, PrimaryAttack, breathDamage, knockback, player.whoAmI,
            state.AbsoluteZeroActive ? 1f : 0f, state.PhaseDriftEmpowered ? 1f : 0f);
        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        BigChillStatePlayer state = player.GetModPlayer<BigChillStatePlayer>();
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.IsBigChillFrigidFracturedFor(player.whoAmI)) {
            if (projectile.type == PrimaryAttack)
                modifiers.SourceDamage *= state.UltimateBigChillActive ? 1.16f : 1.1f;
            else if (projectile.type == SecondaryAttack)
                modifiers.SourceDamage *= state.UltimateBigChillActive ? 1.2f : 1.12f;
        }

        if (!identity.HasBigChillFrostbiteFor(player.whoAmI) || !state.PhaseDriftEmpowered)
            return;

        if (projectile.type == PrimaryAttack)
            modifiers.SourceDamage *= state.AbsoluteZeroActive ? 1.18f : 1.12f;
        else if (projectile.type == SecondaryAttack)
            modifiers.SourceDamage *= state.AbsoluteZeroActive ? 1.26f : 1.18f;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        BigChillStatePlayer state = omp.Player.GetModPlayer<BigChillStatePlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => state.AbsoluteZeroActive
                ? compact ? "Wide Hoarfrost" : "Longer, wider breath that keeps Hoarfrost painted on targets"
                : state.PhaseDriftEmpowered
                    ? compact ? "Phase damage +" : "Marking breath with bonus post-phase pressure"
                    : compact ? "Apply Hoarfrost" : "Rapid breath that marks targets with Hoarfrost",
            OmnitrixPlayer.AttackSelection.Secondary => state.PhaseDriftEmpowered
                ? compact ? "Burst +" : "Tighter barrage with a stronger Shiverburst window"
                : compact ? "Pop Hoarfrost" : "Consume Hoarfrost into Shiverburst",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => omp.IsPrimaryAbilityActive
                ? compact
                    ? $"Phase {OmnitrixPlayer.FormatCooldownTicks(state.PhaseDriftTicksRemaining)}"
                    : $"Spectral Phase active • {OmnitrixPlayer.FormatCooldownTicks(state.PhaseDriftTicksRemaining)} left"
                : compact
                    ? $"{GetPrimaryAbilityCost(omp)} OE"
                    : $"Dash intangible, phase through pressure, and open a damage window • {GetPrimaryAbilityCost(omp)} OE",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => state.WailingWakeActive
                ? compact
                    ? $"Wake {OmnitrixPlayer.FormatCooldownTicks(state.WailingWakeTicksRemaining)}"
                    : $"Wailing Wake active • {OmnitrixPlayer.FormatCooldownTicks(state.WailingWakeTicksRemaining)} left"
                : compact
                    ? $"{GetSecondaryAbilityCost(omp)} OE"
                    : $"Leave drifting mist clouds behind your movement • {GetSecondaryAbilityCost(omp)} OE",
            OmnitrixPlayer.AttackSelection.Ultimate => state.AbsoluteZeroActive
                ? compact
                    ? $"Winter {OmnitrixPlayer.FormatCooldownTicks(state.AbsoluteZeroTicksRemaining)}"
                    : $"Dead Winter active • {OmnitrixPlayer.FormatCooldownTicks(state.AbsoluteZeroTicksRemaining)} left"
                : compact
                    ? $"{GetUltimateAbilityCost(omp)} OE"
                    : $"Flight overdrive with auto Wailing Wake and spreading Shiverbursts • {GetUltimateAbilityCost(omp)} OE",
            _ => base.GetAttackResourceSummary(selection, omp, compact)
        };
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<BigChill>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.wings = EquipLoader.GetEquipSlot(Mod, nameof(BigChillWings), EquipType.Wings);
    }

    internal static bool IsUltimateBigChill(Player player) => BigChillStatePlayer.IsUltimateBigChill(player);

    internal static void ResolveBreathHit(Projectile projectile, NPC target, int damageDone) {
        if (!TryGetActiveBigChillOwner(projectile, out Player owner))
            return;

        BigChillStatePlayer state = owner.GetModPlayer<BigChillStatePlayer>();
        int duration = state.AbsoluteZeroActive
            ? HoarfrostDurationTicks + 90
            : state.UltimateBigChillActive
                ? HoarfrostDurationTicks + 45
                : HoarfrostDurationTicks;
        ApplyHoarfrost(owner, target, duration);
    }

    internal static void ResolveCryoLanceHit(Projectile projectile, NPC target, int damageDone) {
        if (!TryGetActiveBigChillOwner(projectile, out Player owner))
            return;

        BigChillStatePlayer state = owner.GetModPlayer<BigChillStatePlayer>();
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!identity.HasBigChillFrostbiteFor(owner.whoAmI)) {
            ApplyHoarfrost(owner, target, HoarfrostDurationTicks);
            return;
        }

        bool shiverburstTriggered = TriggerShiverburst(owner, target, projectile.GetSource_FromThis(),
            Math.Max(projectile.damage, damageDone), projectile.knockBack + 0.85f, consumeHoarfrost: true,
            spreadHoarfrost: state.AbsoluteZeroActive || state.UltimateBigChillActive);
        if (!shiverburstTriggered)
            return;

        int splinterCount = state.PhaseDriftEmpowered
            ? (state.AbsoluteZeroActive ? 5 : 4)
            : (state.AbsoluteZeroActive ? 4 : 3);
        SpawnCryoSplinters(projectile, target.Center, projectile.damage, splinterCount);
        SpawnColdfirePatch(projectile, target.Center, state.AbsoluteZeroActive);
    }

    internal static void ResolveMistHit(Projectile projectile, NPC target, int damageDone, bool trailSegment) {
        if (!TryGetActiveBigChillOwner(projectile, out Player owner))
            return;

        ApplyHoarfrost(owner, target, trailSegment ? HoarfrostDurationTicks / 2 : HoarfrostDurationTicks);
    }

    internal static void ResolveShardHit(Projectile projectile, NPC target, int damageDone) {
        if (!TryGetActiveBigChillOwner(projectile, out Player owner))
            return;

        ApplyHoarfrost(owner, target, projectile.ai[0] >= 0.5f ? HoarfrostDurationTicks / 2 : 90);
    }

    internal static void ResolvePhaseDriftHit(Projectile projectile, NPC target, int damageDone) {
        if (!TryGetActiveBigChillOwner(projectile, out Player owner))
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.HasBigChillFrostbiteFor(owner.whoAmI)) {
            TriggerShiverburst(owner, target, projectile.GetSource_FromThis(), Math.Max(projectile.damage, damageDone),
                projectile.knockBack + 0.55f, consumeHoarfrost: false,
                spreadHoarfrost: owner.GetModPlayer<BigChillStatePlayer>().AbsoluteZeroActive);
            return;
        }

        ApplyHoarfrost(owner, target, HoarfrostDurationTicks);
    }

    internal static void ResolvePulseHit(Projectile projectile, NPC target, int damageDone, bool spreadFrostbite) {
        if (!TryGetActiveBigChillOwner(projectile, out Player owner))
            return;

        target.AddBuff(BuffID.Frostburn2, spreadFrostbite ? 150 : 90);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), spreadFrostbite ? (target.boss ? 20 : 45) : (target.boss ? 12 : 30));
        if (spreadFrostbite)
            ApplyHoarfrost(owner, target, HoarfrostDurationTicks / 2);
    }

    internal static void HandleAbsoluteZeroActivated(Player player) {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item30 with { Pitch = -0.36f, Volume = 0.68f }, player.Center);
        for (int i = 0; i < 24; i++) {
            Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(18f, 28f),
                i % 2 == 0 ? DustID.IceTorch : DustID.Frost, Main.rand.NextVector2Circular(3.2f, 3.2f), 100,
                new Color(205, 245, 255), Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }

    internal static void TriggerAbsoluteZeroShutdownPulse(Player player) {
        if (player.whoAmI != Main.myPlayer)
            return;

        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
            ModContent.ProjectileType<BigChillAbsoluteZeroPulseProjectile>(),
            ResolveHeroDamage(player, FinalPulseDamageMultiplier), 2.2f, player.whoAmI,
            BigChillAbsoluteZeroPulseProjectile.VariantFinalPulse, 1f);
    }

    internal static void TriggerSpectralPhasePulse(Player player, bool absoluteZero) {
        if (player.whoAmI != Main.myPlayer)
            return;

        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
            ModContent.ProjectileType<BigChillAbsoluteZeroPulseProjectile>(),
            ResolveHeroDamage(player, absoluteZero ? 0.32f : 0.24f), 1.15f, player.whoAmI,
            BigChillAbsoluteZeroPulseProjectile.VariantPhasePulse, 1f);
    }

    internal static void SpawnPhaseTrailSegment(Player owner, Vector2 center, bool absoluteZero) {
        if (owner.whoAmI != Main.myPlayer)
            return;

        Projectile.NewProjectile(owner.GetSource_FromThis(), center, new Vector2(owner.direction * 0.22f, -0.04f),
            ModContent.ProjectileType<BigChillGraveMistProjectile>(),
            ResolveHeroDamage(owner, absoluteZero ? 0.18f : 0.14f), 0.2f, owner.whoAmI,
            BigChillGraveMistProjectile.VariantTrail, absoluteZero ? 1f : 0f);
    }

    internal static void TrySpawnWailingWakeCloud(Player player) {
        if (player.whoAmI != Main.myPlayer)
            return;

        BigChillStatePlayer state = player.GetModPlayer<BigChillStatePlayer>();
        if (!state.WailingWakeActive || player.dead || player.velocity.LengthSquared() < 2.56f)
            return;

        int interval = state.AbsoluteZeroActive ? 10 : 14;
        if (Main.GameUpdateCount % interval != player.whoAmI % interval)
            return;

        Vector2 direction = player.velocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        Vector2 spawnPosition = player.Center - direction * 18f + new Vector2(0f, 6f);
        Vector2 driftVelocity = -direction * (state.AbsoluteZeroActive ? 0.42f : 0.28f) + new Vector2(0f, -0.06f);
        Projectile.NewProjectile(player.GetSource_FromThis(), spawnPosition, driftVelocity,
            ModContent.ProjectileType<BigChillGraveMistProjectile>(),
            ResolveHeroDamage(player, state.AbsoluteZeroActive ? GraveMistDamageMultiplier * 0.72f : GraveMistDamageMultiplier * 0.58f),
            0.35f, player.whoAmI, BigChillGraveMistProjectile.VariantMist, state.AbsoluteZeroActive ? 1f : 0f);
    }

    private static void SpawnColdfirePatch(Projectile projectile, Vector2 center, bool absoluteZero) {
        if (projectile.owner != Main.myPlayer)
            return;

        int projectileIndex = Projectile.NewProjectile(projectile.GetSource_FromThis(), center, Vector2.Zero,
            ModContent.ProjectileType<BigChillGraveMistProjectile>(),
            Math.Max(1, (int)Math.Round(projectile.damage * (absoluteZero ? 0.28f : 0.22f))),
            projectile.knockBack * 0.22f, projectile.owner, BigChillGraveMistProjectile.VariantTrail,
            absoluteZero ? 1f : 0f);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Main.projectile[projectileIndex].timeLeft = absoluteZero ? 96 : 78;
            Main.projectile[projectileIndex].netUpdate = true;
        }
    }

    private static void ApplyHoarfrost(Player owner, NPC target, int duration) {
        BigChillStatePlayer state = owner.GetModPlayer<BigChillStatePlayer>();
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int armorPen = state.AbsoluteZeroActive || state.UltimateBigChillActive
            ? UltimateDeepFreezeArmorPenetration
            : BaseDeepFreezeArmorPenetration;
        identity.ApplyBigChillHoarfrost(owner.whoAmI, duration, armorPen);
        target.AddBuff(BuffID.Frostburn2, Math.Max(90, duration));
        target.AddBuff(ModContent.BuffType<EnemySlow>(), target.boss ? 18 : 60);
    }

    private static bool TriggerShiverburst(Player owner, NPC target, IEntitySource source, int referenceDamage, float knockback,
        bool consumeHoarfrost, bool spreadHoarfrost) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!identity.CanTriggerBigChillShiverburst(owner.whoAmI))
            return false;

        if (consumeHoarfrost) {
            if (!identity.ConsumeBigChillHoarfrost(owner.whoAmI))
                return false;
        }
        else if (!identity.HasBigChillFrostbiteFor(owner.whoAmI)) {
            return false;
        }

        identity.TriggerBigChillShiverburstCooldown();

        BigChillStatePlayer state = owner.GetModPlayer<BigChillStatePlayer>();
        bool absoluteZero = state.AbsoluteZeroActive;
        bool ultimateForm = state.UltimateBigChillActive;
        int shatterDamage = ResolveShiverburstDamage(owner, referenceDamage, absoluteZero);
        Projectile.NewProjectile(source, target.Center, Vector2.Zero,
            ModContent.ProjectileType<BigChillAbsoluteZeroPulseProjectile>(), shatterDamage, knockback, owner.whoAmI,
            BigChillAbsoluteZeroPulseProjectile.VariantShatter, spreadHoarfrost ? 1f : 0f);

        if (state.PhaseDriftEmpowered || absoluteZero) {
            owner.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(target.boss
                ? (absoluteZero ? 1.3f : 0.9f)
                : (absoluteZero ? 2.2f : 1.5f));
            state.ApplyHungerSurge();
        }

        if (ultimateForm) {
            int shardDamage = Math.Max(1, (int)Math.Round(referenceDamage * FrostShardDamageMultiplier));
            SpawnHomingFrostShards(owner, source, target.Center, shardDamage, knockback * 0.35f,
                absoluteZero ? 3 : 2, absoluteZero);
        }

        if (target.boss && (ultimateForm || absoluteZero))
            identity.ApplyBigChillFrigidFracture(owner.whoAmI, UltimateFrigidFractureDurationTicks);

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item50 with { Pitch = -0.1f, Volume = 0.54f }, target.Center);
            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustPerfect(target.Center, i % 2 == 0 ? DustID.IceTorch : DustID.Frost,
                    Main.rand.NextVector2Circular(4.2f, 4.2f), 100, new Color(205, 245, 255),
                    Main.rand.NextFloat(1f, absoluteZero ? 1.42f : 1.18f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    private static void SpawnHomingFrostShards(Player owner, IEntitySource source, Vector2 center, int damage, float knockback,
        int count, bool absoluteZero) {
        if (owner.whoAmI != Main.myPlayer)
            return;

        for (int i = 0; i < count; i++) {
            float angle = MathHelper.TwoPi * i / Math.Max(1, count);
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(8.5f, 11.5f);
            Projectile.NewProjectile(source, center, velocity, ModContent.ProjectileType<BigChillFrostShardProjectile>(),
                damage, knockback, owner.whoAmI, absoluteZero ? 1f : 0f);
        }
    }

    private static void SpawnCryoSplinters(Projectile projectile, Vector2 center, int damage, int count) {
        if (projectile.owner != Main.myPlayer)
            return;

        for (int i = 0; i < count; i++) {
            float spread = count == 1 ? 0f : MathHelper.Lerp(-0.18f, 0.18f, i / (float)(count - 1));
            Vector2 velocity = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(spread) *
                               Main.rand.NextFloat(8.5f, 11f);
            Projectile.NewProjectile(projectile.GetSource_FromThis(), center, velocity,
                ModContent.ProjectileType<BigChillFrostShardProjectile>(),
                Math.Max(1, (int)Math.Round(damage * 0.26f)), projectile.knockBack * 0.3f, projectile.owner,
                projectile.ai[0], 0f);
        }
    }

    private static int ResolveShiverburstDamage(Player owner, int referenceDamage, bool absoluteZero) {
        bool ultimateForm = IsUltimateBigChill(owner);
        float heroMultiplier = ShatterDamageMultiplier * (absoluteZero ? 1.1f : ultimateForm ? 1.06f : 1f);
        float referenceMultiplier = absoluteZero ? (ultimateForm ? 0.9f : 0.84f) : (ultimateForm ? 0.78f : 0.72f);
        int heroDamage = ResolveHeroDamage(owner, heroMultiplier);
        return Math.Max(heroDamage, Math.Max(1, (int)Math.Round(referenceDamage * referenceMultiplier)));
    }

    private static void ExecutePhaseDrift(Player player) {
        BigChillStatePlayer state = player.GetModPlayer<BigChillStatePlayer>();
        bool ultimateForm = state.UltimateBigChillActive;
        Vector2 fallbackDirection = new Vector2(player.direction == 0 ? 1 : player.direction, 0f);
        Vector2 direction = ResolveAimDirection(player, fallbackDirection);
        float fallbackDistance = state.AbsoluteZeroActive ? 240f : ultimateForm ? 214f : 190f;
        Vector2 targetPosition = ResolveTargetPosition(player, direction, fallbackDistance);
        Vector2 offset = targetPosition - player.Center;
        if (offset == Vector2.Zero)
            offset = direction * 180f;

        float dashDistance = Math.Min(offset.Length(), fallbackDistance);
        float dashSpeed = state.AbsoluteZeroActive
            ? BigChillPhaseStrikeProjectile.DashSpeed + 3f
            : ultimateForm
                ? BigChillPhaseStrikeProjectile.DashSpeed + 1.5f
                : BigChillPhaseStrikeProjectile.DashSpeed;
        int dashFrames = Utils.Clamp((int)Math.Ceiling(dashDistance / (state.AbsoluteZeroActive
                ? BigChillPhaseStrikeProjectile.DashSpeed + 3f
                : dashSpeed)),
            BigChillPhaseStrikeProjectile.MinDashFrames, BigChillPhaseStrikeProjectile.MaxDashFrames);
        int dashDamage = ResolveHeroDamage(player, PhaseDriftDamageMultiplier *
            (state.AbsoluteZeroActive ? 1.08f : ultimateForm ? 1.04f : 1f));
        int projectileIndex = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center + direction * 18f,
            direction * dashSpeed, ModContent.ProjectileType<BigChillPhaseStrikeProjectile>(),
            dashDamage, 1.2f, player.whoAmI, state.AbsoluteZeroActive ? 1f : 0f);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Projectile projectile = Main.projectile[projectileIndex];
            projectile.timeLeft = dashFrames;
            projectile.netUpdate = true;
        }

        state.StartPhaseDrift();
        SpawnPhaseTrailSegment(player, player.Center, state.AbsoluteZeroActive);

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.32f, Volume = 0.48f }, player.Center);
            for (int i = 0; i < 14; i++) {
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(14f, 18f), DustID.Frost,
                    Main.rand.NextVector2Circular(2.8f, 2.8f), 110, new Color(180, 240, 255), 1.05f);
                dust.noGravity = true;
            }
        }
    }

    private static void ApplyNecroflight(Player player, BigChillStatePlayer state) {
        bool ultimateForm = state.UltimateBigChillActive;
        float riseAcceleration = state.AbsoluteZeroActive
            ? 0.56f
            : ultimateForm
                ? state.HungerBoostActive ? 0.54f : 0.48f
                : state.HungerBoostActive ? 0.5f : 0.44f;
        float gentleRiseAcceleration = state.AbsoluteZeroActive
            ? 0.46f
            : ultimateForm
                ? state.HungerBoostActive ? 0.44f : 0.38f
                : state.HungerBoostActive ? 0.4f : 0.34f;
        float maxRiseSpeed = state.AbsoluteZeroActive
            ? -7.6f
            : ultimateForm
                ? state.HungerBoostActive ? -7.2f : -6.6f
                : state.HungerBoostActive ? -6.9f : -6.2f;
        float cruiseFallSpeed = state.AbsoluteZeroActive ? 1.05f : ultimateForm ? 1.45f : 1.8f;

        if (player.controlJump || player.controlUp) {
            float acceleration = player.controlUp ? riseAcceleration : gentleRiseAcceleration;
            player.velocity.Y = Math.Max(maxRiseSpeed, player.velocity.Y - acceleration);
        }
        else if (player.velocity.Y > -0.6f) {
            player.velocity.Y = Math.Min(player.velocity.Y, cruiseFallSpeed);
        }

        if (player.controlDown)
            player.velocity.Y = Math.Min(player.velocity.Y + 0.28f, state.AbsoluteZeroActive ? 6f : 7f);
        else if (player.velocity.Y > 0f)
            player.velocity.Y *= state.AbsoluteZeroActive ? 0.82f : 0.88f;

        player.maxFallSpeed = state.AbsoluteZeroActive ? 5.6f : ultimateForm ? 6.4f : 7f;
        player.fallStart = (int)(player.position.Y / 16f);
    }

    private static bool TryGetActiveBigChillOwner(Projectile projectile, out Player owner) {
        owner = null;
        if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            return false;

        owner = Main.player[projectile.owner];
        return owner.active &&
               BigChillStatePlayer.IsBigChillTransformationId(owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId);
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction;
    }

    private static Vector2 ResolveTargetPosition(Player player, Vector2 fallbackDirection, float fallbackDistance) {
        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer)
            return Main.MouseWorld;

        return player.Center + fallbackDirection * fallbackDistance;
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
    }

    private static int ResolveHeroDamage(Player player, float multiplier) {
        float baseDamage = ResolveBaseDamage(player) * multiplier;
        return Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(baseDamage)));
    }

    private static int ResolveBaseDamage(Player player) {
        Item heldItem = player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }

    private static void KillOwnedProjectiles(Player player, params int[] projectileTypes) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active || projectile.owner != player.whoAmI)
                continue;

            for (int j = 0; j < projectileTypes.Length; j++) {
                if (projectile.type != projectileTypes[j])
                    continue;

                projectile.Kill();
                break;
            }
        }
    }
}
