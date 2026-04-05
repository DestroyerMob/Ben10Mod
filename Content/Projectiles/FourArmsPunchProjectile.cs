using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsPunchProjectile : PunchProjectile {
    private const float BaseCollisionWidth = 20f;
    private const float SideCollisionOffset = 10f;

    protected override Color Background => Color.Black;
    protected override Color Foreground => Color.Red;

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.width = 34;
        Projectile.height = 28;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.localNPCHitCooldown = 8;
    }

    protected override Vector2 GetShoulderOffset(Player owner, Vector2 direction, float scale) {
        return new Vector2(owner.direction * 12f * scale, -2f * scale);
    }

    protected override float GetExtension(float progress, float scale) {
        float extensionCurve = progress < 0.24f ? progress / 0.24f : 1f - (progress - 0.24f) / 0.76f * 0.34f;
        return MathHelper.Lerp(18f, 52f * scale, MathHelper.Clamp(extensionCurve, 0f, 1f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 lineStart = GetPunchAnchor(owner, direction, Projectile.scale);
        Vector2 lineEnd = Projectile.Center + direction * (16f * Projectile.scale);
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        float collisionPoint = 0f;
        float collisionWidth = BaseCollisionWidth * Projectile.scale;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
                   collisionWidth, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   lineStart + perpendicular * SideCollisionOffset * Projectile.scale,
                   lineEnd + perpendicular * (SideCollisionOffset * 0.55f) * Projectile.scale,
                   collisionWidth * 0.68f, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   lineStart - perpendicular * SideCollisionOffset * Projectile.scale,
                   lineEnd - perpendicular * (SideCollisionOffset * 0.55f) * Projectile.scale,
                   collisionWidth * 0.68f, ref collisionPoint);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + pushDirection.X * 4.8f, -12f, 12f),
            MathHelper.Clamp(target.velocity.Y - 1.6f, -8f, 10f));
        target.netUpdate = true;
    }
}
