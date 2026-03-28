using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Armodrillo;

public class ArmodrilloTransformation : Transformation {
    public override string FullID => "Ben10Mod:Armodrillo";
    public override string TransformationName => "Armodrillo";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Armodrillo_Buff>();

    public override string Description =>
        "A heavily-armored powerhouse that pulverizes enemies with piston drills and seismic ground-shattering force.";

    public override List<string> Abilities => new() {
        "Piston drill strike",
        "Siege plating",
        "Seismic Slam that shatters the ground"
    };

    public override string PrimaryAttackName => "Piston Drill";
    public override string PrimaryAbilityName => "Siege Plating";
    public override string UltimateAttackName => "Seismic Slam";
    public override int PrimaryAttack => ModContent.ProjectileType<ArmodrilloDrillProjectile>();
    public override int PrimaryAttackSpeed => 24;
    public override int PrimaryShootSpeed => 10;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 8 * 60;
    public override int PrimaryAbilityCooldown => 40 * 60;
    public override int UltimateAttack => ModContent.ProjectileType<ArmodrilloUltimateSlamProjectile>();
    public override float UltimateAttackModifier => 3f;
    public override int UltimateAttackSpeed => 30;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override int UltimateEnergyCost => 75;
    public override int UltimateAbilityCost => 75;
    public override int UltimateAbilityCooldown => 60 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.14f;
        player.statDefense += 10;
        player.endurance += 0.05f;
        player.GetKnockback<HeroDamage>() += 0.6f;
        player.GetArmorPenetration<HeroDamage>() += 10;
        player.noKnockback = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.18f;
        player.statDefense += 14;
        player.endurance += 0.08f;
        player.moveSpeed -= 0.05f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (omp.ultimateAttack) {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, UltimateAttack,
                (int)(damage * UltimateAttackModifier), knockback + 2f, player.whoAmI);
            return false;
        }

        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        Projectile.NewProjectile(source, player.Center + direction * 18f, direction * 6f,
            ModContent.ProjectileType<ArmodrilloDrillProjectile>(), damage, knockback + 1f, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MeteorHelmet;
        player.body = ArmorIDs.Body.MeteorSuit;
        player.legs = ArmorIDs.Legs.MeteorLeggings;
    }
}
