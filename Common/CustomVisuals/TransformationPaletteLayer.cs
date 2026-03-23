using System.Collections.Generic;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class TransformationPaletteLayer : PlayerDrawLayer {
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

        int originalCount = drawInfo.DrawDataCache.Count;
        for (int i = 0; i < originalCount; i++) {
            DrawData source = drawInfo.DrawDataCache[i];
            if (source.texture == null || source.color.A == 0)
                continue;

            for (int j = 0; j < overlays.Count; j++) {
                ResolvedOverlay overlay = overlays[j];
                if (source.texture != overlay.BaseTexture)
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
}
