using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.NRG;

public class NRGTransformation : Transformation {
    public override string FullID => "Ben10Mod:NRG";
    public override string TransformationName => "NRG";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<NRG_Buff>();

    public override string Description =>
        "A living reactor sealed in armor that batters foes with radioactive blasts and vented energy bursts.";

    public override List<string> Abilities => new() {
        "Radiation cannon shot",
        "Containment burst",
        "Overheated core"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<NRGRadiationProjectile>();
    public override int PrimaryAttackSpeed => 20;
    public override int PrimaryShootSpeed => 11;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<NRGBurstProjectile>();
    public override int SecondaryAttackSpeed => 32;
    public override int SecondaryShootSpeed => 1;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.55f;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 9 * 60;
    public override int PrimaryAbilityCooldown => 42 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.14f;
        player.statDefense += 16;
        player.endurance += 0.08f;
        player.GetKnockback<HeroDamage>() += 0.4f;
        player.fireWalk = true;
        player.lavaImmune = true;
        player.noKnockback = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.2f;
        player.statDefense += 10;
        player.endurance += 0.06f;
        Lighting.AddLight(player.Center, 1f, 0.45f, 0.12f);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.altAttack) {
            Projectile.NewProjectile(source, player.Center, direction,
                ModContent.ProjectileType<NRGBurstProjectile>(), (int)(damage * SecondaryAttackModifier),
                knockback + 2f, player.whoAmI);
            return false;
        }

        Projectile.NewProjectile(source, player.Center + direction * 16f, direction * 10f,
            ModContent.ProjectileType<NRGRadiationProjectile>(), damage, knockback + 0.5f, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MoltenHelmet;
        player.body = ArmorIDs.Body.MoltenBreastplate;
        player.legs = ArmorIDs.Legs.MoltenGreaves;
    }
}
