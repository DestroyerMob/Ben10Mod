using System.Collections.Generic;
using Ben10Mod.Content;
using Ben10Mod.Content.Buffs.Transformations;
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
        "A spectral Necrofriggian that glides through the air, freezes enemies solid, and can evolve into an ultimate form.";

    public override List<string> Abilities => new() {
        "Homing ice volley",
        "Frost breath",
        "Phase movement",
        "Flight",
        "Ultimate evolution"
    };

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
    public override int PrimaryAbilityCooldown => 60 * 60;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityCost => 50;
    public override int UltimateAbilityDuration => 30 * 60;
    public override int UltimateAbilityCooldown => 60 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

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
