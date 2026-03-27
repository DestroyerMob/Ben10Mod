using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class XLR8StarlightProjectile : ModProjectile {
    private const int SlashLifetime = 12;
    private const float BaseForwardRange = 68f;
    private const float OverdriveForwardRange = 82f;
    private const float BaseSlashLength = 58f;
    private const float OverdriveSlashLength = 72f;
    private const float BaseCollisionWidth = 16f;
    private const float OverdriveCollisionWidth = 20f;
    private const float ForwardOffset = 14f;
    private const float SideOffset = 8f;

    private bool Empowered => Projectile.ai[0] >= 0.5f;
    private int SlashSerial => (int)Math.Round(Projectile.ai[1]);
    private float SlashSide => (SlashSerial & 1) == 0 ? -1f : 1f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 50;
        Projectile.height = 50;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = SlashLifetime;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = SlashLifetime;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Vector2 aimDirection = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        if (aimDirection.X != 0f)
            owner.direction = aimDirection.X > 0f ? 1 : -1;

        float progress = 1f - Projectile.timeLeft / (float)SlashLifetime;
        float thrustProgress = progress < 0.28f
            ? progress / 0.28f
            : 1f - (progress - 0.28f) / 0.72f * 0.24f;
        thrustProgress = MathHelper.Clamp(thrustProgress, 0f, 1f);

        float sweepAngle = MathHelper.Lerp(0.58f * SlashSide, -0.12f * SlashSide, progress);
        Vector2 slashDirection = aimDirection.RotatedBy(sweepAngle);
        Vector2 perpendicular = aimDirection.RotatedBy(MathHelper.PiOver2);
        float slashScale = Empowered ? 1.12f : 1f;
        float forwardRange = MathHelper.Lerp(22f, Empowered ? OverdriveForwardRange : BaseForwardRange, thrustProgress);
        Vector2 anchor = owner.MountedCenter + aimDirection * ForwardOffset +
                         perpendicular * SideOffset * SlashSide * (1f - progress * 0.55f);

        Projectile.scale = slashScale;
        Projectile.Center = anchor + aimDirection * forwardRange;
        Projectile.rotation = slashDirection.ToRotation() + MathHelper.PiOver2;
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = Math.Max(owner.itemTime, 2);
        owner.itemAnimation = Math.Max(owner.itemAnimation, 2);
        owner.itemRotation = aimDirection.ToRotation() * owner.direction;
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, aimDirection.ToRotation() - MathHelper.PiOver2);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SpawnOpeningDust(anchor, slashDirection, slashScale);
        }

        if (Main.rand.NextBool(Empowered ? 2 : 3)) {
            Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.BlueCrystalShard,
                slashDirection * Main.rand.NextFloat(0.35f, 1.1f), 100, new Color(90, 175, 255),
                Main.rand.NextFloat(0.95f, 1.22f));
            trailDust.noGravity = true;
        }

        if (Main.rand.NextBool(4)) {
            Dust smokeDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.Smoke,
                -slashDirection * Main.rand.NextFloat(0.15f, 0.6f), 120, new Color(12, 16, 26), 0.95f);
            smokeDust.noGravity = true;
        }

        Lighting.AddLight(Projectile.Center, Empowered ? new Vector3(0.06f, 0.18f, 0.52f) : new Vector3(0.04f, 0.12f, 0.36f));
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 slashDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
        float progress = 1f - Projectile.timeLeft / (float)SlashLifetime;
        float fadeIn = Utils.GetLerpValue(0f, 0.12f, progress, true);
        float fadeOut = Utils.GetLerpValue(0f, 0.42f, Projectile.timeLeft / (float)SlashLifetime, true);
        float opacity = fadeIn * fadeOut;
        float slashLength = (Empowered ? OverdriveSlashLength : BaseSlashLength) * Projectile.scale;
        Vector2 origin = new(0.5f, 0.5f);
        Rectangle source = new(0, 0, 1, 1);

        Main.EntitySpriteDraw(pixel, center - slashDirection * (8f * Projectile.scale), source,
            new Color(6, 10, 18, 225) * opacity, Projectile.rotation, origin,
            new Vector2(10f * Projectile.scale, slashLength), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, source,
            new Color(18, 74, 255, 210) * opacity, Projectile.rotation, origin,
            new Vector2(5f * Projectile.scale, slashLength * 0.9f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + slashDirection * (4f * Projectile.scale), source,
            new Color(130, 220, 255, 220) * opacity, Projectile.rotation, origin,
            new Vector2(1.9f * Projectile.scale, slashLength * 0.72f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, source,
            new Color(30, 32, 45, 190) * opacity, 0f, origin,
            new Vector2(12f * Projectile.scale, 12f * Projectile.scale), SpriteEffects.None, 0);
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float halfLength = (Empowered ? OverdriveSlashLength : BaseSlashLength) * Projectile.scale * 0.5f;
        float collisionWidth = (Empowered ? OverdriveCollisionWidth : BaseCollisionWidth) * Projectile.scale;
        Vector2 slashDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
        Vector2 lineStart = Projectile.Center - slashDirection * halfLength;
        Vector2 lineEnd = Projectile.Center + slashDirection * halfLength;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(
            new Vector2(targetHitbox.X, targetHitbox.Y),
            new Vector2(targetHitbox.Width, targetHitbox.Height),
            lineStart,
            lineEnd,
            collisionWidth,
            ref collisionPoint
        );
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 slashDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
        for (int i = 0; i < 10; i++) {
            Dust blueDust = Dust.NewDustPerfect(target.Center, DustID.BlueCrystalShard,
                slashDirection.RotatedByRandom(0.55f) * Main.rand.NextFloat(0.8f, 3.2f), 100, new Color(100, 195, 255),
                Main.rand.NextFloat(0.95f, 1.25f));
            blueDust.noGravity = true;
        }

        for (int i = 0; i < 6; i++) {
            Dust smokeDust = Dust.NewDustPerfect(target.Center, DustID.Smoke,
                Main.rand.NextVector2Circular(2.2f, 2.2f), 120, new Color(10, 12, 18), Main.rand.NextFloat(0.85f, 1.05f));
            smokeDust.noGravity = true;
        }
    }

    private static void SpawnOpeningDust(Vector2 anchor, Vector2 slashDirection, float slashScale) {
        Vector2 normal = slashDirection.RotatedBy(MathHelper.PiOver2);
        float halfLength = 24f * slashScale;

        for (int i = 0; i < 4; i++) {
            float along = Main.rand.NextFloat(-halfLength, halfLength);
            float across = Main.rand.NextFloat(-3f, 3f) * slashScale;
            Vector2 dustPosition = anchor + slashDirection * along + normal * across;
            Dust smokeDust = Dust.NewDustPerfect(dustPosition, DustID.Smoke,
                slashDirection * Main.rand.NextFloat(0.15f, 0.65f), 120, new Color(8, 10, 18), 0.95f);
            smokeDust.noGravity = true;
        }

        for (int i = 0; i < 4; i++) {
            float along = Main.rand.NextFloat(-halfLength, halfLength);
            float across = Main.rand.NextFloat(-2f, 2f) * slashScale;
            Vector2 dustPosition = anchor + slashDirection * along + normal * across;
            Dust blueDust = Dust.NewDustPerfect(dustPosition, DustID.BlueCrystalShard,
                slashDirection * Main.rand.NextFloat(0.2f, 0.8f), 100, new Color(95, 185, 255), 1.02f);
            blueDust.noGravity = true;
        }
    }
}
