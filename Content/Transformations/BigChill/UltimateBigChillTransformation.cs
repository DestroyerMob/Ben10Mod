using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
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
    public override Transformation ChildTransformation => null;
    public override Transformation ParentTransformation => ModContent.GetInstance<BigChillTransformation>();

    public override string Description =>
        "An evolved Necrofriggian form with denser ice power, improved aerial control, and all of Big Chill's spectral mobility.";

    public override List<string> Abilities => new() {
        "Enhanced homing ice volley",
        "Enhanced frost breath",
        "Phase movement",
        "Flight",
        "Ultimate form attacks and abilities"
    };

    public override float PrimaryAttackModifier => 1.25f;
    public override float SecondaryAttackModifier => 0.45f;
    public override int UltimateAbilityCost => 65;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

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
