using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WildVineGasCloudProjectile : ModProjectile {
    private const int Lifetime = 4 * 60;
    private const float BaseRadius = 18f;
    private const float MaxRadius = 70f;

    private float CurrentRadius => MathHelper.Lerp(BaseRadius, MaxRadius,
        1f - Projectile.timeLeft / (float)Lifetime);

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += 0.015f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.12f, 0.26f, 0.08f));

        if (Main.rand.NextBool(2)) {
            Vector2 offset = Main.rand.NextVector2Circular(CurrentRadius * 0.42f, CurrentRadius * 0.34f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool() ? DustID.Poisoned : DustID.Grass,
                new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.7f, -0.15f)),
                105, new Color(165, 225, 110), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }

        if (Main.rand.NextBool(5)) {
            Vector2 sporeOffset = Main.rand.NextVector2Circular(CurrentRadius * 0.3f, CurrentRadius * 0.26f);
            Dust spore = Dust.NewDustPerfect(Projectile.Center + sporeOffset, DustID.JunglePlants,
                new Vector2(Main.rand.NextFloat(-0.15f, 0.15f), Main.rand.NextFloat(-0.45f, -0.1f)),
                120, new Color(120, 185, 85), Main.rand.NextFloat(0.8f, 1f));
            spore.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float lifetimeProgress = 1f - Projectile.timeLeft / (float)Lifetime;
        float fadeIn = Utils.GetLerpValue(0f, 0.15f, lifetimeProgress, true);
        float fadeOut = Utils.GetLerpValue(0f, 0.25f, Projectile.timeLeft / (float)Lifetime, true);
        float opacity = fadeIn * fadeOut;

        DrawRing(pixel, center, CurrentRadius * 0.82f, 4.4f, new Color(126, 188, 92, 70) * opacity, Projectile.rotation);
        DrawRing(pixel, center, CurrentRadius * 0.58f, 4f, new Color(172, 232, 118, 82) * opacity, -Projectile.rotation * 1.15f);
        DrawRing(pixel, center, CurrentRadius * 0.34f, 3.2f, new Color(205, 255, 165, 92) * opacity, Projectile.rotation * 1.6f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity *= 0.9f;
        target.AddBuff(BuffID.Poisoned, 4 * 60);
        target.netUpdate = true;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 16;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.2f), SpriteEffects.None, 0f);
        }
    }
}
