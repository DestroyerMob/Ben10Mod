using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Ben10Mod.Content.Projectiles;

internal static class VanillaBeamDrawHelper {
    public const int LastPrismFrameCount = 3;

    public static void DrawLastPrismBeam(Vector2 start, Vector2 direction, float length, Color beamColor, Color highlightColor,
        Vector2 startScale, Vector2 outerScale, Vector2 midScale, Vector2 innerScale,
        float outerOpacity = 0.18f, float midOpacity = 0.32f, float innerOpacity = 0.58f,
        float beamColorIntensity = 1.25f) {
        if (direction.LengthSquared() < 0.0001f || length <= 4f)
            return;

        direction.Normalize();

        Texture2D texture = TextureAssets.Projectile[ProjectileID.LastPrismLaser].Value;
        int frameHeight = texture.Height / LastPrismFrameCount;
        int frameWidth = texture.Width;

        Rectangle startFrame = new(0, 0, frameWidth, frameHeight);
        Rectangle midFrame = new(0, frameHeight, frameWidth, frameHeight);
        Rectangle endFrame = new(0, frameHeight * 2, frameWidth, frameHeight);

        float rotation = direction.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;
        Vector2 origin = new(frameWidth * 0.5f, frameHeight * 0.5f);

        float t = Main.GlobalTimeWrappedHourly;
        float pulse = 0.88f + 0.12f * (float)Math.Sin(t * 10f);
        float shimmer = 0.82f + 0.18f * (float)Math.Sin(t * 6.5f);
        Color baseColor = beamColor * (shimmer * beamColorIntensity);

        Main.EntitySpriteDraw(
            texture,
            start - Main.screenPosition,
            startFrame,
            baseColor,
            rotation,
            origin,
            startScale * new Vector2(pulse, 1f),
            SpriteEffects.None,
            0
        );

        float step = frameHeight * 0.60f;
        float distance = step * 0.50f;

        while (distance < length - step * 0.50f) {
            float along = distance / length;
            float fadeOut = along > 0.90f
                ? MathHelper.SmoothStep(1f, 0f, (along - 0.90f) / 0.10f)
                : 1f;

            Vector2 position = start + direction * distance;

            Main.EntitySpriteDraw(texture, position - Main.screenPosition, midFrame, baseColor * (outerOpacity * fadeOut), rotation,
                origin, outerScale * new Vector2(pulse, 1f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, position - Main.screenPosition, midFrame, baseColor * (midOpacity * fadeOut), rotation,
                origin, midScale * new Vector2(pulse, 1f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, position - Main.screenPosition, midFrame, highlightColor * (innerOpacity * fadeOut), rotation,
                origin, innerScale * new Vector2(pulse, 1f), SpriteEffects.None, 0);

            distance += step;
        }

        Vector2 endPosition = start + direction * length;
        Main.EntitySpriteDraw(
            texture,
            endPosition - Main.screenPosition,
            endFrame,
            baseColor * 1.15f,
            rotation,
            origin,
            startScale * new Vector2(pulse, 1f),
            SpriteEffects.None,
            0
        );
    }
}
