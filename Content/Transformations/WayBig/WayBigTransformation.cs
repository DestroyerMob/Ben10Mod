using System;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WayBig;

public class WayBigTransformation : SimpleRangedTransformationBase {
    private const float WayBigScale = 10f;
    private const float WayBigAttackForwardOffset = 14f;
    private const float WayBigAttackHeightOffset = 10f;

    public override string FullID => "Ben10Mod:WayBig";
    public override string TransformationName => "Way Big";
    public override int TransformationBuffId => ModContent.BuffType<WayBig_Buff>();
    protected override string BasicDescription => "A simple giant base-form implementation with a basic projectile primary attack.";

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        omp.SetTransformationScale(WayBigScale, 180, 1f, WayBigScale);
    }
    
    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        float growthScale = Math.Max(1f, omp.CurrentTransformationScale);
        Vector2 attackDirection = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        Vector2 punchVelocity = attackDirection * 10f;
        Vector2 handAnchor = omp.GetScaledVisualPoint(player.itemLocation);
        Vector2 attackOrigin = handAnchor + new Vector2(
            attackDirection.X * WayBigAttackForwardOffset * growthScale,
            WayBigAttackHeightOffset * growthScale
        );

        Projectile.NewProjectile(source, attackOrigin, punchVelocity,
            ModContent.ProjectileType<WayBigPunchProjectile>(), (int)(damage * growthScale), knockback,
            player.whoAmI, growthScale);
        return false;
    }
    
    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player         player = drawInfo.drawPlayer;
        OmnitrixPlayer omp    = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsPrimaryAbilityActive && !omp.IsUltimateAbilityActive)
            return;

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch, Scale: 1.2f);
            dust.velocity  *= 0.2f;
            dust.noGravity =  true;
        }
    }

    protected override int HeadSlot => ArmorIDs.Head.MoltenHelmet;
    protected override int BodySlot => ArmorIDs.Body.MoltenBreastplate;
    protected override int LegSlot => ArmorIDs.Legs.MoltenGreaves;
}
