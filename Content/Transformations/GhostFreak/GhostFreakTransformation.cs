using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.GhostFreak;

public class GhostFreakTransformation : Transformation {
    private const int HauntEnergyCost = 16;
    private const int HauntCooldown = 11 * 60;

    public override string FullID => "Ben10Mod:GhostFreak";
    public override string TransformationName => "Ghostfreak";
    public override string IconPath => "Ben10Mod/Content/Interface/GhostFreakSelect";
    public override int TransformationBuffId => ModContent.BuffType<GhostFreak_Buff>();

    public override string Description =>
        "A fear and possession controller who avoids direct fights, phases through danger, and manipulates enemies before taking over.";

    public override List<string> Abilities => new() {
        "Weak Fear Bolt that marks enemies",
        "Curse Wave that spreads fear and panic",
        "Intangibility and phasing movement",
        "Haunt target for delayed damage and possession setup",
        "Possession payoff against feared or haunted enemies"
    };

    public override string PrimaryAttackName => "Fear Bolt";
    public override string SecondaryAttackName => "Curse Wave";
    public override string PrimaryAbilityName => "Intangibility";
    public override string SecondaryAbilityAttackName => "Haunt";
    public override string UltimateAttackName => "Possession";
    public override int PrimaryAttack => ModContent.ProjectileType<GhostFreakProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 13;
    public override float PrimaryAttackModifier => 0.58f;

    public override int SecondaryAttack => ModContent.ProjectileType<GhostFreakFearWaveProjectile>();
    public override int SecondaryAttackSpeed => 34;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAttackModifier => 0.72f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 30 * 60;
    public override int PrimaryAbilityCooldown => 45 * 60;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<GhostFreakHauntProjectile>();
    public override int SecondaryAbilityAttackSpeed => 24;
    public override int SecondaryAbilityAttackShootSpeed => 13;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAbilityAttackModifier => 0.88f;
    public override int SecondaryAbilityAttackEnergyCost => HauntEnergyCost;
    public override int SecondaryAbilityCooldown => HauntCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<GhostFreakPossesionProjectile>();
    public override int UltimateAttackSpeed => 24;
    public override int UltimateShootSpeed => 12;
    public override float UltimateAttackModifier => 0.9f;
    public override int UltimateEnergyCost => 50;
    public override int UltimateAbilityCooldown => 30 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.noFallDmg = true;
        player.GetCritChance<HeroDamage>() += 10f;
        player.moveSpeed += 0.12f;
        player.maxRunSpeed += 0.8f;
        player.aggro -= 400;
        player.nightVision = true;
        player.endurance += 0.04f;
    }

    public override bool? CanUseItem(Player player, OmnitrixPlayer omp, Item item) {
        if (!omp.PrimaryAbilityEnabled)
            return base.CanUseItem(player, omp, item);

        if (item?.ModItem is not PlumbersBadge)
            return false;

        OmnitrixPlayer.AttackSelection selectedAttack = ResolveAttackSelection(omp.setAttack, omp);
        if (selectedAttack == OmnitrixPlayer.AttackSelection.Primary)
            return false;

        return CanStartCurrentAttack(player, omp);
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        OmnitrixPlayer.AttackSelection selectedAttack = ResolveAttackSelection(omp.setAttack, omp);
        if (omp.PrimaryAbilityEnabled && selectedAttack == OmnitrixPlayer.AttackSelection.Primary)
            return false;

        return base.CanStartCurrentAttack(player, omp);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 spawnPosition = player.MountedCenter + new Vector2(player.direction * 6f, -10f) + direction * 12f;
        bool intangible = omp.PrimaryAbilityEnabled;

        if (omp.ultimateAttack) {
            int finalDamage = System.Math.Max(1, (int)System.Math.Round(damage * UltimateAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * UltimateShootSpeed, UltimateAttack, finalDamage,
                knockback + 0.4f, player.whoAmI, intangible ? 1f : 0f);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            int finalDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAbilityAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, direction * SecondaryAbilityAttackShootSpeed,
                SecondaryAbilityAttack, finalDamage, knockback + 0.2f, player.whoAmI, intangible ? 1f : 0f);
            return false;
        }

        if (omp.altAttack) {
            int finalDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, player.Center + direction * 24f, Vector2.Zero, SecondaryAttack,
                finalDamage, knockback + 0.1f, player.whoAmI, intangible ? 1f : 0f);
            return false;
        }

        int primaryDamage = System.Math.Max(1, (int)System.Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed, PrimaryAttack, primaryDamage,
            knockback, player.whoAmI, intangible ? 1f : 0f);
        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (!IsGhostFreakControlProjectile(projectile.type))
            return;

        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int fearStacks = state.GetGhostFreakFearStacks(player.whoAmI);
        if (fearStacks > 0)
            modifiers.FinalDamage *= 1f + fearStacks * 0.04f;

        if (state.IsGhostFreakHauntedFor(player.whoAmI))
            modifiers.FinalDamage *= projectile.type == UltimateAttack ? 1.22f : 1.12f;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = selection == OmnitrixPlayer.AttackSelection.PrimaryAbility &&
                                                           HasPrimaryAbilityForState(omp)
            ? OmnitrixPlayer.AttackSelection.PrimaryAbility
            : ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.PrimaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.SecondaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Ultimate)
            return base.GetAttackResourceSummary(selection, omp, compact);

        int fearedTargets = CountFearedTargets(omp.Player);
        string fearText = compact ? $"Fear {fearedTargets}" : $"Feared targets {fearedTargets}";
        string identityText = resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"{fearText} • Tag"
                : $"{fearText} • weak shot applies Fear",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{fearText} • Spread"
                : $"{fearText} • spreads Fear and lowers aggression",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? "Phase"
                : "Phase through danger; control attacks stay available while intangible",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"{fearText} • Haunt"
                : $"{fearText} • Haunt adds delayed damage and possession setup",
            OmnitrixPlayer.AttackSelection.Ultimate => compact
                ? $"{fearText} • Cashout"
                : $"{fearText} • possession lasts longer against feared or haunted targets",
            _ => fearText
        };

        string baseText = base.GetAttackResourceSummary(selection, omp, compact);
        return string.IsNullOrWhiteSpace(baseText) ? identityText : $"{baseText} • {identityText}";
    }

    public override void ModifyDrawInfo(Player player, OmnitrixPlayer omp, ref PlayerDrawSet drawInfo) {
        if (omp.PrimaryAbilityEnabled) {
            drawInfo.colorArmorHead.A /= 2;
            drawInfo.colorArmorBody.A /= 2;
            drawInfo.colorArmorLegs.A /= 2;
        }

        if (omp.inPossessionMode)
            player.invis = true;
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        ApplyPhaseMovement(player);
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        if (omp.inPossessionMode) {
            player.head = -1;
            player.body = -1;
            player.legs = -1;
            return;
        }

        var costume = ModContent.GetInstance<GhostFreak>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    private static void ApplyPhaseMovement(Player player) {
        Vector2 input = Vector2.Zero;
        if (player.controlLeft) input.X -= 1f;
        if (player.controlRight) input.X += 1f;
        if (player.controlUp) input.Y -= 1f;
        if (player.controlDown) input.Y += 1f;

        const float speed = 14.5f;
        const float damp = 0.82f;

        if (input != Vector2.Zero) {
            input.Normalize();
            Vector2 move = input * speed;
            if (input.Y < 0f)
                move.Y -= 3f;

            player.position += move;
        }
        else {
            player.velocity *= damp;
            player.position += player.velocity;
        }

        player.velocity = Vector2.Zero;
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

    private static bool IsGhostFreakControlProjectile(int projectileType) {
        return projectileType == ModContent.ProjectileType<GhostFreakProjectile>() ||
               projectileType == ModContent.ProjectileType<GhostFreakFearWaveProjectile>() ||
               projectileType == ModContent.ProjectileType<GhostFreakHauntProjectile>() ||
               projectileType == ModContent.ProjectileType<GhostFreakPossesionProjectile>();
    }

    private static int CountFearedTargets(Player player) {
        int count = 0;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.GetGlobalNPC<AlienIdentityGlobalNPC>().IsGhostFreakFearedFor(player.whoAmI))
                count++;
        }

        return count;
    }
    
    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => [
        new TransformationPaletteChannel(
            "base",
            "Base",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/GhostFreak/GhostFreak_Head",
                "Ben10Mod/Content/Transformations/GhostFreak/GhostFreakBaseMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/GhostFreak/GhostFreak_Body",
                "Ben10Mod/Content/Transformations/GhostFreak/GhostFreakBaseMask_Body"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/GhostFreak/GhostFreak_Legs",
                "Ben10Mod/Content/Transformations/GhostFreak/GhostFreakBaseMask_Legs")),
        new TransformationPaletteChannel(
            "eye",
            "Eye",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/GhostFreak/GhostFreak_Head",
                "Ben10Mod/Content/Transformations/GhostFreak/GhostFreakEyeMask_Head"))
    ];
}
