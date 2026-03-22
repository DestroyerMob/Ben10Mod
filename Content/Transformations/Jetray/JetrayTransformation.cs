using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
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
        "A hyperspeed aerial hunter that fires piercing eye-lasers, stays airborne with ease, and lunges through prey.";

    public override List<string> Abilities => new() {
        "Rapid green laser fire",
        "Short-range click dash",
        "Quick aerial flight"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<JetrayLaserProjectile>();
    public override int PrimaryAttackSpeed => 15;
    public override int PrimaryShootSpeed => 30;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<JetrayDiveProjectile>();
    public override int SecondaryAttackSpeed => 40;
    public override int SecondaryShootSpeed => 1;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.35f;
    public override bool HasPrimaryAbility => false;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.14f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.moveSpeed += 0.22f;
        player.maxRunSpeed += 2.4f;
        player.accRunSpeed += 2f;
        player.jumpSpeedBoost += 1.25f;
        player.ignoreWater = true;
        player.gills = true;
        player.noFallDmg = true;
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<JetrayWings>());
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.altAttack) {
            if (player.whoAmI == Main.myPlayer) {
                Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
                if (mouseDirection != Vector2.Zero)
                    direction = mouseDirection;
            }

            Projectile.NewProjectile(source, player.Center + direction * 12f, direction * 34f,
                ModContent.ProjectileType<JetrayDiveProjectile>(), (int)(damage * SecondaryAttackModifier),
                knockback + 1.5f, player.whoAmI);
            return false;
        }

        Projectile.NewProjectile(source, player.Center + direction * 10f, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<JetrayLaserProjectile>(), damage, knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.NecroHelmet;
        player.body = ArmorIDs.Body.NecroBreastplate;
        player.legs = ArmorIDs.Legs.NecroGreaves;
        player.wings = EquipLoader.GetEquipSlot(Mod, nameof(JetrayWings), EquipType.Wings);
    }
}
