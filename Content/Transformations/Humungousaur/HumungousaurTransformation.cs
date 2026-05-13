using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Humungousaur;

public class HumungousaurTransformation : Transformation {
    public const string TransformationId = "Ben10Mod:Humungousaur";
    public const string UltimateTransformationId = "Ben10Mod:UltimateHumungousaur";
    public const float GrownScale = 1.65f;
    public const float RampageScale = 1.85f;
    public const float UltimateGrownScale = 1f + (GrownScale - 1f) * 2f;
    public const int GrowthRampDuration = 3 * 60;
    public const int UltimateAbilityDurationTicks = 12 * 60;
    public const int UltimateAbilityCooldownTicks = 70 * 60;
    private const float PrimaryDamageMultiplier = 1.12f;
    private const float SecondaryDamageMultiplier = 1.16f;
    private const float RampageShockwaveSpeed = 15f;
    private const int RampagePulseInterval = 3;

    public override string FullID => TransformationId;
    public override string TransformationName => "Humungousaur";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Humungousaur_Buff>();
    public override Transformation ChildTransformation => ModContent.GetInstance<UltimateHumungousaurTransformation>();

    public override string Description =>
        "A towering Vaxasaurian bruiser that grows stronger mid-battle, then enters Titanic Rampage to trade blows with bosses and smash the ground apart.";

    public override List<string> Abilities => new() {
        "Close-range power punch",
        "Armored crushing charge that closes distance and shakes the ground on impact",
        "Growth surge that boosts strength and toughness",
        "Titanic Rampage rewards melee pressure with guard, punch shockwaves, and Crater Stomp",
        "Ultimate evolution"
    };

    public override string PrimaryAttackName => "Power Punch";
    public override string SecondaryAttackName => "Crushing Charge";
    public override string PrimaryAbilityName => "Growth Surge";
    public override string UltimateAbilityName => "Titanic Rampage";
    public override int PrimaryAttack => ModContent.ProjectileType<HumungousaurPunchProjectile>();
    public override int PrimaryAttackSpeed => 24;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;
    public override int PrimaryShootSpeed => 12;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<HumungousaurCrushingChargeProjectile>();
    public override int SecondaryAttackSpeed => 28;
    public override float SecondaryAttackModifier => SecondaryDamageMultiplier;
    public override int SecondaryShootSpeed => 15;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 60 * 60;
    public override int PrimaryAbilityCooldown => 50 * 60;
    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityCost => 75;
    public override int UltimateAbilityDuration => UltimateAbilityDurationTicks;
    public override int UltimateAbilityCooldown => UltimateAbilityCooldownTicks;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        bool rampageActive = IsTitanicRampageActive(omp);
        float growthScale = GetActiveGrowthScale(omp);
        float growthBonusMultiplier = GetGrowthBonusMultiplier(growthScale);
        bool growthActive = growthBonusMultiplier > 0f;

        omp.SetTransformationScale(growthScale, GrowthRampDuration, 1f, growthScale);
        player.statDefense += 14;
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetKnockback<HeroDamage>() += 0.25f;
        player.endurance += 0.07f;

        if (growthActive) {
            player.statDefense += (int)Math.Round(18f * growthBonusMultiplier);
            player.GetDamage<HeroDamage>() += 0.2f * growthBonusMultiplier;
            player.GetKnockback<HeroDamage>() += 0.5f * growthBonusMultiplier;
            player.endurance += 0.08f * growthBonusMultiplier;
            player.moveSpeed *= Math.Max(0.65f, 1f - 0.1f * growthBonusMultiplier);
            player.noKnockback = true;
        }

        if (!rampageActive)
            return;

        player.statDefense += 10;
        player.GetDamage<HeroDamage>() += 0.14f;
        player.GetAttackSpeed<HeroDamage>() += 0.16f;
        player.GetKnockback<HeroDamage>() += 0.35f;
        player.GetArmorPenetration<HeroDamage>() += 10;
        player.endurance += 0.08f;
        player.moveSpeed += 0.08f;
        player.maxRunSpeed += 0.6f;
        player.runAcceleration *= 1.12f;
        player.noKnockback = true;
        player.armorEffectDrawShadow = true;
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        HumungousaurCombatPlayer combat = player.GetModPlayer<HumungousaurCombatPlayer>();
        if (!combat.GuardActive)
            return;

        float guardStrength = combat.GuardStrength;
        player.statDefense += 8 + (int)Math.Round(guardStrength * 52f);
        player.endurance += 0.04f + guardStrength * 0.18f;
        player.noKnockback = true;
    }

    public override void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) {
        float growthRatio = GetGrowthBonusMultiplier(GetCurrentCombatScale(omp));
        if (growthRatio > 0f) {
            modifiers.FinalDamage *= 1f - 0.05f * growthRatio;
            modifiers.Knockback *= MathHelper.Lerp(0.65f, 0.32f, growthRatio);
        }

        if (IsTitanicRampageActive(omp)) {
            modifiers.FinalDamage *= 0.88f;
            modifiers.Knockback *= 0.5f;
        }

        HumungousaurCombatPlayer combat = player.GetModPlayer<HumungousaurCombatPlayer>();
        if (!combat.GuardActive)
            return;

        modifiers.FinalDamage *= 1f - combat.GuardStrength;
        modifiers.Knockback *= 0.25f;
    }

    public override string GetDisplayName(OmnitrixPlayer omp) {
        if (IsTitanicRampageActive(omp))
            return "Humungousaur (Rampage)";

        return omp.IsPrimaryAbilityActive ? "Humungousaur (Grown)" : base.GetDisplayName(omp);
    }

    public override string GetAttackSelectionDisplayName(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp) {
        if (selection == OmnitrixPlayer.AttackSelection.Secondary && IsTitanicRampageActive(omp))
            return "Crater Stomp";

        return base.GetAttackSelectionDisplayName(selection, omp);
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        if (!IsTitanicRampageActive(omp))
            return;

        if (omp.setAttack == OmnitrixPlayer.AttackSelection.Primary) {
            item.useTime = item.useAnimation = Math.Max(16, (int)Math.Round(item.useTime * 0.78f));
        }
        else if (omp.setAttack == OmnitrixPlayer.AttackSelection.Secondary) {
            item.useTime = item.useAnimation = Math.Max(24, (int)Math.Round(item.useTime * 0.86f));
        }
    }

    public override bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<UltimateAbility>() ||
            player.HasBuff<UltimateAbilityCooldown>() ||
            player.dead ||
            player.CCed ||
            omp.ultimateAttack ||
            omp.HasLoadedAbilityAttack) {
            return true;
        }

        int rampageCost = GetUltimateAbilityCost(omp);
        if (omp.omnitrixEnergy < rampageCost) {
            omp.ShowTransformFailureFeedback($"Need {rampageCost} OE for {UltimateAbilityName}.");
            return true;
        }

        omp.omnitrixEnergy -= rampageCost;
        omp.ultimateAbilityTransformationId = FullID;
        player.AddBuff(ModContent.BuffType<UltimateAbility>(), GetUltimateAbilityDuration(omp));
        player.GetModPlayer<HumungousaurCombatPlayer>().RegisterAttackGuard(60, 0.24f);

        TriggerRampageStartPulse(player);
        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        float growthScale = GetCurrentCombatScale(omp);
        float attackScale = GetGrowthAttackMultiplier(growthScale);
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 horizontalDirection = ResolveHorizontalDirection(player, direction);
        HumungousaurCombatPlayer combat = player.GetModPlayer<HumungousaurCombatPlayer>();
        bool rampageActive = IsTitanicRampageActive(omp);

        if (omp.altAttack) {
            if (rampageActive) {
                TriggerCraterStomp(player, source, damage, knockback, growthScale);
                return false;
            }

            Vector2 chargeSpawn = player.MountedCenter + horizontalDirection * (18f + 4f * Math.Min(growthScale, GrownScale));
            combat.RegisterAttackGuard(24, 0.14f + GetGrowthBonusMultiplier(growthScale) * 0.07f);
            Projectile.NewProjectile(source, chargeSpawn, horizontalDirection * SecondaryShootSpeed,
                SecondaryAttack, ScaleDamage(damage, SecondaryAttackModifier * attackScale),
                knockback + 1.3f + (growthScale - 1f) * 0.65f, player.whoAmI, growthScale);
            return false;
        }

        Vector2 punchSpawn = player.MountedCenter + direction * (18f + 4f * Math.Min(growthScale, GrownScale));
        combat.RegisterAttackGuard(rampageActive ? 22 : 16,
            0.12f + GetGrowthBonusMultiplier(growthScale) * 0.07f + (rampageActive ? 0.06f : 0f));
        Projectile.NewProjectile(source, punchSpawn, direction * Math.Max(PrimaryShootSpeed, 10), PrimaryAttack,
            ScaleDamage(damage, PrimaryAttackModifier * attackScale * (rampageActive ? 1.12f : 1f)),
            knockback + 0.5f + (growthScale - 1f) * 0.45f + (rampageActive ? 0.8f : 0f),
            player.whoAmI, growthScale, rampageActive ? 1f : 0f);

        if (rampageActive && combat.RegisterRampagePunch(RampagePulseInterval)) {
            SpawnForwardRampageShockwave(player, source, horizontalDirection, damage, knockback, growthScale, attackScale);
        }

        return false;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsHumungousaurMeleePressureProjectile(projectile.type))
            return;

        bool shockwave = projectile.type == ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>();
        float growthScale = projectile.ai[0] == 0f ? GetCurrentCombatScale(omp) : Math.Abs(projectile.ai[0]);
        player.GetModPlayer<HumungousaurCombatPlayer>().RegisterImpactGuard(target, growthScale, shockwave,
            projectile.ai[1] > 0f || projectile.type == ModContent.ProjectileType<HumungousaurCrushingChargeProjectile>());
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MoltenHelmet;
        player.body = ArmorIDs.Body.MoltenBreastplate;
        player.legs = ArmorIDs.Legs.MoltenGreaves;
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsPrimaryAbilityActive && !IsTitanicRampageActive(omp))
            return;

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch, Scale: 1.2f);
            dust.velocity *= 0.2f;
            dust.noGravity = true;
        }
    }

    private static float GetActiveGrowthScale(OmnitrixPlayer omp) {
        if (IsTitanicRampageActive(omp))
            return RampageScale;

        return omp.IsPrimaryAbilityActive ? GrownScale : 1f;
    }

    private static float GetCurrentCombatScale(OmnitrixPlayer omp) {
        return Math.Max(1f, omp.CurrentTransformationScale);
    }

    private static float GetGrowthAttackMultiplier(float growthScale) {
        return 1f + Math.Max(0f, growthScale - 1f) * 0.75f;
    }

    private static float GetGrowthBonusMultiplier(float growthScale) {
        if (growthScale <= 1f || GrownScale <= 1f)
            return 0f;

        return (growthScale - 1f) / (GrownScale - 1f);
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

    private static Vector2 ResolveHorizontalDirection(Player player, Vector2 aimDirection) {
        float xDirection = Math.Abs(aimDirection.X) > 0.08f
            ? Math.Sign(aimDirection.X)
            : player.direction == 0 ? 1f : player.direction;

        player.direction = xDirection > 0f ? 1 : -1;
        return new Vector2(xDirection, 0f);
    }

    protected static bool IsHumungousaurMeleePressureProjectile(int projectileType) {
        return projectileType == ModContent.ProjectileType<HumungousaurPunchProjectile>() ||
               projectileType == ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>() ||
               projectileType == ModContent.ProjectileType<HumungousaurCrushingChargeProjectile>();
    }

    protected static bool IsTitanicRampageActive(OmnitrixPlayer omp) {
        return omp.IsUltimateAbilityActive &&
               string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
    }

    private static void TriggerRampageStartPulse(Player player) {
        if (player.whoAmI == Main.myPlayer) {
            int pulseDamage = ResolveHeroDamage(player, 0.58f);
            SpawnRampageShockwaveBurst(player, player.GetSource_FromThis(), player.Bottom + new Vector2(0f, -8f),
                pulseDamage, 7f, RampageScale, 1, 2f);
        }

        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.12f, Volume = 0.75f }, player.Center);
        for (int i = 0; i < 28; i++) {
            Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(28f, 34f),
                i % 3 == 0 ? DustID.Torch : DustID.Smoke,
                Main.rand.NextVector2Circular(4.8f, 4.8f), 105,
                i % 3 == 0 ? new Color(255, 160, 92) : new Color(220, 198, 184),
                Main.rand.NextFloat(1.1f, 1.6f));
            dust.noGravity = true;
        }
    }

    private static void TriggerCraterStomp(Player player, IEntitySource source, int damage, float knockback, float growthScale) {
        HumungousaurCombatPlayer combat = player.GetModPlayer<HumungousaurCombatPlayer>();
        float attackScale = GetGrowthAttackMultiplier(growthScale);
        int stompDamage = ScaleDamage(damage, SecondaryDamageMultiplier * attackScale * 1.26f);

        combat.RegisterAttackGuard(36, 0.24f);
        SpawnRampageShockwaveBurst(player, source, player.Bottom + new Vector2(0f, -8f), stompDamage, knockback + 2.1f,
            growthScale * 1.08f, 2, 2f);

        player.velocity.Y = Math.Min(player.velocity.Y, -3.2f);
        player.fallStart = (int)(player.position.Y / 16f);

        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.18f, Volume = 0.78f }, player.Center);
        for (int i = 0; i < 24; i++) {
            Dust dust = Dust.NewDustPerfect(player.Bottom + Main.rand.NextVector2Circular(36f, 12f),
                i % 4 == 0 ? DustID.Torch : DustID.Smoke,
                new Vector2(Main.rand.NextFloat(-4.2f, 4.2f), Main.rand.NextFloat(-3f, -0.2f)),
                110, new Color(255, 165, 100), Main.rand.NextFloat(1.08f, 1.48f));
            dust.noGravity = true;
        }
    }

    private static void SpawnForwardRampageShockwave(Player player, IEntitySource source, Vector2 horizontalDirection,
        int damage, float knockback, float growthScale, float attackScale) {
        Vector2 spawnPosition = player.Bottom + new Vector2(horizontalDirection.X * (20f + 5f * growthScale), -8f * growthScale);
        Vector2 shockwaveVelocity = horizontalDirection * (RampageShockwaveSpeed + (growthScale - 1f) * 1.4f);
        int shockwaveDamage = ScaleDamage(damage, SecondaryDamageMultiplier * attackScale * 0.48f);

        Projectile.NewProjectile(source, spawnPosition, shockwaveVelocity,
            ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), shockwaveDamage, knockback + 0.9f,
            player.whoAmI, growthScale * 0.92f, 1f);
    }

    private static void SpawnRampageShockwaveBurst(Player player, IEntitySource source, Vector2 origin, int damage,
        float knockback, float scale, int wavePairs, float variant) {
        for (int pair = 0; pair < wavePairs; pair++) {
            float pairScale = scale * (1f + pair * 0.16f);
            Projectile.NewProjectile(source, origin + new Vector2(8f + pair * 8f, 0f), Vector2.Zero,
                ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), damage, knockback, player.whoAmI,
                pairScale, variant);
            Projectile.NewProjectile(source, origin + new Vector2(-8f - pair * 8f, 0f), Vector2.Zero,
                ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), damage, knockback, player.whoAmI,
                -pairScale, variant);
        }
    }

    private static int ResolveHeroDamage(Player player, float ratio) {
        Item heldItem = player.HeldItem;
        int baseDamage = heldItem != null && !heldItem.IsAir ? heldItem.damage : 20;
        float heroDamage = player.GetDamage<HeroDamage>().ApplyTo(baseDamage);
        return Math.Max(1, (int)Math.Round(heroDamage * ratio));
    }
}
