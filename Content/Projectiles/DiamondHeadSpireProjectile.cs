using Ben10Mod.Content.DamageClasses;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class DiamondHeadSpireProjectile : ModProjectile {
    private const int LifetimeTicks = 28;
    private const float StartScale = 0.25f;
    private const float MaxScale = 1.1f;
    private const float CollisionWidthScale = 0.38f;
    public const int BaseHeight = 118;

    public override string Texture => "Ben10Mod/Content/Projectiles/GiantDiamondProjectile";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 50;
        Projectile.height = BaseHeight;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.scale = StartScale;
        if (Projectile.ai[1] == 0f)
            Projectile.ai[1] = Projectile.Center.Y + Projectile.height * 0.5f;

        Projectile.localAI[0] = Projectile.Center.X;
        Projectile.rotation = Projectile.ai[0];
        UpdateAnchoredPosition();
    }

    public override void AI() {
        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = progress * progress * (3f - 2f * progress);
        Projectile.scale = MathHelper.Lerp(StartScale, MaxScale, easedProgress);
        Projectile.rotation = Projectile.ai[0];
        UpdateAnchoredPosition();

        Lighting.AddLight(Projectile.Center, 0.2f, 0.34f, 0.48f);
        SpawnSpireDust();
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 12; i++) {
            Dust dust = Dust.NewDustPerfect(GetGroundPoint() +
                Main.rand.NextVector2Circular(16f, 8f), DustID.GemDiamond,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 95, new Color(220, 255, 255), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 groundPoint = GetGroundPoint();
        Vector2 tipPoint = groundPoint + GetGrowthDirection() * (Projectile.height * Projectile.scale);
        float collisionPoint = 0f;
        float collisionWidth = Math.Max(14f, Projectile.width * Projectile.scale * CollisionWidthScale);
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), groundPoint, tipPoint,
            collisionWidth, ref collisionPoint);
    }

    private void UpdateAnchoredPosition() {
        float scaledHeight = Projectile.height * Projectile.scale;
        Projectile.Center = GetGroundPoint() + GetGrowthDirection() * (scaledHeight * 0.5f);
    }

    private Vector2 GetGroundPoint() {
        return new Vector2(Projectile.localAI[0], Projectile.ai[1]);
    }

    private Vector2 GetGrowthDirection() {
        return (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
    }

    private void SpawnSpireDust() {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        float scaledHeight = Projectile.height * Projectile.scale;
        Vector2 groundPoint = GetGroundPoint();
        Vector2 growthDirection = GetGrowthDirection();
        Vector2 normalDirection = new(-growthDirection.Y, growthDirection.X);
        float progress = Main.rand.NextFloat(0.08f, 0.96f);
        Vector2 dustPosition = new(
            groundPoint.X,
            groundPoint.Y
        );
        dustPosition += growthDirection * (scaledHeight * progress);
        dustPosition += normalDirection * Main.rand.NextFloat(-Projectile.width * Projectile.scale * 0.18f,
            Projectile.width * Projectile.scale * 0.18f);

        Dust dust = Dust.NewDustPerfect(dustPosition, DustID.GemDiamond,
            growthDirection * Main.rand.NextFloat(0.6f, 1.5f) +
            normalDirection * Main.rand.NextFloat(-0.35f, 0.35f), 100,
            new Color(200, 255, 255), Main.rand.NextFloat(0.95f, 1.25f));
        dust.noGravity = true;
    }
}
