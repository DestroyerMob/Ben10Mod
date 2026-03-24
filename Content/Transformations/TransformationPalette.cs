using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations;

public readonly struct TransformationPaletteColorEntry {
    public TransformationPaletteColorEntry(string transformationId, string channelId, Color color) {
        TransformationId = transformationId ?? string.Empty;
        ChannelId = channelId ?? string.Empty;
        Color = new Color(color.R, color.G, color.B, 255);
    }

    public string TransformationId { get; }
    public string ChannelId { get; }
    public Color Color { get; }
}

public sealed class TransformationPaletteOverlay {
    private Asset<Texture2D> _baseTextureAsset;
    private Asset<Texture2D> _maskTextureAsset;
    private Texture2D _preparedMaskTexture;
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
            maskTexture = PrepareMaskTexture(_maskTextureAsset.Value);
            return baseTexture != null && maskTexture != null;
        }
        catch {
            _loadFailed = true;
            return false;
        }
    }

    private Texture2D PrepareMaskTexture(Texture2D sourceMaskTexture) {
        if (sourceMaskTexture == null)
            return null;

        if (_preparedMaskTexture != null && !_preparedMaskTexture.IsDisposed)
            return _preparedMaskTexture;

        GraphicsDevice graphicsDevice = Main.instance?.GraphicsDevice;
        if (graphicsDevice == null || graphicsDevice.IsDisposed)
            return sourceMaskTexture;

        Color[] sourceData = new Color[sourceMaskTexture.Width * sourceMaskTexture.Height];
        sourceMaskTexture.GetData(sourceData);

        for (int i = 0; i < sourceData.Length; i++) {
            Color pixel = sourceData[i];
            byte brightness = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B));
            byte coverage = (byte)(brightness * pixel.A / 255);
            sourceData[i] = new Color(255, 255, 255, coverage);
        }

        _preparedMaskTexture = new Texture2D(graphicsDevice, sourceMaskTexture.Width, sourceMaskTexture.Height);
        _preparedMaskTexture.SetData(sourceData);
        return _preparedMaskTexture;
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
