using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BlitzwolferSonicBoltProjectile : ModProjectile {
    private const int MaxLifetime = 72;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = MaxLifetime;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.velocity.LengthSquared() < 625f)
            Projectile.velocity *= 1.01f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.72f, 0.82f, 0.95f) * 0.45f);

        if (Main.rand.NextBool(2)) {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 offset = perpendicular * Main.rand.NextFloatDirection() * Main.rand.NextFloat(4f, 9f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? DustID.GemDiamond : DustID.Smoke,
                Projectile.velocity * 0.04f, 120, new Color(235, 245, 255), Main.rand.NextFloat(0.85f, 1.05f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 lineStart = Projectile.Center - direction * 14f;
        Vector2 lineEnd = Projectile.Center + direction * 16f;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, 12f,
            ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float rotation = direction.ToRotation();

        DrawArc(pixel, Projectile.Center - direction * 4f, 12f, 2.8f, 0.55f, rotation, new Color(175, 220, 255, 110));
        DrawArc(pixel, Projectile.Center + direction * 2f, 18f, 2.2f, 0.65f, rotation, new Color(235, 245, 255, 140));
        DrawArc(pixel, Projectile.Center + direction * 6f, 24f, 1.8f, 0.75f, rotation, new Color(145, 170, 210, 95));
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 75);
        target.AddBuff(BuffID.Confused, 45);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 9; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GemDiamond : DustID.Smoke,
                Main.rand.NextVector2Circular(2.6f, 2.6f), 100, new Color(235, 245, 255), Main.rand.NextFloat(0.85f, 1.1f));
            dust.noGravity = true;
        }
    }

    private static void DrawArc(Texture2D pixel, Vector2 center, float radius, float thickness, float arcHalfWidth,
        float rotation, Color color) {
        const int Segments = 9;
        for (int i = 0; i < Segments; i++) {
            float completion = i / (float)(Segments - 1);
            float angle = rotation + MathHelper.Lerp(-arcHalfWidth, arcHalfWidth, completion);
            Vector2 position = center + angle.ToRotationVector2() * radius - Main.screenPosition;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.2f), SpriteEffects.None, 0);
        }
    }
}
