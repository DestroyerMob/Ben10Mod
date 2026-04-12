using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EchoEcho;

internal enum UltimateEchoEchoShotKind {
    PlayerCenter = 1,
    PlayerSide = 2,
    Speaker = 3,
    OverclockSpeaker = 4,
    CataclysmCenter = 5,
    CataclysmSide = 6,
    OverclockVolley = 7
}

public class UltimateEchoEchoTransformation : EchoEchoTransformation {
    private const int SpeakersPerSentrySlot = 3;
    internal const int FocusedDurationTicks = 150;
    private const int FocusedSearchDistance = 720;
    private const int OverclockCost = 24;
    private const int FeedbackPulseCost = 16;
    private const int RelayCost = 12;
    private const int RelayDashDistance = 144;
    private const int OverclockVolleyCooldownTicks = 8;
    private const float SideShotDamageMultiplier = 0.82f;
    private const float FeedbackPulseDamageMultiplier = 0.86f;
    private const float SpeakerFeedbackPulseDamageMultiplier = 0.56f;
    private const float RelayPulseDamageMultiplier = 0.48f;
    private const float FinalDischargeDamageMultiplier = 0.74f;
    private const float CataclysmEchoCenterDamageMultiplier = 0.46f;
    private const float CataclysmEchoSideDamageMultiplier = 0.3f;
    private const float CataclysmPopFollowupDamageMultiplier = 0.34f;
    private const float OverclockVolleyDamageMultiplier = 0.64f;
    private const float SpeakerShotSpeed = 12f;
    private const float OverclockSpeakerShotSpeed = 15.5f;
    private const float VolleyShotSpeed = 17f;

    public override string FullID => UltimateEchoEchoStatePlayer.TransformationId;
    public override string TransformationName => "Ultimate Echo Echo";
    public override int TransformationBuffId => ModContent.BuffType<UltimateEchoEcho_Buff>();
    public override Transformation ParentTransformation => ModContent.GetInstance<EchoEchoTransformation>();
    public override Transformation ChildTransformation => null;

    public override string Description =>
        "An evolved sonic commander that trades clone trickery for detached speakers, focused artillery lines, and timed battlefield overclocks.";

    public override List<string> Abilities => new() {
        "Resonance Burst keeps the current 3-shot spread and marks its target as Focused for your speakers.",
        "Speaker Deployment anchors detached arrays that act as full Resonance sources and can be nudged instead of constantly rebuilt.",
        "Overclock Array turns every active speaker into fast-response artillery for 6 seconds.",
        "Feedback Pulse detonates a peel burst from you and delayed follow-up pulses from every active speaker.",
        "Resonant Relay swaps through your speaker network or falls back to a short sonic dash.",
        "Harmonic Cataclysm synchronizes the whole field: speaker echoes, pop follow-ups, cheaper relay, and a final discharge."
    };

    public override string PrimaryAttackName => "Resonance Burst";
    public override string SecondaryAttackName => "Speaker Deployment";
    public override string PrimaryAbilityName => "Overclock Array";
    public override string SecondaryAbilityAttackName => "Feedback Pulse";
    public override string TertiaryAbilityAttackName => "Resonant Relay";
    public override string UltimateAbilityName => "Harmonic Cataclysm";

    public override int PrimaryAttack => ModContent.ProjectileType<UltimateEchoEchoSonicBlastProjectile>();
    public override int PrimaryAttackSpeed => 10;
    public override int PrimaryShootSpeed => 16;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 1.15f;

    public override int SecondaryAttack => ModContent.ProjectileType<UltimateEchoEchoSpeakerProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 0.85f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => UltimateEchoEchoStatePlayer.OverclockDurationTicks;
    public override int PrimaryAbilityCooldown => UltimateEchoEchoStatePlayer.OverclockCooldownTicks;
    public override int PrimaryAbilityCost => OverclockCost;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<UltimateEchoEchoPulseProjectile>();
    public override int SecondaryAbilityAttackSpeed => 1;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int TertiaryAbilityAttack => ModContent.ProjectileType<UltimateEchoEchoPulseProjectile>();
    public override int TertiaryAbilityAttackSpeed => 1;
    public override int TertiaryAbilityAttackShootSpeed => 0;
    public override int TertiaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override bool TertiaryAbilityAttackSingleUse => true;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => UltimateEchoEchoStatePlayer.HarmonicCataclysmDurationTicks;
    public override int UltimateAbilityCooldown => UltimateEchoEchoStatePlayer.HarmonicCataclysmCooldownTicks;
    public override int UltimateAbilityCost => 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<UltimateEchoEchoSpeakerProjectile>(),
            ModContent.ProjectileType<UltimateEchoEchoSonicBlastProjectile>(),
            ModContent.ProjectileType<UltimateEchoEchoPulseProjectile>(),
            ModContent.ProjectileType<EchoEchoResonancePopProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        UltimateEchoEchoStatePlayer state = player.GetModPlayer<UltimateEchoEchoStatePlayer>();
        player.GetDamage<HeroDamage>() += 0.18f;
        player.GetAttackSpeed<HeroDamage>() += 0.14f;
        player.maxTurrets += 1;

        if (!state.CataclysmActive)
            return;

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.1f;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.36f, 0.55f, 0.9f) * 0.58f);
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
    }

    public override int GetSecondaryAbilityCost(OmnitrixPlayer omp) => FeedbackPulseCost;

    public override int GetSecondaryAbilityCooldown(OmnitrixPlayer omp) {
        return ApplyAbilityCooldownMultiplier(UltimateEchoEchoStatePlayer.FeedbackPulseCooldownTicks,
            omp.secondaryAbilityCooldownMultiplier);
    }

    public override int GetTertiaryAbilityCost(OmnitrixPlayer omp) {
        UltimateEchoEchoStatePlayer state = omp.Player.GetModPlayer<UltimateEchoEchoStatePlayer>();
        return state.CataclysmActive ? 0 : RelayCost;
    }

    public override int GetTertiaryAbilityCooldown(OmnitrixPlayer omp) {
        UltimateEchoEchoStatePlayer state = omp.Player.GetModPlayer<UltimateEchoEchoStatePlayer>();
        int cooldown = state.CataclysmActive
            ? UltimateEchoEchoStatePlayer.ResonantRelayCataclysmCooldownTicks
            : UltimateEchoEchoStatePlayer.ResonantRelayCooldownTicks;
        return ApplyAbilityCooldownMultiplier(cooldown, omp.tertiaryAbilityCooldownMultiplier);
    }

    public override bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) {
        return player.GetModPlayer<UltimateEchoEchoStatePlayer>().CataclysmActive;
    }

    public override bool TryActivateSecondaryAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<SecondaryAbilityCooldown>() || player.dead || player.CCed)
            return true;

        if (omp.omnitrixEnergy < GetSecondaryAbilityCost(omp)) {
            omp.ShowTransformFailureFeedback($"Need {GetSecondaryAbilityCost(omp)} OE for {SecondaryAbilityAttackName}.");
            return true;
        }

        omp.omnitrixEnergy -= GetSecondaryAbilityCost(omp);
        omp.secondaryAbilityTransformationId = omp.currentTransformationId;
        player.AddBuff(ModContent.BuffType<SecondaryAbilityCooldown>(), GetSecondaryAbilityCooldown(omp));
        FireFeedbackPulse(player);

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.08f, Volume = 0.46f }, player.Center);

        return true;
    }

    public override bool TryActivateTertiaryAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<TertiaryAbilityCooldown>() || player.dead || player.CCed)
            return true;

        int relayCost = GetTertiaryAbilityCost(omp);
        if (omp.omnitrixEnergy < relayCost) {
            omp.ShowTransformFailureFeedback($"Need {relayCost} OE for {TertiaryAbilityAttackName}.");
            return true;
        }

        omp.omnitrixEnergy -= relayCost;
        omp.tertiaryAbilityTransformationId = omp.currentTransformationId;
        player.AddBuff(ModContent.BuffType<TertiaryAbilityCooldown>(), GetTertiaryAbilityCooldown(omp));

        Vector2 cursorWorld = Main.MouseWorld;
        if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer) {
            RequestResonantRelay(cursorWorld);
            return true;
        }

        ExecuteResonantRelay(player, cursorWorld);
        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        UltimateEchoEchoStatePlayer state = player.GetModPlayer<UltimateEchoEchoStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.altAttack) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 anchorPosition = Main.MouseWorld;
            if (UltimateEchoEchoSpeakerProjectile.TryRepositionSpeakerNearAnchor(player, anchorPosition))
                return false;

            int speakerType = ModContent.ProjectileType<UltimateEchoEchoSpeakerProjectile>();
            int maxSpeakers = GetSpeakerCapacity(player);
            List<Projectile> activeSpeakers = UltimateEchoEchoSpeakerProjectile.GetOwnedSpeakers(player);
            if (activeSpeakers.Count >= maxSpeakers)
                activeSpeakers[0].Kill();

            player.AddBuff(ModContent.BuffType<UltimateEchoEchoSpeakerBuff>(), 2);
            Vector2 spawnVelocity = player.Center.DirectionTo(anchorPosition) * 14f;
            int projectileIndex = Projectile.NewProjectile(source, player.Center, spawnVelocity, speakerType,
                ScaleDamage(damage, SecondaryAttackModifier), knockback, player.whoAmI, anchorPosition.X, anchorPosition.Y);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                omp.ultimateEchoEchoSpeakerSpawnSerial++;
                Projectile speaker = Main.projectile[projectileIndex];
                speaker.originalDamage = ScaleDamage(damage, SecondaryAttackModifier);
                speaker.localAI[1] = omp.ultimateEchoEchoSpeakerSpawnSerial;
                speaker.netUpdate = true;
            }

            return false;
        }

        NPC target = FindPriorityBurstTarget(player, direction, FocusedSearchDistance);
        float sideSpreadDegrees = target != null ? 4.2f : 7f;
        SpawnBurstFromOrigin(player, source, player.MountedCenter + direction * 12f, direction, damage,
            knockback, sideSpreadDegrees);

        if (state.CataclysmActive) {
            List<Projectile> speakers = UltimateEchoEchoSpeakerProjectile.GetOwnedSpeakers(player);
            for (int i = 0; i < speakers.Count; i++) {
                Projectile speaker = speakers[i];
                if (!speaker.active)
                    continue;

                Vector2 echoDirection = target != null
                    ? speaker.DirectionTo(target.Center)
                    : direction;
                SpawnCataclysmEchoBurst(player, speaker, echoDirection, damage, knockback);
            }
        }

        return false;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        UltimateEchoEchoStatePlayer state = omp.Player.GetModPlayer<UltimateEchoEchoStatePlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        int speakerCount = UltimateEchoEchoSpeakerProjectile.GetOwnedSpeakers(omp.Player).Count;
        int speakerCap = GetSpeakerCapacity(omp.Player);

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? "Focus + Resonance"
                : "Center lane builds the most Resonance and briefly Focuses the target for your speakers",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{speakerCount}/{speakerCap} Speakers"
                : $"Deploy or reposition detached speakers • {speakerCount}/{speakerCap} active",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => state.CataclysmActive && !state.OverclockActive
                ? compact
                    ? "Auto in ult"
                    : "Speakers are already Overclocked during Harmonic Cataclysm"
                : state.OverclockActive
                    ? compact
                        ? $"Clock {OmnitrixPlayer.FormatCooldownTicks(state.OverclockTicksRemaining)}"
                        : $"Overclock active • {OmnitrixPlayer.FormatCooldownTicks(state.OverclockTicksRemaining)} left"
                    : compact
                        ? $"{OverclockCost} OE"
                        : $"6s speaker overclock with immediate artillery follow-up shots • {OverclockCost} OE",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"{FeedbackPulseCost} OE"
                : $"Detonate a peel pulse from you, then delayed pulses from each speaker • {FeedbackPulseCost} OE",
            OmnitrixPlayer.AttackSelection.TertiaryAbility => compact
                ? $"{GetTertiaryAbilityCost(omp)} OE"
                : state.CataclysmActive
                    ? "Swap through your speaker network or dash if none exists • Free during Cataclysm"
                    : $"Swap through your speaker network or dash if none exists • {GetTertiaryAbilityCost(omp)} OE",
            OmnitrixPlayer.AttackSelection.Ultimate => state.CataclysmActive
                ? compact
                    ? $"Sync {OmnitrixPlayer.FormatCooldownTicks(state.CataclysmTicksRemaining)}"
                    : $"Harmonic Cataclysm active • {OmnitrixPlayer.FormatCooldownTicks(state.CataclysmTicksRemaining)} left"
                : compact
                    ? "60 OE"
                    : "Synchronize every speaker, echo your burst, empower Fracture damage, and end with a final discharge • 60 OE",
            _ => base.GetAttackResourceSummary(selection, omp, compact)
        };
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.PlatinumHelmet;
        player.body = ArmorIDs.Body.PlatinumChainmail;
        player.legs = ArmorIDs.Legs.PlatinumGreaves;
    }

    internal static int EncodeShotSource(UltimateEchoEchoShotKind kind, int sourceIndex) {
        return (int)kind * 10000 + Utils.Clamp(Math.Abs(sourceIndex), 0, 9999);
    }

    internal static int EncodePulseSourceId(int mode, int sourceIndex) {
        return 900000 + Utils.Clamp(Math.Abs(mode), 0, 99) * 10000 + Utils.Clamp(Math.Abs(sourceIndex), 0, 9999);
    }

    internal static UltimateEchoEchoShotKind DecodeShotKind(int encodedSourceId) {
        int kindValue = Math.Abs(encodedSourceId) / 10000;
        return Enum.IsDefined(typeof(UltimateEchoEchoShotKind), kindValue)
            ? (UltimateEchoEchoShotKind)kindValue
            : UltimateEchoEchoShotKind.Speaker;
    }

    internal static bool IsPlayerPrimaryShot(UltimateEchoEchoShotKind kind) {
        return kind is UltimateEchoEchoShotKind.PlayerCenter or UltimateEchoEchoShotKind.PlayerSide;
    }

    internal static bool IsSpeakerDrivenShot(UltimateEchoEchoShotKind kind) {
        return kind is UltimateEchoEchoShotKind.Speaker
            or UltimateEchoEchoShotKind.OverclockSpeaker
            or UltimateEchoEchoShotKind.CataclysmCenter
            or UltimateEchoEchoShotKind.CataclysmSide
            or UltimateEchoEchoShotKind.OverclockVolley;
    }

    internal static bool IsHeavyResonanceShot(UltimateEchoEchoShotKind kind) {
        return kind is UltimateEchoEchoShotKind.PlayerCenter
            or UltimateEchoEchoShotKind.OverclockSpeaker
            or UltimateEchoEchoShotKind.CataclysmCenter
            or UltimateEchoEchoShotKind.OverclockVolley;
    }

    internal static void TryTriggerImmediateVolley(Player owner, NPC target, int baseDamage, float knockback) {
        if (!owner.active || target == null || !target.active)
            return;

        UltimateEchoEchoStatePlayer state = owner.GetModPlayer<UltimateEchoEchoStatePlayer>();
        if (!state.EffectiveOverclockActive || !state.CanTriggerImmediateVolley)
            return;

        Projectile speaker = UltimateEchoEchoSpeakerProjectile.FindNearestSpeakerToPoint(owner, target.Center, 640f);
        if (speaker == null)
            return;

        int damage = Math.Max(1, (int)Math.Round(baseDamage * OverclockVolleyDamageMultiplier));
        SpawnSonicShot(speaker.GetSource_FromThis(), speaker.Center,
            speaker.DirectionTo(target.Center) * VolleyShotSpeed, damage, knockback, owner.whoAmI,
            UltimateEchoEchoShotKind.OverclockVolley, speaker.whoAmI);
        state.ConsumeImmediateVolleyCooldown(OverclockVolleyCooldownTicks);
    }

    internal static void HandleCataclysmResonancePop(Player owner, NPC target) {
        if (!owner.active || target == null || !target.active)
            return;

        UltimateEchoEchoStatePlayer state = owner.GetModPlayer<UltimateEchoEchoStatePlayer>();
        if (!state.CataclysmActive)
            return;

        List<Projectile> speakers = UltimateEchoEchoSpeakerProjectile.GetOwnedSpeakers(owner);
        int followupDamage = ResolveHeroDamage(owner, CataclysmPopFollowupDamageMultiplier);
        for (int i = 0; i < speakers.Count; i++) {
            Projectile speaker = speakers[i];
            if (!speaker.active)
                continue;

            SpawnSonicShot(speaker.GetSource_FromThis(), speaker.Center,
                speaker.DirectionTo(target.Center) * OverclockSpeakerShotSpeed,
                followupDamage, 1.1f, owner.whoAmI, UltimateEchoEchoShotKind.OverclockVolley, speaker.whoAmI);
        }
    }

    internal static void TriggerCataclysmShutdown(Player player) {
        if (!player.active)
            return;

        List<Projectile> speakers = UltimateEchoEchoSpeakerProjectile.GetOwnedSpeakers(player);
        if (speakers.Count == 0)
            return;

        int pulseDamage = ResolveHeroDamage(player, FinalDischargeDamageMultiplier);
        for (int i = 0; i < speakers.Count; i++) {
            Projectile speaker = speakers[i];
            if (!speaker.active)
                continue;

            SpawnPulse(speaker.GetSource_FromThis(), speaker.Center, pulseDamage, 1.8f, player.whoAmI,
                UltimateEchoEchoPulseProjectile.ModeFinalDischarge);
        }

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.05f, Volume = 0.46f }, player.Center);
    }

    internal static void ExecuteResonantRelay(Player player, Vector2 cursorWorld) {
        Projectile relaySpeaker = UltimateEchoEchoSpeakerProjectile.FindRelaySpeaker(player, cursorWorld);
        Vector2 startCenter = player.Center;
        Vector2 destination = relaySpeaker != null
            ? relaySpeaker.Center - new Vector2(player.width * 0.5f, player.height * 0.5f)
            : FindRelayDashDestination(player, cursorWorld);

        SpawnPulse(player.GetSource_FromThis(), startCenter, ResolveHeroDamage(player, RelayPulseDamageMultiplier),
            2f, player.whoAmI, UltimateEchoEchoPulseProjectile.ModeRelayPulse);

        if (relaySpeaker != null) {
            relaySpeaker.Center = startCenter;
            relaySpeaker.velocity = Vector2.Zero;
            relaySpeaker.ai[0] = startCenter.X;
            relaySpeaker.ai[1] = startCenter.Y;
            relaySpeaker.netUpdate = true;
        }

        player.Teleport(destination, TeleportationStyleID.DebugTeleport);
        player.velocity = Vector2.Zero;
        player.fallStart = (int)(player.position.Y / 16f);

        SpawnPulse(player.GetSource_FromThis(), player.Center, ResolveHeroDamage(player, RelayPulseDamageMultiplier),
            2.2f, player.whoAmI, UltimateEchoEchoPulseProjectile.ModeRelayPulse);

        if (relaySpeaker != null) {
            NPC target = FindPriorityBurstTarget(player, ResolveAimDirection(player, player.DirectionTo(cursorWorld)), 700f);
            if (target != null) {
                SpawnSonicShot(relaySpeaker.GetSource_FromThis(), player.Center,
                    player.Center.DirectionTo(target.Center) * VolleyShotSpeed,
                    ResolveHeroDamage(player, OverclockVolleyDamageMultiplier * 0.86f), 1.1f, player.whoAmI,
                    UltimateEchoEchoShotKind.OverclockVolley, relaySpeaker.whoAmI);
            }
        }

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, destination.X,
                destination.Y, TeleportationStyleID.DebugTeleport);
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
        }

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f, Volume = 0.52f }, player.Center);
    }

    internal static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction;
    }

    internal static NPC FindFocusedTarget(Player owner, Vector2 origin, float maxDistance) {
        NPC bestNpc = null;
        float bestDistance = maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            if (!identity.IsUltimateEchoEchoFocusedFor(owner.whoAmI))
                continue;

            float distance = origin.Distance(npc.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestNpc = npc;
        }

        return bestNpc;
    }

    internal static int GetSpeakerCapacity(Player player) {
        return Math.Max(1, player.maxTurrets) * SpeakersPerSentrySlot;
    }

    internal static int ResolveSpeakerShotDamage(Projectile speaker, Player owner, bool overclocked) {
        int baseDamage = speaker.originalDamage > 0 ? speaker.originalDamage : speaker.damage;
        if (!overclocked)
            return Math.Max(1, baseDamage);

        return Math.Max(1, (int)Math.Round(baseDamage * 1.08f));
    }

    internal static void SpawnSonicShot(IEntitySource source, Vector2 origin, Vector2 velocity, int damage,
        float knockback, int owner, UltimateEchoEchoShotKind kind, int sourceIndex, float delayTicks = 0f) {
        Projectile.NewProjectile(source, origin, velocity, ModContent.ProjectileType<UltimateEchoEchoSonicBlastProjectile>(),
            Math.Max(1, damage), knockback, owner, EncodeShotSource(kind, sourceIndex), delayTicks);
    }

    internal static void SpawnPulse(IEntitySource source, Vector2 center, int damage, float knockback, int owner,
        int mode, float delayTicks = 0f) {
        Projectile.NewProjectile(source, center, Vector2.Zero, ModContent.ProjectileType<UltimateEchoEchoPulseProjectile>(),
            Math.Max(1, damage), knockback, owner, delayTicks, mode);
    }

    private void FireFeedbackPulse(Player player) {
        int mainDamage = ResolveHeroDamage(player, FeedbackPulseDamageMultiplier);
        SpawnPulse(player.GetSource_FromThis(), player.Center, mainDamage, 3.2f, player.whoAmI,
            UltimateEchoEchoPulseProjectile.ModeFeedbackMain);

        List<Projectile> speakers = UltimateEchoEchoSpeakerProjectile.GetOwnedSpeakers(player);
        int speakerDamage = ResolveHeroDamage(player, SpeakerFeedbackPulseDamageMultiplier);
        for (int i = 0; i < speakers.Count; i++) {
            Projectile speaker = speakers[i];
            if (!speaker.active)
                continue;

            SpawnPulse(speaker.GetSource_FromThis(), speaker.Center, speakerDamage, 2.2f, player.whoAmI,
                UltimateEchoEchoPulseProjectile.ModeFeedbackSpeaker, 6f + i * 4f);
        }
    }

    private void SpawnBurstFromOrigin(Player player, IEntitySource source, Vector2 origin, Vector2 direction, int damage,
        float knockback, float sideSpreadDegrees) {
        for (int i = -1; i <= 1; i++) {
            bool centerShot = i == 0;
            float spreadRadians = MathHelper.ToRadians(sideSpreadDegrees * i);
            Vector2 shotVelocity = direction.RotatedBy(spreadRadians) * PrimaryShootSpeed;
            int shotDamage = ScaleDamage(damage, centerShot ? PrimaryAttackModifier : SideShotDamageMultiplier);
            SpawnSonicShot(source, origin, shotVelocity, shotDamage, knockback + (centerShot ? 0.3f : 0f), player.whoAmI,
                centerShot ? UltimateEchoEchoShotKind.PlayerCenter : UltimateEchoEchoShotKind.PlayerSide,
                centerShot ? 0 : i < 0 ? 1 : 2);
        }
    }

    private void SpawnCataclysmEchoBurst(Player player, Projectile speaker, Vector2 direction, int damage, float knockback) {
        float sideSpreadDegrees = FindPriorityBurstTarget(player, direction, FocusedSearchDistance) != null ? 4f : 7f;
        for (int i = -1; i <= 1; i++) {
            bool centerShot = i == 0;
            float spreadRadians = MathHelper.ToRadians(sideSpreadDegrees * i);
            Vector2 shotVelocity = direction.RotatedBy(spreadRadians) * OverclockSpeakerShotSpeed;
            int shotDamage = ScaleDamage(damage,
                centerShot ? CataclysmEchoCenterDamageMultiplier : CataclysmEchoSideDamageMultiplier);
            SpawnSonicShot(speaker.GetSource_FromThis(), speaker.Center, shotVelocity, shotDamage,
                knockback + 0.2f, player.whoAmI,
                centerShot ? UltimateEchoEchoShotKind.CataclysmCenter : UltimateEchoEchoShotKind.CataclysmSide,
                speaker.whoAmI * 4 + (i + 1));
        }
    }

    internal static NPC FindPriorityBurstTarget(Player player, Vector2 direction, float maxDistance) {
        NPC priorityTarget = null;
        float bestScore = float.MaxValue;

        if (player.HasMinionAttackTargetNPC) {
            NPC forcedTarget = Main.npc[player.MinionAttackTargetNPC];
            if (forcedTarget.CanBeChasedBy() && player.Center.Distance(forcedTarget.Center) <= maxDistance)
                return forcedTarget;
        }

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            bool focused = identity.IsUltimateEchoEchoFocusedFor(player.whoAmI);
            bool fractured = identity.IsEchoEchoFracturedFor(player.whoAmI);
            if (!focused && !fractured)
                continue;

            Vector2 toTarget = npc.Center - player.Center;
            float distance = toTarget.Length();
            if (distance > maxDistance)
                continue;

            float anglePenalty = Math.Abs(MathHelper.WrapAngle(
                direction.ToRotation() - toTarget.SafeNormalize(direction).ToRotation())) * 120f;
            float score = distance + anglePenalty - (focused ? 160f : 0f);
            if (score >= bestScore)
                continue;

            bestScore = score;
            priorityTarget = npc;
        }

        return priorityTarget;
    }

    private static Vector2 FindRelayDashDestination(Player player, Vector2 cursorWorld) {
        Vector2 desiredDirection = player.DirectionTo(cursorWorld);
        if (desiredDirection == Vector2.Zero)
            desiredDirection = new Vector2(player.direction == 0 ? 1 : player.direction, 0f);

        float[] distances = { RelayDashDistance, 112f, 84f, 56f };
        for (int i = 0; i < distances.Length; i++) {
            Vector2 candidate = player.position + desiredDirection * distances[i];
            if (!Collision.SolidCollision(candidate, player.width, player.height))
                return candidate;
        }

        return player.position;
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
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

    private static void RequestResonantRelay(Vector2 cursorWorld) {
        ModPacket packet = ModContent.GetInstance<global::Ben10Mod.Ben10Mod>().GetPacket();
        packet.Write((byte)global::Ben10Mod.Ben10Mod.MessageType.ExecuteUltimateEchoEchoRelay);
        packet.Write(cursorWorld.X);
        packet.Write(cursorWorld.Y);
        packet.Send();
    }
}
