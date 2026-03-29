using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.BigChill;

public class BigChillTransformation : Transformation {
    public override string FullID => "Ben10Mod:BigChill";
    public override string TransformationName => "Bigchill";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<BigChill_Buff>();
    public override Transformation ChildTransformation => ModContent.GetInstance<UltimateBigChillTransformation>();
    

    public override string Description =>
        "A spectral Necrofriggian that glides through the air, freezes enemies solid, and can evolve into an even deadlier ultimate form.";

    public override List<string> Abilities => new() {
        "Homing ice volley",
        "Frost breath",
        "Phase movement",
        "Flight",
        "Phase-through freeze strike"
    };

    public override string PrimaryAttackName => "Ice Volley";
    public override string SecondaryAttackName => "Frost Breath";
    public override string PrimaryAbilityName => "Phase Movement";
    public override string UltimateAttackName => "Phase Strike";
    public override int PrimaryAttack => ModContent.ProjectileType<BigChillProjectile>();
    public override int PrimaryAttackSpeed => 25;
    public override int PrimaryShootSpeed => 20;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;

    public override int SecondaryAttack => ModContent.ProjectileType<BigChillFrostBreathProjectile>();
    public override int SecondaryAttackSpeed => 10;
    public override int SecondaryShootSpeed => 3;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 0.3f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 30 * 60;
    public override int PrimaryAbilityCooldown => 45 * 60;

    public override bool HasUltimateAbility => false;
    public override int UltimateAttack => ModContent.ProjectileType<BigChillPhaseStrikeProjectile>();
    public override int UltimateAttackSpeed => 18;
    public override int UltimateEnergyCost => 50;
    public override int UltimateAbilityCost => 50;
    public override int UltimateAbilityDuration => 30 * 60;
    public override int UltimateAbilityCooldown => 60 * 60;

    internal const int UltimateFreezeDuration = 10 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.moveSpeed += 0.12f;
        player.noFallDmg = true;
        player.endurance += 0.04f;
        player.iceSkate = true;
        player.buffImmune[BuffID.Chilled] = true;
        player.buffImmune[BuffID.Frozen] = true;
        player.buffImmune[BuffID.Frostburn] = true;
        player.buffImmune[BuffID.Frostburn2] = true;

        var abilitySlot = ModContent.GetInstance<AbilitySlot>();
        abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BigChillWings>());
    }

    public override void ModifyDrawInfo(Player player, OmnitrixPlayer omp, ref PlayerDrawSet drawInfo) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        drawInfo.colorArmorHead.A /= 2;
        drawInfo.colorArmorBody.A /= 2;
        drawInfo.colorArmorLegs.A /= 2;
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        ApplyPhaseMovement(player);
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<BigChill>();

        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.wings = EquipLoader.GetEquipSlot(Mod, nameof(BigChillWings), EquipType.Wings);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (!omp.ultimateAttack)
            return base.Shoot(player, omp, source, position, velocity, damage, knockback);

        omp.ResetAttackToBaseSelection();
        if (Main.netMode == NetmodeID.Server ||
            (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
            return false;

        Vector2 destination = Main.MouseWorld;
        Vector2 direction = player.DirectionTo(destination);
        if (direction == Vector2.Zero)
            direction = new Vector2(player.direction, 0f);

        float dashDistance = Vector2.Distance(player.Center, destination);
        int dashFrames = Utils.Clamp((int)Math.Ceiling(dashDistance / BigChillPhaseStrikeProjectile.DashSpeed),
            BigChillPhaseStrikeProjectile.MinDashFrames, BigChillPhaseStrikeProjectile.MaxDashFrames);

        int projectileIndex = Projectile.NewProjectile(source, player.Center + direction * 18f,
            direction * BigChillPhaseStrikeProjectile.DashSpeed, UltimateAttack, damage, knockback, player.whoAmI);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Projectile projectile = Main.projectile[projectileIndex];
            projectile.timeLeft = dashFrames;
            projectile.netUpdate = true;
        }

        if (!Main.dedServ) {
            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(16f, 20f), DustID.Frost,
                    Main.rand.NextVector2Circular(2.2f, 2.2f), 120, new Color(180, 240, 255), 1.15f);
                dust.noGravity = true;
            }
        }

        return false;
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

}
