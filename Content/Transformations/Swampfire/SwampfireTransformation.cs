using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Swampfire;

public class SwampfireTransformation : Transformation {
    public override string FullID => "Ben10Mod:Swampfire";
    public override string TransformationName => "Swampfire";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Swampfire_Buff>();

    public override string Description =>
        "A methane-fueled plant creature that hurls fire and seeds while healing through rooted regeneration.";

    public override List<string> Abilities => new() {
        "Flaming seed shots",
        "Burst seed pod",
        "Regenerative rooting"
    };

    public override string PrimaryAttackName => "Methane Bolt";
    public override string SecondaryAttackName => "Seed Pod";
    public override string PrimaryAbilityName => "Regenerative Rooting";
    public override int PrimaryAttack => ModContent.ProjectileType<SwampfireBoltProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 13;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<SwampfireSeedProjectile>();
    public override int SecondaryAttackSpeed => 32;
    public override int SecondaryShootSpeed => 10;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.25f;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAbilityCooldown => 36 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.14f;
        player.lifeRegen += 2;
        player.fireWalk = true;
        player.buffImmune[BuffID.OnFire] = true;
        player.buffImmune[BuffID.OnFire3] = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.lifeRegen += 6;
        player.statDefense += 10;
        player.endurance += 0.04f;
        player.moveSpeed *= 0.8f;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.JungleHat;
        player.body = ArmorIDs.Body.JungleShirt;
        player.legs = ArmorIDs.Legs.JunglePants;
    }
}
