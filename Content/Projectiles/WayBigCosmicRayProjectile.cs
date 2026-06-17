using Ben10Mod.Content.Projectiles.UltimateAttacks;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Transformations.WayBig;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WayBigCosmicRayProjectile : ChannelBeamUltimateProjectile {
    protected override float MaxLength => SurgedRay ? 3900f : 3200f;
    protected override float BeamThickness => SurgedRay ? 68f : 54f;
    protected override float StartOffset => 34f;
    protected override int MinEnergyToSustain => 22;
    protected override Vector2 StartScale => SurgedRay ? new Vector2(3.15f, 1.32f) : new Vector2(2.5f, 1.2f);
    protected override Vector2 OuterScale => SurgedRay ? new Vector2(4.45f, 1.34f) : new Vector2(3.6f, 1.2f);
    protected override Vector2 MidScale => SurgedRay ? new Vector2(3.35f, 1.24f) : new Vector2(2.7f, 1.15f);
    protected override Vector2 InnerScale => SurgedRay ? new Vector2(2.25f, 1.16f) : new Vector2(1.8f, 1.1f);
    protected override Color BeamColor => new(110, 255, 235);
    protected override Color BeamHighlightColor => new(225, 255, 255);
    protected override int EndDustType => DustID.GemSapphire;
    protected override int EndDustCount => SurgedRay ? 11 : 7;
    protected override float LightR => 0.16f;
    protected override float LightG => 1.2f;
    protected override float LightB => 1.25f;

    protected override Vector2 GetLocalAimDirection(Player owner) {
        Vector2 targetDirection = Main.MouseWorld - owner.Center;
        if (targetDirection.LengthSquared() < 0.0001f)
            targetDirection = new Vector2(owner.direction, 0f);

        targetDirection.Normalize();

        Vector2 currentDirection = Projectile.velocity.SafeNormalize(targetDirection);
        float maxTurn = SurgedRay ? 0.018f : 0.026f;
        float currentRotation = currentDirection.ToRotation();
        float targetRotation = targetDirection.ToRotation();
        float turn = MathHelper.Clamp(MathHelper.WrapAngle(targetRotation - currentRotation), -maxTurn, maxTurn);
        return (currentRotation + turn).ToRotationVector2();
    }

    protected override Vector2 GetBeamStart(Player owner, Vector2 direction) {
        float scale = System.Math.Max(1f, owner.GetModPlayer<OmnitrixPlayer>().CurrentTransformationScale);
        Vector2 chestAnchor = owner.Bottom + new Vector2(0f, -owner.height * 0.82f);
        float forwardOffset = 28f + scale * (SurgedRay ? 7.5f : 6f);
        return chestAnchor + direction * forwardOffset + new Vector2(direction.X * scale * (SurgedRay ? 9.5f : 8f), 0f);
    }

    protected override void OnBeamUpdated(Player owner, OmnitrixPlayer omp, Vector2 start, Vector2 direction) {
        WayBigCombatPlayer combat = owner.GetModPlayer<WayBigCombatPlayer>();
        combat.RegisterCommitment(10, SurgedRay ? 0.035f : 0.055f, rayBrace: true);
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.noKnockback = true;
        owner.velocity.X *= AlienIdentityPlayer.IsGrounded(owner) ? 0.05f : 0.16f;
        if (owner.velocity.Y < 0f)
            owner.velocity.Y *= 0.82f;

        if (!Main.rand.NextBool(2))
            return;

        Vector2 end = start + direction * BeamHitLength;
        Dust startDust = Dust.NewDustPerfect(start + Main.rand.NextVector2Circular(SurgedRay ? 26f : 18f, SurgedRay ? 26f : 18f),
            DustID.GemSapphire, Main.rand.NextVector2Circular(1.2f, 1.2f), 110, new Color(180, 255, 250),
            Main.rand.NextFloat(1.2f, SurgedRay ? 1.95f : 1.6f));
        startDust.noGravity = true;

        Dust endDust = Dust.NewDustPerfect(end + Main.rand.NextVector2Circular(SurgedRay ? 38f : 26f, SurgedRay ? 38f : 26f),
            DustID.GemDiamond, Main.rand.NextVector2Circular(SurgedRay ? 3f : 2f, SurgedRay ? 3f : 2f), 110,
            Color.White, Main.rand.NextFloat(1.2f, SurgedRay ? 2.15f : 1.75f));
        endDust.noGravity = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.BrokenArmor, SurgedRay ? 240 : 160);
        target.AddBuff(BuffID.Slow, SurgedRay ? 150 : 90);
    }

    private bool SurgedRay => Projectile.ai[0] > 0.5f;
}
