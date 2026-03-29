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
        public ResolvedOverlay(Texture2D baseTexture, Texture2D maskTexture,
            TransformationPaletteChannelSettings settings, bool usePaletteColor) {
            BaseTexture = baseTexture;
            MaskTexture = maskTexture;
            Settings = settings;
            UsePaletteColor = usePaletteColor;
        }

        public Texture2D BaseTexture { get; }
        public Texture2D MaskTexture { get; }
        public TransformationPaletteChannelSettings Settings { get; }
        public bool UsePaletteColor { get; }
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis)
            return false;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        global::Ben10Mod.Content.Transformations.Transformation transformation = omp.CurrentTransformation;
        if (transformation == null || !transformation.SupportsPaletteCustomization(omp))
            return false;

        IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(omp);
        for (int i = 0; i < channels.Count; i++) {
            TransformationPaletteChannel channel = channels[i];
            if (channel == null || !channel.IsValid)
                continue;

            TransformationPaletteChannelSettings settings = omp.GetPaletteSettings(transformation, channel.Id);
            if (omp.IsPaletteChannelEnabled(transformation, channel.Id) || !settings.HasNeutralAdjustments)
                return true;
        }

        return false;
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

            TransformationPaletteChannelSettings settings = omp.GetPaletteSettings(transformation, channel.Id);
            bool usePaletteColor = omp.IsPaletteChannelEnabled(transformation, channel.Id);
            if (!usePaletteColor && settings.HasNeutralAdjustments)
                continue;

            for (int j = 0; j < channel.Overlays.Count; j++) {
                TransformationPaletteOverlay overlay = channel.Overlays[j];
                if (overlay == null || !overlay.TryGetTextures(out Texture2D baseTexture, out Texture2D maskTexture))
                    continue;

                overlays.Add(new ResolvedOverlay(baseTexture, maskTexture, settings, usePaletteColor));
            }
        }

        if (overlays.Count == 0)
            return;

        int originalCount = drawInfo.DrawDataCache.Count;
        for (int i = 0; i < originalCount; i++) {
            DrawData source = drawInfo.DrawDataCache[i];
            if (source.texture == null || source.color.A == 0)
                continue;

            List<Texture2D> masksForBase = null;
            for (int j = 0; j < overlays.Count; j++) {
                ResolvedOverlay overlay = overlays[j];
                if (overlay.BaseTexture != source.texture)
                    continue;

                masksForBase ??= new List<Texture2D>();
                masksForBase.Add(overlay.MaskTexture);
            }

            if (masksForBase == null || masksForBase.Count == 0)
                continue;

            for (int j = 0; j < overlays.Count; j++) {
                ResolvedOverlay overlay = overlays[j];
                if (overlay.BaseTexture != source.texture)
                    continue;

                Texture2D overlayTexture = TransformationPaletteTextureCache.GetProcessedOverlayTexture(
                    overlay.BaseTexture,
                    overlay.MaskTexture,
                    overlay.Settings,
                    overlay.UsePaletteColor
                );

                if (overlayTexture == null)
                    continue;

                DrawData overlayDraw = source;
                overlayDraw.texture = overlayTexture;
                overlayDraw.color = Color.White;
                drawInfo.DrawDataCache.Add(overlayDraw);
            }
        }
    }
}
