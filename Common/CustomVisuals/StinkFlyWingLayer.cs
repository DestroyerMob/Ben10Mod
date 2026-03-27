using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

internal static class StinkFlyWingDrawHelper {
    private const string StinkFlyTransformationId = "Ben10Mod:StinkFly";
    private const string WingTexturePath = "Ben10Mod/Content/Items/Accessories/Wings/StinkFlyWings_Wings";
    private const string BodyTexturePath = "Ben10Mod/Content/Transformations/StinkFly/StinkFly_Body";
    // The Stinkfly wing sheet is hand-packed rather than evenly divided into four bands.
    // Keep these explicit frame starts in sync with the asset so we don't clip or cross into
    // the next frame's rows.
    private static readonly int[] WingFrameTopOffsets = { 2, 62, 126, 186 };
    private const int WingFrameWidth = 86;
    private const int WingFrameHeight = 48;
    // This texture already contains the near wing and the far wing in one sprite.
    // Anchor from the body frame's upper back so both halves stay locked to the torso.
    private static readonly Vector2 BodyBackAnchorInFrame = new(20f, 17f);
    private static readonly Vector2 WingBackAnchorInFrame = new(43f, 19f);
    private const int HeightAnimInterval = 6;

    public static bool ShouldDraw(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis)
            return false;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        return omp.currentTransformationId == StinkFlyTransformationId;
    }

    public static void DrawWings(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (!ShouldDraw(drawInfo))
            return;

        Texture2D texture = ModContent.Request<Texture2D>(WingTexturePath).Value;
        Texture2D bodyTexture = ModContent.Request<Texture2D>(BodyTexturePath).Value;
        if (texture.Height <= 0)
            return;

        if (!TryGetBodyDrawData(drawInfo, bodyTexture, out int bodyDrawIndex, out DrawData bodyDrawData))
            return;

        int frame = ResolveWingFrame(player);
        Rectangle sourceRectangle = GetWingFrameRectangle(frame);
        Vector2 anchor = ResolveBodyAnchor(bodyDrawData, BodyBackAnchorInFrame);
        bool facingLeft = (bodyDrawData.effect & SpriteEffects.FlipHorizontally) != 0;
        Vector2 wingOrigin = facingLeft
            ? new Vector2(sourceRectangle.Width - WingBackAnchorInFrame.X, WingBackAnchorInFrame.Y)
            : WingBackAnchorInFrame;

        DrawData wingPair = new(
            texture,
            anchor,
            sourceRectangle,
            bodyDrawData.color,
            bodyDrawData.rotation,
            wingOrigin,
            bodyDrawData.scale,
            bodyDrawData.effect,
            0
        ) {
            shader = bodyDrawData.shader
        };

        drawInfo.DrawDataCache.Insert(bodyDrawIndex, wingPair);
    }

    private static int ResolveWingFrame(Player player) {
        float verticalMotion = player.velocity.Y * player.gravDir;
        bool grounded = System.Math.Abs(verticalMotion) <= 0.01f;
        bool activelyFlying = player.controlJump && player.wingTime > 0f && player.wingFrame > 0;

        if (grounded)
            return 0;

        if (!activelyFlying)
            return 1;

        return 2 + (int)(Main.GameUpdateCount / HeightAnimInterval % 2);
    }

    private static Rectangle GetWingFrameRectangle(int frame) {
        return new Rectangle(0, WingFrameTopOffsets[frame], WingFrameWidth, WingFrameHeight);
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

public class StinkFlyWingLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return StinkFlyWingDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return new AfterParent(PlayerDrawLayers.Torso);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        StinkFlyWingDrawHelper.DrawWings(ref drawInfo);
    }
}
