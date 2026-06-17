using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WayBig;

public class WayBigTransformation : Transformation {
    public const float BaseScale = 6f;
    public const float EmpoweredScale = 8f;

    private const int ScaleRampDuration = 300;
    private const int PrimaryAbilityDurationTicks = 12 * 60;
    private const int PrimaryAbilityCooldownTicks = 60 * 60;
    private const int CosmicSurgeCost = 35;
    private const int CosmicRayActivationCost = 120;
    private const int CosmicRaySustainCost = 22;
    private const int CosmicRaySustainInterval = 15;
    private const int CosmicRayCooldownTicks = 90 * 60;
    private const int PunchCommitmentTicks = 34;
    private const int StompCommitmentTicks = 62;
    private const int RayCommitmentTicks = 90;

    public override string FullID => WayBigCombatPlayer.TransformationId;
    public override string TransformationName => "Way Big";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<WayBig_Buff>();

    public override string Description =>
        "A colossal set-piece form with slow, committed attacks that dominate the arena at heavy Omnitrix energy cost.";

    public override List<string> Abilities => new() {
        "Committed titan punch with huge collateral impact",
        "Planetary stomp that tears long shockwave lines through the arena",
        "Cosmic surge that makes Way Big absurdly strong but awkward",
        "Cosmic Ray, a rooted sustained stellar beam"
    };

    public override string PrimaryAttackName => "Cosmic Punch";
    public override string SecondaryAttackName => "Planetary Stomp";
    public override string PrimaryAbilityName => "Cosmic Surge";
    public override string UltimateAttackName => "Cosmic Ray";
    public override int PrimaryAttack => ModContent.ProjectileType<WayBigPunchProjectile>();
    public override int PrimaryAttackSpeed => 46;
    public override int PrimaryShootSpeed => 7;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 1.85f;
    public override int PrimaryEnergyCost => 3;
    public override int SecondaryAttack => ModContent.ProjectileType<WayBigShockwaveProjectile>();
    public override int SecondaryAttackSpeed => 72;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 2.35f;
    public override int SecondaryEnergyCost => 8;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityCost => CosmicSurgeCost;
    public override int PrimaryAbilityDuration => PrimaryAbilityDurationTicks;
    public override int PrimaryAbilityCooldown => PrimaryAbilityCooldownTicks;
    public override int UltimateAttack => ModContent.ProjectileType<WayBigCosmicRayProjectile>();
    public override int UltimateAttackSpeed => CosmicRaySustainInterval;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override float UltimateAttackModifier => 1.7f;
    public override bool UltimateChannel => true;
    public override bool UltimateNoMelee => true;
    public override int UltimateEnergyCost => CosmicRaySustainCost;
    public override int UltimateAttackSustainEnergyCost => CosmicRaySustainCost;
    public override int UltimateAttackSustainInterval => CosmicRaySustainInterval;
    public override int UltimateAbilityCost => CosmicRayActivationCost;
    public override int UltimateAbilityCooldown => CosmicRayCooldownTicks;

    public override string GetDisplayName(OmnitrixPlayer omp) {
        return omp.IsPrimaryAbilityActive ? "Way Big (Cosmic Surge)" : base.GetDisplayName(omp);
    }

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        bool surged = omp.IsPrimaryAbilityActive;
        WayBigCombatPlayer combat = player.GetModPlayer<WayBigCombatPlayer>();
        float targetScale = surged ? EmpoweredScale : BaseScale;
        omp.SetTransformationScale(targetScale, ScaleRampDuration, 1f, targetScale);

        player.GetDamage<HeroDamage>() += surged ? 0.52f : 0.3f;
        player.statDefense += surged ? 38 : 26;
        player.endurance += surged ? 0.16f : 0.1f;
        player.GetKnockback<HeroDamage>() += surged ? 1.8f : 1.1f;
        player.GetArmorPenetration<HeroDamage>() += surged ? 18 : 10;
        player.noKnockback = true;
        player.moveSpeed *= surged ? 0.68f : 0.78f;
        player.maxRunSpeed *= surged ? 0.5f : 0.62f;
        player.accRunSpeed *= surged ? 0.52f : 0.68f;
        player.runAcceleration *= surged ? 0.42f : 0.58f;

        if (combat.IsCommitted) {
            player.endurance += 0.04f;
            player.moveSpeed *= MathHelper.Lerp(0.72f, 1f, combat.CommitmentMoveMultiplier);
            player.maxRunSpeed *= MathHelper.Lerp(0.66f, 1f, combat.CommitmentMoveMultiplier);
        }

        if (omp.ultimateAttack || omp.IsPrimaryAbilityActive)
            Lighting.AddLight(player.Center, 0.15f, 0.55f, 0.65f);
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        WayBigCombatPlayer combat = player.GetModPlayer<WayBigCombatPlayer>();
        bool grounded = AlienIdentityPlayer.IsGrounded(player);

        if (combat.RayBraced) {
            player.velocity.X *= grounded ? 0.08f : 0.18f;
            if (grounded && Math.Abs(player.velocity.X) < 0.12f)
                player.velocity.X = 0f;
            if (!grounded && player.velocity.Y < 0f)
                player.velocity.Y *= 0.82f;
            return;
        }

        if (combat.IsCommitted) {
            player.velocity.X *= MathHelper.Clamp(combat.CommitmentMoveMultiplier, 0.02f, 1f);
            if (grounded && Math.Abs(player.velocity.X) < 0.08f)
                player.velocity.X = 0f;
        }

        if (omp.IsPrimaryAbilityActive && grounded)
            player.velocity.X *= 0.86f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 aimDirection = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        Vector2 horizontalDirection = new(aimDirection.X >= 0f ? 1f : -1f, 0f);
        float combatScale = Math.Max(1f, omp.CurrentTransformationScale);
        bool surged = omp.IsPrimaryAbilityActive;
        WayBigCombatPlayer combat = player.GetModPlayer<WayBigCombatPlayer>();

        if (omp.ultimateAttack) {
            combat.RegisterCommitment(RayCommitmentTicks, surged ? 0.035f : 0.06f, rayBrace: true);
            Projectile.NewProjectile(source, player.Center, aimDirection,
                ModContent.ProjectileType<WayBigCosmicRayProjectile>(),
                ScaleDamage(damage, UltimateAttackModifier * (surged ? 1.16f : 1f)),
                knockback + 2.4f + (surged ? 1f : 0f), player.whoAmI, surged ? 1f : 0f, combatScale);
            return false;
        }

        if (omp.altAttack) {
            combat.RegisterCommitment(surged ? StompCommitmentTicks + 18 : StompCommitmentTicks, surged ? 0.12f : 0.2f);
            Vector2 stompOrigin = player.Bottom + new Vector2(horizontalDirection.X * (12f + combatScale * 5f), -16f);
            int stompDamage = ScaleDamage(damage, SecondaryAttackModifier * (surged ? 1.28f : 1f));
            int mainFlags = BuildShockwaveFlags(surged, collateral: false);
            int collateralFlags = BuildShockwaveFlags(surged, collateral: true);

            SpawnShockwave(source, stompOrigin, (int)horizontalDirection.X, combatScale, stompDamage,
                knockback + 3.6f + (surged ? 1.2f : 0f), mainFlags, player.whoAmI);
            SpawnShockwave(source, stompOrigin + new Vector2(-horizontalDirection.X * 18f, 0f), -(int)horizontalDirection.X,
                combatScale * 0.94f, ScaleDamage(stompDamage, 0.68f), knockback + 2.6f, collateralFlags, player.whoAmI);

            if (surged) {
                SpawnShockwave(source, stompOrigin + new Vector2(horizontalDirection.X * 58f, -4f),
                    (int)horizontalDirection.X, combatScale * 0.88f, ScaleDamage(stompDamage, 0.54f),
                    knockback + 2f, collateralFlags, player.whoAmI);
                SpawnShockwave(source, stompOrigin + new Vector2(-horizontalDirection.X * 58f, -4f),
                    -(int)horizontalDirection.X, combatScale * 0.88f, ScaleDamage(stompDamage, 0.54f),
                    knockback + 2f, collateralFlags, player.whoAmI);
            }

            return false;
        }

        combat.RegisterCommitment(surged ? PunchCommitmentTicks + 14 : PunchCommitmentTicks, surged ? 0.18f : 0.28f);
        Projectile.NewProjectile(source, player.Center, horizontalDirection * PrimaryShootSpeed,
            ModContent.ProjectileType<WayBigPunchProjectile>(),
            ScaleDamage(damage, PrimaryAttackModifier * (surged ? 1.22f : 1f)),
            knockback + 4.5f + (surged ? 1f : 0f), player.whoAmI, combatScale * (surged ? 1.08f : 1f),
            surged ? 1f : 0f);
        return false;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.PrimaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Ultimate)
            return base.GetAttackResourceSummary(selection, omp, compact);

        WayBigCombatPlayer combat = omp.Player.GetModPlayer<WayBigCombatPlayer>();
        string baseText = base.GetAttackResourceSummary(selection, omp, compact);
        string commitText = combat.RayBraced
            ? compact ? "Rooted beam" : "Rooted in Cosmic Ray"
            : combat.IsCommitted
                ? compact ? "Committed" : $"Committed {OmnitrixPlayer.FormatCooldownTicks(combat.CommitmentTicks)}"
                : compact ? "Slow" : "Slow committed action";
        string surgeText = omp.IsPrimaryAbilityActive ? compact ? "Surged" : "Cosmic Surge active" : compact ? $"{CosmicSurgeCost} OE" : $"Cosmic Surge costs {CosmicSurgeCost} OE";
        string identityText = resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"{commitText} • Huge"
                : $"{commitText} • horizontal collateral punch",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{commitText} • Lines"
                : $"{commitText} • forward and back shockwave lines",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? surgeText
                : $"{surgeText} • stronger, larger, and clumsier",
            OmnitrixPlayer.AttackSelection.Ultimate => compact
                ? $"{commitText} • Root"
                : $"{commitText} • high-drain rooted beam",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(baseText) ? identityText : $"{baseText} • {identityText}";
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MoltenHelmet;
        player.body = ArmorIDs.Body.MoltenBreastplate;
        player.legs = ArmorIDs.Legs.MoltenGreaves;
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsPrimaryAbilityActive && !omp.ultimateAttack)
            return;

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GemSapphire,
                Scale: omp.IsPrimaryAbilityActive ? 1.6f : 1.3f);
            dust.velocity *= 0.18f;
            dust.noGravity = true;
        }
    }

    private static int BuildShockwaveFlags(bool surged, bool collateral) {
        int flags = 0;
        if (surged)
            flags |= WayBigShockwaveProjectile.SurgedFlag;
        if (collateral)
            flags |= WayBigShockwaveProjectile.CollateralFlag;
        return flags;
    }

    private static void SpawnShockwave(EntitySource_ItemUse_WithAmmo source, Vector2 origin, int direction, float scale,
        int damage, float knockback, int flags, int owner) {
        Projectile.NewProjectile(source, origin, new Vector2(direction, 0f), ModContent.ProjectileType<WayBigShockwaveProjectile>(),
            damage, knockback, owner, direction, scale, flags);
    }

    private static int ScaleDamage(int damage, float multiplier) =>
        Math.Max(1, (int)Math.Round(damage * multiplier));
}
