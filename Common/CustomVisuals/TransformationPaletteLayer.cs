using System;
using System.Collections.Generic;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class TransformationPaletteLayer : PlayerDrawLayer {
    private static readonly Dictionary<string, Texture2D> MaskedBaseTextureCache = new(StringComparer.Ordinal);

    private readonly struct ResolvedOverlay {
        public ResolvedOverlay(Texture2D baseTexture, Texture2D maskTexture, Color color) {
            BaseTexture = baseTexture;
            MaskTexture = maskTexture;
            Color = color;
        }

        public Texture2D BaseTexture { get; }
        public Texture2D MaskTexture { get; }
        public Color Color { get; }
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis)
            return false;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        global::Ben10Mod.Content.Transformations.Transformation transformation = omp.CurrentTransformation;
        return transformation != null && transformation.SupportsPaletteCustomization(omp);
    }

    public override Position GetDefaultPosition() {
        return new AfterParent(PlayerDrawLayers.ArmOverItem);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        global::Ben10Mod.Content.Transformations.Transformation transformation = omp.CurrentTransformation;
        if (transformation == null)
            return;

        IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(omp);
        if (channels == null || channels.Count == 0)
            return;

        List<ResolvedOverlay> overlays = new();
        for (int i = 0; i < channels.Count; i++) {
            TransformationPaletteChannel channel = channels[i];
            if (channel == null || !channel.IsValid)
                continue;

            Color channelColor = omp.GetPaletteColor(transformation, channel.Id);
            for (int j = 0; j < channel.Overlays.Count; j++) {
                TransformationPaletteOverlay overlay = channel.Overlays[j];
                if (overlay == null || !overlay.TryGetTextures(out Texture2D baseTexture, out Texture2D maskTexture))
                    continue;

                overlays.Add(new ResolvedOverlay(baseTexture, maskTexture, channelColor));
            }
        }

        if (overlays.Count == 0)
            return;

        Dictionary<Texture2D, List<Texture2D>> masksByBaseTexture = new();
        Dictionary<Texture2D, Texture2D> maskedBaseTextures = new();
        for (int i = 0; i < overlays.Count; i++) {
            ResolvedOverlay overlay = overlays[i];
            if (!masksByBaseTexture.TryGetValue(overlay.BaseTexture, out List<Texture2D> maskTextures)) {
                maskTextures = new List<Texture2D>();
                masksByBaseTexture[overlay.BaseTexture] = maskTextures;
            }

            if (!maskTextures.Contains(overlay.MaskTexture))
                maskTextures.Add(overlay.MaskTexture);
        }

        foreach ((Texture2D baseTexture, List<Texture2D> maskTextures) in masksByBaseTexture)
            maskedBaseTextures[baseTexture] = GetMaskedBaseTexture(baseTexture, maskTextures);

        int originalCount = drawInfo.DrawDataCache.Count;
        for (int i = 0; i < originalCount; i++) {
            DrawData source = drawInfo.DrawDataCache[i];
            if (source.texture == null || source.color.A == 0)
                continue;

            Texture2D originalTexture = source.texture;

            if (maskedBaseTextures.TryGetValue(originalTexture, out Texture2D maskedBaseTexture) &&
                maskedBaseTexture != null && maskedBaseTexture != source.texture) {
                source.texture = maskedBaseTexture;
                drawInfo.DrawDataCache[i] = source;
            }

            for (int j = 0; j < overlays.Count; j++) {
                ResolvedOverlay overlay = overlays[j];
                if (overlay.BaseTexture != originalTexture)
                    continue;

                DrawData overlayDraw = source;
                overlayDraw.texture = overlay.MaskTexture;
                overlayDraw.color = MultiplyColor(source.color, overlay.Color);
                overlayDraw.shader = source.shader;
                drawInfo.DrawDataCache.Add(overlayDraw);
            }
        }
    }

    private static Color MultiplyColor(Color source, Color tint) {
        return new Color(
            (byte)(source.R * tint.R / 255),
            (byte)(source.G * tint.G / 255),
            (byte)(source.B * tint.B / 255),
            source.A
        );
    }

    private static Texture2D GetMaskedBaseTexture(Texture2D baseTexture, IReadOnlyList<Texture2D> maskTextures) {
        if (baseTexture == null || maskTextures == null || maskTextures.Count == 0)
            return baseTexture;

        string cacheKey = BuildMaskedBaseTextureCacheKey(baseTexture, maskTextures);
        if (MaskedBaseTextureCache.TryGetValue(cacheKey, out Texture2D cachedTexture) &&
            cachedTexture != null && !cachedTexture.IsDisposed) {
            return cachedTexture;
        }

        Color[] baseData = new Color[baseTexture.Width * baseTexture.Height];
        baseTexture.GetData(baseData);
        byte[] combinedMaskAlpha = new byte[baseData.Length];

        for (int i = 0; i < maskTextures.Count; i++) {
            Texture2D maskTexture = maskTextures[i];
            if (maskTexture == null || maskTexture.Width != baseTexture.Width || maskTexture.Height != baseTexture.Height)
                continue;

            Color[] maskData = new Color[combinedMaskAlpha.Length];
            maskTexture.GetData(maskData);
            for (int j = 0; j < maskData.Length; j++)
                combinedMaskAlpha[j] = Math.Max(combinedMaskAlpha[j], maskData[j].A);
        }

        bool changed = false;
        for (int i = 0; i < baseData.Length; i++) {
            byte maskAlpha = combinedMaskAlpha[i];
            if (maskAlpha == 0)
                continue;

            Color pixel = baseData[i];
            baseData[i] = new Color(pixel.R, pixel.G, pixel.B, (byte)(pixel.A * (255 - maskAlpha) / 255));
            changed = true;
        }

        if (!changed)
            return baseTexture;

        Texture2D maskedBaseTexture = new(baseTexture.GraphicsDevice, baseTexture.Width, baseTexture.Height);
        maskedBaseTexture.SetData(baseData);
        MaskedBaseTextureCache[cacheKey] = maskedBaseTexture;
        return maskedBaseTexture;
    }

    private static string BuildMaskedBaseTextureCacheKey(Texture2D baseTexture, IReadOnlyList<Texture2D> maskTextures) {
        List<int> maskIds = new(maskTextures.Count);
        for (int i = 0; i < maskTextures.Count; i++)
            maskIds.Add(maskTextures[i]?.GetHashCode() ?? 0);

        maskIds.Sort();
        return $"{baseTexture.GetHashCode()}:{string.Join(",", maskIds)}";
    }
}
