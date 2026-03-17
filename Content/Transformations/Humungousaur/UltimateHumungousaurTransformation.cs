using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Humungousaur;

public class UltimateHumungousaurTransformation : HumungousaurTransformation {
    public override string FullID => "Ben10Mod:UltimateHumungousaur";
    public override string TransformationName => "Ultimate Humungousaur";
    public override int TransformationBuffId => ModContent.BuffType<UltimateHumungousaur_Buff>();
    public override Transformation ParentTransformation => ModContent.GetInstance<HumungousaurTransformation>();
    public override Transformation ChildTransformation => null;
    public override bool HasPrimaryAbility => false;

    public override string Description =>
        "A denser and more destructive Vaxasaurian form that trades growth for direct rocket-powered firepower.";

    public override List<string> Abilities => new() {
        "Hand rockets",
        "Rocket spread barrage",
        "Heavy durability"
    };

    public override int PrimaryAttack => ProjectileID.RocketIII;
    public override int PrimaryAttackSpeed => 20;
    public override int PrimaryShootSpeed => 14;
    public override int SecondaryAttack => ProjectileID.RocketIII;
    public override int SecondaryAttackSpeed => 30;
    public override int SecondaryShootSpeed => 13;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.statDefense += 18;
        player.GetDamage(DamageClass.Generic) += 0.22f;
        player.GetKnockback(DamageClass.Generic) += 0.45f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        int rocketCount = omp.altAttack ? 3 : 1;
        float spread = omp.altAttack ? 12f : 0f;
        float damageMult = omp.altAttack ? 0.7f : 1f;

        for (int i = 0; i < rocketCount; i++) {
            float offsetIndex = i - (rocketCount - 1) / 2f;
            Vector2 rocketVelocity = velocity.RotatedBy(MathHelper.ToRadians(spread * offsetIndex));
            Projectile.NewProjectile(source, position, rocketVelocity,
                omp.altAttack ? SecondaryAttack : PrimaryAttack,
                (int)(damage * damageMult), knockback, player.whoAmI);
        }

        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.PlatinumHelmet;
        player.body = ArmorIDs.Body.PlatinumChainmail;
        player.legs = ArmorIDs.Legs.PlatinumGreaves;
    }
}
