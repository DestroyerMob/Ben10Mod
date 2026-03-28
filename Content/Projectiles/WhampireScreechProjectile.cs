using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WhampireScreechProjectile : ModProjectile {
    private const int MaxLifetime = 30;
    private const float BaseRadius = 28f;
    private const float MaxRadius = 92f;

    private bool Cloaked => Projectile.ai[0] >= 0.5f;
    private float CurrentRadius => MathHelper.Lerp(BaseRadius, Cloaked ? MaxRadius + 12f : MaxRadius,
        1f - Projectile.timeLeft / (float)MaxLifetime);

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
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.55f, Volume = 0.7f }, Projectile.Center);
        }

        Projectile.rotation = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
        Projectile.velocity *= 0.96f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.72f, 0.08f, 0.14f) * 0.52f);

        if (Main.rand.NextBool(2)) {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 spawnPosition = Projectile.Center + direction.RotatedByRandom(0.9f) *
                Main.rand.NextFloat(CurrentRadius * 0.32f, CurrentRadius);
            Dust dust = Dust.NewDustPerfect(spawnPosition, Main.rand.NextBool() ? DustID.Shadowflame : DustID.Smoke,
                direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.35f, 1f), 120, new Color(145, 30, 40),
                Main.rand.NextFloat(0.85f, 1.08f));
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
            Utils.GetLerpValue(0f, 0.26f, Projectile.timeLeft / (float)MaxLifetime, true);

        DrawArc(pixel, Projectile.Center, CurrentRadius, 4f, 0.95f, rotation, new Color(70, 18, 24, 92) * opacity);
        DrawArc(pixel, Projectile.Center, CurrentRadius * 0.7f, 3.6f, 1.08f, rotation, new Color(150, 24, 38, 110) * opacity);
        DrawArc(pixel, Projectile.Center, CurrentRadius * 0.42f, 2.8f, 1.2f, rotation, new Color(235, 108, 118, 125) * opacity);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Confused, Cloaked ? 180 : 150);
        target.AddBuff(BuffID.Bleeding, Cloaked ? 150 : 105);
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
                new Vector2(thickness, thickness * 2.5f), SpriteEffects.None, 0);
        }
    }
}
