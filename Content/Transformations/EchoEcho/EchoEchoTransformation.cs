using System;
using System.Collections.Generic;
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

public class EchoEchoTransformation : Transformation {
    internal const float FirstEchoDamageMultiplier = 0.48f;
    internal const float SecondEchoDamageMultiplier = 0.34f;
    internal const float ChorusEchoDamageMultiplier = 0.24f;

    private const float SonicPulseDamageMultiplier = 0.74f;
    private const float FeedbackBurstDamageMultiplier = 0.94f;
    private const int FeedbackBurstEnergyCost = 16;
    private const int DuplicateEnergyCost = 6;
    private const int EchoShiftBaseCost = 8;
    private const int ChorusOverloadEnergyCost = 60;
    private const int FallbackBaseDamage = 24;

    public override string FullID => EchoEchoStatePlayer.TransformationId;
    public override string TransformationName => "Echo Echo";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<EchoEcho_Buff>();
    public override Transformation ChildTransformation => ModContent.GetInstance<UltimateEchoEchoTransformation>();

    public override string Description =>
        "A slippery sonic splitter that fights through independent duplicates. Echo Echo sets up sentient clones, layers Resonance from multiple sources, and cashes that pressure out in Chorus Overload.";

    public override List<string> Abilities => new() {
        "Sonic Pulse uses the older sweeping wave visual and builds Resonance from the player directly.",
        "Feedback Burst spends Omnitrix Energy to detonate shockwaves from you and every active Echo.",
        "Duplicate creates independent Echo clones that move, pick targets, and attack on their own.",
        "Echo Shift swaps with the nearest Echo or performs a short backstep if you do not have one out.",
        "Resonance stacks faster when hits come from different sources, then pops for bonus area damage and OE refund.",
        "Chorus Overload briefly raises the Echo cap to three and supercharges the whole setup loop."
    };

    public override string PrimaryAttackName => "Sonic Pulse";
    public override string SecondaryAttackName => "Feedback Burst";
    public override string PrimaryAbilityName => "Duplicate";
    public override string SecondaryAbilityName => "Echo Shift";
    public override string TertiaryAbilityName => "Resonance Field";
    public override string UltimateAbilityName => "Chorus Overload";

    public override int PrimaryAttack => ModContent.ProjectileType<EchoEchoSonicBlastProjectile>();
    public override int PrimaryAttackSpeed => 10;
    public override int PrimaryShootSpeed => 15;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => SonicPulseDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<EchoEchoFeedbackBurstProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => FeedbackBurstDamageMultiplier;
    public override int SecondaryEnergyCost => FeedbackBurstEnergyCost;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => EchoEchoStatePlayer.DuplicateCooldownTicks;
    public override int PrimaryAbilityCost => DuplicateEnergyCost;

    public override bool HasSecondaryAbility => true;
    public override int SecondaryAbilityDuration => 1;
    public override int SecondaryAbilityCooldown => EchoEchoStatePlayer.EchoShiftCooldownTicks;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => EchoEchoStatePlayer.ChorusOverloadDurationTicks;
    public override int UltimateAbilityCooldown => EchoEchoStatePlayer.ChorusOverloadCooldownTicks;
    public override int UltimateAbilityCost => ChorusOverloadEnergyCost;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<EchoEchoCloneProjectile>(),
            ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(),
            ModContent.ProjectileType<EchoEchoFeedbackBurstProjectile>(),
            ModContent.ProjectileType<EchoEchoResonancePopProjectile>());
    }

    public override int GetSecondaryAbilityCost(OmnitrixPlayer omp) {
        EchoEchoStatePlayer state = omp.Player.GetModPlayer<EchoEchoStatePlayer>();
        return state.ChorusActive ? 0 : EchoShiftBaseCost;
    }

    public override int GetSecondaryAbilityCooldown(OmnitrixPlayer omp) {
        EchoEchoStatePlayer state = omp.Player.GetModPlayer<EchoEchoStatePlayer>();
        int cooldown = state.ChorusActive ? EchoEchoStatePlayer.ChorusShiftCooldownTicks : EchoEchoStatePlayer.EchoShiftCooldownTicks;
        return ApplyAbilityCooldownMultiplier(cooldown, omp.secondaryAbilityCooldownMultiplier);
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        EchoEchoStatePlayer state = player.GetModPlayer<EchoEchoStatePlayer>();
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.moveSpeed += 0.12f;
        player.runAcceleration += 0.12f;
        player.maxRunSpeed += 0.75f;
        player.statDefense += 4;
        player.aggro -= 320;

        if (!state.ChorusActive)
            return;

        player.GetAttackSpeed<HeroDamage>() += 0.18f;
        player.GetDamage<HeroDamage>() += 0.1f;
        player.moveSpeed += 0.06f;
        player.armorEffectDrawShadow = true;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        EchoEchoStatePlayer state = omp.Player.GetModPlayer<EchoEchoStatePlayer>();
        if (!state.ChorusActive)
            return;

        if (omp.setAttack == OmnitrixPlayer.AttackSelection.Primary) {
            item.useTime = item.useAnimation = Math.Max(7, (int)Math.Round(item.useTime * 0.72f));
        }
        else if (omp.setAttack == OmnitrixPlayer.AttackSelection.Secondary) {
            item.useTime = item.useAnimation = Math.Max(12, (int)Math.Round(item.useTime * 0.82f));
        }
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (player.whoAmI != Main.myPlayer)
            return;

        if (omp.PrimaryAbilityEnabled && !omp.PrimaryAbilityWasEnabled)
            PlaceDuplicate(player);

        if (!omp.SecondaryAbilityEnabled || omp.SecondaryAbilityWasEnabled)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            RequestEchoShift();
            return;
        }

        ExecuteEchoShift(player);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        EchoEchoStatePlayer state = player.GetModPlayer<EchoEchoStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        List<Projectile> echoes = EchoEchoCloneProjectile.GetOwnedEchoProjectiles(player);

        if (omp.altAttack) {
            int burstDamage = ScaleDamage(damage, SecondaryAttackModifier);
            Projectile.NewProjectile(source, player.Center, direction, SecondaryAttack, burstDamage, knockback + 1.8f,
                player.whoAmI, 0f, 0f);

            for (int i = 0; i < echoes.Count; i++) {
                Projectile echo = echoes[i];
                int echoDamage = ScaleDamage(damage, SecondaryAttackModifier * state.GetEchoDamageMultiplier(i));
                Projectile.NewProjectile(source, echo.Center, direction, SecondaryAttack, echoDamage,
                    knockback + 1.4f, player.whoAmI, state.GetEchoRepeatDelayTicks(i), i + 1);
            }

            return false;
        }

        int pulseDamage = ScaleDamage(damage, PrimaryAttackModifier);
        Vector2 spawnPosition = player.Center + direction * 12f;
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed, PrimaryAttack, pulseDamage,
            knockback, player.whoAmI, 0f, 0f);

        return false;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        EchoEchoStatePlayer state = omp.Player.GetModPlayer<EchoEchoStatePlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        string echoText = $"{state.ActiveEchoCount}/{state.MaxEchoCount} Echoes";

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? "Build Resonance"
                : "Player-fired sonic wave • old arc visual restored",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{FeedbackBurstEnergyCost} OE • {echoText}"
                : $"Burst from you and every Echo • {FeedbackBurstEnergyCost} OE",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? $"{DuplicateEnergyCost} OE"
                : $"Spawn or replace an Echo clone • {DuplicateEnergyCost} OE",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => state.HasAnyEchoes
                ? compact
                    ? $"{GetSecondaryAbilityCost(omp)} OE • Swap"
                    : $"Swap with nearest Echo • {GetSecondaryAbilityCost(omp)} OE"
                : compact
                    ? $"{GetSecondaryAbilityCost(omp)} OE • Backstep"
                    : $"Short backstep if no Echo exists • {GetSecondaryAbilityCost(omp)} OE",
            OmnitrixPlayer.AttackSelection.Ultimate => state.ChorusActive
                ? compact
                    ? $"Chorus {OmnitrixPlayer.FormatCooldownTicks(state.ChorusTicksRemaining)}"
                    : $"Chorus active • {OmnitrixPlayer.FormatCooldownTicks(state.ChorusTicksRemaining)} left"
                : compact
                    ? $"{UltimateAbilityCost} OE"
                    : $"Raise Echo cap to 3 and overclock repeats • {UltimateAbilityCost} OE",
            _ => base.GetAttackResourceSummary(selection, omp, compact)
        };
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.CopperHelmet;
        player.body = ArmorIDs.Body.CopperChainmail;
        player.legs = ArmorIDs.Legs.CopperGreaves;
    }

    internal static void ResolveResonanceHit(Projectile projectile, NPC target, int damageDone, int sourceId, bool heavyHit) {
        if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            return;

        Player owner = Main.player[projectile.owner];
        if (!owner.active)
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.IsEchoEchoResonancePrimedFor(projectile.owner)) {
            int consumedStacks = identity.ConsumeEchoEchoResonance(projectile.owner);
            int popDamage = Math.Max(1, (int)Math.Round(Math.Max(projectile.damage, damageDone) *
                (heavyHit ? 0.82f : 0.66f) + consumedStacks * 3f));
            float popScale = heavyHit ? 1f : 0.86f;
            SpawnResonancePop(owner, projectile.GetSource_FromThis(), target.Center, popDamage,
                projectile.knockBack + (heavyHit ? 1.5f : 1.1f), popScale, allowChorusChain: true);
            owner.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(2f + consumedStacks * 0.45f);
            if (target.boss)
                identity.ApplyEchoEchoFracture(projectile.owner, 180);
            UltimateEchoEchoTransformation.HandleCataclysmResonancePop(owner, target);
            return;
        }

        identity.ApplyEchoEchoResonance(projectile.owner, sourceId, heavyHit ? 2 : 1, heavyHit ? 330 : 300);
    }

    internal static void SpawnResonancePop(Player owner, IEntitySource source, Vector2 center, int damage,
        float knockback, float scale = 1f, bool allowChorusChain = true) {
        if (Main.netMode == NetmodeID.MultiplayerClient && owner.whoAmI != Main.myPlayer)
            return;

        Projectile.NewProjectile(source, center, Vector2.Zero,
            ModContent.ProjectileType<EchoEchoResonancePopProjectile>(), damage, knockback, owner.whoAmI,
            allowChorusChain ? 0f : 1f, scale);
    }

    internal static int ResolveHeroDamage(Player player, float multiplier) {
        float baseDamage = ResolveBaseDamage(player) * multiplier;
        return Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(baseDamage)));
    }

    internal static int ResolveBaseDamage(Player player) {
        Item heldItem = player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }

    internal static void ExecuteEchoShift(Player player) {
        Projectile nearestEcho = EchoEchoCloneProjectile.FindNearestOwnedEcho(player);
        Vector2 startCenter = player.Center;
        Vector2 destination = nearestEcho != null
            ? nearestEcho.Center - new Vector2(player.width * 0.5f, player.height * 0.5f)
            : FindBackstepDestination(player);

        if (nearestEcho != null) {
            nearestEcho.Center = startCenter;
            nearestEcho.velocity = Vector2.Zero;
            nearestEcho.netUpdate = true;
        }

        EmitShiftPulse(player, startCenter);
        player.Teleport(destination, TeleportationStyleID.DebugTeleport);
        player.velocity = Vector2.Zero;
        player.fallStart = (int)(player.position.Y / 16f);
        EmitShiftPulse(player, player.Center);

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, destination.X,
                destination.Y, TeleportationStyleID.DebugTeleport);
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
        }

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.18f, Volume = 0.62f }, player.Center);
    }

    private void PlaceDuplicate(Player player) {
        EchoEchoStatePlayer state = player.GetModPlayer<EchoEchoStatePlayer>();
        int anchorDamage = ResolveHeroDamage(player, 0.42f);
        EchoEchoCloneProjectile.TryPlaceOrMoveEcho(player, player.GetSource_FromThis(), Main.MouseWorld, anchorDamage,
            0f, state.MaxEchoCount, temporary: false);

        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.26f, Volume = 0.48f }, player.Center);
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

    private static Vector2 FindBackstepDestination(Player player) {
        int retreatDirection = -Math.Sign(player.direction == 0 ? 1 : player.direction);
        float[] distances = { 54f, 42f, 32f };
        float[] verticalOffsets = { -6f, -18f, 0f, 12f };

        foreach (float distance in distances) {
            foreach (float verticalOffset in verticalOffsets) {
                Vector2 candidate = player.position + new Vector2(retreatDirection * distance, verticalOffset);
                if (!Collision.SolidCollision(candidate, player.width, player.height))
                    return candidate;
            }
        }

        return player.position;
    }

    private static void EmitShiftPulse(Player player, Vector2 center) {
        int pulseDamage = ResolveHeroDamage(player, 0.46f);
        SpawnResonancePop(player, player.GetSource_FromThis(), center, pulseDamage, 2.8f, 0.7f, allowChorusChain: false);
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

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
    }

    private static void RequestEchoShift() {
        ModPacket packet = ModContent.GetInstance<global::Ben10Mod.Ben10Mod>().GetPacket();
        packet.Write((byte)global::Ben10Mod.Ben10Mod.MessageType.ExecuteEchoEchoShift);
        packet.Send();
    }
}
