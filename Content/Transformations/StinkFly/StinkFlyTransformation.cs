using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.StinkFly;

public class StinkFlyTransformation : Transformation {
    public override string FullID => "Ben10Mod:StinkFly";
    public override string TransformationName => "Stinkfly";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<StinkFly_Buff>();

    public override string Description =>
        "A fast flier that peppers enemies with slowing slime and corrosive poison from the air.";

    public override List<string> Abilities => new() {
        "Sticky slime shot",
        "Poison spit",
        "Flight"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<StinkFlySlowProjectile>();
    public override int PrimaryAttackSpeed => 30;
    public override int PrimaryShootSpeed => 25;

    public override int SecondaryAttack => ModContent.ProjectileType<StinkFlyPoisonProjectile>();
    public override int SecondaryAttackSpeed => 30;
    public override int SecondaryShootSpeed => 25;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<StinkFlyWings>());
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<StinkFly>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.wings = EquipLoader.GetEquipSlot(Mod, nameof(StinkFlyWings), EquipType.Wings);
    }
}
