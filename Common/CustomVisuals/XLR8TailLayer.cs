using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

internal static class XLR8TailDrawHelper {
    private const string XLR8TransformationId = "Ben10Mod:XLR8";
    private const string TailTexturePath = "Ben10Mod/Content/Transformations/XLR8/XLR8_Tail";
    private const string BodyTexturePath = "Ben10Mod/Content/Transformations/XLR8/XLR8_Body";
    private const string LegsTexturePath = "Ben10Mod/Content/Transformations/XLR8/XLR8_Legs";
    private const int TailFrameWidth = 58;
    private const int TailFrameHeight = 56;

    // Anchor to the trailing edge of the torso so the tail grows out from XLR8's back.
    private static readonly Vector2 BodyBackAnchorInFrame = new(12f, 33f);
    private static readonly Vector2 TailRootAnchorInFrame = new(1f, 32f);

    public static bool ShouldDraw(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis)
            return false;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        return omp.currentTransformationId == XLR8TransformationId;
    }

    public static void DrawTail(ref PlayerDrawSet drawInfo) {
        if (!ShouldDraw(drawInfo))
            return;

        Texture2D tailTexture = ModContent.Request<Texture2D>(TailTexturePath).Value;
        Texture2D bodyTexture = ModContent.Request<Texture2D>(BodyTexturePath).Value;
        Texture2D legsTexture = ModContent.Request<Texture2D>(LegsTexturePath).Value;

        if (!TryGetBodyDrawData(drawInfo, bodyTexture, out int bodyDrawIndex, out DrawData bodyDrawData))
            return;

        int frameIndex = ResolveFrameIndex(bodyDrawData);
        Rectangle sourceRectangle = new(0, frameIndex * TailFrameHeight, TailFrameWidth, TailFrameHeight);
        Vector2 anchor = ResolveBodyAnchor(bodyDrawData, BodyBackAnchorInFrame);
        bool facingLeft = (bodyDrawData.effect & SpriteEffects.FlipHorizontally) != 0;
        Vector2 tailOrigin = facingLeft
            ? new Vector2(sourceRectangle.Width - TailRootAnchorInFrame.X, TailRootAnchorInFrame.Y)
            : TailRootAnchorInFrame;

        DrawData tailDraw = new(
            tailTexture,
            anchor,
            sourceRectangle,
            bodyDrawData.color,
            bodyDrawData.rotation,
            tailOrigin,
            bodyDrawData.scale,
            bodyDrawData.effect,
            0
        ) {
            shader = bodyDrawData.shader
        };

        int insertIndex = ResolveTailInsertIndex(drawInfo, bodyDrawIndex, bodyTexture, legsTexture);
        drawInfo.DrawDataCache.Insert(insertIndex, tailDraw);
    }

    private static int ResolveFrameIndex(DrawData bodyDrawData) {
        Rectangle bodyFrame = bodyDrawData.sourceRect ?? bodyDrawData.texture.Bounds;
        if (bodyFrame.Height <= 0)
            return 0;

        return bodyFrame.Y / bodyFrame.Height;
    }

    private static int ResolveTailInsertIndex(PlayerDrawSet drawInfo, int fallbackIndex, Texture2D bodyTexture, Texture2D legsTexture) {
        int insertIndex = fallbackIndex;

        for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
            Texture2D texture = drawInfo.DrawDataCache[i].texture;
            if (texture == bodyTexture || texture == legsTexture) {
                insertIndex = i;
                break;
            }
        }

        return insertIndex < 0 ? 0 : insertIndex;
    }

    private static bool TryGetBodyDrawData(PlayerDrawSet drawInfo, Texture2D bodyTexture, out int bodyDrawIndex, out DrawData bodyDrawData) {
        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
            DrawData drawData = drawInfo.DrawDataCache[i];
            if (drawData.texture == bodyTexture) {
                bodyDrawIndex = i;
                bodyDrawData = drawData;
                return true;
            }
        }

        bodyDrawIndex = -1;
        bodyDrawData = default;
        return false;
    }

    private static Vector2 ResolveBodyAnchor(DrawData bodyDrawData, Vector2 anchorInBodyFrame) {
        Rectangle bodyFrame = bodyDrawData.sourceRect ?? bodyDrawData.texture.Bounds;
        Vector2 localAnchor = anchorInBodyFrame;

        if ((bodyDrawData.effect & SpriteEffects.FlipHorizontally) != 0)
            localAnchor.X = bodyFrame.Width - localAnchor.X;

        if ((bodyDrawData.effect & SpriteEffects.FlipVertically) != 0)
            localAnchor.Y = bodyFrame.Height - localAnchor.Y;

        Vector2 relativeAnchor = (localAnchor - bodyDrawData.origin) * bodyDrawData.scale;
        return bodyDrawData.position + relativeAnchor.RotatedBy(bodyDrawData.rotation);
    }
}

public class XLR8TailLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return XLR8TailDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return new AfterParent(PlayerDrawLayers.Torso);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        XLR8TailDrawHelper.DrawTail(ref drawInfo);
    }
}
