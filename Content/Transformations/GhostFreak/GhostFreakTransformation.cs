using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.GhostFreak;

public class GhostFreakTransformation : Transformation {
    public override string FullID => "Ben10Mod:GhostFreak";
    public override string TransformationName => "Ghostfreak";
    public override string IconPath => "Ben10Mod/Content/Interface/GhostFreakSelect";
    public override int TransformationBuffId => ModContent.BuffType<GhostFreak_Buff>();

    public override string Description =>
        "An eerie Ectonurite that slips through space, turns semi-transparent, and can possess enemies outright.";

    public override List<string> Abilities => new() {
        "Ectoplasmic shots",
        "Phasing movement",
        "Intangibility",
        "Enemy possession"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<GhostFreakProjectile>();
    public override int PrimaryAttackSpeed => 14;
    public override int PrimaryShootSpeed => 12;

    public override int SecondaryAttack => ModContent.ProjectileType<GhostFreakProjectile>();
    public override int SecondaryAttackSpeed => 14;
    public override int SecondaryShootSpeed => 12;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 30 * 60;
    public override int PrimaryAbilityCooldown => 60 * 60;

    public override int UltimateAttack => ModContent.ProjectileType<GhostFreakPossesionProjectile>();
    public override int UltimateAttackSpeed => 14;
    public override int UltimateShootSpeed => 12;
    public override int UltimateEnergyCost => 50;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.noFallDmg = true;
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
}
