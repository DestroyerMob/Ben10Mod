using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
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

namespace Ben10Mod.Content.Transformations.Frankenstrike;

public class FrankenstrikeTransformation : Transformation {
    private const int FallbackBaseDamage = 28;
    private const int ThunderLeapEnergyCost = 26;
    private const int CapacitorSpireEnergyCost = 16;
    private const int StormheartCost = 60;
    private const float PrimaryDamageMultiplier = 0.94f;
    private const float SecondaryDamageMultiplier = 1.18f;
    private const float ThunderLeapDamageMultiplier = 1.26f;
    private const float SpireDamageMultiplier = 0.84f;
    private const float StormheartConductiveFallbackRatio = 0.5f;
    private const float BaseLeapRange = 360f;
    private const float StormheartLeapRange = 440f;

    public override string FullID => FrankenstrikeStatePlayer.TransformationId;
    public override string TransformationName => "Frankenstrike";
    public override int TransformationBuffId => ModContent.BuffType<Frankenstrike_Buff>();
    public override string Description =>
        "A brutal Transylian bruiser who loads enemies with Conductive, turns them Overcharged, then detonates that setup with bolts, leaps, and a tesla field.";

    public override List<string> Abilities => new() {
        "Galvanic Fists builds Conductive through a 3-hit combo",
        "Tesla Bolt cashes out Overcharged into a chaining Thunderburst",
        "Thunder Leap crashes through targets and punishes Overcharged landings",
        "Capacitor Spires lock lanes down and form a damaging lightning tether",
        "Consuming Overcharged grants Galvanized speed, attack tempo, and OE sustain",
        "Stormheart Reanimation overclocks your whole thunder network"
    };

    public override string PrimaryAttackName => "Galvanic Fists";
    public override string SecondaryAttackName => "Tesla Bolt";
    public override string PrimaryAbilityAttackName => "Thunder Leap";
    public override string SecondaryAbilityAttackName => "Capacitor Spire";
    public override string UltimateAbilityName => "Stormheart Reanimation";

    public override int PrimaryAttack => ModContent.ProjectileType<FrankenstrikeGalvanicFistProjectile>();
    public override int PrimaryAttackSpeed => 12;
    public override int PrimaryShootSpeed => 1;
    public override int PrimaryUseStyle => ItemUseStyleID.Swing;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<FrankenstrikeTeslaProjectile>();
    public override int SecondaryAttackSpeed => 20;
    public override int SecondaryShootSpeed => 18;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;

    public override int PrimaryAbilityAttack => ModContent.ProjectileType<FrankenstrikeStormLeapProjectile>();
    public override int PrimaryAbilityAttackSpeed => 18;
    public override int PrimaryAbilityAttackShootSpeed => 0;
    public override int PrimaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float PrimaryAbilityAttackModifier => ThunderLeapDamageMultiplier;
    public override int PrimaryAbilityAttackEnergyCost => ThunderLeapEnergyCost;
    public override int PrimaryAbilityCooldown => FrankenstrikeStatePlayer.ThunderLeapCooldownTicks;
    public override bool PrimaryAbilityAttackSingleUse => true;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<FrankenstrikeCapacitorSpireProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => SpireDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => CapacitorSpireEnergyCost;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => FrankenstrikeStatePlayer.StormheartDurationTicks;
    public override int UltimateAbilityCooldown => FrankenstrikeStatePlayer.StormheartCooldownTicks;
    public override int UltimateAbilityCost => StormheartCost;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        FrankenstrikeStatePlayer state = player.GetModPlayer<FrankenstrikeStatePlayer>();
        player.GetDamage<HeroDamage>() += 0.12f;
        player.statDefense += 12;
        player.noFallDmg = true;
        player.GetKnockback<HeroDamage>() += 0.65f;
        player.buffImmune[BuffID.Electrified] = true;

        if (state.GalvanizedActive) {
            player.GetAttackSpeed<HeroDamage>() += 0.12f;
            player.moveSpeed += 0.1f;
            player.maxRunSpeed += 0.75f;
            player.runAcceleration += 0.12f;
            Lighting.AddLight(player.Center, new Vector3(0.2f, 0.42f, 0.84f));
        }

        if (state.StormheartActive) {
            player.GetDamage<HeroDamage>() += 0.08f;
            player.GetAttackSpeed<HeroDamage>() += 0.14f;
            player.moveSpeed += 0.08f;
            player.statDefense += 4;
            player.endurance += 0.04f;
            player.armorEffectDrawShadow = true;
            Lighting.AddLight(player.Center, new Vector3(0.3f, 0.52f, 0.92f));
        }
    }

    public override void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) {
        modifiers.Knockback *= 0.8f;
    }

    public override bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<UltimateAbility>() ||
            player.HasBuff<UltimateAbilityCooldown>() ||
            player.dead ||
            player.CCed ||
            omp.HasLoadedAbilityAttack) {
            return true;
        }

        if (omp.omnitrixEnergy < GetUltimateAbilityCost(omp)) {
            omp.ShowTransformFailureFeedback($"Need {GetUltimateAbilityCost(omp)} OE for {UltimateAbilityName}.");
            return true;
        }

        omp.omnitrixEnergy -= GetUltimateAbilityCost(omp);
        omp.ultimateAbilityTransformationId = FullID;
        player.AddBuff(ModContent.BuffType<UltimateAbility>(), GetUltimateAbilityDuration(omp));
        player.AddBuff(ModContent.BuffType<UltimateAbilityCooldown>(), GetUltimateAbilityCooldown(omp));

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.12f, Volume = 0.84f }, player.Center);
            for (int i = 0; i < 22; i++) {
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(18f, 32f),
                    i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                    Main.rand.NextVector2Circular(4.6f, 4.6f), 100, new Color(185, 228, 255),
                    Main.rand.NextFloat(1.05f, 1.45f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        FrankenstrikeStatePlayer state = player.GetModPlayer<FrankenstrikeStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.IsPrimaryAbilityAttackLoaded) {
            float maxRange = state.StormheartActive ? StormheartLeapRange : BaseLeapRange;
            Vector2 leapOffset = ResolveTargetPosition(player, direction, maxRange) - player.Center;
            if (leapOffset == Vector2.Zero)
                leapOffset = direction * maxRange;

            float requestedDistance = Math.Min(leapOffset.Length(), maxRange);
            Vector2 leapDirection = leapOffset.SafeNormalize(new Vector2(player.direction, 0f));
            float leapSpeed = FrankenstrikeStormLeapProjectile.GetLeapSpeed(state.StormheartActive);
            int leapFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / leapSpeed),
                FrankenstrikeStormLeapProjectile.MinLeapFrames, FrankenstrikeStormLeapProjectile.MaxLeapFrames);
            int leapDamage = ScaleDamage(damage, PrimaryAbilityAttackModifier);

            int projectileIndex = Projectile.NewProjectile(source, player.Center + leapDirection * 20f,
                leapDirection * leapSpeed, PrimaryAbilityAttack, leapDamage, knockback + 1.8f, player.whoAmI,
                state.StormheartActive ? 1f : 0f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.timeLeft = leapFrames;
                projectile.netUpdate = true;
            }

            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer)) {
                return false;
            }

            Vector2 anchor = Main.MouseWorld;
            if (FrankenstrikeCapacitorSpireProjectile.TryRepositionSpireNearAnchor(player, anchor))
                return false;

            List<Projectile> activeSpires = FrankenstrikeCapacitorSpireProjectile.GetOwnedSpires(player);
            if (activeSpires.Count >= 2) {
                Projectile oldestSpire = FrankenstrikeCapacitorSpireProjectile.FindOldestSpire(player);
                oldestSpire?.Kill();
            }

            int spireDamage = ScaleDamage(damage, SecondaryAbilityAttackModifier);
            int projectileIndex = Projectile.NewProjectile(source, anchor, Vector2.Zero, SecondaryAbilityAttack,
                spireDamage, knockback + 0.5f, player.whoAmI, anchor.X, anchor.Y);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.Center = anchor;
                projectile.localAI[1] = Main.GameUpdateCount;
                projectile.netUpdate = true;
            }

            return false;
        }

        if (omp.altAttack) {
            int boltDamage = ScaleDamage(damage, SecondaryAttackModifier);
            Vector2 spawnPosition = player.MountedCenter + direction * 16f;
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed, SecondaryAttack, boltDamage,
                knockback + 0.85f, player.whoAmI, (float)FrankenstrikeTeslaProjectile.ShotVariant.Main);

            if (state.StormheartActive) {
                int splitDamage = ScaleDamage(damage, SecondaryAttackModifier * 0.58f);
                for (int i = -1; i <= 1; i += 2) {
                    Projectile.NewProjectile(source, spawnPosition, direction.RotatedBy(0.14f * i) * (SecondaryShootSpeed - 1f),
                        SecondaryAttack, splitDamage, knockback + 0.35f, player.whoAmI,
                        (float)FrankenstrikeTeslaProjectile.ShotVariant.Side);
                }
            }

            return false;
        }

        int comboStep = state.ConsumeComboStep();
        float comboMultiplier = comboStep switch {
            1 => PrimaryAttackModifier * 1.02f,
            2 => PrimaryAttackModifier * 1.16f,
            _ => PrimaryAttackModifier
        };
        Projectile.NewProjectile(source, player.MountedCenter, direction, PrimaryAttack, ScaleDamage(damage, comboMultiplier),
            knockback + (comboStep == 2 ? 1.4f : 0.7f), player.whoAmI, 1f, comboStep);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MoltenHelmet;
        player.body = ArmorIDs.Body.MoltenBreastplate;
        player.legs = ArmorIDs.Legs.MoltenGreaves;
    }

    internal static void ApplyConductiveHit(Player owner, NPC target, int baseStacks, int refreshTime) {
        if (owner == null || !owner.active || target == null || !target.active)
            return;

        int stacks = baseStacks + (target.wet ? 1 : 0);
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyFrankenstrikeConductive(owner.whoAmI, stacks, refreshTime);
        target.AddBuff(BuffID.Electrified, 180);
        target.netUpdate = true;
    }

    internal static bool TryConsumeOvercharged(Player owner, NPC target, IEntitySource source, int bonusDamage,
        float knockback, bool chainBurst, bool lightningStrike) {
        if (owner == null || !owner.active || target == null || !target.active)
            return false;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!identity.IsFrankenstrikeOverchargedFor(owner.whoAmI))
            return false;

        FrankenstrikeStatePlayer state = owner.GetModPlayer<FrankenstrikeStatePlayer>();
        int residualConductive = state.StormheartActive
            ? Math.Max(1, (int)Math.Round(FrankenstrikeStatePlayer.ConductiveMaxStacks * StormheartConductiveFallbackRatio))
            : 0;
        identity.ConsumeFrankenstrikeOvercharged(owner.whoAmI, residualConductive);
        RewardOverchargedDetonation(owner, target);
        SpawnThunderburst(owner, source, target.Center, bonusDamage, knockback, chainBurst, lightningStrike);
        return true;
    }

    internal static void SpawnThunderclap(Player owner, IEntitySource source, Vector2 center, int damage,
        float knockback, float radiusScale = 1f, bool empowered = false) {
        if (!CanSpawnOwnedProjectile(owner))
            return;

        Projectile.NewProjectile(source, center, Vector2.Zero, ModContent.ProjectileType<FrankenstrikeThunderclapProjectile>(),
            Math.Max(1, damage), knockback, owner.whoAmI, radiusScale, empowered ? 1f : 0f);
    }

    internal static void SpawnLightningStrike(Player owner, IEntitySource source, Vector2 center, int damage,
        float knockback, int delay = 8, float radiusScale = 1f) {
        if (!CanSpawnOwnedProjectile(owner))
            return;

        Projectile.NewProjectile(source, center, Vector2.Zero, ModContent.ProjectileType<FrankenstrikeLightningStrikeProjectile>(),
            Math.Max(1, damage), knockback, owner.whoAmI, delay, radiusScale);
    }

    internal static void TriggerStormheartShutdown(Player player) {
        if (player == null || !player.active || player.dead)
            return;

        IEntitySource source = player.GetSource_FromThis();
        int playerClapDamage = ResolveHeroDamage(player, 0.62f);
        SpawnThunderclap(player, source, player.Center, playerClapDamage, 2.4f, 1.16f, empowered: true);

        foreach (Projectile spire in FrankenstrikeCapacitorSpireProjectile.GetOwnedSpires(player)) {
            if (!spire.active)
                continue;

            int spireClapDamage = Math.Max(1, (int)Math.Round(spire.damage * 0.76f));
            SpawnThunderclap(player, source, spire.Center, spireClapDamage, 1.4f, 0.96f, empowered: true);
        }

        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.22f, Volume = 0.82f }, player.Center);
        for (int i = 0; i < 26; i++) {
            Dust dust = Dust.NewDustPerfect(player.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(5.2f, 5.2f), 110, new Color(185, 232, 255), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    public static int ResolveHeroDamage(Player player, float multiplier) {
        float baseDamage = ResolveBaseDamage(player) * multiplier;
        return Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(baseDamage)));
    }

    private static void RewardOverchargedDetonation(Player owner, NPC target) {
        float energyRefund = target.boss ? 4f : 7f;
        owner.GetModPlayer<OmnitrixPlayer>().RestoreOmnitrixEnergy(energyRefund);
        owner.GetModPlayer<FrankenstrikeStatePlayer>().ApplyGalvanized();
    }

    private static void SpawnThunderburst(Player owner, IEntitySource source, Vector2 center, int damage, float knockback,
        bool chainBurst, bool lightningStrike) {
        SpawnThunderclap(owner, source, center, damage, knockback, chainBurst ? 1.04f : 0.9f, empowered: chainBurst);
        if (lightningStrike)
            SpawnLightningStrike(owner, source, center, Math.Max(1, (int)Math.Round(damage * 0.72f)), knockback + 0.5f, 4, 1f);

        if (!chainBurst || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int remainingChains = 3;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (remainingChains <= 0 || !npc.CanBeChasedBy())
                continue;

            float distance = npc.Center.Distance(center);
            if (distance < 24f || distance > 168f)
                continue;

            int chainDamage = Math.Max(1, (int)Math.Round(damage * 0.46f));
            npc.SimpleStrikeNPC(chainDamage, owner.direction, false, 0f, ModContent.GetInstance<HeroDamage>());
            ApplyConductiveHit(owner, npc, 1, 180);
            remainingChains--;

            if (Main.dedServ)
                continue;

            Vector2 arcDirection = (npc.Center - center).SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 8; i++) {
                float progress = i / 7f;
                Dust dust = Dust.NewDustPerfect(Vector2.Lerp(center, npc.Center, progress), DustID.Electric,
                    arcDirection.RotatedByRandom(0.45f) * Main.rand.NextFloat(0.15f, 0.8f), 105,
                    new Color(185, 228, 255), Main.rand.NextFloat(0.88f, 1.1f));
                dust.noGravity = true;
            }
        }
    }

    private static bool CanSpawnOwnedProjectile(Player owner) {
        return owner != null &&
               owner.active &&
               (Main.netMode != NetmodeID.MultiplayerClient || owner.whoAmI == Main.myPlayer);
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
    }

    private static int ResolveBaseDamage(Player player) {
        Item heldItem = player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
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

    private static Vector2 ResolveTargetPosition(Player player, Vector2 fallbackDirection, float fallbackDistance) {
        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer)
            return Main.MouseWorld;

        return player.Center + fallbackDirection * fallbackDistance;
    }
}
