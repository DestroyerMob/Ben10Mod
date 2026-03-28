using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArctiguanaFreezeRayProjectile : ModProjectile {
    private const float BaseRayLength = 42f;
    private const float BaseRayWidth = 8.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 6;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 90;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.velocity.LengthSquared() < 784f)
            Projectile.velocity *= 1.014f;

        float speedProgress = Utils.GetLerpValue(8f, 24f, Projectile.velocity.Length(), true);
        Projectile.scale = MathHelper.Lerp(0.94f, 1.14f, speedProgress);
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.direction = Projectile.velocity.X >= 0f ? 1 : -1;
        Lighting.AddLight(Projectile.Center, new Vector3(0.18f, 0.56f, 0.82f));

        if (Main.rand.NextBool(2)) {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 dustPosition = Projectile.Center - direction * Main.rand.NextFloat(10f, 18f) +
                normal * Main.rand.NextFloat(-4f, 4f);

            Dust dust = Dust.NewDustPerfect(dustPosition,
                Main.rand.NextBool(4) ? DustID.IceTorch : DustID.Frost,
                -Projectile.velocity * Main.rand.NextFloat(0.03f, 0.1f), 100, new Color(155, 235, 255),
                Main.rand.NextFloat(0.9f, 1.18f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        GetRayShape(out Vector2 tail, out Vector2 tip, out float width);
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            tail,
            tip,
            width * 0.78f,
            ref collisionPoint
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            Vector2 oldCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f;
            float trailOpacity = (Projectile.oldPos.Length - i) / (float)(Projectile.oldPos.Length + 1) * 0.28f;
            float trailScale = Projectile.scale * MathHelper.Lerp(0.82f, 0.96f, i / (float)Projectile.oldPos.Length);
            DrawIceRay(pixel, oldCenter, Projectile.velocity.SafeNormalize(Vector2.UnitX), trailScale, trailOpacity);
        }

        DrawIceRay(pixel, Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), Projectile.scale, Projectile.Opacity);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Frostburn2, 120);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 75);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Vector2 burstVelocity = Projectile.velocity.RotatedByRandom(0.48f) * Main.rand.NextFloat(0.08f, 0.18f) +
                Main.rand.NextVector2Circular(1.2f, 1.2f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.IceTorch : DustID.Frost,
                burstVelocity, 95, new Color(175, 240, 255), Main.rand.NextFloat(0.95f, 1.22f));
            dust.noGravity = true;
        }
    }

    private void GetRayShape(out Vector2 tail, out Vector2 tip, out float width) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float length = BaseRayLength * Projectile.scale;
        width = BaseRayWidth * Projectile.scale;
        tail = Projectile.Center - direction * length * 0.42f;
        tip = Projectile.Center + direction * length * 0.58f;
    }

    private static void DrawIceRay(Texture2D pixel, Vector2 center, Vector2 direction, float scale, float opacity) {
        float length = BaseRayLength * scale;
        float width = BaseRayWidth * scale;
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 tail = center - direction * length * 0.42f;
        Vector2 tip = center + direction * length * 0.58f;
        Vector2 bodyStart = tail + direction * length * 0.1f;
        Vector2 spineEnd = tip - direction * length * 0.04f;

        DrawShard(pixel, tail, tip, width * 1.55f, new Color(92, 170, 235, 95), opacity);
        DrawShard(pixel, bodyStart, tip, width * 1.02f, new Color(150, 225, 255, 165), opacity);
        DrawShard(pixel, bodyStart + direction * 2f * scale, spineEnd, width * 0.44f, new Color(245, 252, 255, 235), opacity);

        float[] finPositions = { 0.22f, 0.48f, 0.7f };
        for (int i = 0; i < finPositions.Length; i++) {
            float side = i % 2 == 0 ? 1f : -1f;
            Vector2 pivot = Vector2.Lerp(tail, tip, finPositions[i]);
            Vector2 finDirection = direction.RotatedBy(side * MathHelper.Pi * 0.28f);
            float finLength = length * (0.18f - i * 0.025f);

            DrawShard(pixel, pivot, pivot + finDirection * finLength, width * 0.42f, new Color(175, 232, 255, 150), opacity);
            DrawShard(pixel, pivot, pivot + finDirection * finLength * 0.55f, width * 0.2f, new Color(245, 252, 255, 210), opacity);
        }

        Vector2 headBase = tip - direction * length * 0.18f;
        DrawShard(pixel, headBase - normal * width * 0.16f, tip, width * 0.62f, new Color(205, 240, 255, 180), opacity);
        DrawShard(pixel, headBase + normal * width * 0.16f, tip, width * 0.62f, new Color(205, 240, 255, 180), opacity);
        DrawShard(pixel, headBase, tip + direction * 2f * scale, width * 0.24f, Color.White, opacity);
    }

    private static void DrawShard(Texture2D pixel, Vector2 start, Vector2 end, float width, Color color, float opacity) {
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length <= 0.5f)
            return;

        float rotation = delta.ToRotation();
        Main.EntitySpriteDraw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color * opacity,
            rotation, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0);
    }
}
