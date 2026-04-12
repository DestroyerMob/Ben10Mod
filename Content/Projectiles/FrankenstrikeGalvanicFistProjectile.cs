using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.Frankenstrike;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FrankenstrikeGalvanicFistProjectile : PunchProjectile {
    private int ComboIndex => Utils.Clamp((int)Projectile.ai[1], 0, 2);
    private bool IsFinisher => ComboIndex >= 2;

    protected override Color Background => IsFinisher
        ? new Color(110, 182, 255, 225)
        : new Color(80, 140, 235, 215);

    protected override Color Foreground => IsFinisher
        ? new Color(245, 250, 255, 210)
        : new Color(215, 240, 255, 190);

    protected override int SpawnDustType => DustID.Electric;
    protected override Color SpawnDustColor => new(150, 210, 255);
    protected override int TrailDustType => DustID.BlueTorch;
    protected override Color TrailDustColor => new(155, 215, 255);
    protected override Vector3 LightEmission => new(0.18f, 0.42f, 0.86f);
    protected override int ImpactDustType => DustID.Electric;
    protected override Color ImpactDustColor => new(200, 235, 255);

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.width = 34;
        Projectile.height = 28;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.localNPCHitCooldown = 8;
    }

    protected override Vector2 GetShoulderOffset(Player owner, Vector2 direction, float scale) {
        return ComboIndex switch {
            1 => new Vector2(owner.direction * 14f * scale, -7f * scale),
            2 => new Vector2(owner.direction * 16f * scale, -2f * scale),
            _ => new Vector2(owner.direction * 11f * scale, 1f * scale)
        };
    }

    protected override float GetExtension(float progress, float scale) {
        if (IsFinisher) {
            float curve = progress < 0.26f
                ? progress / 0.26f
                : 1f - (progress - 0.26f) / 0.74f * 0.24f;
            return MathHelper.Lerp(18f, 62f * scale, MathHelper.Clamp(curve, 0f, 1f));
        }

        float extensionCurve = progress < 0.24f
            ? progress / 0.24f
            : 1f - (progress - 0.24f) / 0.76f * 0.34f;
        return MathHelper.Lerp(16f, (ComboIndex == 1 ? 54f : 48f) * scale, MathHelper.Clamp(extensionCurve, 0f, 1f));
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
        float collisionWidth = IsFinisher ? 22f : 18f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
                   collisionWidth * Projectile.scale, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   lineStart + perpendicular * 8f * Projectile.scale,
                   lineEnd + perpendicular * 4f * Projectile.scale,
                   collisionWidth * 0.52f * Projectile.scale, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   lineStart - perpendicular * 8f * Projectile.scale,
                   lineEnd - perpendicular * 4f * Projectile.scale,
                   collisionWidth * 0.52f * Projectile.scale, ref collisionPoint);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.IsFrankenstrikeOverchargedFor(Projectile.owner))
            modifiers.SourceDamage *= IsFinisher ? 1.14f : 1.08f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return;

        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float pushStrength = IsFinisher ? 7.2f : ComboIndex == 1 ? 5.2f : 4.4f;
        float liftStrength = IsFinisher ? 2.5f : 1.6f;
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + pushDirection.X * pushStrength, -15f, 15f),
            MathHelper.Clamp(target.velocity.Y - liftStrength, -9f, 9f));
        target.netUpdate = true;

        FrankenstrikeTransformation.ApplyConductiveHit(owner, target, IsFinisher ? 2 : 1, 240);

        FrankenstrikeStatePlayer state = owner.GetModPlayer<FrankenstrikeStatePlayer>();
        if (IsFinisher && Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;
            FrankenstrikeTransformation.SpawnThunderclap(owner, Projectile.GetSource_FromThis(), target.Center,
                System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.52f)), Projectile.knockBack + 0.9f, 0.92f);
            FrankenstrikeTransformation.TryConsumeOvercharged(owner, target, Projectile.GetSource_FromThis(),
                System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.72f)), Projectile.knockBack + 1.1f,
                chainBurst: false, lightningStrike: false);
        }
        else if (state.StormheartActive && Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            FrankenstrikeTransformation.SpawnThunderclap(owner, Projectile.GetSource_FromThis(), target.Center,
                System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.26f)), Projectile.knockBack + 0.3f,
                0.7f, empowered: true);
        }
    }
}
