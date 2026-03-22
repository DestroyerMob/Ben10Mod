using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class RathClawProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private const int SlashLifetime = 10;
    private const int BaseHitboxSize = 44;
    private const float DefaultForwardRange = 60f;
    private const float DefaultRageForwardRange = 84f;
    public override void SetDefaults() {
        Projectile.width = BaseHitboxSize;
        Projectile.height = BaseHitboxSize;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = 1;
        Projectile.timeLeft = SlashLifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.ai[0] = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f)).ToRotation();
            float fallbackRange = Projectile.ai[1] > 1f ? DefaultRageForwardRange : DefaultForwardRange;
            Projectile.localAI[1] = Vector2.Distance(owner.MountedCenter, Projectile.Center);
            if (Projectile.localAI[1] <= 0.01f) {
                Projectile.localAI[1] = fallbackRange;
            }
            Projectile.ai[2] = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        float slashScale = Projectile.ai[1] > 0f ? Projectile.ai[1] : 1f;
        float forwardRange = Projectile.localAI[1] > 0f
            ? Projectile.localAI[1]
            : (slashScale > 1f ? DefaultRageForwardRange : DefaultForwardRange);
        float anchorAngle = Projectile.ai[0];
        Vector2 anchorDirection = anchorAngle.ToRotationVector2();
        Vector2 anchorPoint = owner.MountedCenter + anchorDirection * forwardRange;
        float lineRotation = Projectile.ai[2];
        Vector2 swingDirection = lineRotation.ToRotationVector2();
        Projectile.rotation = lineRotation + MathHelper.PiOver2;
        Projectile.Center = anchorPoint;
        Projectile.velocity = swingDirection * 6f;
        Projectile.scale = slashScale;
        owner.itemRotation = MathHelper.WrapAngle(anchorAngle) * owner.direction;
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, anchorAngle - MathHelper.PiOver2);
        UpdateHitboxSize(slashScale);
        SpawnSlashDustLine(anchorPoint, swingDirection, slashScale);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 swingDirection = Projectile.rotation.ToRotationVector2();
        float slashScale = Projectile.ai[1] > 0f ? Projectile.ai[1] : 1f;
        float lifeProgress = 1f - Projectile.timeLeft / (float)SlashLifetime;
        float opacity = Utils.GetLerpValue(0f, 0.18f, lifeProgress, true) * Utils.GetLerpValue(1f, 0.45f, lifeProgress, true);
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(210, 220, 235, 210) * opacity,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(8f * slashScale, 62f * slashScale), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 255, 255, 235) * opacity,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(3f * slashScale, 46f * slashScale), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), new Color(255, 255, 255, 145) * opacity,
            0f, new Vector2(0.5f, 0.5f), new Vector2(10f * slashScale, 10f * slashScale), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center, i % 2 == 0 ? DustID.Smoke : DustID.SilverCoin,
                Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.45f) * Main.rand.NextFloat(1f, 3.4f), 90,
                new Color(255, 255, 255), 1.15f);
            dust.noGravity = true;
        }
    }

    private void UpdateHitboxSize(float slashScale) {
        int targetSize = (int)Math.Round(BaseHitboxSize * slashScale);
        if (Projectile.width == targetSize && Projectile.height == targetSize)
            return;

        Vector2 center = Projectile.Center;
        Projectile.width = targetSize;
        Projectile.height = targetSize;
        Projectile.Center = center;
    }

    private void SpawnSlashDustLine(Vector2 center, Vector2 lineDirection, float slashScale) {
        float halfLength = 30f * slashScale;
        Vector2 normal = lineDirection.RotatedBy(MathHelper.PiOver2);

        for (int i = 0; i < 3; i++) {
            float along = Main.rand.NextFloat(-halfLength, halfLength);
            float across = Main.rand.NextFloat(-3f, 3f) * slashScale;
            Vector2 dustPosition = center + lineDirection * along + normal * across;
            Dust smoke = Dust.NewDustPerfect(dustPosition, DustID.Smoke,
                lineDirection * Main.rand.NextFloat(0.25f, 0.9f), 110, new Color(240, 240, 240), 1.05f);
            smoke.noGravity = true;
        }

        for (int i = 0; i < 2; i++) {
            float along = Main.rand.NextFloat(-halfLength, halfLength);
            float across = Main.rand.NextFloat(-2f, 2f) * slashScale;
            Vector2 dustPosition = center + lineDirection * along + normal * across;
            Dust slashDust = Dust.NewDustPerfect(dustPosition, DustID.SilverCoin,
                lineDirection * Main.rand.NextFloat(0.2f, 0.75f), 105, new Color(255, 255, 255), 0.98f);
            slashDust.noGravity = true;
        }
    }
}
