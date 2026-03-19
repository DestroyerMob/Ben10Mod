using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Jetray;

public class JetrayTransformation : Transformation {
    public override string FullID => "Ben10Mod:Jetray";
    public override string TransformationName => "Jetray";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Jetray_Buff>();

    public override string Description =>
        "A hyperspeed aerial hunter that fires neuroshocks and rams targets in a lightning-fast dive.";

    public override List<string> Abilities => new() {
        "Rapid neuroshock bolts",
        "High-speed dive strike",
        "Afterburner flight"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<JetrayBoltProjectile>();
    public override int PrimaryAttackSpeed => 14;
    public override int PrimaryShootSpeed => 16;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<JetrayDiveProjectile>();
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 14;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.45f;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 7 * 60;
    public override int PrimaryAbilityCooldown => 28 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage(DamageClass.Generic) += 0.12f;
        player.moveSpeed += 0.18f;
        player.maxRunSpeed += 1.8f;
        player.accRunSpeed += 1.6f;
        player.ignoreWater = true;
        player.gills = true;
        player.noFallDmg = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage(DamageClass.Generic) += 0.1f;
        player.moveSpeed += 0.2f;
        player.maxRunSpeed += 2.5f;
        player.accRunSpeed += 2f;
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<StinkFlyWings>());
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.altAttack) {
            player.velocity = direction * (omp.PrimaryAbilityEnabled ? 19f : 16f);
            player.immune = true;
            player.immuneTime = 12;
            Projectile.NewProjectile(source, player.Center + direction * 12f, direction * 9f,
                ModContent.ProjectileType<JetrayDiveProjectile>(), (int)(damage * SecondaryAttackModifier),
                knockback + 1.5f, player.whoAmI);
            return false;
        }

        Projectile.NewProjectile(source, player.Center + direction * 10f, direction * (omp.PrimaryAbilityEnabled ? 20f : 17f),
            ModContent.ProjectileType<JetrayBoltProjectile>(), damage, knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.NecroHelmet;
        player.body = ArmorIDs.Body.NecroBreastplate;
        player.legs = ArmorIDs.Legs.NecroGreaves;
        player.wings = EquipLoader.GetEquipSlot(Mod, nameof(StinkFlyWings), EquipType.Wings);
    }
}
