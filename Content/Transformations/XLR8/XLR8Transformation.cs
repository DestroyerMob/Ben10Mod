using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.XLR8;

public class XLR8Transformation : Transformation {
    public override string FullID                  => "Ben10Mod:XLR8";
    public override string TransformationName      => "XLR8";
    public override string IconPath                => "Ben10Mod/Content/Interface/XLR8Select";
    public override int    TransformationBuffId    => ModContent.BuffType<XLR8_Buff>();
    public override string Description =>
        "A Kineceleran speedster built to blur across the battlefield, chain rapid strikes, and distort the pace of combat.";

    public override List<string> Abilities => new() {
        "Rapid strike rush",
        "Extreme speed boost",
        "Dash mobility",
        "Water running at speed",
        "Time-slowing ultimate field"
    };

    public override int   PrimaryAbilityDuration  => 10 * 60;
    public override int   PrimaryAbilityCooldown  => 30 * 60;
    public override int   PrimaryAttack           => ModContent.ProjectileType<XLR8PunchProjectile>();
    public override int   PrimaryAttackSpeed      => 12;
    public override int   PrimaryShootSpeed       => 30;
    public override float PrimaryAttackModifier   => 0.75f;
    public override bool  HasUltimateAbility      => true;
    public override int   UltimateAbilityCost     => 100;
    public override int   UltimateAbilityDuration => 4 * 60;
    public override int   UltimateAbilityCooldown => 60 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        
        player.moveSpeed *= omp.PrimaryAbilityEnabled ? 5f : 2.5f;
        player.accRunSpeed *= omp.PrimaryAbilityEnabled ? 4f : 2f;
        player.GetAttackSpeed<HeroDamage>() += omp.PrimaryAbilityEnabled ? 0.35f : 0.2f;
        player.GetCritChance<HeroDamage>() += omp.PrimaryAbilityEnabled ? 14f : 8f;
        player.pickSpeed *= omp.PrimaryAbilityEnabled ? 0.45f : 0.65f;
        player.jumpSpeedBoost += omp.PrimaryAbilityEnabled ? 3f : 1.6f;
        if (Math.Abs(player.velocity.X) > 2) {
            player.waterWalk =  true;
        }
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<XLR8>();
        player.armorEffectDrawShadow = true;
        player.head                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        if (omp.PrimaryAbilityEnabled)
            player.head = EquipLoader.GetEquipSlot(Mod, "XLR8_alt", EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.waist = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Waist);
    }
}
