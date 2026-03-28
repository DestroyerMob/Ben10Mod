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

public class PeskyDustLullabyCloudProjectile : ModProjectile {
    private const int MaxLifetime = 42;
    private const float BaseRadius = 24f;
    private const float MaxRadius = 86f;

    private bool Drifting => Projectile.ai[0] >= 0.5f;
    private float CurrentRadius => MathHelper.Lerp(BaseRadius, Drifting ? MaxRadius + 10f : MaxRadius,
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
            SoundEngine.PlaySound(SoundID.Item9 with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
        }

        Projectile.rotation += 0.04f;
        Projectile.velocity *= Drifting ? 0.96f : 0.94f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.95f, 0.78f, 0.95f) * 0.4f);

        if (Main.rand.NextBool(2)) {
            Vector2 offset = Main.rand.NextVector2Circular(CurrentRadius * 0.45f, CurrentRadius * 0.35f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool() ? DustID.GoldFlame : DustID.PinkFairy,
                offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.4f, 0.4f),
                110, new Color(255, 220, 205), Main.rand.NextFloat(0.8f, 1.06f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float opacity = Utils.GetLerpValue(0f, 0.14f, 1f - Projectile.timeLeft / (float)MaxLifetime, true) *
            Utils.GetLerpValue(0f, 0.25f, Projectile.timeLeft / (float)MaxLifetime, true);

        DrawRing(pixel, center, CurrentRadius * 0.84f, 3.8f, new Color(255, 215, 130, 70) * opacity, Projectile.rotation);
        DrawRing(pixel, center, CurrentRadius * 0.56f, 3.4f, new Color(255, 175, 225, 90) * opacity, -Projectile.rotation * 1.2f);
        DrawRing(pixel, center, CurrentRadius * 0.28f, 2.8f, new Color(205, 245, 255, 110) * opacity, Projectile.rotation * 1.5f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity *= 0.84f;
        target.AddBuff(BuffID.Confused, 120);
        target.netUpdate = true;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 16;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.2f), SpriteEffects.None, 0);
        }
    }
}
