using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.FourArms;

public class FourArmsTransformation : Transformation {
    public const int BerserkDurationTicks = 10 * 60;

    private const int GroundSlamCooldownTicks = 5 * 60;
    private const int HaymakerCooldownTicks = 8 * 60;
    private const int BerserkCooldownTicks = 38 * 60;
    private const float PrimaryDamageMultiplier = 1.04f;
    private const float FinisherDamageMultiplier = 1.28f;
    private const float SecondaryDamageMultiplier = 1.16f;
    private const float HaymakerDamageMultiplier = 1.7f;

    public override string FullID => "Ben10Mod:FourArms";
    public override string TransformationName => "Fourarms";
    public override string IconPath => "Ben10Mod/Content/Interface/FourArmsSelect";
    public override int TransformationBuffId => ModContent.BuffType<FourArms_Buff>();

    public override string Description =>
        "A Tetramand brawler built around fists, slams, and crowd-control shockwaves. Four Arms wins by staying in melee, building Rage, and cashing that meter out in Berserker mode.";

    public override List<string> Abilities => new() {
        "Titan Combo chains fast punches, then widens into a cleaving third hit.",
        "Shock Clap sends a short-range shockwave through crowds.",
        "Ground Slam triggers from the ability hotkey or a double tap down input.",
        "Haymaker is a charged super-armored punch for big single-target damage.",
        "Passive Rage builds from dealing or taking close-range punishment and feeds attack speed.",
        "Ultimate Berserker mode spends full Rage for faster, larger combos and fissure slams."
    };

    public override string PrimaryAttackName => "Titan Combo";
    public override string SecondaryAttackName => "Shock Clap";
    public override string PrimaryAbilityName => "Ground Slam";
    public override string SecondaryAbilityName => "Haymaker";
    public override string SecondaryAbilityAttackName => "Haymaker";
    public override string UltimateAbilityName => "Berserker";

    public override int PrimaryAttack => ModContent.ProjectileType<FourArmsPunchProjectile>();
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;
    public override int PrimaryAttackSpeed => 12;
    public override int PrimaryShootSpeed => 12;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int PrimaryArmorPenetration => 10;

    public override int SecondaryAttack => ModContent.ProjectileType<FourArmsClap>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 16;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;
    public override int SecondaryArmorPenetration => 12;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => GroundSlamCooldownTicks;
    public override int PrimaryAbilityCost => 0;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<FourArmsHaymakerChargeProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.Shoot;
    public override bool SecondaryAbilityAttackChannel => true;
    public override float SecondaryAbilityAttackModifier => HaymakerDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => 0;
    public override int SecondaryAbilityCooldown => HaymakerCooldownTicks;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => BerserkDurationTicks;
    public override int UltimateAbilityCooldown => BerserkCooldownTicks;
    public override int UltimateAbilityCost => 0;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<FourArmsHaymakerChargeProjectile>(),
            ModContent.ProjectileType<FourArmsGroundSlamSequenceProjectile>(),
            ModContent.ProjectileType<FourArmsFissureProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        float rageRatio = state.RageRatio;

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetAttackSpeed<HeroDamage>() += 0.04f + rageRatio * 0.12f;
        player.GetKnockback<HeroDamage>() += 0.65f;
        player.GetArmorPenetration<HeroDamage>() += 10;
        player.statDefense += 13;
        player.noFallDmg = true;
        player.jumpSpeedBoost += 1.2f;
        player.runAcceleration *= 0.74f;
        player.maxRunSpeed *= 0.92f;

        if (state.HaymakerCharging) {
            player.endurance += 0.12f;
            player.noKnockback = true;
            player.runAcceleration *= 0.65f;
            player.moveSpeed *= 0.8f;
        }

        if (!state.BerserkActive)
            return;

        player.GetDamage<HeroDamage>() += 0.18f;
        player.GetAttackSpeed<HeroDamage>() += 0.2f;
        player.GetKnockback<HeroDamage>() += 0.45f;
        player.GetArmorPenetration<HeroDamage>() += 8;
        player.statDefense += 6;
        player.endurance += 0.06f;
        player.armorEffectDrawShadow = true;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        FourArmsGroundSlamPlayer state = omp.Player.GetModPlayer<FourArmsGroundSlamPlayer>();
        if (omp.setAttack != OmnitrixPlayer.AttackSelection.Primary)
            return;

        float comboSpeedMultiplier = 1f - state.RageRatio * 0.08f - (state.BerserkActive ? 0.14f : 0f);
        item.useTime = item.useAnimation = Math.Max(7, (int)Math.Round(item.useTime * comboSpeedMultiplier));
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        if (!base.CanStartCurrentAttack(player, omp))
            return false;

        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        return !state.GroundSlamActive && !state.HaymakerCharging;
    }

    public override bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<FourArmsGroundSlamPlayer>().TryStartGroundSlam();
        return true;
    }

    public override bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<UltimateAbility>() ||
            player.HasBuff<UltimateAbilityCooldown>() ||
            omp.ultimateAttack ||
            omp.HasLoadedAbilityAttack) {
            return true;
        }

        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        if (!state.HasFullRage)
            return true;

        state.ConsumeAllRage();
        player.AddBuff(ModContent.BuffType<UltimateAbility>(), BerserkDurationTicks);
        omp.ultimateAbilityTransformationId = FullID;

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.18f, Volume = 0.75f }, player.Center);
            for (int i = 0; i < 24; i++) {
                Vector2 velocity = Main.rand.NextVector2Circular(4.6f, 4.6f);
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(18f, 30f), DustID.Torch,
                    velocity, 90, new Color(255, 145, 90), Main.rand.NextFloat(1.15f, 1.65f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    public override void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) {
        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        modifiers.Knockback *= 0.28f;

        if (state.HaymakerCharging) {
            modifiers.FinalDamage *= 0.82f;
            modifiers.Knockback *= 0f;
        }

        if (state.BerserkActive)
            modifiers.FinalDamage *= 0.92f;
    }

    public override void OnHurt(Player player, OmnitrixPlayer omp, Player.HurtInfo info) {
        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        state.AddRage(Math.Min(24f, 4f + info.Damage * 0.42f));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + direction * 18f;
        bool berserk = state.BerserkActive;

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (HasActiveOwnedProjectile(player, SecondaryAbilityAttack))
                return false;

            int haymakerDamage = ScaleDamage(damage, SecondaryAbilityAttackModifier * (berserk ? 1.15f : 1f));
            Projectile.NewProjectile(source, player.Center, direction, SecondaryAbilityAttack, haymakerDamage,
                knockback + 2.4f, player.whoAmI, berserk ? 1f : 0f);
            return false;
        }

        if (omp.altAttack) {
            int clapDamage = ScaleDamage(damage, SecondaryAttackModifier * (berserk ? 1.08f : 1f));
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryShootSpeed, SecondaryAttack, clapDamage,
                knockback + 2f, player.whoAmI);
            return false;
        }

        int comboStep = state.ConsumeComboStep();
        bool finisher = comboStep >= 2;
        float punchScale = 1.18f + state.RageRatio * 0.12f + (berserk ? 0.2f : 0f);

        if (finisher) {
            float[] spread = { -0.18f, 0f, 0.18f };
            int finisherDamage = ScaleDamage(damage, FinisherDamageMultiplier * (berserk ? 1.12f : 1f));
            for (int i = 0; i < spread.Length; i++) {
                Vector2 punchDirection = direction.RotatedBy(spread[i]).SafeNormalize(direction);
                Vector2 finisherSpawn = player.MountedCenter + punchDirection * 20f;
                Projectile.NewProjectile(source, finisherSpawn, punchDirection * Math.Max(PrimaryShootSpeed, 10),
                    PrimaryAttack, finisherDamage, knockback + 3.1f, player.whoAmI, punchScale + 0.16f, 2f);
            }

            return false;
        }

        int punchDamage = ScaleDamage(damage, PrimaryAttackModifier * (berserk ? 1.08f : 1f));
        Projectile.NewProjectile(source, spawnPosition, direction * Math.Max(PrimaryShootSpeed, 10), PrimaryAttack,
            punchDamage, knockback + comboStep * 0.35f, player.whoAmI, punchScale, comboStep);
        return false;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsFourArmsProjectile(projectile.type))
            return;

        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        float gain = projectile.type switch {
            _ when projectile.type == PrimaryAttack => 5.5f,
            _ when projectile.type == SecondaryAttack => 4f,
            _ when projectile.type == ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>() => 7f,
            _ when projectile.type == ModContent.ProjectileType<FourArmsFissureProjectile>() => 2.8f,
            _ => 6.5f
        };
        gain += Math.Min(8f, damageDone * 0.022f);
        state.AddRage(gain);
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        FourArmsGroundSlamPlayer state = omp.Player.GetModPlayer<FourArmsGroundSlamPlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        string rageText = compact
            ? $"Rage {(int)Math.Round(state.RageRatio * 100f)}%"
            : $"Rage {(int)Math.Round(state.RageRatio * 100f)}%";

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"{(state.FinisherReady ? "Finisher" : $"Hit {state.NextComboHit}")} • {rageText}"
                : $"{(state.FinisherReady ? "Next hit cleaves" : $"Combo {state.NextComboHit}/3")} • {rageText}",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? $"Tap down/F • {rageText}"
                : $"Double tap down or press F • {rageText}",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"Hold to charge • {rageText}"
                : $"Hold attack to charge • {rageText}",
            OmnitrixPlayer.AttackSelection.Ultimate => state.BerserkActive
                ? compact
                    ? $"Berserk {OmnitrixPlayer.FormatCooldownTicks(state.BerserkTicksRemaining)}"
                    : $"Berserk active • {OmnitrixPlayer.FormatCooldownTicks(state.BerserkTicksRemaining)} left"
                : state.HasFullRage
                    ? compact ? "Full Rage" : "Full Rage ready"
                    : compact ? "Need full Rage" : $"Needs full Rage • {rageText}",
            _ => rageText
        };
    }
    
    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => [
        new TransformationPaletteChannel(
            "skin",
            "Skin",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/FourArms/FourArms_Head",
                "Ben10Mod/Content/Transformations/FourArms/FourArmsSkinMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/FourArms/FourArms_Body",
                "Ben10Mod/Content/Transformations/FourArms/FourArmsSkinMask_Body"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/FourArms/FourArms_Legs",
                "Ben10Mod/Content/Transformations/FourArms/FourArmsSkinMask_Legs")),
        new TransformationPaletteChannel(
            "eye",
            "Eye",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/FourArms/FourArms_Head",
                "Ben10Mod/Content/Transformations/FourArms/FourArmsEyeMask_Head"))
    ];

    public override bool TryGetTransformationTint(Player player, OmnitrixPlayer omp, out Color tint,
        out float blendStrength, out bool forceFullBright) {
        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        tint = state.BerserkActive ? new Color(255, 115, 70) : new Color(255, 92, 62);
        blendStrength = state.BerserkActive ? 0.13f : state.RageRatio * 0.08f;
        forceFullBright = state.BerserkActive;
        return blendStrength > 0f;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<FourArms>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
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

    private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
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

    private bool IsFourArmsProjectile(int projectileType) {
        return projectileType == PrimaryAttack ||
               projectileType == SecondaryAttack ||
               projectileType == ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>() ||
               projectileType == ModContent.ProjectileType<FourArmsFissureProjectile>();
    }
}
