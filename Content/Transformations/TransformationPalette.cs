using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations;

public readonly struct TransformationPaletteColorEntry {
    public const byte NeutralHue = 128;
    public const byte NeutralSaturation = 128;
    public const byte NeutralBrightness = 128;

    public TransformationPaletteColorEntry(string transformationId, string channelId, Color color,
        byte hue = NeutralHue, byte saturation = NeutralSaturation, byte brightness = NeutralBrightness) {
        TransformationId = transformationId ?? string.Empty;
        ChannelId = channelId ?? string.Empty;
        Color = new Color(color.R, color.G, color.B, 255);
        Hue = hue;
        Saturation = saturation;
        Brightness = brightness;
    }

    public string TransformationId { get; }
    public string ChannelId { get; }
    public Color Color { get; }
    public byte Hue { get; }
    public byte Saturation { get; }
    public byte Brightness { get; }
}

public readonly struct TransformationPaletteChannelSettings {
    public TransformationPaletteChannelSettings(Color color, byte hue = TransformationPaletteColorEntry.NeutralHue,
        byte saturation = TransformationPaletteColorEntry.NeutralSaturation,
        byte brightness = TransformationPaletteColorEntry.NeutralBrightness) {
        Color = new Color(color.R, color.G, color.B, 255);
        Hue = hue;
        Saturation = saturation;
        Brightness = brightness;
    }

    public Color Color { get; }
    public byte Hue { get; }
    public byte Saturation { get; }
    public byte Brightness { get; }

    public bool HasNeutralAdjustments =>
        Hue == TransformationPaletteColorEntry.NeutralHue &&
        Saturation == TransformationPaletteColorEntry.NeutralSaturation &&
        Brightness == TransformationPaletteColorEntry.NeutralBrightness;
}

public sealed class TransformationPaletteOverlay {
    private Asset<Texture2D> _baseTextureAsset;
    private Asset<Texture2D> _maskTextureAsset;
    private bool _loadFailed;

    public TransformationPaletteOverlay(string baseTexturePath, string maskTexturePath) {
        BaseTexturePath = baseTexturePath ?? string.Empty;
        MaskTexturePath = maskTexturePath ?? string.Empty;
    }

    public string BaseTexturePath { get; }
    public string MaskTexturePath { get; }

    public bool TryGetTextures(out Texture2D baseTexture, out Texture2D maskTexture) {
        baseTexture = null;
        maskTexture = null;

        if (Main.dedServ || _loadFailed || string.IsNullOrWhiteSpace(BaseTexturePath) || string.IsNullOrWhiteSpace(MaskTexturePath))
            return false;

        try {
            _baseTextureAsset ??= ModContent.Request<Texture2D>(BaseTexturePath);
            _maskTextureAsset ??= ModContent.Request<Texture2D>(MaskTexturePath);
            baseTexture = _baseTextureAsset.Value;
            maskTexture = TransformationPaletteTextureCache.GetPreparedMaskTexture(_maskTextureAsset.Value);
            return baseTexture != null && maskTexture != null;
        }
        catch {
            _loadFailed = true;
            return false;
        }
    }
}

public sealed class TransformationPaletteChannel {
    public TransformationPaletteChannel(string id, string displayName, Color defaultColor,
        params TransformationPaletteOverlay[] overlays) {
        Id = string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
        DefaultColor = new Color(defaultColor.R, defaultColor.G, defaultColor.B, 255);
        Overlays = overlays == null || overlays.Length == 0
            ? Array.Empty<TransformationPaletteOverlay>()
            : Array.AsReadOnly(overlays);
    }

    public string Id { get; }
    public string DisplayName { get; }
    public Color DefaultColor { get; }
    public IReadOnlyList<TransformationPaletteOverlay> Overlays { get; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Id) && Overlays.Count > 0;
}

public static class TransformationPaletteMath {
    public static float GetHueShiftDegrees(byte value) {
        if (value == TransformationPaletteColorEntry.NeutralHue)
            return 0f;

        return value >= TransformationPaletteColorEntry.NeutralHue
            ? (value - TransformationPaletteColorEntry.NeutralHue) / 127f * 180f
            : (value - TransformationPaletteColorEntry.NeutralHue) / 128f * 180f;
    }

    public static float GetSaturationMultiplier(byte value) {
        if (value == TransformationPaletteColorEntry.NeutralSaturation)
            return 1f;

        return value >= TransformationPaletteColorEntry.NeutralSaturation
            ? 1f + (value - TransformationPaletteColorEntry.NeutralSaturation) / 127f
            : value / 128f;
    }

    public static float GetBrightnessMultiplier(byte value) {
        if (value == TransformationPaletteColorEntry.NeutralBrightness)
            return 1f;

        return value >= TransformationPaletteColorEntry.NeutralBrightness
            ? 1f + (value - TransformationPaletteColorEntry.NeutralBrightness) / 127f
            : value / 128f;
    }

    public static Color ApplyHueAndSaturation(Color source, byte hue, byte saturation) {
        return ApplyHueSaturationAndBrightness(source, hue, saturation, TransformationPaletteColorEntry.NeutralBrightness);
    }

    public static Color ApplyHueSaturationAndBrightness(Color source, byte hue, byte saturation, byte brightness) {
        if ((source.A == 0) || (hue == TransformationPaletteColorEntry.NeutralHue &&
            saturation == TransformationPaletteColorEntry.NeutralSaturation &&
            brightness == TransformationPaletteColorEntry.NeutralBrightness))
            return source;

        Vector3 hsv = RgbToHsv(source);
        hsv.X = WrapHue01(hsv.X + GetHueShiftDegrees(hue) / 360f);
        hsv.Y = MathHelper.Clamp(hsv.Y * GetSaturationMultiplier(saturation), 0f, 1f);
        hsv.Z = MathHelper.Clamp(hsv.Z * GetBrightnessMultiplier(brightness), 0f, 1f);
        Color shifted = HsvToRgb(hsv.X, hsv.Y, hsv.Z);
        shifted.A = source.A;
        return shifted;
    }

    public static Color ApplyColorizedPalette(Color source, Color target, byte hue, byte saturation) {
        return ApplyColorizedPalette(source, target, hue, saturation, TransformationPaletteColorEntry.NeutralBrightness);
    }

    public static Color ApplyColorizedPalette(Color source, Color target, byte hue, byte saturation, byte brightness) {
        if (source.A == 0)
            return source;

        float sourceBrightness = Math.Max(source.R, Math.Max(source.G, source.B)) / 255f;
        if (sourceBrightness <= 0f)
            sourceBrightness = ((source.R + source.G + source.B) / 3f) / 255f;

        Color tinted = new Color(
            (byte)Math.Clamp((int)Math.Round(target.R * sourceBrightness), 0, 255),
            (byte)Math.Clamp((int)Math.Round(target.G * sourceBrightness), 0, 255),
            (byte)Math.Clamp((int)Math.Round(target.B * sourceBrightness), 0, 255),
            source.A
        );

        return ApplyHueSaturationAndBrightness(tinted, hue, saturation, brightness);
    }

    private static Vector3 RgbToHsv(Color color) {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float delta = max - min;

        float hue = 0f;
        if (delta > 0f) {
            if (max == r)
                hue = ((g - b) / delta) % 6f;
            else if (max == g)
                hue = (b - r) / delta + 2f;
            else
                hue = (r - g) / delta + 4f;

            hue /= 6f;
            if (hue < 0f)
                hue += 1f;
        }

        float saturation = max <= 0f ? 0f : delta / max;
        return new Vector3(hue, saturation, max);
    }

    private static Color HsvToRgb(float hue, float saturation, float value) {
        hue = WrapHue01(hue);
        saturation = MathHelper.Clamp(saturation, 0f, 1f);
        value = MathHelper.Clamp(value, 0f, 1f);

        if (saturation <= 0f) {
            byte grayscale = (byte)Math.Clamp((int)Math.Round(value * 255f), 0, 255);
            return new Color(grayscale, grayscale, grayscale, 255);
        }

        float scaled = hue * 6f;
        int sector = (int)Math.Floor(scaled);
        float fraction = scaled - sector;

        float p = value * (1f - saturation);
        float q = value * (1f - fraction * saturation);
        float t = value * (1f - (1f - fraction) * saturation);

        return (sector % 6) switch {
            0 => ToColor(value, t, p),
            1 => ToColor(q, value, p),
            2 => ToColor(p, value, t),
            3 => ToColor(p, q, value),
            4 => ToColor(t, p, value),
            _ => ToColor(value, p, q)
        };
    }

    private static float WrapHue01(float hue) {
        hue %= 1f;
        if (hue < 0f)
            hue += 1f;
        return hue;
    }

    private static Color ToColor(float r, float g, float b) {
        return new Color(
            (byte)Math.Clamp((int)Math.Round(r * 255f), 0, 255),
            (byte)Math.Clamp((int)Math.Round(g * 255f), 0, 255),
            (byte)Math.Clamp((int)Math.Round(b * 255f), 0, 255),
            255
        );
    }
}

public static class TransformationPaletteTextureCache {
    private readonly record struct PreparedMaskKey(Texture2D MaskTexture);
    private readonly record struct MaskedBaseKey(Texture2D BaseTexture, string MaskSignature);
    private readonly record struct ProcessedOverlayKey(Texture2D BaseTexture, Texture2D MaskTexture, Color Color,
        byte Hue, byte Saturation, byte Brightness, bool UsePaletteColor);
    private readonly record struct PreviewFrameKey(Texture2D BaseTexture, string MaskSignature, int FrameWidth, int FrameHeight);

    private static readonly Dictionary<PreparedMaskKey, Texture2D> PreparedMasks = new();
    private static readonly Dictionary<MaskedBaseKey, Texture2D> MaskedBases = new();
    private static readonly Dictionary<ProcessedOverlayKey, Texture2D> ProcessedOverlays = new();
    private static readonly Dictionary<PreviewFrameKey, Rectangle> PreviewFrames = new();
    private static readonly Dictionary<Texture2D, Color[]> PixelCache = new();

    public static void Clear() {
        // Mod unload happens on a worker thread during rebuilds. Releasing GPU-backed textures there can
        // crash FNA on macOS, so we only drop our managed references and let the process/resource teardown
        // reclaim them safely.
        PreparedMasks.Clear();
        MaskedBases.Clear();
        ProcessedOverlays.Clear();
        PreviewFrames.Clear();
        PixelCache.Clear();
    }

    public static Texture2D GetPreparedMaskTexture(Texture2D maskTexture) {
        if (maskTexture == null || Main.dedServ)
            return null;

        PreparedMaskKey key = new(maskTexture);
        if (PreparedMasks.TryGetValue(key, out Texture2D prepared))
            return prepared;

        Color[] sourcePixels = GetPixels(maskTexture);
        if (sourcePixels == null || sourcePixels.Length == 0)
            return maskTexture;

        Color[] preparedPixels = new Color[sourcePixels.Length];
        for (int i = 0; i < sourcePixels.Length; i++) {
            Color pixel = sourcePixels[i];
            byte coverage = pixel.A;
            preparedPixels[i] = new Color(255, 255, 255, coverage);
        }

        Texture2D preparedTexture = new(Main.graphics.GraphicsDevice, maskTexture.Width, maskTexture.Height);
        preparedTexture.SetData(preparedPixels);
        PreparedMasks[key] = preparedTexture;
        return preparedTexture;
    }

    public static Texture2D GetMaskedBaseTexture(Texture2D baseTexture, IReadOnlyList<Texture2D> maskTextures) {
        if (baseTexture == null || maskTextures == null || maskTextures.Count == 0 || Main.dedServ)
            return baseTexture;

        string signature = BuildMaskSignature(maskTextures);
        if (string.IsNullOrEmpty(signature))
            return baseTexture;

        MaskedBaseKey key = new(baseTexture, signature);
        if (MaskedBases.TryGetValue(key, out Texture2D masked))
            return masked;

        Color[] basePixels = GetPixels(baseTexture);
        if (basePixels == null || basePixels.Length == 0)
            return baseTexture;

        Color[] maskedPixels = (Color[])basePixels.Clone();
        for (int maskIndex = 0; maskIndex < maskTextures.Count; maskIndex++) {
            Texture2D maskTexture = GetPreparedMaskTexture(maskTextures[maskIndex]);
            if (maskTexture == null || maskTexture.Width != baseTexture.Width || maskTexture.Height != baseTexture.Height)
                continue;

            Color[] maskPixels = GetPixels(maskTexture);
            if (maskPixels == null || maskPixels.Length != maskedPixels.Length)
                continue;

            for (int i = 0; i < maskedPixels.Length; i++) {
                if (maskedPixels[i].A == 0 || maskPixels[i].A == 0)
                    continue;

                maskedPixels[i].A = (byte)(maskedPixels[i].A * (255 - maskPixels[i].A) / 255);
            }
        }

        Texture2D maskedTexture = new(Main.graphics.GraphicsDevice, baseTexture.Width, baseTexture.Height);
        maskedTexture.SetData(maskedPixels);
        MaskedBases[key] = maskedTexture;
        return maskedTexture;
    }

    public static Texture2D GetProcessedOverlayTexture(Texture2D baseTexture, Texture2D maskTexture,
        TransformationPaletteChannelSettings settings, bool usePaletteColor) {
        if (baseTexture == null || maskTexture == null || Main.dedServ)
            return null;

        maskTexture = GetPreparedMaskTexture(maskTexture);
        ProcessedOverlayKey key = new(baseTexture, maskTexture, settings.Color, settings.Hue, settings.Saturation,
            settings.Brightness,
            usePaletteColor);
        if (ProcessedOverlays.TryGetValue(key, out Texture2D overlay))
            return overlay;

        Color[] basePixels = GetPixels(baseTexture);
        Color[] maskPixels = GetPixels(maskTexture);
        if (basePixels == null || maskPixels == null || basePixels.Length == 0 || basePixels.Length != maskPixels.Length)
            return null;

        Color[] overlayPixels = new Color[basePixels.Length];
        for (int i = 0; i < basePixels.Length; i++) {
            Color basePixel = basePixels[i];
            Color maskPixel = maskPixels[i];
            if (basePixel.A == 0 || maskPixel.A == 0)
                continue;

            Color processed = usePaletteColor
                ? TransformationPaletteMath.ApplyColorizedPalette(basePixel, settings.Color, settings.Hue, settings.Saturation,
                    settings.Brightness)
                : TransformationPaletteMath.ApplyHueSaturationAndBrightness(basePixel, settings.Hue, settings.Saturation,
                    settings.Brightness);

            processed.A = (byte)(basePixel.A * maskPixel.A / 255);
            overlayPixels[i] = processed;
        }

        Texture2D overlayTexture = new(Main.graphics.GraphicsDevice, baseTexture.Width, baseTexture.Height);
        overlayTexture.SetData(overlayPixels);
        ProcessedOverlays[key] = overlayTexture;
        return overlayTexture;
    }

    public static Rectangle ResolvePreviewFrame(Texture2D baseTexture, IReadOnlyList<Texture2D> maskTextures,
        int frameWidth = 40, int frameHeight = 56) {
        if (baseTexture == null)
            return Rectangle.Empty;

        if (frameWidth <= 0 || frameHeight <= 0 ||
            baseTexture.Width < frameWidth || baseTexture.Height < frameHeight ||
            baseTexture.Width % frameWidth != 0 || baseTexture.Height % frameHeight != 0) {
            return new Rectangle(0, 0, baseTexture.Width, baseTexture.Height);
        }

        string maskSignature = BuildMaskSignature(maskTextures);
        PreviewFrameKey key = new(baseTexture, maskSignature, frameWidth, frameHeight);
        if (PreviewFrames.TryGetValue(key, out Rectangle cachedFrame))
            return cachedFrame;

        Color[] basePixels = GetPixels(baseTexture);
        if (basePixels == null || basePixels.Length == 0) {
            Rectangle fullFrame = new(0, 0, frameWidth, frameHeight);
            PreviewFrames[key] = fullFrame;
            return fullFrame;
        }

        List<Color[]> resolvedMaskPixels = new();
        if (maskTextures != null) {
            for (int i = 0; i < maskTextures.Count; i++) {
                Texture2D maskTexture = GetPreparedMaskTexture(maskTextures[i]);
                if (maskTexture == null || maskTexture.Width != baseTexture.Width || maskTexture.Height != baseTexture.Height)
                    continue;

                Color[] maskPixels = GetPixels(maskTexture);
                if (maskPixels != null && maskPixels.Length == basePixels.Length)
                    resolvedMaskPixels.Add(maskPixels);
            }
        }

        int columns = baseTexture.Width / frameWidth;
        int rows = baseTexture.Height / frameHeight;
        bool hasMasks = resolvedMaskPixels.Count > 0;
        int bestScore = -1;
        Rectangle bestFrame = new(0, 0, frameWidth, frameHeight);

        for (int row = 0; row < rows; row++) {
            for (int column = 0; column < columns; column++) {
                Rectangle frame = new(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
                int baseOpaqueCount = CountOpaquePixels(basePixels, baseTexture.Width, frame);
                if (baseOpaqueCount <= 0)
                    continue;

                int maskOpaqueCount = hasMasks
                    ? CountMaskedPixels(resolvedMaskPixels, baseTexture.Width, frame)
                    : 0;

                int score = hasMasks && maskOpaqueCount > 0
                    ? 1_000_000 + maskOpaqueCount * 4 + baseOpaqueCount
                    : baseOpaqueCount;

                if (score > bestScore) {
                    bestScore = score;
                    bestFrame = frame;
                }
            }
        }

        PreviewFrames[key] = bestFrame;
        return bestFrame;
    }

    private static Color[] GetPixels(Texture2D texture) {
        if (texture == null || Main.dedServ)
            return null;

        if (PixelCache.TryGetValue(texture, out Color[] cachedPixels))
            return cachedPixels;

        Color[] pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);
        PixelCache[texture] = pixels;
        return pixels;
    }

    private static string BuildMaskSignature(IReadOnlyList<Texture2D> maskTextures) {
        if (maskTextures == null || maskTextures.Count == 0)
            return string.Empty;

        List<int> ids = new(maskTextures.Count);
        for (int i = 0; i < maskTextures.Count; i++) {
            if (maskTextures[i] != null)
                ids.Add(maskTextures[i].GetHashCode());
        }

        if (ids.Count == 0)
            return string.Empty;

        ids.Sort();
        return string.Join("|", ids);
    }

    private static int CountOpaquePixels(Color[] pixels, int textureWidth, Rectangle frame) {
        int count = 0;
        for (int y = frame.Top; y < frame.Bottom; y++) {
            int rowOffset = y * textureWidth;
            for (int x = frame.Left; x < frame.Right; x++) {
                if (pixels[rowOffset + x].A > 0)
                    count++;
            }
        }

        return count;
    }

    private static int CountMaskedPixels(IReadOnlyList<Color[]> maskPixels, int textureWidth, Rectangle frame) {
        int count = 0;
        for (int y = frame.Top; y < frame.Bottom; y++) {
            int rowOffset = y * textureWidth;
            for (int x = frame.Left; x < frame.Right; x++) {
                int index = rowOffset + x;
                for (int maskIndex = 0; maskIndex < maskPixels.Count; maskIndex++) {
                    if (maskPixels[maskIndex][index].A <= 0)
                        continue;

                    count++;
                    break;
                }
            }
        }

        return count;
    }
}
