using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Rath;

    public class RathTransformation : Transformation {
        private const float RageClawScale = 1.45f;
        private const float BaseClawSpawnOffset = 100f;
        private const float RageClawSpawnOffset = 100f;

    public override string FullID => "Ben10Mod:Rath";
    public override string TransformationName => "Rath";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Rath_Buff>();

    public override string Description =>
        "An Appoplexian brawler that rips into enemies with savage claw strikes and reckless lunges.";

    public override List<string> Abilities => new() {
        "Savage claw combo",
        "Leaping tackle",
        "Battle rage"
    };

    public override string PrimaryAttackName => "Claw Slash";
    public override string SecondaryAttackName => "Pounce";
    public override int PrimaryAttack => ModContent.ProjectileType<RathClawProjectile>();
    public override int PrimaryAttackSpeed => 20;
    public override int PrimaryShootSpeed => 10;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<RathPounceProjectile>();
    public override int SecondaryAttackSpeed => 120;
    public override int SecondaryShootSpeed => 14;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.35f;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 8 * 60;
    public override int PrimaryAbilityCooldown => 32 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.15f;
        player.GetAttackSpeed<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.moveSpeed += 0.08f;
        player.GetKnockback<HeroDamage>() += 0.45f;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.GetDamage<HeroDamage>() += 0.18f;
        player.GetAttackSpeed<HeroDamage>() += 0.18f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.moveSpeed += 0.22f;
        player.runAcceleration *= 1.2f;
        player.endurance += 0.04f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.altAttack) {
            player.velocity = direction * 13f + new Vector2(0f, -2f);
            Projectile.NewProjectile(source, player.Center + direction * 18f, direction * 8f,
                ModContent.ProjectileType<RathPounceProjectile>(), (int)(damage * SecondaryAttackModifier), knockback + 2f,
                player.whoAmI);
            return false;
        }

        float clawScale = omp.PrimaryAbilityEnabled ? RageClawScale : 1f;
        float clawSpawnOffset = omp.PrimaryAbilityEnabled ? RageClawSpawnOffset : BaseClawSpawnOffset;
        Projectile.NewProjectile(source, player.Center + (direction * clawSpawnOffset), direction * 6f,
            ModContent.ProjectileType<RathClawProjectile>(), damage, knockback, player.whoAmI, 0f, clawScale);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.AshWoodHelmet;
        player.body = ArmorIDs.Body.Gi;
        player.legs = ArmorIDs.Legs.FossilGreaves;
    }
}
