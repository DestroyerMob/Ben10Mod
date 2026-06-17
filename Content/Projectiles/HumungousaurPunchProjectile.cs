using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Humungousaur;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HumungousaurPunchProjectile : PunchProjectile {
    private bool IsFinisher => Projectile.ai[1] > 0.5f;
    private bool Braced => Projectile.ai[2] > 0.5f;
    private int GrowthTier => HumungousaurTransformation.GetGrowthTier(Projectile.scale);

    protected override Color Background => IsFinisher
        ? new Color(176, 78, 40, 230)
        : Braced
            ? new Color(160, 92, 54, 225)
            : new Color(138, 74, 46, 220);

    protected override Color Foreground => IsFinisher
        ? new Color(255, 210, 150, 215)
        : Braced
            ? new Color(255, 198, 140, 205)
            : new Color(235, 180, 145, 195);

    protected override int SpawnDustType => DustID.Torch;
    protected override Color SpawnDustColor => new Color(255, 180, 110);
    protected override int TrailDustType => DustID.Smoke;
    protected override Color TrailDustColor => new Color(255, 170, 120);
    protected override Vector3 LightEmission => new(0.95f, 0.42f, 0.18f);
    protected override int ImpactDustType => DustID.Torch;
    protected override Color ImpactDustColor => new Color(255, 175, 118);

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.width = 34;
        Projectile.height = 28;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.localNPCHitCooldown = 8;
    }

    protected override Vector2 GetShoulderOffset(Player owner, Vector2 direction, float scale) {
        return IsFinisher
            ? new Vector2(owner.direction * 16f * scale, -3f * scale)
            : new Vector2(owner.direction * 13f * scale, 0f);
    }

    protected override float GetExtension(float progress, float scale) {
        float tierReach = 1f + GrowthTier * 0.08f + (Braced ? 0.12f : 0f);
        if (IsFinisher) {
            float curve = progress < 0.28f
                ? progress / 0.28f
                : 1f - (progress - 0.28f) / 0.72f * 0.22f;
            return MathHelper.Lerp(18f, 60f * scale * tierReach, MathHelper.Clamp(curve, 0f, 1f));
        }

        float extensionCurve = progress < 0.24f
            ? progress / 0.24f
            : 1f - (progress - 0.24f) / 0.76f * 0.32f;
        return MathHelper.Lerp(16f, 50f * scale * tierReach, MathHelper.Clamp(extensionCurve, 0f, 1f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 lineStart = GetPunchAnchor(owner, direction, Projectile.scale);
        Vector2 lineEnd = Projectile.Center + direction * (IsFinisher ? 18f : 14f) * Projectile.scale;
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        float collisionPoint = 0f;
        float collisionWidth = (IsFinisher ? 22f : 18f) + GrowthTier * 3f + (Braced ? 3f : 0f);

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
                   collisionWidth * Projectile.scale, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   lineStart + perpendicular * 9f * Projectile.scale,
                   lineEnd + perpendicular * 5f * Projectile.scale,
                   collisionWidth * 0.56f * Projectile.scale, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   lineStart - perpendicular * 9f * Projectile.scale,
                   lineEnd - perpendicular * 5f * Projectile.scale,
                   collisionWidth * 0.56f * Projectile.scale, ref collisionPoint);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        int growthTier = GrowthTier;
        float pushStrength = (IsFinisher ? 7.4f : 5f) + growthTier * 1.15f + (Braced ? 1.35f : 0f);
        float liftStrength = (IsFinisher ? 2.4f : 1.5f) + growthTier * 0.35f;
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + pushDirection.X * pushStrength, -16f, 16f),
            MathHelper.Clamp(target.velocity.Y - liftStrength, -10f, 10f));
        target.netUpdate = true;

        if (growthTier > 0)
            target.AddBuff(BuffID.BrokenArmor, growthTier >= 2 ? 220 : 150);

        TrySpawnGroundShockwave(pushDirection, growthTier);
    }

    private void TrySpawnGroundShockwave(Vector2 direction, int growthTier) {
        if (Projectile.owner != Main.myPlayer || growthTier <= 0 && !Braced)
            return;

        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return;

        float shockScale = Projectile.scale * (Braced ? 0.94f : 0.76f);
        int shockDamage = (int)System.Math.Round(Projectile.damage * (Braced ? 0.34f : growthTier >= 2 ? 0.28f : 0.2f));
        Vector2 shockPosition = owner.Bottom + new Vector2(direction.X * (14f + growthTier * 4f), -8f);
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), shockPosition, direction * (10.5f + growthTier * 1.5f),
            ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), System.Math.Max(1, shockDamage),
            Projectile.knockBack * (Braced ? 0.62f : 0.45f), Projectile.owner, shockScale, Braced ? 2f : 1f);
    }
}
