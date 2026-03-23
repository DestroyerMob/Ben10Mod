using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EyeGuy;

public class EyeGuyTransformation : Transformation {
    public override string FullID => "Ben10Mod:EyeGuy";
    public override string TransformationName => "Eye Guy";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<EyeGuy_Buff>();
    public override string Description =>
        "A precision Opticoid that overwhelms targets with relentless beam fire and an enormous channeled eye blast.";

    public override List<string> Abilities => new() {
        "Rapid eye laser",
        "Long-range beam pressure",
        "Channeled ultimate eye beam"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<EyeGuyLaserbeam>();
    public override int UltimateAttack => ModContent.ProjectileType<EyeGuyUltimateBeam>();
    public override int UltimateAttackSpeed => 10;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override bool UltimateChannel => true;
    public override bool UltimateNoMelee => true;
    public override int UltimateEnergyCost => 10;
    public override int UltimateAbilityCooldown => 30 * 60;
    public override int PrimaryShootSpeed => 30;
    public override int PrimaryAttackSpeed => 15;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 12f;
        player.GetArmorPenetration<HeroDamage>() += 6;
        player.nightVision = true;
        player.detectCreature = true;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<EyeGuy>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }
}
