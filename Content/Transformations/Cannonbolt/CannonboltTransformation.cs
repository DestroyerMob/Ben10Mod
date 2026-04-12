using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Cannonbolt;

public class CannonboltTransformation : Transformation {
    public const int RicochetDurationTicks = 7 * 60;
    public const int RicochetCooldownTicks = 16 * 60;
    public const int VaultSlamCooldownTicks = 10 * 60;
    public const int GyroShellDurationTicks = 6 * 60;
    public const int GyroShellCooldownTicks = 18 * 60;
    public const int SiegeRollDurationTicks = 8 * 60;
    public const int SiegeRollCooldownTicks = 50 * 60;
    public const float GroundSwipeDamageMultiplier = 1.02f;
    public const float RollContactDamageMultiplier = 0.92f;

    public override string FullID => CannonboltStatePlayer.TransformationId;
    public override string TransformationName => "Cannonbolt";
    public override int TransformationBuffId => ModContent.BuffType<Cannonbolt_Buff>();
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";

    public override string Description =>
        "An armored momentum tank built almost entirely around rolling state changes. Cannonbolt wants to control when he is uncurled, when he commits to shell mode, and how hard he cashes out every bounce.";

    public override List<string> Abilities => new() {
        "Grounded Shell Bash alternates between a compact body-check and a wider spin swipe while unrolled.",
        "Roll Mode completely changes movement and turns your shell into the hitbox.",
        "Ricochet Drive lets rolled Cannonbolt pinball off walls, building impact charge and speed with each bounce.",
        "Vault Slam launches upward from Roll Mode, then crashes back down into an impact burst.",
        "Gyro Shell makes hostile projectiles glance away or hit for less while you stay at high speed.",
        "Siege Roll locks Cannonbolt into a full-speed shell rush with explosive impacts and shockwaves on every bounce."
    };

    public override string PrimaryAttackName => "Shell Bash";
    public override string SecondaryAttackName => "Roll Mode";
    public override string PrimaryAbilityName => "Ricochet Drive";
    public override string SecondaryAbilityName => "Vault Slam";
    public override string TertiaryAbilityName => "Gyro Shell";
    public override string UltimateAbilityName => "Siege Roll";

    public override int PrimaryAttack => ModContent.ProjectileType<CannonboltSwipeProjectile>();
    public override int PrimaryAttackSpeed => 16;
    public override int PrimaryShootSpeed => 12;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => GroundSwipeDamageMultiplier;
    public override int PrimaryArmorPenetration => 8;

    public override int SecondaryAttack => ModContent.ProjectileType<CannonboltRollProjectile>();
    public override int SecondaryAttackSpeed => 14;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAttackModifier => RollContactDamageMultiplier;
    public override int SecondaryArmorPenetration => 12;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => RicochetDurationTicks;
    public override int PrimaryAbilityCooldown => RicochetCooldownTicks;
    public override int PrimaryAbilityCost => 18;

    public override bool HasSecondaryAbility => true;
    public override int SecondaryAbilityDuration => 1;
    public override int SecondaryAbilityCooldown => VaultSlamCooldownTicks;
    public override int SecondaryAbilityCost => 0;

    public override int TertiaryAbilityDuration => GyroShellDurationTicks;
    public override int TertiaryAbilityCooldown => GyroShellCooldownTicks;
    public override int TertiaryAbilityCost => 20;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => SiegeRollDurationTicks;
    public override int UltimateAbilityCooldown => SiegeRollCooldownTicks;
    public override int UltimateAbilityCost => 60;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player, ModContent.ProjectileType<CannonboltRollProjectile>());
        player.GetModPlayer<CannonboltStatePlayer>().ClearRollTelemetry();
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        CannonboltStatePlayer state = player.GetModPlayer<CannonboltStatePlayer>();

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetKnockback<HeroDamage>() += 0.55f;
        player.GetArmorPenetration<HeroDamage>() += 8;
        player.statDefense += 16;
        player.endurance += 0.04f;
        player.noKnockback = true;
        player.noFallDmg = true;
        player.runAcceleration *= 0.68f;
        player.maxRunSpeed *= 0.92f;

        if (state.IsRolled) {
            player.statDefense += state.SiegeActive ? 14 : 9;
            player.endurance += state.SiegeActive ? 0.12f : 0.08f;
            player.armorEffectDrawShadow = true;
            player.runAcceleration *= 0.3f;
            player.moveSpeed *= state.SiegeActive ? 1.15f : 0.82f;
            player.GetAttackSpeed<HeroDamage>() -= 0.1f;
        }

        if (omp.PrimaryAbilityEnabled) {
            player.GetDamage<HeroDamage>() += 0.05f;
            player.GetKnockback<HeroDamage>() += 0.18f;
        }

        if (omp.TertiaryAbilityEnabled) {
            player.endurance += 0.05f;
            player.statDefense += 4;
        }

        if (!state.SiegeActive)
            return;

        player.GetDamage<HeroDamage>() += 0.16f;
        player.GetKnockback<HeroDamage>() += 0.35f;
        player.GetArmorPenetration<HeroDamage>() += 8;
        player.statDefense += 6;
        player.endurance += 0.08f;
        player.moveSpeed += 0.18f;
        player.maxRunSpeed += 2.2f;
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        CannonboltStatePlayer state = player.GetModPlayer<CannonboltStatePlayer>();
        if (state.IsRolled && !omp.altAttack)
            return false;

        return base.CanStartCurrentAttack(player, omp);
    }

    public override bool TryActivateSecondaryAbility(Player player, OmnitrixPlayer omp) {
        if (!player.HasBuff<SecondaryAbilityCooldown>())
            player.GetModPlayer<CannonboltStatePlayer>().TryActivateVaultLaunch();

        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        CannonboltStatePlayer state = player.GetModPlayer<CannonboltStatePlayer>();

        if (omp.altAttack) {
            int rollDamage = ScaleDamage(damage, SecondaryAttackModifier);
            state.TryToggleRoll(source, ResolveRollEntryDirection(player), rollDamage, knockback + 2.6f);
            return false;
        }

        if (state.IsRolled)
            return false;

        Vector2 direction = ResolveAimDirection(player, velocity);
        int swipeDamage = ScaleDamage(damage, PrimaryAttackModifier);
        int variant = state.ConsumeGroundSwipeVariant();
        float swipeScale = variant == 0 ? 1.08f : 1.18f;
        float swipeKnockback = knockback + (variant == 0 ? 1.2f : 2f);

        Projectile.NewProjectile(source, player.MountedCenter + direction * 16f,
            direction * Math.Max(PrimaryShootSpeed, 10), PrimaryAttack, swipeDamage, swipeKnockback, player.whoAmI,
            swipeScale, variant);
        return false;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        CannonboltStatePlayer state = omp.Player.GetModPlayer<CannonboltStatePlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => state.IsRolled
                ? compact ? "Use Roll mode" : "Shell Bash only works while unrolled"
                : compact ? "Grounded melee" : "Grounded bash / swipe filler",
            OmnitrixPlayer.AttackSelection.Secondary => state.IsRolled
                ? state.SiegeActive
                    ? compact ? "Siege locked" : "Siege Roll keeps you curled until it ends"
                    : compact ? "Press again to exit" : "Use Roll Mode again to uncurl"
                : compact ? "Enter Roll" : "Enter Roll Mode",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => state.IsRolled
                ? (omp.IsPrimaryAbilityActive
                    ? compact
                        ? $"Ricochet {OmnitrixPlayer.FormatCooldownTicks(omp.GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.PrimaryAbility))}"
                        : $"Ricochet active • {OmnitrixPlayer.FormatCooldownTicks(omp.GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.PrimaryAbility))} left"
                    : compact ? "Ricochet ready" : "Ricochet ready while rolled")
                : compact ? "Needs Roll" : "Best used while already rolled",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => state.IsRolled
                ? (omp.GetAttackActionCooldownTicks(OmnitrixPlayer.AttackSelection.SecondaryAbility) > 0
                    ? compact
                        ? $"CD {OmnitrixPlayer.FormatCooldownTicks(omp.GetAttackActionCooldownTicks(OmnitrixPlayer.AttackSelection.SecondaryAbility))}"
                        : $"Vault Slam cooldown • {OmnitrixPlayer.FormatCooldownTicks(omp.GetAttackActionCooldownTicks(OmnitrixPlayer.AttackSelection.SecondaryAbility))}"
                    : compact ? "Jump / G" : "Press jump or G to launch upward")
                : compact ? "Jump / G" : "Press jump or G in Roll Mode to launch upward",
            OmnitrixPlayer.AttackSelection.TertiaryAbility => omp.IsTertiaryAbilityActive
                ? (compact
                    ? $"Gyro {OmnitrixPlayer.FormatCooldownTicks(omp.GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.TertiaryAbility))}"
                    : $"Gyro Shell active • {OmnitrixPlayer.FormatCooldownTicks(omp.GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.TertiaryAbility))} left")
                : compact ? "Fast graze" : "High-speed projectile graze / damage reduction",
            OmnitrixPlayer.AttackSelection.Ultimate => state.SiegeActive
                ? compact ? $"Siege {OmnitrixPlayer.FormatCooldownTicks(state.SiegeTicksRemaining)}"
                : $"Siege Roll active • {OmnitrixPlayer.FormatCooldownTicks(state.SiegeTicksRemaining)} left"
                : compact ? "Auto-rolls" : "Automatically curls into full-speed Siege Roll",
            _ => base.GetAttackResourceSummary(selection, omp, compact)
        };
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MeteorHelmet;
        player.body = ArmorIDs.Body.MeteorSuit;
        player.legs = ArmorIDs.Legs.MeteorLeggings;
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
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

    private static Vector2 ResolveRollEntryDirection(Player player) {
        int inputX = (player.controlRight ? 1 : 0) - (player.controlLeft ? 1 : 0);
        if (inputX != 0)
            return new Vector2(inputX, 0f);

        if (Math.Abs(player.velocity.X) > 0.15f)
            return new Vector2(MathF.Sign(player.velocity.X), 0f);

        return new Vector2(player.direction == 0 ? 1 : player.direction, 0f);
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
