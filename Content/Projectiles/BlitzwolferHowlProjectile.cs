using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BlitzwolferHowlProjectile : ModProjectile {
    private const int MaxLifetime = 30;
    private const float BaseRadius = 24f;
    private const float MaxRadius = 82f;

    private float CurrentRadius => MathHelper.Lerp(BaseRadius, MaxRadius, 1f - Projectile.timeLeft / (float)MaxLifetime);

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxLifetime;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 16;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.3f, Volume = 0.6f }, Projectile.Center);
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity *= 0.97f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.72f, 0.75f, 0.92f) * 0.55f);

        if (Main.rand.NextBool(2)) {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 spawnPosition = Projectile.Center + direction.RotatedByRandom(0.85f) * Main.rand.NextFloat(CurrentRadius * 0.3f, CurrentRadius);
            Dust dust = Dust.NewDustPerfect(spawnPosition, Main.rand.NextBool(3) ? DustID.GemDiamond : DustID.Smoke,
                direction.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.4f, 1.2f), 110, new Color(235, 245, 255),
                Main.rand.NextFloat(0.9f, 1.18f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        float rotation = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
        float opacity = Utils.GetLerpValue(0f, 0.18f, 1f - Projectile.timeLeft / (float)MaxLifetime, true) *
            Utils.GetLerpValue(0f, 0.28f, Projectile.timeLeft / (float)MaxLifetime, true);

        DrawArc(pixel, Projectile.Center, CurrentRadius, 3.6f, 0.95f, rotation, new Color(145, 180, 220, 90) * opacity);
        DrawArc(pixel, Projectile.Center, CurrentRadius * 0.72f, 3f, 1.08f, rotation, new Color(230, 240, 255, 130) * opacity);
        DrawArc(pixel, Projectile.Center, CurrentRadius * 0.46f, 2.4f, 1.18f, rotation, new Color(255, 255, 255, 160) * opacity);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 120);
        target.AddBuff(BuffID.Confused, 90);
        target.AddBuff(BuffID.BrokenArmor, 90);
        target.netUpdate = true;
    }

    private static void DrawArc(Texture2D pixel, Vector2 center, float radius, float thickness, float arcHalfWidth,
        float rotation, Color color) {
        const int Segments = 12;
        for (int i = 0; i < Segments; i++) {
            float completion = i / (float)(Segments - 1);
            float angle = rotation + MathHelper.Lerp(-arcHalfWidth, arcHalfWidth, completion);
            Vector2 position = center + angle.ToRotationVector2() * radius - Main.screenPosition;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.6f), SpriteEffects.None, 0);
        }
    }
}
