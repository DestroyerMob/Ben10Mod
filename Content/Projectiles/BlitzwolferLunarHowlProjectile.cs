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

public class BlitzwolferLunarHowlProjectile : ModProjectile {
    private const int MaxLifetime = 44;
    private const float StartRadius = 34f;
    private const float EndRadius = 156f;

    private float CurrentRadius => MathHelper.Lerp(StartRadius, EndRadius, 1f - Projectile.timeLeft / (float)MaxLifetime);

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxLifetime;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 22;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 0.8f }, Projectile.Center);
        }

        Projectile.rotation = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
        Projectile.velocity *= 0.985f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.82f, 0.84f, 1f) * 0.7f);

        if (Main.rand.NextBool(2)) {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 spawnPosition = Projectile.Center + direction.RotatedByRandom(1.08f) * Main.rand.NextFloat(CurrentRadius * 0.28f, CurrentRadius);
            Dust dust = Dust.NewDustPerfect(spawnPosition, Main.rand.NextBool(3) ? DustID.GemDiamond : DustID.Smoke,
                direction.RotatedByRandom(0.65f) * Main.rand.NextFloat(0.8f, 2.2f), 115, new Color(235, 245, 255),
                Main.rand.NextFloat(1f, 1.26f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        float rotation = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
        float progress = 1f - Projectile.timeLeft / (float)MaxLifetime;
        float opacity = Utils.GetLerpValue(0f, 0.12f, progress, true) *
            Utils.GetLerpValue(0f, 0.28f, Projectile.timeLeft / (float)MaxLifetime, true);

        DrawArc(pixel, Projectile.Center, CurrentRadius, 5.2f, 1.08f, rotation, new Color(145, 175, 220, 90) * opacity);
        DrawArc(pixel, Projectile.Center, CurrentRadius * 0.78f, 4.1f, 1.22f, rotation, new Color(225, 235, 255, 130) * opacity);
        DrawArc(pixel, Projectile.Center, CurrentRadius * 0.52f, 3.2f, 1.34f, rotation, new Color(255, 255, 255, 170) * opacity);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 pushDirection = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
        target.velocity = Vector2.Lerp(target.velocity, pushDirection * 8f, 0.5f);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 180);
        target.AddBuff(BuffID.Confused, 180);
        target.AddBuff(BuffID.BrokenArmor, 180);
        target.netUpdate = true;
    }

    private static void DrawArc(Texture2D pixel, Vector2 center, float radius, float thickness, float arcHalfWidth,
        float rotation, Color color) {
        const int Segments = 16;
        for (int i = 0; i < Segments; i++) {
            float completion = i / (float)(Segments - 1);
            float angle = rotation + MathHelper.Lerp(-arcHalfWidth, arcHalfWidth, completion);
            Vector2 position = center + angle.ToRotationVector2() * radius - Main.screenPosition;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.8f), SpriteEffects.None, 0);
        }
    }
}
