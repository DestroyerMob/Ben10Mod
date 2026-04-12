using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EyeGuy;

public class EyeGuyTransformation : Transformation {
    internal const int ExposedDurationTicks = 5 * 60;

    private const int FallbackBaseDamage = 26;
    private const float PrimaryBoltDamageMultiplier = 0.62f;
    private const float WatcherBoltDamageMultiplier = 0.52f;
    private const float ChestBeamDamageMultiplier = 0.22f;
    private const float WatcherBeamDamageMultiplier = 0.58f;
    private const float OmniGazeDamageMultiplier = 0.56f;
    private const float CompoundVisionDamageMultiplier = 0.34f;
    private const float SpectrumBreakDamageMultiplier = 0.72f;
    private const float AllEyesOpenSpectrumBreakDamageMultiplier = 0.92f;
    private const float ExposedBonusDamageMultiplier = 0.16f;
    private const int ExposedArmorCrack = 8;
    private const int OcularBurstUseTime = 8;
    private const int ChestBeamUseTime = 10;
    private const int OmniGazeCost = 18;
    private const int OmniGazeCooldownTicks = 16 * 60;
    private const int WatcherArrayCost = 22;
    private const int AllEyesOpenCost = 60;
    private const int ChestBeamSustainCost = 2;
    private const int ChestBeamSustainInterval = 12;
    private const int FireMarkDurationTicks = 240;
    private const int FrostMarkDurationTicks = 240;
    private const int ShockMarkDurationTicks = 240;
    private const float BoltSpeed = 24f;
    private const float OmniGazeBoltSpeed = 16f;
    private const float ShockChainSpeed = 22f;
    private const float ProjectileSpeedMultiplier = 1.1f;

    public override string FullID => EyeGuyStatePlayer.TransformationId;
    public override string TransformationName => "Eye Guy";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<EyeGuy_Buff>();
    public override string Description =>
        "An Opticoid artillery form built around elemental mark cycling, hovering crossfire, and a beam that cashes those marks out into explosive Spectrum Breaks.";

    public override List<string> Abilities => new() {
        "Ocular Burst rotates through fire, frost, and shock marks in a fixed order.",
        "Landing all 3 marks triggers Compound Vision, then opens a 5 second Exposed window.",
        "Chest Eye Beam is the cash-out tool: it drains light OE while you hold it, and detonates Exposed targets into Spectrum Breaks.",
        "Omni-Gaze fires a 360-degree barrage that fills missing marks first instead of wasting duplicates.",
        "Watcher Array opens 4 hovering eyes that repeat lighter versions of your burst and beam attacks.",
        "All Eyes Open overloads the full loop for 8 seconds and ends with a final Omni-Gaze pulse."
    };

    public override string PrimaryAttackName => "Ocular Burst";
    public override string SecondaryAttackName => "Chest Eye Beam";
    public override string PrimaryAbilityName => "Omni-Gaze";
    public override string SecondaryAbilityName => "Watcher Array";
    public override string UltimateAbilityName => "All Eyes Open";

    public override int PrimaryAttack => ModContent.ProjectileType<EyeGuyLaserbeam>();
    public override int PrimaryAttackSpeed => OcularBurstUseTime;
    public override int PrimaryShootSpeed => (int)BoltSpeed;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryBoltDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<EyeGuyChestBeamProjectile>();
    public override int SecondaryAttackSpeed => ChestBeamUseTime;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => ChestBeamDamageMultiplier;
    public override bool SecondaryChannel => true;
    public override bool SecondaryNoMelee => true;
    public override int SecondaryAttackSustainEnergyCost => ChestBeamSustainCost;
    public override int SecondaryAttackSustainInterval => ChestBeamSustainInterval;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => OmniGazeCooldownTicks;
    public override int PrimaryAbilityCost => OmniGazeCost;

    public override bool HasSecondaryAbility => true;
    public override int SecondaryAbilityDuration => EyeGuyStatePlayer.WatcherArrayDurationTicks;
    public override int SecondaryAbilityCooldown => EyeGuyStatePlayer.WatcherArrayCooldownTicks;
    public override int SecondaryAbilityCost => WatcherArrayCost;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => EyeGuyStatePlayer.AllEyesOpenDurationTicks;
    public override int UltimateAbilityCooldown => EyeGuyStatePlayer.AllEyesOpenCooldownTicks;
    public override int UltimateAbilityCost => AllEyesOpenCost;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player, ModContent.ProjectileType<EyeGuyChestBeamProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        EyeGuyStatePlayer state = player.GetModPlayer<EyeGuyStatePlayer>();
        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 6f;

        if (state.HasWatcherEyes)
            player.GetAttackSpeed<HeroDamage>() += 0.04f;

        if (!state.AllEyesOpenActive)
            return;

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 4f;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.45f, 0.36f, 0.12f));
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (player.whoAmI != Main.myPlayer)
            return;

        EyeGuyStatePlayer state = player.GetModPlayer<EyeGuyStatePlayer>();
        if (!state.HasWatcherEyes || !HasActiveMainBeam(player) || HasActiveWatcherBeam(player))
            return;

        Vector2 direction = ResolveAimDirection(player, new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        int watcherDamage = ResolveHeroDamage(player,
            ChestBeamDamageMultiplier * WatcherBeamDamageMultiplier * (state.AllEyesOpenActive ? 1.1f : 1f));
        int eyeIndex = state.ConsumeWatcherEyeIndex();
        Projectile.NewProjectile(player.GetSource_FromThis(), GetWatcherOrigin(player, direction, eyeIndex), direction,
            SecondaryAttack, watcherDamage, 1f, player.whoAmI, EyeGuyChestBeamProjectile.VariantWatcher, eyeIndex);
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        EyeGuyStatePlayer state = player.GetModPlayer<EyeGuyStatePlayer>();
        if (!state.HasWatcherEyes || Main.dedServ)
            return;

        Vector2 direction = ResolveAimDirection(player, new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        for (int i = 0; i < 4; i++) {
            Vector2 eyePosition = GetWatcherOrigin(player, direction, i);
            Color dustColor = i switch {
                0 => new Color(255, 150, 110),
                1 => new Color(140, 235, 255),
                2 => new Color(155, 190, 255),
                _ => new Color(255, 220, 155)
            };
            Dust dust = Dust.NewDustPerfect(eyePosition + Main.rand.NextVector2Circular(4f, 4f),
                i % 2 == 0 ? DustID.GemDiamond : DustID.GoldFlame,
                Main.rand.NextVector2Circular(0.4f, 0.4f), 95, dustColor,
                Main.rand.NextFloat(0.8f, state.AllEyesOpenActive ? 1.22f : 1.02f));
            dust.noGravity = true;
        }
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        if (!base.CanStartCurrentAttack(player, omp))
            return false;

        if (!omp.altAttack)
            return true;

        return !HasActiveMainBeam(player);
    }

    public override bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<PrimaryAbilityCooldown>() || player.HasBuff<PrimaryAbility>() || player.dead || player.CCed)
            return true;

        if (omp.omnitrixEnergy < OmniGazeCost) {
            omp.ShowTransformFailureFeedback($"Need {OmniGazeCost} OE for {PrimaryAbilityName}.");
            return true;
        }

        omp.omnitrixEnergy -= OmniGazeCost;
        player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), GetPrimaryAbilityCooldown(omp));
        FireOmniGaze(player, player.GetSource_FromThis(), ResolveHeroDamage(player, 1f), 1.8f,
            finalPulse: false, allEyesOpen: player.GetModPlayer<EyeGuyStatePlayer>().AllEyesOpenActive);

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item33 with { Pitch = 0.18f, Volume = 0.46f }, player.Center);

        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int damage, float knockback) {
        EyeGuyStatePlayer state = player.GetModPlayer<EyeGuyStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.altAttack) {
            if (HasActiveMainBeam(player))
                return false;

            int beamDamage = ScaleDamage(damage, SecondaryAttackModifier * (state.AllEyesOpenActive ? 1.14f : 1f));
            Projectile.NewProjectile(source, player.Center, direction, SecondaryAttack, beamDamage, knockback + 0.8f,
                player.whoAmI, EyeGuyChestBeamProjectile.VariantPrimary);

            if (state.HasWatcherEyes) {
                int watcherDamage = ScaleDamage(damage,
                    SecondaryAttackModifier * WatcherBeamDamageMultiplier * (state.AllEyesOpenActive ? 1.08f : 1f));
                int eyeIndex = state.ConsumeWatcherEyeIndex();
                Projectile.NewProjectile(source, GetWatcherOrigin(player, direction, eyeIndex), direction,
                    SecondaryAttack, watcherDamage, knockback + 0.35f, player.whoAmI,
                    EyeGuyChestBeamProjectile.VariantWatcher, eyeIndex);
            }

            return false;
        }

        if (state.AllEyesOpenActive) {
            FireAllEyesBurst(player, source, direction, damage, knockback, watcherEcho: false);
            if (state.HasWatcherEyes)
                FireAllEyesBurst(player, source, direction, damage, knockback, watcherEcho: true);
            return false;
        }

        EyeGuyElement element = state.ConsumeBurstElement();
        FireSingleBurst(player, source, direction, damage, knockback, element, watcherEcho: false);
        if (state.HasWatcherEyes)
            FireSingleBurst(player, source, direction, damage, knockback, element, watcherEcho: true);
        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (!IsEyeGuyProjectile(projectile.type))
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!identity.IsEyeGuyExposedFor(player.whoAmI))
            return;

        modifiers.SourceDamage *= 1f + ExposedBonusDamageMultiplier +
                                  (player.GetModPlayer<EyeGuyStatePlayer>().AllEyesOpenActive ? 0.04f : 0f);
        modifiers.ArmorPenetration += ExposedArmorCrack;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        EyeGuyStatePlayer state = omp.Player.GetModPlayer<EyeGuyStatePlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        string cycleText = $"{FormatElementName(state.PeekBurstElement())} > {FormatElementName(state.PeekBurstElement(1))} > {FormatElementName(state.PeekBurstElement(2))}";

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => state.AllEyesOpenActive
                ? compact
                    ? "Tri-element salvo"
                    : "Fires fire, frost, and shock together while overload is active"
                : compact
                    ? cycleText
                    : $"Cycles {cycleText} into Exposed",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? FormatEnergyRate(ChestBeamSustainCost, ChestBeamSustainInterval, compact)
                : $"Focused cash-out beam • Drain {FormatEnergyRate(ChestBeamSustainCost, ChestBeamSustainInterval, compact)}",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? $"{OmniGazeCost} OE"
                : $"360 barrage that fills missing marks first • {OmniGazeCost} OE",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => state.AllEyesOpenActive && !state.WatcherArrayActive
                ? compact
                    ? "Auto during ult"
                    : "Auto-active while All Eyes Open is running"
                : state.WatcherArrayActive
                    ? compact
                        ? $"Eyes {OmnitrixPlayer.FormatCooldownTicks(state.WatcherTicksRemaining)}"
                        : $"Watcher Array active • {OmnitrixPlayer.FormatCooldownTicks(state.WatcherTicksRemaining)} left"
                    : compact
                        ? $"{WatcherArrayCost} OE"
                        : $"4 hovering eyes repeat lighter attacks and beams • {WatcherArrayCost} OE",
            OmnitrixPlayer.AttackSelection.Ultimate => state.AllEyesOpenActive
                ? compact
                    ? $"Open {OmnitrixPlayer.FormatCooldownTicks(state.AllEyesOpenTicksRemaining)}"
                    : $"All Eyes Open active • {OmnitrixPlayer.FormatCooldownTicks(state.AllEyesOpenTicksRemaining)} left"
                : compact
                    ? $"{AllEyesOpenCost} OE"
                    : $"Tri-element bursts, automatic Watcher Array, wider beam, and a final Omni-Gaze pulse • {AllEyesOpenCost} OE",
            _ => base.GetAttackResourceSummary(selection, omp, compact)
        };
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<EyeGuy>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    internal static void ResolveElementalHit(Projectile projectile, NPC target, int damageDone, EyeGuyElement element, int flags) {
        if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            return;

        Player owner = Main.player[projectile.owner];
        if (!owner.active)
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!identity.IsEyeGuyExposedFor(projectile.owner)) {
            bool exposedTriggered = identity.ApplyEyeGuyMark(projectile.owner, element, GetMarkDuration(element), ExposedDurationTicks);
            if (exposedTriggered) {
                int popDamage = System.Math.Max(ResolveHeroDamage(owner, CompoundVisionDamageMultiplier),
                    (int)System.Math.Round(System.Math.Max(projectile.damage, damageDone) * 0.42f));
                SpawnSpectrumBurst(owner, projectile.GetSource_FromThis(), target.Center,
                    projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f)),
                    popDamage, projectile.knockBack * 0.75f, EyeGuySpectrumBurstProjectile.ModeCompoundVision,
                    target.boss ? 0.86f : 1f);
            }
        }

        switch (element) {
            case EyeGuyElement.Fire:
                target.AddBuff(BuffID.OnFire3, 180);
                break;
            case EyeGuyElement.Frost:
                target.AddBuff(BuffID.Frostburn2, 150);
                target.AddBuff(ModContent.BuffType<EnemySlow>(), 75);
                break;
            case EyeGuyElement.Shock:
                target.AddBuff(BuffID.Electrified, 105);
                if ((flags & EyeGuyLaserbeam.FlagDisableShockChain) == 0)
                    TrySpawnShockChain(projectile, target, damageDone);
                break;
        }

        target.netUpdate = true;
    }

    internal static void ResolveChestBeamHit(Projectile projectile, NPC target, int damageDone, bool watcherBeam) {
        if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            return;

        Player owner = Main.player[projectile.owner];
        if (!owner.active)
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!identity.ConsumeEyeGuyExposed(projectile.owner))
            return;

        EyeGuyStatePlayer state = owner.GetModPlayer<EyeGuyStatePlayer>();
        float multiplier = state.AllEyesOpenActive
            ? AllEyesOpenSpectrumBreakDamageMultiplier
            : SpectrumBreakDamageMultiplier;
        if (watcherBeam)
            multiplier *= 0.82f;

        int burstDamage = System.Math.Max(ResolveHeroDamage(owner, multiplier),
            (int)System.Math.Round(System.Math.Max(projectile.damage, damageDone) * (state.AllEyesOpenActive ? 0.84f : 0.68f)));
        int mode = state.AllEyesOpenActive
            ? EyeGuySpectrumBurstProjectile.ModeAllEyesOpen
            : EyeGuySpectrumBurstProjectile.ModeSpectrumBreak;

        SpawnSpectrumBurst(owner, projectile.GetSource_FromThis(), target.Center,
            projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f)),
            burstDamage, projectile.knockBack + 1.2f, mode, watcherBeam ? 0.88f : 1f);
        owner.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(state.AllEyesOpenActive ? 4.5f : 3f);
        target.netUpdate = true;
    }

    internal static void TriggerAllEyesOpenShutdownPulse(Player player) {
        if (!player.active)
            return;

        FireOmniGaze(player, player.GetSource_FromThis(), ResolveHeroDamage(player, 1f), 2f, finalPulse: true,
            allEyesOpen: true);
        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.08f, Volume = 0.48f }, player.Center);
    }

    internal static Vector2 GetWatcherOrigin(Player player, Vector2 aimDirection, int eyeIndex) {
        Vector2 direction = aimDirection.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        float orbit = (float)Main.GameUpdateCount * 0.055f + MathHelper.TwoPi * eyeIndex / 4f;
        EyeGuyStatePlayer state = player.GetModPlayer<EyeGuyStatePlayer>();
        float radiusX = state.AllEyesOpenActive ? 50f : 42f;
        float radiusY = state.AllEyesOpenActive ? 30f : 24f;
        Vector2 anchor = player.MountedCenter + new Vector2(0f, -16f);
        Vector2 orbitOffset = new Vector2((float)Math.Cos(orbit) * radiusX, (float)Math.Sin(orbit) * radiusY);
        Vector2 forwardBias = direction * 8f;
        return anchor + orbitOffset + forwardBias;
    }

    internal static int ResolveHeroDamage(Player player, float multiplier) {
        float baseDamage = ResolveBaseDamage(player) * multiplier;
        return System.Math.Max(1, (int)System.Math.Round(player.GetDamage<HeroDamage>().ApplyTo(baseDamage)));
    }

    private static int ResolveBaseDamage(Player player) {
        Item heldItem = player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return System.Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }

    private void FireSingleBurst(Player player, IEntitySource source, Vector2 direction, int damage, float knockback,
        EyeGuyElement element, bool watcherEcho) {
        float[] lateralOffsets = { -12f, 0f, 12f };
        float[] spreads = { -0.035f, 0f, 0.035f };
        int eyeIndex = (int)element;
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 spawnPosition = watcherEcho
            ? GetWatcherOrigin(player, direction, player.GetModPlayer<EyeGuyStatePlayer>().ConsumeWatcherEyeIndex())
            : player.MountedCenter + direction * 16f + perpendicular * lateralOffsets[eyeIndex];
        Vector2 shotDirection = direction.RotatedBy(spreads[eyeIndex]);
        float damageMultiplier = PrimaryAttackModifier * (watcherEcho ? WatcherBoltDamageMultiplier : 1f);
        int flags = watcherEcho ? EyeGuyLaserbeam.FlagWatcherEcho : 0;

        Projectile.NewProjectile(source, spawnPosition, shotDirection * BoltSpeed * ProjectileSpeedMultiplier, PrimaryAttack,
            ScaleDamage(damage, damageMultiplier), knockback + (watcherEcho ? 0.2f : 0.45f), player.whoAmI,
            (float)element, flags);
    }

    private void FireAllEyesBurst(Player player, IEntitySource source, Vector2 direction, int damage, float knockback,
        bool watcherEcho) {
        EyeGuyElement[] elements = { EyeGuyElement.Fire, EyeGuyElement.Frost, EyeGuyElement.Shock };
        float[] spreads = { -0.06f, 0f, 0.06f };
        float[] lateralOffsets = { -12f, 0f, 12f };
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 watcherOrigin = watcherEcho
            ? GetWatcherOrigin(player, direction, player.GetModPlayer<EyeGuyStatePlayer>().ConsumeWatcherEyeIndex())
            : Vector2.Zero;

        for (int i = 0; i < elements.Length; i++) {
            Vector2 shotDirection = direction.RotatedBy(spreads[i]);
            Vector2 spawnPosition = watcherEcho
                ? watcherOrigin + perpendicular * lateralOffsets[i] * 0.55f
                : player.MountedCenter + direction * 16f + perpendicular * lateralOffsets[i];
            float damageMultiplier = PrimaryAttackModifier * (watcherEcho ? WatcherBoltDamageMultiplier : 1f) *
                                     (watcherEcho ? 0.9f : 1.08f);
            int flags = EyeGuyLaserbeam.FlagOverload | (watcherEcho ? EyeGuyLaserbeam.FlagWatcherEcho : 0);

            Projectile.NewProjectile(source, spawnPosition, shotDirection * BoltSpeed * ProjectileSpeedMultiplier, PrimaryAttack,
                ScaleDamage(damage, damageMultiplier), knockback + (watcherEcho ? 0.35f : 0.7f), player.whoAmI,
                (float)elements[i], flags);
        }
    }

    private static void FireOmniGaze(Player player, IEntitySource source, int damage, float knockback, bool finalPulse,
        bool allEyesOpen) {
        EyeGuyStatePlayer state = player.GetModPlayer<EyeGuyStatePlayer>();
        List<NPC> targets = FindTargets(player.Center, finalPulse ? 440f : 360f, finalPulse ? 10 : 8);
        int shotCount = System.Math.Max(targets.Count + 2, finalPulse ? 10 : 8);
        float baseRotation = (float)Main.GameUpdateCount * 0.034f;

        for (int i = 0; i < shotCount; i++) {
            NPC target = i < targets.Count ? targets[i] : null;
            EyeGuyElement fallback = state.PeekBurstElement(i);
            EyeGuyElement element = target != null
                ? target.GetGlobalNPC<AlienIdentityGlobalNPC>().GetPreferredEyeGuyMark(player.whoAmI, fallback)
                : fallback;

            Vector2 shotDirection = target != null
                ? player.DirectionTo(target.Center)
                : (baseRotation + MathHelper.TwoPi * i / shotCount).ToRotationVector2();
            shotDirection = shotDirection.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));

            int flags = EyeGuyLaserbeam.FlagOmniGaze |
                        (allEyesOpen ? EyeGuyLaserbeam.FlagOverload : 0) |
                        (finalPulse ? EyeGuyLaserbeam.FlagFinalPulse : 0);
            float multiplier = OmniGazeDamageMultiplier * (finalPulse ? 0.92f : 1f) * (allEyesOpen ? 1.08f : 1f);
            Vector2 spawnPosition = player.MountedCenter + shotDirection * 18f;

            Projectile.NewProjectile(source, spawnPosition, shotDirection * OmniGazeBoltSpeed * ProjectileSpeedMultiplier,
                ModContent.ProjectileType<EyeGuyLaserbeam>(), ScaleDamage(damage, multiplier), knockback + 0.8f,
                player.whoAmI, (float)element, flags);
        }
    }

    private static void SpawnSpectrumBurst(Player owner, IEntitySource source, Vector2 center, Vector2 direction,
        int damage, float knockback, int mode, float scale = 1f) {
        if (Main.netMode == NetmodeID.MultiplayerClient && owner.whoAmI != Main.myPlayer)
            return;

        Projectile.NewProjectile(source, center, direction, ModContent.ProjectileType<EyeGuySpectrumBurstProjectile>(),
            damage, knockback, owner.whoAmI, mode, scale);
    }

    private static void TrySpawnShockChain(Projectile projectile, NPC target, int damageDone) {
        Player owner = Main.player[projectile.owner];
        if (!owner.active)
            return;

        NPC chainedTarget = FindClosestNPC(target.Center, 220f, target.whoAmI);
        if (chainedTarget == null)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient && owner.whoAmI != Main.myPlayer)
            return;

        EyeGuyStatePlayer state = owner.GetModPlayer<EyeGuyStatePlayer>();
        Vector2 direction = target.DirectionTo(chainedTarget.Center);
        int flags = EyeGuyLaserbeam.FlagDisableShockChain | (state.AllEyesOpenActive ? EyeGuyLaserbeam.FlagOverload : 0);
        int chainDamage = System.Math.Max(1, (int)System.Math.Round(System.Math.Max(projectile.damage, damageDone) *
            (state.AllEyesOpenActive ? 0.42f : 0.32f)));

        Projectile.NewProjectile(projectile.GetSource_FromThis(), target.Center, direction * ShockChainSpeed, projectile.type,
            chainDamage, projectile.knockBack * 0.55f, projectile.owner, (float)EyeGuyElement.Shock, flags);
    }

    private static List<NPC> FindTargets(Vector2 center, float maxDistance, int maxTargets) {
        List<NPC> targets = new();
        float maxDistanceSquared = maxDistance * maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            float distanceSquared = Vector2.DistanceSquared(center, npc.Center);
            if (distanceSquared > maxDistanceSquared)
                continue;

            targets.Add(npc);
        }

        targets.Sort((left, right) =>
            Vector2.DistanceSquared(center, left.Center).CompareTo(Vector2.DistanceSquared(center, right.Center)));

        if (targets.Count > maxTargets)
            targets.RemoveRange(maxTargets, targets.Count - maxTargets);

        return targets;
    }

    private static NPC FindClosestNPC(Vector2 center, float maxDistance, int ignoreNpc) {
        NPC bestTarget = null;
        float bestDistanceSquared = maxDistance * maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc.whoAmI == ignoreNpc || !npc.CanBeChasedBy())
                continue;

            float distanceSquared = Vector2.DistanceSquared(center, npc.Center);
            if (distanceSquared >= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private static int GetMarkDuration(EyeGuyElement element) {
        return element switch {
            EyeGuyElement.Fire => FireMarkDurationTicks,
            EyeGuyElement.Frost => FrostMarkDurationTicks,
            _ => ShockMarkDurationTicks
        };
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return System.Math.Max(1, (int)System.Math.Round(baseDamage * multiplier));
    }

    private static bool IsEyeGuyProjectile(int projectileType) {
        return projectileType == ModContent.ProjectileType<EyeGuyLaserbeam>() ||
               projectileType == ModContent.ProjectileType<EyeGuyChestBeamProjectile>() ||
               projectileType == ModContent.ProjectileType<EyeGuySpectrumBurstProjectile>();
    }

    private static bool HasActiveMainBeam(Player player) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active ||
                projectile.owner != player.whoAmI ||
                projectile.type != ModContent.ProjectileType<EyeGuyChestBeamProjectile>() ||
                (int)Math.Round(projectile.ai[0]) != EyeGuyChestBeamProjectile.VariantPrimary) {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool HasActiveWatcherBeam(Player player) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active ||
                projectile.owner != player.whoAmI ||
                projectile.type != ModContent.ProjectileType<EyeGuyChestBeamProjectile>() ||
                (int)Math.Round(projectile.ai[0]) != EyeGuyChestBeamProjectile.VariantWatcher) {
                continue;
            }

            return true;
        }

        return false;
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

    private static string FormatElementName(EyeGuyElement element) {
        return element switch {
            EyeGuyElement.Fire => "Fire",
            EyeGuyElement.Frost => "Frost",
            _ => "Shock"
        };
    }
}
