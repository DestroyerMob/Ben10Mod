using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Accessories.Wings;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.BigChill;

public class UltimateBigChillTransformation : BigChillTransformation {
    public override string FullID => "Ben10Mod:UltimateBigChill";
    public override string TransformationName => "Ultimate Bigchill";
    public override int TransformationBuffId => ModContent.BuffType<UltimateBigChill_Buff>();
    public override Transformation ParentTransformation => ModContent.GetInstance<BigChillTransformation>();
    public override Transformation ChildTransformation => null;

    public override string Description =>
        "An evolved Necrofriggian with denser frost output, harsher Deep Freeze payoffs, and even stronger aerial control than base Big Chill.";

    public override List<string> Abilities => new() {
        "The same Frostbite, Deep Freeze, and Shatter loop, but with stronger baseline damage and tighter flight.",
        "Ecto Breath and Cryo Lance both hit harder, so Shatter setups finish faster.",
        "Phase Drift and Grave Mist keep the same controller loop while the evolved body stays harder to pin down.",
        "Absolute Zero still amplifies the whole kit instead of replacing it."
    };

    public override string PrimaryAttackName => "Ecto Breath";
    public override string SecondaryAttackName => "Cryo Lance";
    public override float PrimaryAttackModifier => 0.42f;
    public override float SecondaryAttackModifier => 1.24f;
    public override int UltimateAbilityCost => 65;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.endurance += 0.04f;
        player.moveSpeed += 0.04f;

        var abilitySlot = ModContent.GetInstance<AbilitySlot>();
        abilitySlot.FunctionalItem = new Item(ModContent.ItemType<UltimateBigChillWings>());
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Legs);
        player.wings = EquipLoader.GetEquipSlot(Mod, nameof(UltimateBigChillWings), EquipType.Wings);
    }
}
