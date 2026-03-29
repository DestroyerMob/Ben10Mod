using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WildVineGasCloudProjectile : ModProjectile {
    private const int RegularLifetime = 4 * 60;
    private const int BloomLifetime = 6 * 60;
    private const float BaseRadius = 24f;
    private const float RegularMaxRadius = 92f;
    private const float BloomMaxRadius = 124f;

    private bool IsBloomVariant => Projectile.ai[0] >= WildVineBomb.VariantBloom;
    private int MaxLifetime => IsBloomVariant ? BloomLifetime : RegularLifetime;
    private float MaxRadius => IsBloomVariant ? BloomMaxRadius : RegularMaxRadius;

    private float CurrentRadius {
        get {
            float progress = 1f - MathHelper.Clamp(Projectile.timeLeft / (float)MaxLifetime, 0f, 1f);
            return MathHelper.Lerp(BaseRadius, MaxRadius, progress);
        }
    }

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = BloomLifetime;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.timeLeft = MaxLifetime;
        Projectile.localNPCHitCooldown = IsBloomVariant ? 12 : 18;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += IsBloomVariant ? 0.022f : 0.015f;
        Lighting.AddLight(Projectile.Center, IsBloomVariant
            ? new Vector3(0.18f, 0.34f, 0.1f)
            : new Vector3(0.12f, 0.26f, 0.08f));

        int dustChance = IsBloomVariant ? 1 : 2;
        if (Main.rand.NextBool(dustChance)) {
            Vector2 offset = Main.rand.NextVector2Circular(CurrentRadius * 0.42f, CurrentRadius * 0.34f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool() ? DustID.Poisoned : DustID.Grass,
                new Vector2(Main.rand.NextFloat(-0.35f, 0.35f), Main.rand.NextFloat(-0.8f, -0.15f)),
                105, IsBloomVariant ? new Color(180, 240, 125) : new Color(165, 225, 110),
                Main.rand.NextFloat(0.95f, 1.18f));
            dust.noGravity = true;
        }

        int sporeChance = IsBloomVariant ? 3 : 5;
        if (Main.rand.NextBool(sporeChance)) {
            Vector2 sporeOffset = Main.rand.NextVector2Circular(CurrentRadius * 0.3f, CurrentRadius * 0.26f);
            Dust spore = Dust.NewDustPerfect(Projectile.Center + sporeOffset, DustID.JunglePlants,
                new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-0.5f, -0.12f)),
                120, new Color(120, 185, 85), Main.rand.NextFloat(0.85f, 1.05f));
            spore.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float lifetimeProgress = 1f - Projectile.timeLeft / (float)MaxLifetime;
        float fadeIn = Utils.GetLerpValue(0f, 0.15f, lifetimeProgress, true);
        float fadeOut = Utils.GetLerpValue(0f, 0.25f, Projectile.timeLeft / (float)MaxLifetime, true);
        float opacity = fadeIn * fadeOut;
        float bloomScale = IsBloomVariant ? 1.18f : 1f;

        DrawRing(pixel, center, CurrentRadius * 0.82f, 4.4f * bloomScale,
            new Color(126, 188, 92, IsBloomVariant ? 88 : 70) * opacity, Projectile.rotation);
        DrawRing(pixel, center, CurrentRadius * 0.58f, 4f * bloomScale,
            new Color(172, 232, 118, IsBloomVariant ? 98 : 82) * opacity, -Projectile.rotation * 1.15f);
        DrawRing(pixel, center, CurrentRadius * 0.34f, 3.2f * bloomScale,
            new Color(205, 255, 165, IsBloomVariant ? 108 : 92) * opacity, Projectile.rotation * 1.6f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity *= IsBloomVariant ? 0.82f : 0.9f;
        target.AddBuff(BuffID.Poisoned, IsBloomVariant ? 6 * 60 : 4 * 60);
        if (IsBloomVariant)
            target.AddBuff(BuffID.Venom, 2 * 60);

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
