using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Ben10Mod.Content.Projectiles;

internal static class ChromaStonePrismHelper {
    public static Color GetSpectrumColor(float offset, float brightness = 1f) {
        float time = Main.GlobalTimeWrappedHourly * 3.2f + offset;
        float r = 0.58f + 0.42f * (0.5f + 0.5f * MathF.Sin(time));
        float g = 0.58f + 0.42f * (0.5f + 0.5f * MathF.Sin(time + MathHelper.TwoPi / 3f));
        float b = 0.58f + 0.42f * (0.5f + 0.5f * MathF.Sin(time + MathHelper.TwoPi * 2f / 3f));

        return new Color(
            (byte)MathHelper.Clamp(r * 255f * brightness, 0f, 255f),
            (byte)MathHelper.Clamp(g * 255f * brightness, 0f, 255f),
            (byte)MathHelper.Clamp(b * 255f * brightness, 0f, 255f),
            255);
    }

    public static void DrawRotatedRect(Texture2D pixel, Vector2 center, float rotation, Vector2 scale, Color color) {
        Main.EntitySpriteDraw(pixel, center, null, color, rotation, Vector2.One * 0.5f, scale, SpriteEffects.None, 0);
    }

    public static void DrawBeam(Texture2D pixel, Vector2 start, Vector2 end, float width, Color color) {
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length <= 0.5f)
            return;

        Main.EntitySpriteDraw(pixel, start, null, color, delta.ToRotation(), new Vector2(0f, 0.5f),
            new Vector2(length, width), SpriteEffects.None, 0);
    }

    public static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotation, int segments = 18) {
        for (int i = 0; i < segments; i++) {
            float angle = rotation + MathHelper.TwoPi * i / segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.2f), SpriteEffects.None, 0);
        }
    }
}
