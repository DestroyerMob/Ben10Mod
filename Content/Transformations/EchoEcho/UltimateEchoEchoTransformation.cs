using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EchoEcho;

public class UltimateEchoEchoTransformation : EchoEchoTransformation {
    public override string FullID => "Ben10Mod:UltimateEchoEcho";
    public override string TransformationName => "Ultimate Echo Echo";
    public override int TransformationBuffId => ModContent.BuffType<UltimateEchoEcho_Buff>();
    public override Transformation ParentTransformation => ModContent.GetInstance<EchoEchoTransformation>();
    public override Transformation ChildTransformation => null;

    public override string Description =>
        "An evolved sonic form that abandons duplication in favor of detached speaker arrays and heavier resonance fire.";

    public override List<string> Abilities => new() {
        "Enhanced sonic bursts",
        "Detached speaker barrage",
        "Ultimate evolution attacks"
    };

    public override float PrimaryAttackModifier => 1.15f;
    public override float SecondaryAttackModifier => 0.9f;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        if (!omp.PrimaryAbilityEnabled || Main.myPlayer != player.whoAmI)
            return;

        if (player.ownedProjectileCounts[ModContent.ProjectileType<UltimateEchoEchoSpeakerProjectile>()] >= 3)
            return;

        for (int i = 0; i < 3; i++) {
            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
                ModContent.ProjectileType<UltimateEchoEchoSpeakerProjectile>(), 24, 0f, player.whoAmI,
                MathHelper.TwoPi * i / 3f);
        }
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        int spreadCount = omp.altAttack ? 5 : 3;
        float spreadStep = omp.altAttack ? 7f : 10f;
        float damageMult = omp.altAttack ? 0.6f : 0.85f;

        int mid = spreadCount / 2;
        for (int i = 0; i < spreadCount; i++) {
            int offsetIndex = i - mid;
            Vector2 spreadVelocity = velocity.RotatedBy(MathHelper.ToRadians(spreadStep * offsetIndex));
            Projectile.NewProjectile(source, position, spreadVelocity, ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(),
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
