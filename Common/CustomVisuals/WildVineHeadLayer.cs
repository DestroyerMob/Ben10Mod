using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

internal static class WildVineHeadDrawHelper {
    private const string WildVineTransformationId = "Ben10Mod:WildVine";
    private const string HeadTexturePath = "Ben10Mod/Content/Transformations/WildVine/WildVine_Head";
    private const int SourceFrameHeight = 56;
    private const int PaddedFramePadding = 8;
    private const int PaddedFrameHeight = SourceFrameHeight + PaddedFramePadding * 2;

    private static Texture2D paddedHeadTexture;
    private static Texture2D paddedSourceTexture;

    public static bool ShouldDraw(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis)
            return false;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        return omp.ShouldShowTransformationVisuals() &&
               omp.currentTransformationId == WildVineTransformationId;
    }

    public static void DrawHead(ref PlayerDrawSet drawInfo) {
        if (!ShouldDraw(drawInfo))
            return;

        Texture2D headTexture = ModContent.Request<Texture2D>(HeadTexturePath).Value;
        Texture2D drawTexture = GetPaddedHeadTexture(headTexture);
        List<DrawData> headDraws = null;

        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
            DrawData drawData = drawInfo.DrawDataCache[i];
            if (drawData.texture != headTexture)
                continue;

            headDraws ??= new List<DrawData>();
            headDraws.Add(CreatePaddedHeadDraw(drawInfo, drawData, drawTexture));
            drawInfo.DrawDataCache.RemoveAt(i);
        }

        if (headDraws == null) {
            headDraws = new List<DrawData> {
                CreateFallbackHeadDraw(drawInfo, drawTexture)
            };
        }

        // Preserve vanilla head placement, but draw from a padded source frame so edge pixels do not get clipped.
        for (int i = headDraws.Count - 1; i >= 0; i--)
            drawInfo.DrawDataCache.Add(headDraws[i]);
    }

    private static Texture2D GetPaddedHeadTexture(Texture2D sourceTexture) {
        if (sourceTexture == null || Main.dedServ)
            return sourceTexture;

        if (paddedHeadTexture != null && paddedSourceTexture == sourceTexture)
            return paddedHeadTexture;

        if (sourceTexture.Height <= 0 || sourceTexture.Height % SourceFrameHeight != 0)
            return sourceTexture;

        int frameCount = sourceTexture.Height / SourceFrameHeight;
        int width = sourceTexture.Width;
        int paddedHeight = frameCount * PaddedFrameHeight;
        Color[] sourcePixels = new Color[sourceTexture.Width * sourceTexture.Height];
        Color[] paddedPixels = new Color[width * paddedHeight];
        sourceTexture.GetData(sourcePixels);

        for (int frame = 0; frame < frameCount; frame++) {
            int sourceFrameY = frame * SourceFrameHeight;
            int paddedFrameY = frame * PaddedFrameHeight + PaddedFramePadding;

            for (int y = 0; y < SourceFrameHeight; y++) {
                int sourceRow = (sourceFrameY + y) * width;
                int paddedRow = (paddedFrameY + y) * width;

                for (int x = 0; x < width; x++)
                    paddedPixels[paddedRow + x] = sourcePixels[sourceRow + x];
            }
        }

        paddedHeadTexture = new Texture2D(Main.graphics.GraphicsDevice, width, paddedHeight);
        paddedHeadTexture.SetData(paddedPixels);
        paddedSourceTexture = sourceTexture;
        return paddedHeadTexture;
    }

    private static DrawData CreatePaddedHeadDraw(PlayerDrawSet drawInfo, DrawData sourceDraw, Texture2D drawTexture) {
        int frameCount = System.Math.Max(1, drawTexture.Height / PaddedFrameHeight);
        int frameIndex = ResolveFrameIndex(drawInfo, sourceDraw.sourceRect, frameCount);
        sourceDraw.texture = drawTexture;
        sourceDraw.sourceRect = new Rectangle(0, frameIndex * PaddedFrameHeight, drawTexture.Width, PaddedFrameHeight);
        sourceDraw.origin += new Vector2(0f, PaddedFramePadding);
        return sourceDraw;
    }

    private static DrawData CreateFallbackHeadDraw(PlayerDrawSet drawInfo, Texture2D drawTexture) {
        Player player = drawInfo.drawPlayer;
        int frameCount = System.Math.Max(1, drawTexture.Height / PaddedFrameHeight);
        int frameIndex = ResolveFrameIndex(drawInfo, player.bodyFrame, frameCount);
        Rectangle sourceRectangle = new(0, frameIndex * PaddedFrameHeight, drawTexture.Width, PaddedFrameHeight);
        Vector2 drawPosition = drawInfo.helmetOffset + new Vector2(
            (int)(drawInfo.Position.X - Main.screenPosition.X - player.bodyFrame.Width / 2f + player.width / 2f),
            (int)(drawInfo.Position.Y - Main.screenPosition.Y + player.height - player.bodyFrame.Height + 4f)
        ) + player.headPosition + drawInfo.headVect;

        DrawData headDraw = new(
            drawTexture,
            drawPosition,
            sourceRectangle,
            drawInfo.colorArmorHead,
            player.headRotation,
            drawInfo.headVect + new Vector2(0f, PaddedFramePadding),
            1f,
            drawInfo.playerEffect,
            0
        ) {
            shader = drawInfo.cHead
        };

        return headDraw;
    }

    private static int ResolveFrameIndex(PlayerDrawSet drawInfo, Rectangle? sourceRectangle, int frameCount) {
        if (sourceRectangle.HasValue)
            return System.Math.Clamp(sourceRectangle.Value.Y / SourceFrameHeight, 0, frameCount - 1);

        Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
        if (bodyFrame.Height <= 0)
            return 0;

        return System.Math.Clamp(bodyFrame.Y / SourceFrameHeight, 0, frameCount - 1);
    }
}

public class WildVineHeadLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return WildVineHeadDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return PlayerDrawLayers.AfterLastVanillaLayer;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        WildVineHeadDrawHelper.DrawHead(ref drawInfo);
    }
}
