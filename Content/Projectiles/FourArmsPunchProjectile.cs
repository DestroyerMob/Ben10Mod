using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsPunchProjectile : PunchProjectile {
    private const float BaseCollisionWidth = 20f;
    private const float SideCollisionOffset = 10f;
    private const int PotisVariantOffset = 10;

    private int RawVariant => (int)System.MathF.Round(Projectile.ai[1]);
    private bool PotisInfused => RawVariant >= PotisVariantOffset;
    private int Variant => PotisInfused ? RawVariant - PotisVariantOffset : RawVariant;
    private bool IsFinisher => Variant == 2;
    private bool IsHaymaker => Variant >= 3;

    protected override Color Background => IsHaymaker
        ? PotisInfused ? new Color(190, 72, 20, 235) : new Color(140, 40, 16, 230)
        : IsFinisher
            ? PotisInfused ? new Color(170, 54, 22, 230) : new Color(120, 18, 18, 220)
            : PotisInfused ? new Color(135, 36, 18, 225) : new Color(90, 10, 10, 215);

    protected override Color Foreground => IsHaymaker
        ? PotisInfused ? new Color(255, 238, 178, 225) : new Color(255, 210, 150, 210)
        : IsFinisher
            ? PotisInfused ? new Color(255, 230, 170, 210) : new Color(255, 225, 185, 195)
            : PotisInfused ? new Color(255, 198, 132, 205) : new Color(255, 150, 150, 185);

    protected override int SpawnDustType => PotisInfused ? DustID.WhiteTorch : DustID.Smoke;
    protected override Color SpawnDustColor => PotisInfused ? new Color(255, 210, 135) : new Color(255, 190, 135);
    protected override int TrailDustType => PotisInfused ? DustID.Torch : DustID.Torch;
    protected override Color TrailDustColor => PotisInfused ? new Color(255, 205, 112) : new Color(255, 170, 95);
    protected override Vector3 LightEmission => PotisInfused ? new Vector3(1.18f, 0.58f, 0.16f) : new Vector3(1f, 0.45f, 0.18f);
    protected override int ImpactDustType => PotisInfused ? DustID.WhiteTorch : DustID.Smoke;
    protected override Color ImpactDustColor => PotisInfused ? new Color(255, 218, 150) : new Color(220, 155, 100);
    protected override int SpawnDustBurstCount => PotisInfused ? 18 : 14;
    protected override int TrailDustChance => PotisInfused ? 1 : 2;
    protected override int ImpactDustBurstCount => PotisInfused ? 22 : 16;

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.width = 38;
        Projectile.height = 30;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        if (IsHaymaker) {
            Projectile.penetrate = 1;
            Projectile.localNPCHitCooldown = -1;
        }
        else if (PotisInfused) {
            Projectile.localNPCHitCooldown = 6;
        }

        base.AI();
    }

    protected override Vector2 GetShoulderOffset(Player owner, Vector2 direction, float scale) {
        float potisReach = PotisInfused ? 1.08f : 1f;
        return Variant switch {
            1 => new Vector2(owner.direction * 15f * scale * potisReach, -8f * scale),
            2 => new Vector2(owner.direction * 14f * scale * potisReach, -2f * scale),
            >= 3 => new Vector2(owner.direction * 16f * scale * potisReach, -4f * scale),
            _ => new Vector2(owner.direction * 12f * scale * potisReach, 1f * scale)
        };
    }

    protected override float GetExtension(float progress, float scale) {
        float potisReach = PotisInfused ? 1.16f : 1f;
        if (IsHaymaker) {
            float chargeCurve = progress < 0.45f
                ? progress / 0.45f
                : 1f - (progress - 0.45f) / 0.55f * 0.18f;
            return MathHelper.Lerp(18f, 66f * scale * potisReach, MathHelper.Clamp(chargeCurve, 0f, 1f));
        }

        if (IsFinisher) {
            float cleaveCurve = progress < 0.28f
                ? progress / 0.28f
                : 1f - (progress - 0.28f) / 0.72f * 0.28f;
            return MathHelper.Lerp(18f, 58f * scale * potisReach, MathHelper.Clamp(cleaveCurve, 0f, 1f));
        }

        float extensionCurve = progress < 0.24f ? progress / 0.24f : 1f - (progress - 0.24f) / 0.76f * 0.34f;
        return MathHelper.Lerp(18f, 52f * scale * potisReach, MathHelper.Clamp(extensionCurve, 0f, 1f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 lineStart = GetPunchAnchor(owner, direction, Projectile.scale);
        Vector2 lineEnd = Projectile.Center + direction * (IsHaymaker ? 18f : 16f) * Projectile.scale;
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        float collisionPoint = 0f;
        float potisWidth = PotisInfused ? 1.18f : 1f;
        float collisionWidth = BaseCollisionWidth * Projectile.scale * (IsHaymaker ? 1.55f : IsFinisher ? 1.3f : 1f) *
                               potisWidth;
        float lateralOffset = SideCollisionOffset * Projectile.scale * (IsHaymaker ? 1.9f : IsFinisher ? 1.45f : 1f) *
                              potisWidth;
        float wingWidth = collisionWidth * (IsHaymaker ? 0.92f : 0.74f);

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
                   collisionWidth, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   lineStart + perpendicular * lateralOffset,
                   lineEnd + perpendicular * lateralOffset * 0.55f,
                   wingWidth, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   lineStart - perpendicular * lateralOffset,
                   lineEnd - perpendicular * lateralOffset * 0.55f,
                   wingWidth, ref collisionPoint);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float potisPush = PotisInfused ? 1.18f : 1f;
        float pushStrength = (IsHaymaker ? 9.6f : IsFinisher ? 6.8f : 4.8f) * potisPush;
        float liftStrength = (IsHaymaker ? 3.2f : IsFinisher ? 2.3f : 1.6f) * (PotisInfused ? 1.12f : 1f);
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + pushDirection.X * pushStrength, -16f, 16f),
            MathHelper.Clamp(target.velocity.Y - liftStrength, -10f, 10f));
        target.netUpdate = true;
    }
}
