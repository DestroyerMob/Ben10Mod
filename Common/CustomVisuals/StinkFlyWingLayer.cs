using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

internal static class StinkFlyWingDrawHelper {
    private const string StinkFlyTransformationId = "Ben10Mod:StinkFly";
    private const string WingTexturePath = "Ben10Mod/Content/Items/Accessories/Wings/StinkFlyWings_Wings";
    private static readonly Rectangle[] WingFrames = {
        new(0, 0, 86, 50),
        new(0, 67, 86, 43),
        new(0, 134, 86, 40),
        new(0, 201, 86, 33)
    };
    private static readonly Vector2[] LeftWingOrigins = {
        new(56f, 49f),
        new(56f, 42f),
        new(56f, 39f),
        new(56f, 32f)
    };
    private static readonly Vector2[] RightWingOrigins = {
        new(30f, 49f),
        new(30f, 42f),
        new(30f, 39f),
        new(30f, 32f)
    };
    private static readonly Vector2 WingAnchorOffset = new(0f, 6f);
    private static readonly Vector2 WingSideOffset = new(8f, 0f);
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
        if (texture.Height <= 0)
            return;

        int frame = ResolveWingFrame(player);
        Rectangle sourceRectangle = WingFrames[frame];
        Vector2 anchor = player.MountedCenter - Main.screenPosition + WingAnchorOffset;
        Color color = Lighting.GetColor((int)(player.Center.X / 16f), (int)(player.Center.Y / 16f), Color.White);
        float rotation = player.fullRotation;
        Vector2 bodyRotationAnchor = player.fullRotationOrigin;
        bool facingRight = player.direction == 1;
        SpriteEffects leftEffects = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        SpriteEffects rightEffects = facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 leftOrigin = facingRight ? LeftWingOrigins[frame] : RightWingOrigins[frame];
        Vector2 rightOrigin = facingRight ? RightWingOrigins[frame] : LeftWingOrigins[frame];
        Vector2 leftOffset = new(-WingSideOffset.X * player.direction, WingSideOffset.Y);
        Vector2 rightOffset = new(WingSideOffset.X * player.direction, WingSideOffset.Y);

        DrawData leftWing = new(
            texture,
            anchor + leftOffset,
            sourceRectangle,
            color,
            rotation,
            leftOrigin,
            1f,
            leftEffects,
            0
        ) {
            shader = drawInfo.cBody
        };

        DrawData rightWing = new(
            texture,
            anchor + rightOffset,
            sourceRectangle,
            color,
            rotation,
            rightOrigin,
            1f,
            rightEffects,
            0
        ) {
            shader = drawInfo.cBody
        };

        if (rotation != 0f) {
            leftWing.position = RotateDrawPosition(leftWing.position, player, bodyRotationAnchor);
            rightWing.position = RotateDrawPosition(rightWing.position, player, bodyRotationAnchor);
        }

        drawInfo.DrawDataCache.Add(leftWing);
        drawInfo.DrawDataCache.Add(rightWing);
    }

    private static int ResolveWingFrame(Player player) {
        bool usingFlight = player.wingTime < player.wingTimeMax || player.controlJump;
        bool falling = player.velocity.Y > 0.6f && !usingFlight;
        bool grounded = player.velocity.Y == 0f && !usingFlight;

        if (grounded)
            return 0;

        if (falling)
            return 1;

        return 2 + (int)(Main.GameUpdateCount / HeightAnimInterval % 2);
    }

    private static Vector2 RotateDrawPosition(Vector2 drawPosition, Player player, Vector2 rotationOrigin) {
        Vector2 worldPosition = drawPosition + Main.screenPosition;
        Vector2 rotatedWorldPosition = worldPosition.RotatedBy(player.fullRotation, player.position + rotationOrigin);
        return rotatedWorldPosition - Main.screenPosition;
    }
}

public class StinkFlyWingLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return StinkFlyWingDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return new BeforeParent(PlayerDrawLayers.Torso);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        StinkFlyWingDrawHelper.DrawWings(ref drawInfo);
    }
}
