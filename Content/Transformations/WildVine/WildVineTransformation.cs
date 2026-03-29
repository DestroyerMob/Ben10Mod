using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WildVine;

public class WildVineTransformation : Transformation {
    public override string FullID => "Ben10Mod:WildVine";
    public override string TransformationName => "Wildvine";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<WildVine_Buff>();

    public override string Description =>
        "A flexible Florauna that lashes enemies with a thorn whip and blankets the ground with toxic seed pods.";

    public override List<string> Abilities => new() {
        "Thorn whip that poisons on contact",
        "Gas seed bombs that burst into lingering toxic clouds"
    };

    public override string PrimaryAttackName => "Thorn Whip";
    public override string SecondaryAttackName => "Gas Seed Bombs";
    public override int PrimaryAttack => ModContent.ProjectileType<WildVineWhipProjectile>();
    public override float PrimaryAttackModifier => 0.95f;
    public override int PrimaryAttackSpeed => 26;
    public override int PrimaryShootSpeed => 4;
    public override int SecondaryAttack => ModContent.ProjectileType<WildVineBomb>();
    public override float SecondaryAttackModifier => 0.85f;
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.1f;
        player.moveSpeed += 0.08f;
        player.runAcceleration += 0.06f;
        player.jumpSpeedBoost += 1.8f;
        player.noFallDmg = true;
        player.lifeRegen += player.velocity.Y == 0f ? 3 : 1;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (omp.altAttack) {
            Vector2 aimDirection = ResolveAimDirection(player, velocity);
            Vector2 spawnPosition = player.Center + aimDirection * 14f;
            Vector2 lobVelocity = aimDirection * SecondaryShootSpeed + new Vector2(0f, -2.6f);
            int seedDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, spawnPosition, lobVelocity,
                ModContent.ProjectileType<WildVineBomb>(), seedDamage, knockback, player.whoAmI);
            return false;
        }

        int whipDamage = System.Math.Max(1, (int)System.Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, position, velocity,
            ModContent.ProjectileType<WildVineWhipProjectile>(), whipDamage, knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<WildVine>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction;
    }
}
