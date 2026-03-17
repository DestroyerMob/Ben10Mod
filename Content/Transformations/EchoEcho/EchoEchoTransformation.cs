using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EchoEcho;

public class EchoEchoTransformation : Transformation {
    public override string FullID => "Ben10Mod:EchoEcho";
    public override string TransformationName => "Echo Echo";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<EchoEcho_Buff>();
    public override Transformation ChildTransformation => ModContent.GetInstance<UltimateEchoEchoTransformation>();

    public override string Description =>
        "A living sonic resonator that can split into combat duplicates and fire concentrated sound bursts from its mouth.";

    public override List<string> Abilities => new() {
        "Sonic mouth blasts",
        "Wide sonic burst",
        "Self-duplication",
        "Ultimate evolution"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<EchoEchoSonicBlastProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 14;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttackSpeed => 26;
    public override int SecondaryShootSpeed => 12;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 0.7f;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 16 * 60;
    public override int PrimaryAbilityCooldown => 45 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        if (!omp.PrimaryAbilityEnabled || Main.myPlayer != player.whoAmI)
            return;

        if (player.ownedProjectileCounts[ModContent.ProjectileType<EchoEchoCloneProjectile>()] >= 2)
            return;

        for (int i = 0; i < 2; i++) {
            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
                ModContent.ProjectileType<EchoEchoCloneProjectile>(), 18, 0f, player.whoAmI,
                MathHelper.TwoPi * i / 2f);
        }
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (!omp.altAttack)
            return base.Shoot(player, omp, source, position, velocity, damage, knockback);

        for (int i = -1; i <= 1; i++) {
            Vector2 spreadVelocity = velocity.RotatedBy(MathHelper.ToRadians(9f * i));
            Projectile.NewProjectile(source, position, spreadVelocity, ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(),
                (int)(damage * 0.7f), knockback, player.whoAmI);
        }

        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.CopperHelmet;
        player.body = ArmorIDs.Body.CopperChainmail;
        player.legs = ArmorIDs.Legs.CopperGreaves;
    }
}
