using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SwampfireVineProjectile : ModProjectile {
    private const float VineWaveAmplitude = 16f;
    private const float VineWaveFrequency = 1.65f;
    private const float VineWaveSecondaryAmplitude = 5f;
    private const float VineWaveSpeed = 0.14f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 68;
        Projectile.height = 126;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        Projectile.frameCounter++;

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustPerfect(Projectile.Bottom + new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-12f, 4f)),
                    i % 3 == 0 ? DustID.Torch : DustID.Grass, Main.rand.NextVector2Circular(1.4f, 1.4f), 90,
                    Color.White, 1.15f);
                dust.noGravity = true;
            }
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.34f, 0.85f, 0.18f) * 0.75f);
        for (int i = 0; i < 5; i++)
            SpawnVineDust(Main.rand.NextFloat(), Main.rand.NextBool() ? DustID.Grass : DustID.JunglePlants,
                new Color(130, 230, 90), 1f, 6f);

        if (Main.rand.NextBool())
            SpawnVineDust(Main.rand.NextFloat(), DustID.JungleGrass, new Color(110, 215, 90), 0.95f, 9f, phaseOffset: 0.55f, amplitudeScale: 1.08f);

        if (Main.rand.NextBool(3)) {
            float emberProgress = Main.rand.NextFloat(0.02f, 0.35f);
            Vector2 emberPos = GetVineCurvePoint(emberProgress, phaseOffset: 0.25f, amplitudeScale: 0.7f)
                + Main.rand.NextVector2Circular(4f, 6f);
            Dust ember = Dust.NewDustPerfect(emberPos, DustID.Torch,
                GetVineCurveTangent(emberProgress, phaseOffset: 0.25f, amplitudeScale: 0.7f) * Main.rand.NextFloat(0.15f, 0.45f)
                + Main.rand.NextVector2Circular(0.22f, 0.22f), 120, new Color(255, 150, 70), 0.9f);
            ember.noGravity = true;
        }
    }

    private Vector2 GetVineCurvePoint(float progress, float phaseOffset = 0f, float amplitudeScale = 1f) {
        progress = MathHelper.Clamp(progress, 0f, 1f);

        float waveTime = Projectile.frameCounter * VineWaveSpeed + phaseOffset;
        float vineHeight = Projectile.height - 8f;
        float swayStrength = 0.2f + 0.8f * progress;
        float primaryOffset = MathF.Sin(progress * MathHelper.TwoPi * VineWaveFrequency + waveTime) *
            VineWaveAmplitude * amplitudeScale * swayStrength;
        float secondaryOffset = MathF.Sin(progress * MathHelper.TwoPi * 0.75f - waveTime * 0.7f) *
            VineWaveSecondaryAmplitude * amplitudeScale * progress;

        return Projectile.Bottom + new Vector2(primaryOffset + secondaryOffset, -progress * vineHeight);
    }

    private Vector2 GetVineCurveTangent(float progress, float phaseOffset = 0f, float amplitudeScale = 1f) {
        float step = 0.02f;
        Vector2 start = GetVineCurvePoint(progress - step, phaseOffset, amplitudeScale);
        Vector2 end = GetVineCurvePoint(progress + step, phaseOffset, amplitudeScale);
        Vector2 tangent = end - start;

        return tangent.LengthSquared() > 0.001f ? Vector2.Normalize(tangent) : -Vector2.UnitY;
    }

    private void SpawnVineDust(float progress, int dustType, Color color, float scale, float width, float phaseOffset = 0f, float amplitudeScale = 1f) {
        Vector2 curvePoint = GetVineCurvePoint(progress, phaseOffset, amplitudeScale);
        Vector2 tangent = GetVineCurveTangent(progress, phaseOffset, amplitudeScale);
        Vector2 normal = new(-tangent.Y, tangent.X);
        Vector2 dustPos = curvePoint + normal * Main.rand.NextFloat(-width, width);
        Vector2 drift = tangent * Main.rand.NextFloat(0.12f, 0.35f) + normal * Main.rand.NextFloat(-0.16f, 0.16f);

        Dust dust = Dust.NewDustPerfect(dustPos, dustType, drift, 120, color, scale);
        dust.noGravity = true;
    }

    public override bool? CanHitNPC(NPC target) {
        return !target.friendly && target.CanBeChasedBy(Projectile);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 basePos = Projectile.Bottom - Main.screenPosition;
        float growth = Utils.GetLerpValue(0f, 18f, 600 - Projectile.timeLeft, true);
        float fade = Utils.GetLerpValue(0f, 30f, Projectile.timeLeft, true);
        float opacity = growth * fade;

        for (int i = -4; i <= 4; i++) {
            float bend = i * 8f;
            float width = i == 0 ? 18f : (System.Math.Abs(i) == 1 ? 14f : 11f);
            float height = i == 0 ? 118f : (System.Math.Abs(i) == 1 ? 110f : 102f);
            Vector2 center = basePos + new Vector2(bend, -height * 0.5f);
            Color outerColor = new Color(40, 120, 28, 225) * opacity;
            Color innerColor = new Color(145, 255, 120, 205) * opacity;

            Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), outerColor,
                0.08f * i, new Vector2(0.5f, 0.5f), new Vector2(width, height), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(pixel, center + new Vector2(0f, -6f), new Rectangle(0, 0, 1, 1), innerColor,
                0.08f * i, new Vector2(0.5f, 0.5f), new Vector2(width * 0.42f, height * 0.82f), SpriteEffects.None, 0f);
        }

        for (int i = -3; i <= 3; i++) {
            Vector2 leafCenter = basePos + new Vector2(i * 12f, -58f - System.Math.Abs(i) * 8f);
            Main.spriteBatch.Draw(pixel, leafCenter, new Rectangle(0, 0, 1, 1), new Color(90, 200, 70, 180) * opacity,
                0.65f * i, new Vector2(0.5f, 0.5f), new Vector2(8f, 22f), SpriteEffects.None, 0f);
        }

        for (int i = -2; i <= 2; i++) {
            Vector2 crossLeafCenter = basePos + new Vector2(i * 10f, -32f - System.Math.Abs(i) * 7f);
            Main.spriteBatch.Draw(pixel, crossLeafCenter, new Rectangle(0, 0, 1, 1), new Color(120, 235, 100, 150) * opacity,
                -0.5f * i, new Vector2(0.5f, 0.5f), new Vector2(7f, 18f), SpriteEffects.None, 0f);
        }

        Main.spriteBatch.Draw(pixel, basePos + new Vector2(0f, -10f), new Rectangle(0, 0, 1, 1), new Color(255, 135, 50, 170) * opacity,
            0f, new Vector2(0.5f, 0.5f), new Vector2(38f, 14f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 180);
        target.AddBuff(BuffID.Poisoned, 180);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 50f),
                i % 2 == 0 ? DustID.Grass : DustID.Torch, Main.rand.NextVector2Circular(2.2f, 2.2f),
                120, Color.White, 1.05f);
            dust.noGravity = true;
        }
    }
}
