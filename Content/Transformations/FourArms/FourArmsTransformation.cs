using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
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
    public const int PotisGroundSlamCooldownTicks = 4 * 60;

    private const int GroundSlamCooldownTicks = 5 * 60;
    private const int HaymakerCooldownTicks = 8 * 60;
    private const int PotisHaymakerCooldownTicks = 7 * 60;
    private const int BerserkCooldownTicks = 38 * 60;
    private const int PotisBerserkCooldownTicks = 34 * 60;
    private const float PrimaryDamageMultiplier = 1.04f;
    private const float FinisherDamageMultiplier = 1.28f;
    private const float SecondaryDamageMultiplier = 1.16f;
    private const float HaymakerDamageMultiplier = 1.7f;
    private const float PotisPrimaryDamageMultiplier = 0.98f;
    private const float PotisFinisherDamageMultiplier = 1.2f;
    private const float PotisSecondaryDamageMultiplier = 1.08f;
    private const float PotisHaymakerDamageMultiplier = 1.56f;
    private const float PotisFinisherFissureDamageMultiplier = 0.42f;
    private const int PotisPrimaryAttackSpeed = 10;
    private const int PotisPrimaryShootSpeed = 13;
    private const int PotisSecondaryAttackSpeed = 19;
    private const int PotisSecondaryShootSpeed = 17;
    private const int PotisSecondaryEnergyCost = 3;
    private const int PotisHaymakerAttackSpeed = 16;
    private const float FullRageRange = 145f;
    private const float MinimumRageRange = 430f;
    private const float BaseMeleeDamageBonus = 0.18f;
    private const float BaseMeleeAttackSpeedBonus = 0.1f;
    private const float RageMeleeAttackSpeedBonus = 0.08f;
    private const float TempoHeroAttackSpeedBonus = 0.07f;
    private const float PotisTempoHeroAttackSpeedBonus = 0.09f;
    private const float TempoMeleeAttackSpeedBonus = 0.045f;
    private const float PotisTempoMeleeAttackSpeedBonus = 0.06f;
    private const float BaseMeleeKnockbackBonus = 1.1f;
    private const int BaseMeleeArmorPenBonus = 10;
    private const int BaseMeleeCritBonus = 8;
    private const float BerserkMeleeDamageBonus = 0.14f;
    private const float BerserkMeleeAttackSpeedBonus = 0.16f;
    private const float BerserkMeleeKnockbackBonus = 0.55f;
    private const int BerserkMeleeArmorPenBonus = 6;
    private const int BerserkMeleeCritBonus = 6;

    public override string FullID => "Ben10Mod:FourArms";
    public override string TransformationName => "Fourarms";
    public override string IconPath => "Ben10Mod/Content/Interface/FourArmsSelect";
    public override int TransformationBuffId => ModContent.BuffType<FourArms_Buff>();

    public override string Description =>
        "A Tetramand combo bruiser who thrives in brawling range with punch strings, slams, shock claps, and Rage-fuelled Berserker mode.";

    public override List<string> Abilities => new() {
        "Titan Combo chains fast punches, then widens into a cleaving third hit.",
        "Shock Clap sends a short-range shockwave through crowds.",
        "Ground Slam triggers from the ability hotkey or a double tap down input.",
        "Haymaker is a charged super-armored punch for big single-target damage.",
        "Rage and Brawler Tempo build fastest from close-range hits, speeding up punch strings and melee swings.",
        "Berserker mode activates once Rage reaches 90%, giving Four Arms faster, larger combos and fissure slams."
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

    public override int GetMoveSetIndex(OmnitrixPlayer omp) => HasPotisAltiare(omp?.Player) ? 1 : 0;

    public override string GetDescription(OmnitrixPlayer omp) {
        if (!HasPotisAltiare(omp?.Player))
            return Description;

        return $"{Description} Potis Altiare turns Four Arms into a siege brawler with faultline follow-ups, wider shock claps, stronger slam armor, and safer boss trading.";
    }

    public override List<string> GetAbilities(OmnitrixPlayer omp) {
        if (!HasPotisAltiare(omp?.Player))
            return base.GetAbilities(omp);

        return new List<string> {
            "Quake Combo uses faster Potis-infused punches and launches a faultline on the cleaving hit.",
            "Faultline Clap reaches farther, hits wider, and cracks the ground on impact.",
            "Meteor Slam has a shorter cooldown, stronger guard, and always sends fissures across the floor.",
            "Colossus Haymaker charges faster and releases a seismic follow-through.",
            "Potis armor improves defense and lets Four Arms stay close to bosses longer.",
            "Titan Overdrive keeps Berserker mode active with stronger Potis hits."
        };
    }

    public override string GetAbilitySelectionDisplayName(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp) {
        if (!HasPotisAltiare(omp?.Player))
            return base.GetAbilitySelectionDisplayName(selection, omp);

        return selection switch {
            OmnitrixPlayer.AttackSelection.PrimaryAbility => "Meteor Slam",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => "Colossus Haymaker",
            OmnitrixPlayer.AttackSelection.Ultimate => "Titan Overdrive",
            _ => base.GetAbilitySelectionDisplayName(selection, omp)
        };
    }

    public override int GetPrimaryAbilityCooldown(OmnitrixPlayer omp) {
        int cooldown = HasPotisAltiare(omp?.Player) ? PotisGroundSlamCooldownTicks : GroundSlamCooldownTicks;
        return ApplyAbilityCooldownMultiplier(cooldown, omp.primaryAbilityCooldownMultiplier);
    }

    public override int GetSecondaryAbilityCooldown(OmnitrixPlayer omp) {
        int cooldown = HasPotisAltiare(omp?.Player) ? PotisHaymakerCooldownTicks : HaymakerCooldownTicks;
        return ApplyAbilityCooldownMultiplier(cooldown, omp.secondaryAbilityCooldownMultiplier);
    }

    public override int GetUltimateAbilityCooldown(OmnitrixPlayer omp) {
        int cooldown = HasPotisAltiare(omp?.Player) ? PotisBerserkCooldownTicks : BerserkCooldownTicks;
        return ApplyAbilityCooldownMultiplier(cooldown, omp.ultimateAbilityCooldownMultiplier);
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetPrimaryAttackProfiles() {
        return CreateMoveSetProfiles(
            CreatePrimaryAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = "Quake Combo",
                ProjectileType = PrimaryAttack,
                DamageMultiplier = PotisPrimaryDamageMultiplier,
                UseTime = PotisPrimaryAttackSpeed,
                ShootSpeed = PotisPrimaryShootSpeed,
                UseStyle = PrimaryUseStyle,
                Channel = false,
                NoMelee = true,
                ArmorPenetration = PrimaryArmorPenetration + 4
            });
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetSecondaryAttackProfiles() {
        return CreateMoveSetProfiles(
            CreateSecondaryAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = "Faultline Clap",
                ProjectileType = SecondaryAttack,
                DamageMultiplier = PotisSecondaryDamageMultiplier,
                UseTime = PotisSecondaryAttackSpeed,
                ShootSpeed = PotisSecondaryShootSpeed,
                UseStyle = SecondaryUseStyle,
                Channel = false,
                NoMelee = true,
                ArmorPenetration = SecondaryArmorPenetration + 6,
                EnergyCost = PotisSecondaryEnergyCost
            });
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetSecondaryAbilityAttackProfiles() {
        return CreateMoveSetProfiles(
            CreateSecondaryAbilityAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = "Colossus Haymaker",
                ProjectileType = SecondaryAbilityAttack,
                DamageMultiplier = PotisHaymakerDamageMultiplier,
                UseTime = PotisHaymakerAttackSpeed,
                ShootSpeed = 0f,
                UseStyle = SecondaryAbilityAttackUseStyle,
                Channel = true,
                NoMelee = true,
                ArmorPenetration = SecondaryAbilityAttackArmorPenetration + 8,
                EnergyCost = SecondaryAbilityAttackEnergyCost
            });
    }

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
        float tempoRatio = state.BrawlerTempoRatio;
        bool potis = HasPotisAltiare(player);

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetAttackSpeed<HeroDamage>() += 0.04f + rageRatio * 0.12f +
                                               tempoRatio * (potis ? PotisTempoHeroAttackSpeedBonus : TempoHeroAttackSpeedBonus);
        player.GetKnockback<HeroDamage>() += 0.65f;
        player.GetArmorPenetration<HeroDamage>() += 10;
        player.statDefense += 13;
        player.noFallDmg = true;
        player.jumpSpeedBoost += 1.2f;
        player.runAcceleration *= 0.74f;
        player.maxRunSpeed *= 0.92f;
        player.pickSpeed *= 0.88f;
        player.tileSpeed *= 0.9f;
        player.wallSpeed *= 0.9f;
        player.GetDamage(DamageClass.Melee) += BaseMeleeDamageBonus;
        player.GetAttackSpeed(DamageClass.Melee) += BaseMeleeAttackSpeedBonus + rageRatio * RageMeleeAttackSpeedBonus +
                                                    tempoRatio * (potis ? PotisTempoMeleeAttackSpeedBonus : TempoMeleeAttackSpeedBonus);
        player.GetKnockback(DamageClass.Melee) += BaseMeleeKnockbackBonus;
        player.GetArmorPenetration(DamageClass.Melee) += BaseMeleeArmorPenBonus;
        player.GetCritChance(DamageClass.Melee) += BaseMeleeCritBonus;

        if (potis) {
            player.GetDamage<HeroDamage>() += 0.06f;
            player.GetAttackSpeed<HeroDamage>() += 0.04f;
            player.GetKnockback<HeroDamage>() += 0.25f;
            player.GetArmorPenetration<HeroDamage>() += 4;
            player.statDefense += 5;
            player.endurance += 0.03f;
            player.runAcceleration *= 0.96f;
            player.maxRunSpeed *= 0.98f;
            Lighting.AddLight(player.Center, new Vector3(0.86f, 0.38f, 0.12f) * 0.24f);
        }

        if (state.BrawlerGuardActive) {
            float guardStrength = state.BrawlerGuardStrength;
            player.statDefense += 6 + (int)Math.Round(guardStrength * 48f);
            player.endurance += 0.04f + guardStrength * 0.18f;
            player.noKnockback = true;
        }

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
        player.GetDamage(DamageClass.Melee) += BerserkMeleeDamageBonus;
        player.GetAttackSpeed(DamageClass.Melee) += BerserkMeleeAttackSpeedBonus;
        player.GetKnockback(DamageClass.Melee) += BerserkMeleeKnockbackBonus;
        player.GetArmorPenetration(DamageClass.Melee) += BerserkMeleeArmorPenBonus;
        player.GetCritChance(DamageClass.Melee) += BerserkMeleeCritBonus;

        if (potis) {
            player.GetDamage<HeroDamage>() += 0.08f;
            player.GetAttackSpeed<HeroDamage>() += 0.06f;
            player.endurance += 0.04f;
        }
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        FourArmsGroundSlamPlayer state = omp.Player.GetModPlayer<FourArmsGroundSlamPlayer>();
        if (omp.setAttack != OmnitrixPlayer.AttackSelection.Primary)
            return;

        bool potis = HasPotisAltiare(omp.Player);
        float comboSpeedMultiplier = 1f - state.RageRatio * (potis ? 0.1f : 0.08f) - (state.BerserkActive ? 0.14f : 0f) -
                                     (potis ? 0.06f : 0f) - state.BrawlerTempoRatio * (potis ? 0.08f : 0.06f);
        item.useTime = item.useAnimation = Math.Max(potis ? 6 : 7, (int)Math.Round(item.useTime * comboSpeedMultiplier));
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
        if (!state.HasBerserkThreshold)
            return true;

        state.ConsumeAllRage();
        int duration = GetUltimateAbilityDuration(omp);
        player.AddBuff(ModContent.BuffType<UltimateAbility>(), duration);
        omp.ultimateAbilityTransformationId = FullID;

        bool potis = HasPotisAltiare(player);
        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = potis ? -0.26f : -0.18f, Volume = potis ? 0.86f : 0.75f },
                player.Center);
            int dustCount = potis ? 34 : 24;
            for (int i = 0; i < dustCount; i++) {
                Vector2 velocity = Main.rand.NextVector2Circular(potis ? 5.6f : 4.6f, potis ? 5.6f : 4.6f);
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(22f, 34f),
                    potis && i % 3 == 0 ? DustID.WhiteTorch : DustID.Torch,
                    velocity, 80, potis ? new Color(255, 198, 115) : new Color(255, 145, 90),
                    Main.rand.NextFloat(potis ? 1.25f : 1.15f, potis ? 1.9f : 1.65f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    public override void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) {
        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        modifiers.Knockback *= 0.28f;

        if (state.BrawlerGuardActive) {
            modifiers.FinalDamage *= 1f - state.BrawlerGuardStrength;
            modifiers.Knockback *= 0.45f;
        }

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
        bool potis = HasPotisAltiare(player);

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (HasActiveOwnedProjectile(player, SecondaryAbilityAttack))
                return false;

            float haymakerMultiplier = potis ? PotisHaymakerDamageMultiplier : SecondaryAbilityAttackModifier;
            int haymakerDamage = ScaleDamage(damage, haymakerMultiplier * (berserk ? 1.15f : 1f));
            state.RegisterBrawlerGuard(potis ? 24 : 18, berserk ? potis ? 0.22f : 0.16f : potis ? 0.17f : 0.12f);
            Projectile.NewProjectile(source, player.Center, direction, SecondaryAbilityAttack, haymakerDamage,
                knockback + (potis ? 3.1f : 2.4f), player.whoAmI, berserk ? 1f : 0f, potis ? 1f : 0f);
            return false;
        }

        if (omp.altAttack) {
            float clapMultiplier = potis ? PotisSecondaryDamageMultiplier : SecondaryAttackModifier;
            float clapSpeed = potis ? PotisSecondaryShootSpeed : SecondaryShootSpeed;
            int clapDamage = ScaleDamage(damage, clapMultiplier * (berserk ? 1.08f : 1f));
            state.RegisterBrawlerGuard(potis ? 24 : 18, berserk ? potis ? 0.18f : 0.13f : potis ? 0.14f : 0.1f);
            Projectile.NewProjectile(source, spawnPosition, direction * clapSpeed, SecondaryAttack, clapDamage,
                knockback + (potis ? 2.6f : 2f), player.whoAmI, clapSpeed, potis ? 1f : 0f);
            return false;
        }

        int comboStep = state.ConsumeComboStep();
        bool finisher = comboStep >= 2;
        float punchScale = 1.18f + state.RageRatio * 0.12f + (berserk ? 0.2f : 0f) + (potis ? 0.12f : 0f);
        int comboVariantOffset = potis ? 10 : 0;
        float primaryShootSpeed = potis ? PotisPrimaryShootSpeed : PrimaryShootSpeed;

        if (finisher) {
            float[] spread = { -0.18f, 0f, 0.18f };
            float finisherMultiplier = potis ? PotisFinisherDamageMultiplier : FinisherDamageMultiplier;
            int finisherDamage = ScaleDamage(damage, finisherMultiplier * (berserk ? 1.12f : 1f));
            state.RegisterBrawlerGuard(potis ? 34 : 24, berserk ? potis ? 0.24f : 0.18f : potis ? 0.19f : 0.14f);
            for (int i = 0; i < spread.Length; i++) {
                Vector2 punchDirection = direction.RotatedBy(spread[i]).SafeNormalize(direction);
                Vector2 finisherSpawn = player.MountedCenter + punchDirection * 20f;
                Projectile.NewProjectile(source, finisherSpawn, punchDirection * Math.Max(primaryShootSpeed, 10),
                    PrimaryAttack, finisherDamage, knockback + (potis ? 3.8f : 3.1f), player.whoAmI, punchScale + 0.16f,
                    2f + comboVariantOffset);
            }

            if (potis) {
                int fissureDamage = ScaleDamage(finisherDamage, PotisFinisherFissureDamageMultiplier);
                Projectile.NewProjectile(source, player.Bottom + new Vector2(direction.X * 8f, -10f), Vector2.Zero,
                    ModContent.ProjectileType<FourArmsFissureProjectile>(), fissureDamage, knockback + 1.6f,
                    player.whoAmI, Math.Sign(direction.X == 0f ? player.direction : direction.X), 1f);
            }

            return false;
        }

        float punchMultiplier = potis ? PotisPrimaryDamageMultiplier : PrimaryAttackModifier;
        int punchDamage = ScaleDamage(damage, punchMultiplier * (berserk ? 1.08f : 1f));
        state.RegisterBrawlerGuard(potis ? 20 : 15,
            0.09f + state.RageRatio * (potis ? 0.05f : 0.04f) + (berserk ? 0.03f : 0f) + (potis ? 0.03f : 0f));
        Projectile.NewProjectile(source, spawnPosition, direction * Math.Max(primaryShootSpeed, 10), PrimaryAttack,
            punchDamage, knockback + comboStep * 0.35f + (potis ? 0.35f : 0f), player.whoAmI, punchScale,
            comboStep + comboVariantOffset);
        return false;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsFourArmsProjectile(projectile.type))
            return;

        FourArmsGroundSlamPlayer state = player.GetModPlayer<FourArmsGroundSlamPlayer>();
        bool heavyHit = projectile.type == PrimaryAttack && projectile.ai[1] >= 2f ||
                        projectile.type == ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>();
        float closePressure = ResolveClosePressure(player, target, projectile);
        float rangeMultiplier = ResolveRageRangeMultiplier(projectile.type, closePressure);
        float gain = projectile.type switch {
            _ when projectile.type == PrimaryAttack => 5.5f,
            _ when projectile.type == SecondaryAttack => 4f,
            _ when projectile.type == ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>() => 7f,
            _ when projectile.type == ModContent.ProjectileType<FourArmsFissureProjectile>() => 2.8f,
            _ => 6.5f
        };
        gain += Math.Min(8f, damageDone * 0.022f);
        gain *= rangeMultiplier;
        if (projectile.type == PrimaryAttack && closePressure >= 0.68f)
            gain += heavyHit ? 2.1f : 1.2f;

        state.AddRage(gain);

        if (projectile.type == PrimaryAttack || projectile.type == SecondaryAttack || heavyHit)
            state.RegisterClosePressure(closePressure, heavyHit);

        state.RegisterBrawlerImpact(target, heavyHit);
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        FourArmsGroundSlamPlayer state = omp.Player.GetModPlayer<FourArmsGroundSlamPlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        string rageText = compact
            ? $"Rage {(int)Math.Round(state.RageRatio * 100f)}%"
            : $"Rage {(int)Math.Round(state.RageRatio * 100f)}%";
        string tempoText = state.HasBrawlerTempo
            ? compact ? $"Tempo {state.BrawlerTempoStacks}" : $"Tempo {state.BrawlerTempoStacks}/3"
            : string.Empty;
        bool potis = HasPotisAltiare(omp.Player);
        string berserkName = potis ? "Overdrive" : "Berserk";
        string WithTempo(string text) => string.IsNullOrWhiteSpace(tempoText) ? text : $"{text} • {tempoText}";

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => WithTempo(compact
                ? $"{(state.FinisherReady ? potis ? "Faultline" : "Finisher" : $"Hit {state.NextComboHit}")} • {rageText}"
                : $"{(state.FinisherReady ? potis ? "Next hit cracks ground" : "Next hit cleaves" : $"Combo {state.NextComboHit}/3")} • {rageText}"),
            OmnitrixPlayer.AttackSelection.Secondary => potis
                ? WithTempo(compact ? $"3 OE • {rageText}" : $"Ground crack • 3 OE • {rageText}")
                : WithTempo(rageText),
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? $"Tap down/F • {rageText}"
                : $"{(potis ? "Meteor slam" : "Double tap down or press F")} • {rageText}",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"Hold to charge • {rageText}"
                : $"{(potis ? "Faster charge + quake" : "Hold attack to charge")} • {rageText}",
            OmnitrixPlayer.AttackSelection.Ultimate => state.BerserkActive
                ? compact
                    ? $"{berserkName} {OmnitrixPlayer.FormatCooldownTicks(state.BerserkTicksRemaining)}"
                    : $"{berserkName} active • {OmnitrixPlayer.FormatCooldownTicks(state.BerserkTicksRemaining)} left"
                : state.HasBerserkThreshold
                    ? compact ? "Rage Ready" : "90% Rage ready"
                    : compact ? "Need 90% Rage" : $"Needs 90% Rage • {rageText}",
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
        bool potis = HasPotisAltiare(player);
        tint = potis
            ? state.BerserkActive ? new Color(255, 170, 76) : new Color(255, 132, 54)
            : state.BerserkActive ? new Color(255, 115, 70) : new Color(255, 92, 62);
        blendStrength = state.BerserkActive ? potis ? 0.18f : 0.13f : state.RageRatio * (potis ? 0.11f : 0.08f);
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

    private static float ResolveClosePressure(Player player, NPC target, Projectile projectile) {
        if (player == null || target == null)
            return 0f;

        float targetSizeAllowance = Math.Max(target.width, target.height) * 0.35f;
        float distance = Math.Max(0f, Vector2.Distance(player.Center, target.Center) - targetSizeAllowance);
        float pressure = 1f - MathHelper.Clamp((distance - FullRageRange) / (MinimumRageRange - FullRageRange), 0f, 1f);

        if (projectile.type == ModContent.ProjectileType<FourArmsFissureProjectile>())
            pressure *= 0.62f;
        else if (projectile.type == ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>())
            pressure *= 0.78f;

        return MathHelper.Clamp(pressure, 0f, 1f);
    }

    private static float ResolveRageRangeMultiplier(int projectileType, float closePressure) {
        if (projectileType == ModContent.ProjectileType<FourArmsFissureProjectile>())
            return MathHelper.Lerp(0.1f, 0.68f, closePressure);

        if (projectileType == ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>())
            return MathHelper.Lerp(0.24f, 1f, closePressure);

        if (projectileType == ModContent.ProjectileType<FourArmsClap>())
            return MathHelper.Lerp(0.24f, 0.95f, closePressure);

        return MathHelper.Lerp(0.28f, 1.18f, closePressure);
    }

    private static bool HasPotisAltiare(Player player) {
        return player?.GetModPlayer<PotisAltiarePlayer>()?.potisAltiareEquipped == true;
    }
}
