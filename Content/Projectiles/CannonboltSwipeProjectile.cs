using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class CannonboltSwipeProjectile : PunchProjectile {
    private int Variant => (int)System.MathF.Round(Projectile.ai[1]);
    private bool SpinSwipe => Variant >= 1;

    protected override int SpawnDustType => DustID.GemTopaz;
    protected override Color SpawnDustColor => new(255, 210, 145);
    protected override int TrailDustType => DustID.Smoke;
    protected override Color TrailDustColor => new(220, 205, 170);
    protected override int ImpactDustType => DustID.Stone;
    protected override Color ImpactDustColor => new(235, 220, 190);
    protected override Vector3 LightEmission => SpinSwipe ? new Vector3(0.56f, 0.42f, 0.18f) : new Vector3(0.42f, 0.3f, 0.14f);

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.width = 34;
        Projectile.height = 28;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.localNPCHitCooldown = 8;
    }

    protected override Vector2 GetShoulderOffset(Player owner, Vector2 direction, float scale) {
        return SpinSwipe
            ? new Vector2(owner.direction * 10f * scale, 4f * scale)
            : new Vector2(owner.direction * 14f * scale, -1f * scale);
    }

    protected override float GetExtension(float progress, float scale) {
        if (SpinSwipe) {
            float spinCurve = progress < 0.26f
                ? progress / 0.26f
                : 1f - (progress - 0.26f) / 0.74f * 0.22f;
            return MathHelper.Lerp(18f, 48f * scale, MathHelper.Clamp(spinCurve, 0f, 1f));
        }

        float bashCurve = progress < 0.32f ? progress / 0.32f : 1f - (progress - 0.32f) / 0.68f * 0.3f;
        return MathHelper.Lerp(16f, 42f * scale, MathHelper.Clamp(bashCurve, 0f, 1f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 anchor = GetPunchAnchor(owner, direction, Projectile.scale);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 lineEnd = Projectile.Center + direction * (SpinSwipe ? 20f : 12f) * Projectile.scale;
        float collisionPoint = 0f;
        float bodyWidth = (SpinSwipe ? 22f : 18f) * Projectile.scale;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), anchor, lineEnd,
                   bodyWidth, ref collisionPoint)
               || SpinSwipe && (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                       anchor + normal * 16f * Projectile.scale, lineEnd + normal * 8f * Projectile.scale,
                       bodyWidth * 0.7f, ref collisionPoint)
                   || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                       anchor - normal * 16f * Projectile.scale, lineEnd - normal * 8f * Projectile.scale,
                       bodyWidth * 0.7f, ref collisionPoint));
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float scaleX = SpinSwipe ? 22f : 18f;
        float scaleY = SpinSwipe ? 13f : 11f;
        float rimScale = SpinSwipe ? 0.72f : 0.66f;
        Color shellColor = SpinSwipe ? new Color(150, 112, 62, 230) : new Color(135, 98, 52, 230);
        Color coreColor = SpinSwipe ? new Color(245, 219, 170, 205) : new Color(232, 203, 156, 195);
        Color streakColor = new Color(255, 244, 220, SpinSwipe ? 148 : 118);

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), shellColor, Projectile.rotation - MathHelper.PiOver2,
            new Vector2(0.5f, 0.5f), new Vector2(scaleX, scaleY) * Projectile.scale, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), coreColor, Projectile.rotation - MathHelper.PiOver2,
            new Vector2(0.5f, 0.5f), new Vector2(scaleX * rimScale, scaleY * rimScale) * Projectile.scale, SpriteEffects.None, 0f);

        if (SpinSwipe) {
            Vector2 direction = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
            Vector2 streakCenter = center - direction * 10f * Projectile.scale;
            Main.spriteBatch.Draw(pixel, streakCenter, new Rectangle(0, 0, 1, 1), streakColor,
                Projectile.rotation - MathHelper.PiOver2, new Vector2(0.5f, 0.5f),
                new Vector2(26f, 8f) * Projectile.scale, SpriteEffects.None, 0f);
        }

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float pushStrength = SpinSwipe ? 7.8f : 5.8f;
        float liftStrength = SpinSwipe ? 2.5f : 1.5f;
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + pushDirection.X * pushStrength, -15f, 15f),
            MathHelper.Clamp(target.velocity.Y - liftStrength, -10f, 10f));
        target.netUpdate = true;
    }
}
