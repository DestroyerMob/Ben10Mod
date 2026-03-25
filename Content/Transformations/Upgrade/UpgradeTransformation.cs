using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Upgrade;

public enum UpgradeTechProfile {
    None = 0,
    Melee = 1,
    Ranged = 2,
    Magic = 3,
    Summon = 4
}

public enum UpgradeAttackVariant {
    Primary = 0,
    Special = 1,
    Construct = 2
}

public class UpgradeTransformation : Transformation {
    internal const int DefaultBadgeDamage = 28;
    private const int DefaultPrimaryUseTime = 22;
    private const float DefaultPrimaryShootSpeed = 18f;
    private const float ConstructDamageMultiplier = 0.82f;

    public override string FullID => "Ben10Mod:Upgrade";
    public override string TransformationName => "Upgrade";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Upgrade_Buff>();

    public override string Description =>
        "A Galvanic Mechamorph who scans a weapon signature, rewrites it into a cleaner combat mode, and then fights through upgraded melee, ranged, magic, or summon tech.";

    public override List<string> Abilities => new() {
        "Adaptive primary fire based on your synced weapon class",
        "Assimilate a held, remembered, or hotbar weapon signature",
        "Overclock your current sync",
        "Deploy a class-based recompiled construct",
        "Full Integration weapon matrix"
    };

    public override string PrimaryAttackName => "Adaptive Fire";
    public override string SecondaryAttackName => "Assimilate Weapon";
    public override string SecondaryAbilityAttackName => "Recompile Construct";

    public override int PrimaryAttack => ModContent.ProjectileType<UpgradeOpticRayProjectile>();
    public override int PrimaryAttackSpeed => DefaultPrimaryUseTime;
    public override int PrimaryShootSpeed => (int)DefaultPrimaryShootSpeed;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;

    public override int SecondaryAttack => ModContent.ProjectileType<UpgradeAssimilationPulseProjectile>();
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAttackModifier => 0f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAbilityCooldown => 24 * 60;
    public override int PrimaryAbilityCost => 20;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<UpgradeConstructProjectile>();
    public override int SecondaryAbilityAttackSpeed => 26;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 0.82f;
    public override int SecondaryAbilityAttackEnergyCost => 25;
    public override int SecondaryAbilityCooldown => 18 * 60;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityCost => 70;
    public override int UltimateAbilityDuration => 15 * 60;
    public override int UltimateAbilityCooldown => 65 * 60;

    public override string GetDescription(OmnitrixPlayer omp) {
        UpgradeTechPlayer techPlayer = omp.Player.GetModPlayer<UpgradeTechPlayer>();
        if (!techPlayer.HasSyncedWeapon)
            return $"{Description} Current sync: Optic fallback. Use Assimilate Weapon to scan a melee, ranged, magic, or summon weapon.";

        return $"{Description} Current sync: {GetTechProfileDisplayName(techPlayer.ActiveTechProfile)} from {techPlayer.GetSyncedWeaponName()} " +
               $"({techPlayer.SyncedWeaponDamage} base damage).";
    }

    public override List<string> GetAbilities(OmnitrixPlayer omp) {
        UpgradeTechPlayer techPlayer = omp.Player.GetModPlayer<UpgradeTechPlayer>();
        List<string> abilities = new(Abilities);
        abilities.Add($"Current sync: {GetCurrentPrimaryName(techPlayer)}");

        if (techPlayer.HasSyncedWeapon)
            abilities.Add($"Source weapon: {techPlayer.GetSyncedWeaponName()} ({GetTechProfileDisplayName(techPlayer.ActiveTechProfile)})");
        else
            abilities.Add("Source weapon: none");

        if (techPlayer.TryGetRememberedWeaponName(out string rememberedWeaponName))
            abilities.Add($"Remembered weapon: {rememberedWeaponName}");

        return abilities;
    }

    public override void OnTransform(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<UpgradeTechPlayer>().ResetSyncedWeapon();
    }

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<UpgradeTechPlayer>().ResetSyncedWeapon();
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        UpgradeTechPlayer techPlayer = player.GetModPlayer<UpgradeTechPlayer>();
        techPlayer.UpdateRememberedWeapon(player);

        UpgradeTechProfile activeProfile = techPlayer.ActiveTechProfile;
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.05f;
        player.GetCritChance<HeroDamage>() += 3f;
        player.GetArmorPenetration<HeroDamage>() += 4;
        player.ignoreWater = true;

        switch (activeProfile) {
            case UpgradeTechProfile.Melee:
                player.GetAttackSpeed<HeroDamage>() += 0.12f;
                player.statDefense += 6;
                player.GetKnockback<HeroDamage>() += 0.8f;
                break;
            case UpgradeTechProfile.Ranged:
                player.GetCritChance<HeroDamage>() += 7f;
                player.GetArmorPenetration<HeroDamage>() += 4;
                break;
            case UpgradeTechProfile.Magic:
                player.GetDamage<HeroDamage>() += 0.08f;
                player.GetArmorPenetration<HeroDamage>() += 6;
                break;
            case UpgradeTechProfile.Summon:
                player.moveSpeed += 0.12f;
                player.maxRunSpeed += 0.85f;
                player.endurance += 0.03f;
                break;
        }

        if (omp.PrimaryAbilityEnabled) {
            player.GetDamage<HeroDamage>() += 0.08f;
            player.GetAttackSpeed<HeroDamage>() += 0.08f;

            switch (activeProfile) {
                case UpgradeTechProfile.Melee:
                    player.statDefense += 8;
                    player.endurance += 0.05f;
                    break;
                case UpgradeTechProfile.Ranged:
                    player.GetCritChance<HeroDamage>() += 10f;
                    player.GetArmorPenetration<HeroDamage>() += 6;
                    break;
                case UpgradeTechProfile.Magic:
                    player.moveSpeed += 0.08f;
                    player.GetArmorPenetration<HeroDamage>() += 8;
                    break;
                case UpgradeTechProfile.Summon:
                    player.moveSpeed += 0.18f;
                    player.maxRunSpeed += 1.15f;
                    player.endurance += 0.04f;
                    break;
                default:
                    player.GetCritChance<HeroDamage>() += 4f;
                    break;
            }
        }

        if (omp.IsUltimateAbilityActive) {
            player.GetDamage<HeroDamage>() += 0.12f;
            player.endurance += 0.08f;
            player.GetAttackSpeed<HeroDamage>() += 0.1f;
            player.noFallDmg = true;
            player.armorEffectDrawShadow = true;

            switch (activeProfile) {
                case UpgradeTechProfile.Melee:
                    player.GetKnockback<HeroDamage>() += 1f;
                    player.statDefense += 6;
                    break;
                case UpgradeTechProfile.Ranged:
                    player.GetCritChance<HeroDamage>() += 8f;
                    break;
                case UpgradeTechProfile.Magic:
                    player.GetArmorPenetration<HeroDamage>() += 10;
                    break;
                case UpgradeTechProfile.Summon:
                    player.moveSpeed += 0.12f;
                    player.maxRunSpeed += 1.25f;
                    break;
            }
        }

        Color activeColor = GetTechColor(activeProfile);
        Lighting.AddLight(player.Center, activeColor.ToVector3() * (omp.IsUltimateAbilityActive ? 0.0048f : 0.0031f));

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(player.width * 0.35f, player.height * 0.45f),
                Main.rand.NextBool() ? DustID.Electric : DustID.GreenTorch,
                Main.rand.NextVector2Circular(0.8f, 0.8f), 95, activeColor, Main.rand.NextFloat(0.8f, 1.1f));
            dust.noGravity = true;
        }
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.IsUltimateAbilityActive || player.whoAmI != Main.myPlayer)
            return;

        UpgradeTechPlayer techPlayer = player.GetModPlayer<UpgradeTechPlayer>();
        int projectileType = ModContent.ProjectileType<UpgradeIntegrationMatrixProjectile>();
        int existingMatrix = FindOwnedProjectile(player.whoAmI, projectileType);
        int matrixDamage = Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(techPlayer.ResolveBadgeDamage()) * 0.72f));

        if (existingMatrix >= 0) {
            Projectile matrix = Main.projectile[existingMatrix];
            matrix.ai[0] = (float)techPlayer.ActiveTechProfile;
            matrix.damage = matrixDamage;
            matrix.originalDamage = matrixDamage;
            matrix.Center = player.MountedCenter;
            matrix.timeLeft = 2;
            matrix.netUpdate = true;
            return;
        }

        Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, Vector2.Zero,
            projectileType, matrixDamage, 2f, player.whoAmI, (float)techPlayer.ActiveTechProfile);
    }

    public override bool HasSecondaryAbilityAttackForState(OmnitrixPlayer omp) {
        return omp.Player.GetModPlayer<UpgradeTechPlayer>().HasSyncedWeapon;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        UpgradeTechPlayer techPlayer = omp.Player.GetModPlayer<UpgradeTechPlayer>();
        item.damage = techPlayer.ResolveBadgeDamage();
        item.knockBack = techPlayer.ResolveBadgeKnockback();
        item.crit = techPlayer.ResolveBadgeCrit();
    }

    protected override TransformationAttackProfile GetRawAttackProfile(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp) {
        UpgradeTechPlayer techPlayer = omp.Player.GetModPlayer<UpgradeTechPlayer>();
        return selection switch {
            OmnitrixPlayer.AttackSelection.Primary => BuildPrimaryAttackProfile(techPlayer, omp),
            OmnitrixPlayer.AttackSelection.Secondary => BuildAssimilationProfile(),
            OmnitrixPlayer.AttackSelection.SecondaryAbility => BuildConstructProfile(techPlayer, omp),
            _ => base.GetRawAttackProfile(selection, omp)
        };
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        UpgradeTechPlayer techPlayer = player.GetModPlayer<UpgradeTechPlayer>();

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (techPlayer.HasSyncedWeapon)
                DeployOrRefreshConstruct(player, omp, source, damage, knockback, techPlayer);

            return false;
        }

        if (omp.altAttack) {
            AttemptAssimilation(player, source, techPlayer);
            return false;
        }

        FireAdaptivePrimary(player, omp, source, direction, damage, knockback, techPlayer.ActiveTechProfile);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MeteorHelmet;
        player.body = ArmorIDs.Body.MeteorSuit;
        player.legs = ArmorIDs.Legs.MeteorLeggings;
    }

    private static TransformationAttackProfile BuildPrimaryAttackProfile(UpgradeTechPlayer techPlayer, OmnitrixPlayer omp) {
        UpgradeTechProfile profile = techPlayer.ActiveTechProfile;
        return new TransformationAttackProfile {
            DisplayName = GetCurrentPrimaryName(techPlayer),
            ProjectileType = ResolveAdaptiveProjectileType(profile),
            DamageMultiplier = 1f,
            UseTime = ResolvePrimaryUseTime(techPlayer, omp),
            ShootSpeed = ResolvePrimaryShootSpeed(techPlayer, profile, omp),
            UseStyle = ItemUseStyleID.Shoot,
            NoMelee = true
        };
    }

    private static TransformationAttackProfile BuildAssimilationProfile() {
        return new TransformationAttackProfile {
            DisplayName = "Assimilate Weapon",
            ProjectileType = ModContent.ProjectileType<UpgradeAssimilationPulseProjectile>(),
            DamageMultiplier = 0f,
            UseTime = 28,
            ShootSpeed = 0f,
            UseStyle = ItemUseStyleID.HoldUp,
            NoMelee = true
        };
    }

    private static TransformationAttackProfile BuildConstructProfile(UpgradeTechPlayer techPlayer, OmnitrixPlayer omp) {
        if (!techPlayer.HasSyncedWeapon)
            return null;

        return new TransformationAttackProfile {
            DisplayName = techPlayer.ActiveTechProfile switch {
                UpgradeTechProfile.Melee => "Blade Drone",
                UpgradeTechProfile.Ranged => "Pulse Turret",
                UpgradeTechProfile.Magic => "Arc Node",
                UpgradeTechProfile.Summon => "Drone Swarm",
                _ => "Recompile Construct"
            },
            ProjectileType = ModContent.ProjectileType<UpgradeConstructProjectile>(),
            DamageMultiplier = ConstructDamageMultiplier,
            UseTime = Math.Max(18, ResolvePrimaryUseTime(techPlayer, omp) + 4),
            ShootSpeed = 0f,
            UseStyle = ItemUseStyleID.HoldUp,
            NoMelee = true,
            EnergyCost = 25,
            SingleUse = true
        };
    }

    private static int ResolvePrimaryUseTime(UpgradeTechPlayer techPlayer, OmnitrixPlayer omp) {
        int baseUseTime = techPlayer.ActiveTechProfile switch {
            UpgradeTechProfile.Melee => NormalizeUseTime(techPlayer.SyncedWeaponUseTime, 12, 24, 0.82f),
            UpgradeTechProfile.Ranged => NormalizeUseTime(techPlayer.SyncedWeaponUseTime, 8, 20, 0.8f),
            UpgradeTechProfile.Magic => NormalizeUseTime(techPlayer.SyncedWeaponUseTime, 10, 22, 0.9f),
            UpgradeTechProfile.Summon => NormalizeUseTime(techPlayer.SyncedWeaponUseTime, 15, 26, 0.92f),
            _ => DefaultPrimaryUseTime
        };

        if (omp.PrimaryAbilityEnabled)
            baseUseTime = Math.Max(6, (int)Math.Round(baseUseTime * 0.88f));

        if (omp.IsUltimateAbilityActive)
            baseUseTime = Math.Max(6, (int)Math.Round(baseUseTime * 0.84f));

        return baseUseTime;
    }

    private static float ResolvePrimaryShootSpeed(UpgradeTechPlayer techPlayer, UpgradeTechProfile profile, OmnitrixPlayer omp) {
        float shootSpeed = profile switch {
            UpgradeTechProfile.Melee => 11f,
            UpgradeTechProfile.Ranged => Math.Max(16f, techPlayer.SyncedWeaponShootSpeed + 3f),
            UpgradeTechProfile.Magic => Math.Max(11f, techPlayer.SyncedWeaponShootSpeed + 1f),
            UpgradeTechProfile.Summon => Math.Max(13f, techPlayer.SyncedWeaponShootSpeed + 1.5f),
            _ => DefaultPrimaryShootSpeed
        };

        if (omp.PrimaryAbilityEnabled)
            shootSpeed += 1f;

        if (omp.IsUltimateAbilityActive)
            shootSpeed += 1.5f;

        return shootSpeed;
    }

    private static int NormalizeUseTime(int useTime, int min, int max, float multiplier) {
        if (useTime <= 0)
            return DefaultPrimaryUseTime;

        return Utils.Clamp((int)Math.Round(useTime * multiplier), min, max);
    }

    private static void AttemptAssimilation(Player player, IEntitySource source, UpgradeTechPlayer techPlayer) {
        if (!TryFindAssimilationWeapon(player, techPlayer, out Item weapon)) {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                ModContent.ProjectileType<UpgradeAssimilationPulseProjectile>(), 0, 0f, player.whoAmI,
                (float)UpgradeTechProfile.None, 0f);

            if (player.whoAmI == Main.myPlayer)
                Main.NewText("No compatible melee, ranged, magic, or summon weapon was found to assimilate.", new Color(225, 135, 135));

            return;
        }

        UpgradeTechProfile profile = ClassifyWeapon(weapon);
        techPlayer.SetSyncedWeapon(weapon, profile);
        Projectile.NewProjectile(source, player.Center, Vector2.Zero,
            ModContent.ProjectileType<UpgradeAssimilationPulseProjectile>(), 0, 0f, player.whoAmI,
            (float)profile, 1f);

        Color techColor = GetTechColor(profile);
        SoundEngine.PlaySound(SoundID.Item93 with { Pitch = -0.18f, Volume = 0.78f }, player.Center);
        SpawnAssimilationDust(player.Center, 12, techColor);

        if (player.whoAmI == Main.myPlayer) {
            Main.NewText($"Assimilated {weapon.Name}: {GetTechProfileDisplayName(profile)} online.", techColor);
            CombatText.NewText(player.getRect(), techColor, weapon.Name, dramatic: true);
        }
    }

    private static bool TryFindAssimilationWeapon(Player player, UpgradeTechPlayer techPlayer, out Item weapon) {
        weapon = null;

        if (IsCompatibleWeapon(player.HeldItem)) {
            weapon = player.HeldItem;
            return true;
        }

        if (techPlayer.TryGetRememberedWeapon(player, out Item rememberedWeapon)) {
            weapon = rememberedWeapon;
            return true;
        }

        if (TryGetFavoritedHotbarWeapon(player, out Item favoritedWeapon)) {
            weapon = favoritedWeapon;
            return true;
        }

        if (TryGetFirstHotbarWeapon(player, out Item hotbarWeapon)) {
            weapon = hotbarWeapon;
            return true;
        }

        return TryGetFirstInventoryWeapon(player, out weapon);
    }

    private static bool TryGetFavoritedHotbarWeapon(Player player, out Item weapon) {
        for (int i = 0; i < 10; i++) {
            Item candidate = player.inventory[i];
            if (candidate?.favorited == true && IsCompatibleWeapon(candidate)) {
                weapon = candidate;
                return true;
            }
        }

        weapon = null;
        return false;
    }

    private static bool TryGetFirstHotbarWeapon(Player player, out Item weapon) {
        for (int i = 0; i < 10; i++) {
            Item candidate = player.inventory[i];
            if (IsCompatibleWeapon(candidate)) {
                weapon = candidate;
                return true;
            }
        }

        weapon = null;
        return false;
    }

    private static bool TryGetFirstInventoryWeapon(Player player, out Item weapon) {
        for (int i = 10; i < player.inventory.Length; i++) {
            Item candidate = player.inventory[i];
            if (IsCompatibleWeapon(candidate)) {
                weapon = candidate;
                return true;
            }
        }

        weapon = null;
        return false;
    }

    internal static bool IsCompatibleWeapon(Item item) {
        if (item == null || item.IsAir || item.damage <= 0)
            return false;

        if (item.ModItem is Content.Items.Weapons.PlumbersBadge)
            return false;

        if (item.accessory || item.pick > 0 || item.axe > 0 || item.hammer > 0)
            return false;

        return ClassifyWeapon(item) != UpgradeTechProfile.None;
    }

    internal static UpgradeTechProfile ClassifyWeapon(Item item) {
        if (item == null || item.IsAir)
            return UpgradeTechProfile.None;

        if (item.CountsAsClass(DamageClass.Summon))
            return UpgradeTechProfile.Summon;

        if (item.CountsAsClass(DamageClass.Magic))
            return UpgradeTechProfile.Magic;

        if (item.CountsAsClass(DamageClass.Ranged))
            return UpgradeTechProfile.Ranged;

        if (item.CountsAsClass(DamageClass.Melee))
            return UpgradeTechProfile.Melee;

        return UpgradeTechProfile.None;
    }

    private static void FireAdaptivePrimary(Player player, OmnitrixPlayer omp, IEntitySource source, Vector2 direction, int damage,
        float knockback, UpgradeTechProfile profile) {
        int shotCount = profile switch {
            UpgradeTechProfile.Ranged when omp.PrimaryAbilityEnabled => 2,
            UpgradeTechProfile.Magic when omp.PrimaryAbilityEnabled => 2,
            UpgradeTechProfile.Summon when omp.PrimaryAbilityEnabled => 2,
            _ => 1
        };

        float spread = profile switch {
            UpgradeTechProfile.Ranged => shotCount > 1 ? 0.08f : 0f,
            UpgradeTechProfile.Magic => shotCount > 1 ? 0.06f : 0f,
            UpgradeTechProfile.Summon => shotCount > 1 ? 0.08f : 0f,
            _ => 0f
        };

        for (int i = 0; i < shotCount; i++) {
            float offset = shotCount == 1 ? 0f : MathHelper.Lerp(-spread, spread, i / (float)(shotCount - 1));
            Vector2 shotDirection = direction.RotatedBy(offset);
            FireAdaptiveShot(player, source, shotDirection, damage, knockback, profile, UpgradeAttackVariant.Primary,
                omp.PrimaryAbilityEnabled, omp.IsUltimateAbilityActive, player.MountedCenter);
        }

        SoundEngine.PlaySound(profile switch {
            UpgradeTechProfile.Melee => SoundID.Item71 with { Pitch = -0.2f, Volume = 0.85f },
            UpgradeTechProfile.Ranged => SoundID.Item11 with { Pitch = -0.08f, Volume = 0.72f },
            UpgradeTechProfile.Magic => SoundID.Item20 with { Pitch = -0.1f, Volume = 0.84f },
            UpgradeTechProfile.Summon => SoundID.Item91 with { Pitch = 0.06f, Volume = 0.8f },
            _ => SoundID.Item33 with { Pitch = -0.12f, Volume = 0.8f }
        }, player.Center);
    }

    internal static void FireAdaptiveShot(Player player, IEntitySource source, Vector2 direction, int damage, float knockback,
        UpgradeTechProfile profile, UpgradeAttackVariant variant, bool overclocked, bool fullyIntegrated, Vector2 spawnOrigin) {
        direction = direction.SafeNormalize(new Vector2(player.direction, 0f));
        float shotSpeed = profile switch {
            UpgradeTechProfile.Melee => variant switch {
                UpgradeAttackVariant.Construct => 10.5f,
                UpgradeAttackVariant.Special => 14.5f,
                _ => 12.5f
            },
            UpgradeTechProfile.Ranged => 18.5f + (variant == UpgradeAttackVariant.Construct ? -1.5f : variant == UpgradeAttackVariant.Special ? 1.5f : 0f),
            UpgradeTechProfile.Magic => variant == UpgradeAttackVariant.Special ? 15.5f : 13.5f,
            UpgradeTechProfile.Summon => variant == UpgradeAttackVariant.Special ? 17f : 15f,
            _ => variant == UpgradeAttackVariant.Special ? 20f : 18f
        };

        float damageMultiplier = profile switch {
            UpgradeTechProfile.Melee => variant switch {
                UpgradeAttackVariant.Construct => 0.72f,
                UpgradeAttackVariant.Special => 1.34f,
                _ => 1.08f
            },
            UpgradeTechProfile.Ranged => variant switch {
                UpgradeAttackVariant.Construct => 0.84f,
                UpgradeAttackVariant.Special => 1.18f,
                _ => 1f
            },
            UpgradeTechProfile.Magic => variant switch {
                UpgradeAttackVariant.Construct => 0.76f,
                UpgradeAttackVariant.Special => 1.16f,
                _ => 0.96f
            },
            UpgradeTechProfile.Summon => variant switch {
                UpgradeAttackVariant.Construct => 0.82f,
                UpgradeAttackVariant.Special => 1.02f,
                _ => 0.9f
            },
            _ => variant switch {
                UpgradeAttackVariant.Construct => 0.84f,
                UpgradeAttackVariant.Special => 1.12f,
                _ => 1f
            }
        };

        if (variant == UpgradeAttackVariant.Primary && profile == UpgradeTechProfile.None)
            damageMultiplier = 1f;

        int finalDamage = Math.Max(1, (int)Math.Round(damage * damageMultiplier));
        Vector2 spawnCenter = spawnOrigin + direction * (profile == UpgradeTechProfile.Melee ? 14f : 18f);
        Projectile.NewProjectile(source, spawnCenter, direction * shotSpeed,
            ResolveAdaptiveProjectileType(profile), finalDamage, knockback, player.whoAmI,
            (float)profile, PackOpticFlags(overclocked, fullyIntegrated, variant));
    }

    private static void DeployOrRefreshConstruct(Player player, OmnitrixPlayer omp, IEntitySource source, int damage,
        float knockback, UpgradeTechPlayer techPlayer) {
        UpgradeTechProfile constructProfile = techPlayer.ActiveTechProfile;
        int projectileType = ModContent.ProjectileType<UpgradeConstructProjectile>();
        int constructTier = techPlayer.ResolveConstructTier();
        int finalDamage = Math.Max(1, (int)Math.Round(damage * ConstructDamageMultiplier * (1f + constructTier * 0.08f)));
        int existingConstruct = FindOwnedProjectile(player.whoAmI, projectileType);

        if (existingConstruct >= 0) {
            Projectile existing = Main.projectile[existingConstruct];
            existing.damage = finalDamage;
            existing.originalDamage = finalDamage;
            existing.knockBack = knockback + 0.75f;
            existing.ai[0] = constructTier;
            existing.ai[1] = (float)constructProfile;
            existing.timeLeft = UpgradeConstructProjectile.BaseLifetimeTicks;
            existing.Center = player.Center + new Vector2(player.direction * 28f, -34f);
            existing.netUpdate = true;
        }
        else {
            Projectile.NewProjectile(source, player.Center + new Vector2(player.direction * 28f, -34f), Vector2.Zero,
                projectileType, finalDamage, knockback + 0.75f, player.whoAmI, constructTier, (float)constructProfile);
        }

        if (player.whoAmI == Main.myPlayer)
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.16f, Volume = 0.84f }, player.Center);

        SpawnAssimilationDust(player.Center, 10, GetTechColor(constructProfile));
    }

    internal static int FindOwnedProjectile(int owner, int type) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == owner && projectile.type == type)
                return i;
        }

        return -1;
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

    private static int ResolveAdaptiveProjectileType(UpgradeTechProfile profile) {
        return profile switch {
            UpgradeTechProfile.Melee => ModContent.ProjectileType<UpgradeBladeWaveProjectile>(),
            UpgradeTechProfile.Ranged => ModContent.ProjectileType<UpgradePulseRoundProjectile>(),
            UpgradeTechProfile.Magic => ModContent.ProjectileType<UpgradeMagicOrbProjectile>(),
            UpgradeTechProfile.Summon => ModContent.ProjectileType<UpgradeDirectiveSpikeProjectile>(),
            _ => ModContent.ProjectileType<UpgradeOpticRayProjectile>()
        };
    }

    internal static string GetCurrentPrimaryName(UpgradeTechPlayer techPlayer) {
        return techPlayer.ActiveTechProfile switch {
            UpgradeTechProfile.Melee => "Edge Lash",
            UpgradeTechProfile.Ranged => "Pulse Cannon",
            UpgradeTechProfile.Magic => "Arc Projector",
            UpgradeTechProfile.Summon => "Directive Bolt",
            _ => "Optic Ray"
        };
    }

    internal static void SpawnAssimilationDust(Vector2 center, int count, Color color) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < count; i++) {
            Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(10f, 10f),
                i % 2 == 0 ? DustID.Electric : DustID.GreenTorch,
                Main.rand.NextVector2Circular(2f, 2f), 95, color, Main.rand.NextFloat(0.9f, 1.2f));
            dust.noGravity = true;
        }
    }

    internal static Color GetTechColor(UpgradeTechProfile profile) {
        return profile switch {
            UpgradeTechProfile.Melee => new Color(165, 255, 120),
            UpgradeTechProfile.Ranged => new Color(90, 235, 255),
            UpgradeTechProfile.Magic => new Color(130, 255, 215),
            UpgradeTechProfile.Summon => new Color(180, 255, 155),
            _ => new Color(105, 255, 185)
        };
    }

    internal static string GetTechProfileDisplayName(UpgradeTechProfile profile) {
        return profile switch {
            UpgradeTechProfile.Melee => "Melee Sync",
            UpgradeTechProfile.Ranged => "Ranged Sync",
            UpgradeTechProfile.Magic => "Magic Sync",
            UpgradeTechProfile.Summon => "Summon Sync",
            _ => "Optic Fallback"
        };
    }

    internal static int PackOpticFlags(bool overclocked, bool fullyIntegrated, UpgradeAttackVariant variant) {
        int flags = 0;
        if (overclocked)
            flags |= 1;
        if (fullyIntegrated)
            flags |= 2;

        flags |= ((int)variant & 0x3) << 2;
        return flags;
    }
}

public class UpgradeTechPlayer : ModPlayer {
    public UpgradeTechProfile ActiveTechProfile { get; private set; }
    public int SyncedWeaponItemType { get; private set; }
    public int SyncedWeaponDamage { get; private set; }
    public int SyncedWeaponUseTime { get; private set; }
    public float SyncedWeaponKnockback { get; private set; }
    public int SyncedWeaponCrit { get; private set; }
    public float SyncedWeaponShootSpeed { get; private set; }
    public int RememberedWeaponSlot { get; private set; }
    public int RememberedWeaponItemType { get; private set; }

    public bool HasSyncedWeapon => ActiveTechProfile != UpgradeTechProfile.None && SyncedWeaponItemType > ItemID.None;

    public override void Initialize() {
        ResetSyncedWeapon();
        RememberedWeaponSlot = -1;
        RememberedWeaponItemType = ItemID.None;
    }

    public void ResetSyncedWeapon() {
        ActiveTechProfile = UpgradeTechProfile.None;
        SyncedWeaponItemType = ItemID.None;
        SyncedWeaponDamage = 0;
        SyncedWeaponUseTime = 0;
        SyncedWeaponKnockback = 0f;
        SyncedWeaponCrit = 0;
        SyncedWeaponShootSpeed = 0f;
    }

    public void SetSyncedWeapon(Item item, UpgradeTechProfile profile) {
        if (item == null || item.IsAir || profile == UpgradeTechProfile.None) {
            ResetSyncedWeapon();
            return;
        }

        ActiveTechProfile = profile;
        SyncedWeaponItemType = item.type;
        SyncedWeaponDamage = Math.Max(1, item.damage);
        SyncedWeaponUseTime = item.useAnimation > 0 ? item.useAnimation : item.useTime;
        SyncedWeaponKnockback = Math.Max(1f, item.knockBack);
        SyncedWeaponCrit = Math.Max(0, item.crit);
        SyncedWeaponShootSpeed = item.shootSpeed > 0f ? item.shootSpeed : 10f;
    }

    public void UpdateRememberedWeapon(Player player) {
        if (player == null)
            return;

        if (!UpgradeTransformation.IsCompatibleWeapon(player.HeldItem))
            return;

        RememberedWeaponSlot = player.selectedItem;
        RememberedWeaponItemType = player.HeldItem.type;
    }

    public bool TryGetRememberedWeapon(Player player, out Item weapon) {
        weapon = null;
        if (player == null || RememberedWeaponSlot < 0 || RememberedWeaponSlot >= player.inventory.Length)
            return false;

        Item candidate = player.inventory[RememberedWeaponSlot];
        if (!UpgradeTransformation.IsCompatibleWeapon(candidate))
            return false;

        if (RememberedWeaponItemType > ItemID.None && candidate.type != RememberedWeaponItemType)
            return false;

        weapon = candidate;
        return true;
    }

    public bool TryGetRememberedWeaponName(out string weaponName) {
        if (RememberedWeaponItemType > ItemID.None) {
            weaponName = Lang.GetItemNameValue(RememberedWeaponItemType);
            return !string.IsNullOrWhiteSpace(weaponName);
        }

        weaponName = string.Empty;
        return false;
    }

    public string GetSyncedWeaponName() {
        if (SyncedWeaponItemType <= ItemID.None)
            return "None";

        return Lang.GetItemNameValue(SyncedWeaponItemType);
    }

    public int ResolveBadgeDamage() {
        if (!HasSyncedWeapon)
            return UpgradeTransformation.DefaultBadgeDamage;

        float multiplier = ActiveTechProfile switch {
            UpgradeTechProfile.Melee => 1.12f,
            UpgradeTechProfile.Ranged => 1.08f,
            UpgradeTechProfile.Magic => 1.06f,
            UpgradeTechProfile.Summon => 1.03f,
            _ => 1f
        };
        return Math.Max(UpgradeTransformation.DefaultBadgeDamage, (int)Math.Round(SyncedWeaponDamage * multiplier));
    }

    public float ResolveBadgeKnockback() {
        if (!HasSyncedWeapon)
            return 4.5f;

        float bonus = ActiveTechProfile == UpgradeTechProfile.Melee ? 1.2f : 0.5f;
        return SyncedWeaponKnockback + bonus;
    }

    public int ResolveBadgeCrit() {
        if (!HasSyncedWeapon)
            return 0;

        int bonus = ActiveTechProfile == UpgradeTechProfile.Ranged ? 4 : ActiveTechProfile == UpgradeTechProfile.Magic ? 2 : 0;
        return SyncedWeaponCrit + bonus;
    }

    public int ResolveConstructTier() {
        int sourceDamage = HasSyncedWeapon ? SyncedWeaponDamage : UpgradeTransformation.DefaultBadgeDamage;
        return Utils.Clamp((sourceDamage - 20) / 18, 0, 4);
    }
}
