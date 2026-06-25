using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Jetray;

public class JetrayTransformation : Transformation {
    public const string TransformationId = "Ben10Mod:Jetray";

    private const int JetstreamDiveEnergyCost = 25;
    private const int MaxUltimateLockedTargets = 3;

    public override string FullID => TransformationId;
    public override string TransformationName => "Jetray";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Jetray_Buff>();

    public override string Description =>
        "An aerial lock-on hunter who tags prey with neuro-lasers, holds clean strafe lines, and dives through locked targets.";

    public override List<string> Abilities => new() {
        "Neuro Laser tags quickly; repeated hits apply Lock so Jetray's other attacks can home in.",
        "Neuroshock Bolt hits best against locked targets.",
        "Natural flight lets Jetray fight from clean angles instead of trading on the ground.",
        "Strafe Lock strengthens Jetray while you keep long horizontal or diagonal flight paths.",
        "Jetstream Dive consumes Lock for a huge burst, so use it after tagging a priority target.",
        "Neurostorm Circuit orbits Jetray and repeatedly strikes locked targets."
    };

    public override string PrimaryAttackName => "Neuro Laser";
    public override string SecondaryAttackName => "Neuroshock Bolt";
    public override string PrimaryAbilityName => "Strafe Lock";
    public override string SecondaryAbilityAttackName => "Jetstream Dive";
    public override string UltimateAttackName => "Neurostorm Circuit";

    public override int PrimaryAttack => ModContent.ProjectileType<JetrayLaserProjectile>();
    public override int PrimaryAttackSpeed => 11;
    public override int PrimaryShootSpeed => 26;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.76f;

    public override int SecondaryAttack => ModContent.ProjectileType<JetrayBoltProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 18;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 0.92f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAbilityCooldown => 32 * 60;
    public override int PrimaryAbilityCost => 20;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<JetrayDiveProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 1.35f;
    public override int SecondaryAbilityAttackEnergyCost => JetstreamDiveEnergyCost;
    public override int SecondaryAbilityCooldown => 0;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<JetrayDiveProjectile>();
    public override int UltimateAttackSpeed => 28;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => 2.25f;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 48 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.moveSpeed += 0.18f;
        player.maxRunSpeed += 1.8f;
        player.accRunSpeed += 1.6f;
        player.jumpSpeedBoost += 1.35f;
        player.ignoreWater = true;
        player.gills = true;
        player.noFallDmg = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.armorEffectDrawShadow = true;
        player.wingTimeMax += 90;
        player.wingTime = Math.Max(player.wingTime, 24f);
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<JetrayWings>());

        JetrayFlightPathPlayer path = player.GetModPlayer<JetrayFlightPathPlayer>();
        if (path.HasCleanPath) {
            player.GetCritChance<HeroDamage>() += 4f + path.PathDisciplineRatio * 5f;
            player.GetAttackSpeed<HeroDamage>() += path.PathDisciplineRatio * 0.08f;
        }
        else {
            player.GetDamage<HeroDamage>() -= 0.05f;
        }
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        ApplyHyperspeedFlight(player);
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<JetrayFlightPathPlayer>().UpdateFlightPath(player, omp);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        TransformationAttackProfile profile = GetSelectedAttackProfile(omp);
        if (profile == null || profile.ProjectileType <= 0)
            return false;

        Vector2 direction = ResolveAimDirection(player, velocity);
        JetrayFlightPathPlayer path = player.GetModPlayer<JetrayFlightPathPlayer>();
        float shotQuality = omp.PrimaryAbilityEnabled ? path.ResolveShotQuality(player, direction) : 0f;

        if (omp.ultimateAttack || omp.IsSecondaryAbilityAttackLoaded) {
            if (omp.ultimateAttack) {
                int finalDamage = Math.Max(1, (int)Math.Round(damage * profile.DamageMultiplier));
                List<NPC> lockedTargets = CollectLockedTargets(player, MaxUltimateLockedTargets, 1300f);
                if (lockedTargets.Count > 0) {
                    int boltsPerTarget = lockedTargets.Count == 1 ? 8 : lockedTargets.Count == 2 ? 5 : 4;
                    for (int targetIndex = 0; targetIndex < lockedTargets.Count; targetIndex++) {
                        NPC target = lockedTargets[targetIndex];
                        for (int i = 0; i < boltsPerTarget; i++) {
                            float angle = MathHelper.TwoPi * (i / (float)boltsPerTarget) +
                                          targetIndex * 0.43f + (float)Main.GameUpdateCount * 0.028f;
                            Vector2 orbitOffset = angle.ToRotationVector2() * (72f + 12f * targetIndex);
                            Vector2 spawn = target.Center + orbitOffset;
                            Vector2 boltVelocity = (target.Center - spawn).SafeNormalize(direction) * (18f + shotQuality * 4f);
                            Projectile.NewProjectile(source, spawn, boltVelocity, ModContent.ProjectileType<JetrayBoltProjectile>(),
                                finalDamage, knockback + 1.8f, player.whoAmI, 1f, target.whoAmI + 1, shotQuality);
                        }
                    }
                }
                else {
                    Vector2 focusPoint = player.Center + direction * 280f;
                    int fallbackDamage = Math.Max(1, (int)Math.Round(finalDamage * 0.72f));
                    const int boltCount = 6;
                    for (int i = 0; i < boltCount; i++) {
                        float angle = MathHelper.TwoPi * i / boltCount;
                        Vector2 spawnOffset = angle.ToRotationVector2() * 54f;
                        Vector2 boltVelocity = (focusPoint - (player.Center + spawnOffset)).SafeNormalize(direction) * 16f;
                        Projectile.NewProjectile(source, player.Center + spawnOffset, boltVelocity,
                            ModContent.ProjectileType<JetrayBoltProjectile>(), fallbackDamage, knockback + 1.2f,
                            player.whoAmI, 1f, 0f, shotQuality);
                    }
                }

                return false;
            }

            int diveDamage = Math.Max(1, (int)Math.Round(damage * profile.DamageMultiplier));
            Projectile.NewProjectile(source, player.Center + direction * 12f, direction,
                ModContent.ProjectileType<JetrayDiveProjectile>(), diveDamage, knockback + 1.5f,
                player.whoAmI, JetrayDiveProjectile.VariantAbility, omp.PrimaryAbilityEnabled ? 1f : 0f, shotQuality);
            return false;
        }

        if (omp.altAttack) {
            int boltDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            int boltCount = omp.PrimaryAbilityEnabled && shotQuality >= 0.48f ? 2 : 1;
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            int focusTargetIndex = ResolveNearestLockedTargetIndex(player);
            for (int i = 0; i < boltCount; i++) {
                float offset = boltCount == 1 ? 0f : (i == 0 ? -12f : 12f);
                Vector2 spawn = player.Center + direction * 18f + perpendicular * offset;
                Vector2 boltVelocity = direction * SecondaryShootSpeed;
                Projectile.NewProjectile(source, spawn, boltVelocity, ModContent.ProjectileType<JetrayBoltProjectile>(),
                    boltDamage, knockback + 1f, player.whoAmI, omp.PrimaryAbilityEnabled ? 1f : 0f,
                    focusTargetIndex + 1, shotQuality);
            }

            return false;
        }

        int laserCount = omp.PrimaryAbilityEnabled ? shotQuality >= 0.55f ? 3 : 2 : 1;
        for (int i = 0; i < laserCount; i++) {
            float offset = laserCount switch {
                3 when i == 0 => -0.08f,
                3 when i == 2 => 0.08f,
                2 when i == 0 => -0.06f,
                2 => 0.06f,
                _ => 0f
            };
            Vector2 laserVelocity = direction.RotatedBy(offset) * PrimaryShootSpeed;
            Projectile.NewProjectile(source, player.Center + direction * 12f, laserVelocity,
                ModContent.ProjectileType<JetrayLaserProjectile>(), damage, knockback, player.whoAmI,
                omp.PrimaryAbilityEnabled ? 1f : 0f, shotQuality);
        }

        return false;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.PrimaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.SecondaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Ultimate)
            return base.GetAttackResourceSummary(selection, omp, compact);

        JetrayFlightPathPlayer path = omp.Player.GetModPlayer<JetrayFlightPathPlayer>();
        int lockedCount = CountLockedTargets(omp.Player, 1300f);
        string lockText = compact ? $"Locks {lockedCount}" : $"Locked targets {lockedCount}";
        string pathText = compact ? $"Path {path.PathDisciplinePercent}%" : $"Flight path {path.PathDisciplinePercent}%";
        string identityText = resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"{lockText} • {pathText}"
                : $"{lockText} • neuro-lasers apply Lock • {pathText}",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{lockText} • Needs Lock"
                : $"{lockText} • full Neuroshock value requires Lock",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? pathText
                : $"{pathText} • horizontal or diagonal firing lines strengthen volleys",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"{lockText} • Cashout"
                : $"{lockText} • Jetstream Dive consumes Lock for burst damage",
            OmnitrixPlayer.AttackSelection.Ultimate => compact
                ? $"{lockText} • Orbit"
                : $"{lockText} • Neurostorm orbits and strikes locked targets",
            _ => lockText
        };

        string baseText = base.GetAttackResourceSummary(selection, omp, compact);
        return string.IsNullOrWhiteSpace(baseText) ? identityText : $"{baseText} • {identityText}";
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.NecroHelmet;
        player.body = ArmorIDs.Body.NecroBreastplate;
        player.legs = ArmorIDs.Legs.NecroGreaves;
        player.wings = EquipLoader.GetEquipSlot(Mod, nameof(JetrayWings), EquipType.Wings);
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction;
    }

    private static void ApplyHyperspeedFlight(Player player) {
        if (player.controlJump || player.controlUp) {
            float ascentAcceleration = player.controlUp ? 0.6f : 0.42f;
            float maxRiseSpeed = player.controlUp ? -8f : -6.1f;
            player.velocity.Y = Math.Max(maxRiseSpeed, player.velocity.Y - ascentAcceleration);
        }
        else if (player.velocity.Y > -1.1f) {
            player.velocity.Y = Math.Min(player.velocity.Y, 2.6f);
        }

        if (player.controlDown)
            player.velocity.Y = Math.Min(player.velocity.Y + 0.34f, 8.5f);
        else if (player.velocity.Y > 0f)
            player.velocity.Y *= 0.88f;

        player.fallStart = (int)(player.position.Y / 16f);
        player.maxFallSpeed = 8.5f;
    }

    private static int ResolveNearestLockedTargetIndex(Player player) {
        NPC lockedTarget = FindNearestLockedTarget(player, 1300f);
        return lockedTarget?.whoAmI ?? -1;
    }

    private static NPC FindNearestLockedTarget(Player player, float maxDistance) {
        NPC lockedTarget = null;
        float closestDistanceSquared = maxDistance * maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            if (!identity.IsJetrayLockedFor(player.whoAmI))
                continue;

            float distanceSquared = Vector2.DistanceSquared(player.Center, npc.Center);
            if (distanceSquared >= closestDistanceSquared)
                continue;

            closestDistanceSquared = distanceSquared;
            lockedTarget = npc;
        }

        return lockedTarget;
    }

    private static List<NPC> CollectLockedTargets(Player player, int maxTargets, float maxDistance) {
        List<NPC> targets = new();
        float maxDistanceSquared = maxDistance * maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            if (!identity.IsJetrayLockedFor(player.whoAmI))
                continue;

            float distanceSquared = Vector2.DistanceSquared(player.Center, npc.Center);
            if (distanceSquared > maxDistanceSquared)
                continue;

            targets.Add(npc);
        }

        targets.Sort((left, right) =>
            Vector2.DistanceSquared(player.Center, left.Center).CompareTo(Vector2.DistanceSquared(player.Center, right.Center)));
        if (targets.Count > maxTargets)
            targets.RemoveRange(maxTargets, targets.Count - maxTargets);

        return targets;
    }

    private static int CountLockedTargets(Player player, float maxDistance) {
        int count = 0;
        float maxDistanceSquared = maxDistance * maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            if (!identity.IsJetrayLockedFor(player.whoAmI))
                continue;

            if (Vector2.DistanceSquared(player.Center, npc.Center) <= maxDistanceSquared)
                count++;
        }

        return count;
    }
}
