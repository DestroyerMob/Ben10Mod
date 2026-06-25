using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

internal static class TransformationCostumeDrawHelper {
    private const int PlayerFrameWidth = 40;
    private const int PlayerFrameHeight = 56;
    private const int VisibleAlphaThreshold = 16;
    private static readonly Dictionary<Texture2D, Color[]> TexturePixelCache = new();

    public static bool ShouldDraw(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead)
            return false;

        return player.GetModPlayer<OmnitrixPlayer>().ShouldShowTransformationVisuals();
    }

    public static void EnsureBodyParts(ref PlayerDrawSet drawInfo) {
        RestoreBackArm(ref drawInfo);
        RestoreLegs(ref drawInfo);
        RestoreTorso(ref drawInfo);
        DrawStableHead(ref drawInfo);
        RestoreFrontArm(ref drawInfo);
    }

    public static void EnsureBodyAndLegs(ref PlayerDrawSet drawInfo) {
        RestoreLegs(ref drawInfo, onlyWhenMissing: true);
        RestoreTorso(ref drawInfo, onlyWhenMissing: true);
    }

    public static void EnsureArms(ref PlayerDrawSet drawInfo) {
        RestoreArmLayer(PlayerDrawLayers.Skin, ref drawInfo, frontArm: false, onlyWhenMissing: true);
        RestoreArmLayer(PlayerDrawLayers.ArmOverItem, ref drawInfo, frontArm: true, onlyWhenMissing: true);
    }

    public static void RestoreBackArm(ref PlayerDrawSet drawInfo) {
        RestoreArmLayer(PlayerDrawLayers.Skin, ref drawInfo, frontArm: false, onlyWhenMissing: false);
    }

    public static void RestoreFrontArm(ref PlayerDrawSet drawInfo) {
        RestoreArmLayer(PlayerDrawLayers.ArmOverItem, ref drawInfo, frontArm: true, onlyWhenMissing: false);
    }

    public static void RestoreLegs(ref PlayerDrawSet drawInfo) {
        RestoreLegs(ref drawInfo, onlyWhenMissing: false);
    }

    private static void RestoreLegs(ref PlayerDrawSet drawInfo, bool onlyWhenMissing) {
        Player player = drawInfo.drawPlayer;
        int legsSlot = ResolveLegsSlot(player);
        if (!ShouldDraw(drawInfo) || !TryGetTexture(TextureAssets.ArmorLeg, legsSlot, out Texture2D legsTexture))
            return;

        if (onlyWhenMissing && HasAnyDrawData(drawInfo, legsTexture))
            return;

        Color visibleColor = GetVisibleArmorColor(drawInfo.colorArmorLegs, drawInfo.colorArmorBody, drawInfo.colorArmorHead);
        if (EnsureDrawDataVisible(drawInfo, legsTexture, visibleColor))
            return;

        DrawVanillaLayer(PlayerDrawLayers.Leggings, ref drawInfo,
            forceVisiblePlayer: true,
            forceNeutralPose: true);

        if (!EnsureDrawDataVisible(drawInfo, legsTexture, visibleColor))
            DrawSimpleLegs(ref drawInfo, legsTexture);
    }

    public static void RestoreTorso(ref PlayerDrawSet drawInfo) {
        RestoreTorso(ref drawInfo, onlyWhenMissing: false);
    }

    private static void RestoreTorso(ref PlayerDrawSet drawInfo, bool onlyWhenMissing) {
        Player player = drawInfo.drawPlayer;
        int bodySlot = ResolveBodySlot(player);
        if (!ShouldDraw(drawInfo) || !TryGetBodyTexture(bodySlot,
                out Texture2D bodyTexture, out Texture2D compositeBodyTexture))
            return;

        Texture2D torsoTexture = compositeBodyTexture ?? bodyTexture;
        Rectangle expectedTorsoFrame = GetTorsoFrame(drawInfo, torsoTexture, IsCompositeBodyTexture(torsoTexture));
        if (onlyWhenMissing && HasVisibleDrawData(drawInfo, torsoTexture, expectedTorsoFrame))
            return;

        Color visibleColor = GetVisibleArmorColor(drawInfo.colorArmorBody, drawInfo.colorArmorHead, drawInfo.colorArmorLegs);
        if (EnsureDrawDataVisible(drawInfo, torsoTexture, visibleColor, expectedTorsoFrame))
            return;

        DrawVanillaLayer(PlayerDrawLayers.Torso, ref drawInfo,
            forceVisiblePlayer: true,
            forceCompositeTorso: true,
            forceNeutralPose: true);

        if (EnsureDrawDataVisible(drawInfo, torsoTexture, visibleColor, expectedTorsoFrame))
            return;

        DrawSimpleTorso(ref drawInfo, torsoTexture, IsCompositeBodyTexture(torsoTexture));
    }

    public static void DrawStableHead(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        int headSlot = ResolveHeadSlot(player);
        if (!ShouldDraw(drawInfo) || !TryGetTexture(TextureAssets.ArmorHead, headSlot, out Texture2D headTexture))
            return;

        RemoveDrawData(drawInfo, headTexture);

        Rectangle sourceRectangle = GetVerticalFrame(headTexture, player.bodyFrame);
        if (sourceRectangle.Height <= 0)
            return;

        Vector2 drawPosition = GetStableBasePosition(drawInfo) + drawInfo.helmetOffset + drawInfo.headVect +
                               GetHeadgearOffset(player);
        DrawData headDraw = new(
            headTexture,
            drawPosition,
            sourceRectangle,
            drawInfo.colorArmorHead,
            0f,
            drawInfo.headVect,
            1f,
            drawInfo.playerEffect,
            0
        ) {
            shader = drawInfo.cHead
        };

        drawInfo.DrawDataCache.Add(headDraw);
    }

    private static void RestoreArmLayer(PlayerDrawLayer layer, ref PlayerDrawSet drawInfo, bool frontArm, bool onlyWhenMissing) {
        Player player = drawInfo.drawPlayer;
        int bodySlot = ResolveBodySlot(player);
        if (!ShouldDraw(drawInfo) || !TryGetBodyTexture(bodySlot,
                out Texture2D bodyTexture, out Texture2D compositeBodyTexture))
            return;

        Texture2D armTexture = compositeBodyTexture ?? bodyTexture;
        Rectangle expectedArmFrame = GetCompositeArmFrame(drawInfo, armTexture, frontArm);
        if (onlyWhenMissing && HasVisibleDrawData(drawInfo, armTexture, expectedArmFrame))
            return;

        int beforeCount = drawInfo.DrawDataCache.Count;
        DrawVanillaLayer(layer, ref drawInfo,
            forceVisiblePlayer: true,
            forceCompositeTorso: true,
            forceNeutralPose: true);

        Color visibleColor = GetVisibleArmorColor(drawInfo.colorArmorBody, drawInfo.colorArmorHead, drawInfo.colorArmorLegs);
        bool keptVisibleArmData = false;
        for (int i = drawInfo.DrawDataCache.Count - 1; i >= beforeCount; i--) {
            DrawData drawData = drawInfo.DrawDataCache[i];
            if ((drawData.texture != bodyTexture && drawData.texture != compositeBodyTexture) ||
                !SourceHasVisiblePixels(drawData.texture, drawData.sourceRect ?? drawData.texture.Bounds)) {
                drawInfo.DrawDataCache.RemoveAt(i);
                continue;
            }

            keptVisibleArmData = true;
            if (drawData.color.A <= 0) {
                drawData.color = visibleColor.A > 0 ? visibleColor : Color.White;
                drawInfo.DrawDataCache[i] = drawData;
            }
        }

        if (!keptVisibleArmData)
            DrawSimpleCompositeArm(ref drawInfo, armTexture, frontArm);
    }

    private static void DrawVanillaLayer(PlayerDrawLayer layer, ref PlayerDrawSet drawInfo,
        bool forceVisiblePlayer = false,
        bool forceCompositeTorso = false,
        bool forceNeutralPose = false) {
        MethodInfo drawMethod = FindDrawMethod(layer);
        if (drawMethod == null)
            return;

        bool originalInvis = drawInfo.drawPlayer.invis;
        bool originalUsesCompositeTorso = drawInfo.usesCompositeTorso;
        Vector2 originalHeadPosition = drawInfo.drawPlayer.headPosition;
        Vector2 originalBodyPosition = drawInfo.drawPlayer.bodyPosition;
        Vector2 originalLegPosition = drawInfo.drawPlayer.legPosition;
        float originalHeadRotation = drawInfo.drawPlayer.headRotation;
        float originalBodyRotation = drawInfo.drawPlayer.bodyRotation;
        float originalLegRotation = drawInfo.drawPlayer.legRotation;
        object[] args = { drawInfo };
        try {
            if (forceVisiblePlayer)
                drawInfo.drawPlayer.invis = false;

            if (forceCompositeTorso)
                drawInfo.usesCompositeTorso = true;

            if (forceNeutralPose)
                NeutralizeRacePoseOffsets(drawInfo.drawPlayer);

            drawMethod.Invoke(layer, args);
            drawInfo = (PlayerDrawSet)args[0];
        }
        catch {
        }
        finally {
            drawInfo.drawPlayer.invis = originalInvis;
            drawInfo.usesCompositeTorso = originalUsesCompositeTorso;
            drawInfo.drawPlayer.headPosition = originalHeadPosition;
            drawInfo.drawPlayer.bodyPosition = originalBodyPosition;
            drawInfo.drawPlayer.legPosition = originalLegPosition;
            drawInfo.drawPlayer.headRotation = originalHeadRotation;
            drawInfo.drawPlayer.bodyRotation = originalBodyRotation;
            drawInfo.drawPlayer.legRotation = originalLegRotation;
        }
    }

    private static void NeutralizeRacePoseOffsets(Player player) {
        player.headPosition = Vector2.Zero;
        player.bodyPosition = Vector2.Zero;
        player.legPosition = Vector2.Zero;
        player.headRotation = 0f;
        player.bodyRotation = 0f;
        player.legRotation = 0f;
    }

    private static MethodInfo FindDrawMethod(PlayerDrawLayer layer) {
        for (Type type = layer.GetType(); type != null; type = type.BaseType) {
            MethodInfo drawMethod = type.GetMethod("Draw",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (drawMethod != null)
                return drawMethod;
        }

        return null;
    }

    private static void DrawSimpleLegs(ref PlayerDrawSet drawInfo, Texture2D legsTexture) {
        Player player = drawInfo.drawPlayer;
        Rectangle sourceRectangle = GetVerticalFrame(legsTexture, player.legFrame);
        if (sourceRectangle.Height <= 0)
            return;

        DrawData legsDraw = new(
            legsTexture,
            GetStableBasePosition(drawInfo) + drawInfo.legVect,
            sourceRectangle,
            GetVisibleArmorColor(drawInfo.colorArmorLegs, drawInfo.colorArmorBody, drawInfo.colorArmorHead),
            0f,
            drawInfo.legVect,
            1f,
            drawInfo.playerEffect,
            0
        ) {
            shader = drawInfo.cLegs
        };

        drawInfo.DrawDataCache.Add(legsDraw);
    }

    private static void DrawSimpleTorso(ref PlayerDrawSet drawInfo, Texture2D bodyTexture, bool useCompositeFrame) {
        Player player = drawInfo.drawPlayer;
        Rectangle sourceRectangle = useCompositeFrame
            ? GetCompositeTorsoFrame(drawInfo, bodyTexture)
            : GetVerticalFrame(bodyTexture, player.bodyFrame);
        if (sourceRectangle.Height <= 0)
            return;

        DrawData bodyDraw = new(
            bodyTexture,
            GetStableBasePosition(drawInfo) + drawInfo.bodyVect,
            sourceRectangle,
            GetVisibleArmorColor(drawInfo.colorArmorBody, drawInfo.colorArmorHead, drawInfo.colorArmorLegs),
            0f,
            drawInfo.bodyVect,
            1f,
            drawInfo.playerEffect,
            0
        ) {
            shader = drawInfo.cBody
        };

        drawInfo.DrawDataCache.Add(bodyDraw);
    }

    private static void DrawSimpleCompositeArm(ref PlayerDrawSet drawInfo, Texture2D bodyTexture, bool frontArm) {
        if (!IsCompositeBodyTexture(bodyTexture))
            return;

        Rectangle sourceRectangle = GetCompositeArmFrame(drawInfo, bodyTexture, frontArm);
        if (sourceRectangle.Height <= 0)
            return;

        DrawData armDraw = new(
            bodyTexture,
            GetStableBasePosition(drawInfo) + drawInfo.bodyVect,
            sourceRectangle,
            GetVisibleArmorColor(drawInfo.colorArmorBody, drawInfo.colorArmorHead, drawInfo.colorArmorLegs),
            0f,
            drawInfo.bodyVect,
            1f,
            drawInfo.playerEffect,
            0
        ) {
            shader = drawInfo.cBody
        };

        drawInfo.DrawDataCache.Add(armDraw);
    }

    private static Vector2 GetStableBasePosition(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        int frameWidth = player.bodyFrame.Width > 0 ? player.bodyFrame.Width : PlayerFrameWidth;
        int frameHeight = player.bodyFrame.Height > 0 ? player.bodyFrame.Height : PlayerFrameHeight;
        return new Vector2(
            (int)(drawInfo.Position.X - Main.screenPosition.X - frameWidth / 2f + player.width / 2f),
            (int)(drawInfo.Position.Y - Main.screenPosition.Y + player.height - frameHeight + 4f)
        );
    }

    private static Vector2 GetHeadgearOffset(Player player) {
        int frameHeight = player.bodyFrame.Height > 0 ? player.bodyFrame.Height : PlayerFrameHeight;
        int frameIndex = frameHeight > 0 ? Math.Max(0, player.bodyFrame.Y / frameHeight) : 0;
        if (frameIndex >= Main.OffsetsPlayerHeadgear.Length)
            return Vector2.Zero;

        return Main.OffsetsPlayerHeadgear[frameIndex] * player.gravDir;
    }

    private static Rectangle GetVerticalFrame(Texture2D texture, Rectangle playerFrame) {
        int frameHeight = playerFrame.Height > 0 ? playerFrame.Height : PlayerFrameHeight;
        int frameIndex = Math.Max(0, playerFrame.Y / frameHeight);
        int sourceY = Math.Min(frameIndex * frameHeight, Math.Max(0, texture.Height - 1));
        int height = Math.Min(frameHeight, texture.Height - sourceY);
        return new Rectangle(0, sourceY, texture.Width, height);
    }

    private static Rectangle GetCompositeTorsoFrame(PlayerDrawSet drawInfo, Texture2D texture) {
        Rectangle drawInfoFrame = ClampFrame(drawInfo.compTorsoFrame, texture);
        if (drawInfoFrame.Width > 0 && SourceHasVisiblePixels(texture, drawInfoFrame))
            return drawInfoFrame;

        return Rectangle.Empty;
    }

    private static Rectangle GetTorsoFrame(PlayerDrawSet drawInfo, Texture2D texture, bool useCompositeFrame) {
        return useCompositeFrame
            ? GetCompositeTorsoFrame(drawInfo, texture)
            : GetVerticalFrame(texture, drawInfo.drawPlayer.bodyFrame);
    }

    private static Rectangle GetCompositeArmFrame(PlayerDrawSet drawInfo, Texture2D texture, bool frontArm) {
        Rectangle drawInfoFrame = ClampFrame(frontArm ? drawInfo.compFrontArmFrame : drawInfo.compBackArmFrame, texture);
        if (drawInfoFrame.Width > 0 && SourceHasVisiblePixels(texture, drawInfoFrame))
            return drawInfoFrame;

        int row = ResolveCompositeFrameRow(drawInfo, texture);
        int[] candidateColumns = frontArm
            ? new[] { 8, 7, 6, 5, 4, 3, 2 }
            : new[] { 2, 3, 4, 5, 6, 7, 8 };

        foreach (int column in candidateColumns) {
            Rectangle candidateFrame = new(column * PlayerFrameWidth, row * PlayerFrameHeight,
                PlayerFrameWidth, PlayerFrameHeight);
            candidateFrame = ClampFrame(candidateFrame, texture);
            if (candidateFrame.Width > 0 && SourceHasVisiblePixels(texture, candidateFrame))
                return candidateFrame;
        }

        return Rectangle.Empty;
    }

    private static int ResolveCompositeFrameRow(PlayerDrawSet drawInfo, Texture2D texture) {
        int rowCount = Math.Max(1, texture.Height / PlayerFrameHeight);
        if (drawInfo.compTorsoFrame.Height > 0)
            return Math.Clamp(drawInfo.compTorsoFrame.Y / PlayerFrameHeight, 0, rowCount - 1);

        Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
        if (bodyFrame.Height > 0)
            return Math.Clamp(bodyFrame.Y / bodyFrame.Height, 0, rowCount - 1);

        return 0;
    }

    private static Rectangle ClampFrame(Rectangle frame, Texture2D texture) {
        if (texture == null || frame.Width <= 0 || frame.Height <= 0)
            return Rectangle.Empty;

        Rectangle textureBounds = new(0, 0, texture.Width, texture.Height);
        return Rectangle.Intersect(frame, textureBounds);
    }

    private static bool TryGetTexture(ReLogic.Content.Asset<Texture2D>[] assets, int slot, out Texture2D texture) {
        if (slot < 0 || slot >= assets.Length || assets[slot] == null) {
            texture = null;
            return false;
        }

        texture = assets[slot].Value;
        return texture != null;
    }

    private static int ResolveHeadSlot(Player player) {
        int slot = player.GetModPlayer<OmnitrixPlayer>().activeTransformationHeadSlot;
        return slot >= 0 ? slot : player.head;
    }

    private static int ResolveBodySlot(Player player) {
        int slot = player.GetModPlayer<OmnitrixPlayer>().activeTransformationBodySlot;
        return slot >= 0 ? slot : player.body;
    }

    private static int ResolveLegsSlot(Player player) {
        int slot = player.GetModPlayer<OmnitrixPlayer>().activeTransformationLegsSlot;
        return slot >= 0 ? slot : player.legs;
    }

    private static bool TryGetBodyTexture(int slot, out Texture2D bodyTexture, out Texture2D compositeBodyTexture) {
        bool hasBodyTexture = TryGetTexture(TextureAssets.ArmorBody, slot, out bodyTexture);
        bool hasCompositeBodyTexture = TryGetTexture(TextureAssets.ArmorBodyComposite, slot, out compositeBodyTexture);

        if (!hasBodyTexture)
            bodyTexture = null;

        if (!hasCompositeBodyTexture)
            compositeBodyTexture = null;

        return hasBodyTexture || hasCompositeBodyTexture;
    }

    private static bool IsCompositeBodyTexture(Texture2D texture) {
        return texture != null && texture.Width > PlayerFrameWidth;
    }

    private static bool HasAnyDrawData(PlayerDrawSet drawInfo, Texture2D texture) {
        if (texture == null)
            return false;

        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
            if (drawInfo.DrawDataCache[i].texture == texture)
                return true;
        }

        return false;
    }

    private static bool HasVisibleDrawData(PlayerDrawSet drawInfo, Texture2D texture, Rectangle sourceRectangle) {
        if (texture == null || sourceRectangle.Width <= 0 || sourceRectangle.Height <= 0)
            return false;

        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
            DrawData drawData = drawInfo.DrawDataCache[i];
            if (drawData.texture != texture)
                continue;

            Rectangle drawFrame = drawData.sourceRect ?? texture.Bounds;
            if (drawFrame != sourceRectangle)
                continue;

            if (drawData.color.A > 0 && SourceHasVisiblePixels(texture, drawFrame))
                return true;
        }

        return false;
    }

    private static Color GetVisibleArmorColor(Color preferred, Color fallback, Color finalFallback) {
        if (preferred.A > 0)
            return preferred;

        if (fallback.A > 0)
            return fallback;

        if (finalFallback.A > 0)
            return finalFallback;

        return Color.White;
    }

    private static bool EnsureDrawDataVisible(PlayerDrawSet drawInfo, Texture2D texture, Color visibleColor) {
        bool foundDrawableData = false;
        if (texture == null)
            return false;

        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
            DrawData drawData = drawInfo.DrawDataCache[i];
            if (drawData.texture != texture)
                continue;

            if (!SourceHasVisiblePixels(texture, drawData.sourceRect ?? texture.Bounds))
                continue;

            foundDrawableData = true;
            if (drawData.color.A <= 0) {
                drawData.color = visibleColor.A > 0 ? visibleColor : Color.White;
                drawInfo.DrawDataCache[i] = drawData;
            }
        }

        return foundDrawableData;
    }

    private static bool EnsureDrawDataVisible(PlayerDrawSet drawInfo, Texture2D texture, Color visibleColor,
        Rectangle sourceRectangle) {
        bool foundDrawableData = false;
        if (texture == null || sourceRectangle.Width <= 0 || sourceRectangle.Height <= 0)
            return false;

        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
            DrawData drawData = drawInfo.DrawDataCache[i];
            if (drawData.texture != texture)
                continue;

            Rectangle drawFrame = drawData.sourceRect ?? texture.Bounds;
            if (drawFrame != sourceRectangle || !SourceHasVisiblePixels(texture, drawFrame))
                continue;

            foundDrawableData = true;
            if (drawData.color.A <= 0) {
                drawData.color = visibleColor.A > 0 ? visibleColor : Color.White;
                drawInfo.DrawDataCache[i] = drawData;
            }
        }

        return foundDrawableData;
    }

    private static bool SourceHasVisiblePixels(Texture2D texture, Rectangle sourceRectangle) {
        int score = GetFrameAlphaScore(texture, sourceRectangle);
        return score != 0;
    }

    private static int GetFrameAlphaScore(Texture2D texture, Rectangle sourceRectangle) {
        if (texture == null || sourceRectangle.Width <= 0 || sourceRectangle.Height <= 0)
            return 0;

        Rectangle frame = ClampFrame(sourceRectangle, texture);
        if (frame.Width <= 0 || frame.Height <= 0)
            return 0;

        if (!TryGetTexturePixels(texture, out Color[] pixels))
            return -1;

        int score = 0;
        int textureWidth = texture.Width;
        for (int y = frame.Top; y < frame.Bottom; y++) {
            int rowStart = y * textureWidth;
            for (int x = frame.Left; x < frame.Right; x++) {
                if (pixels[rowStart + x].A > VisibleAlphaThreshold)
                    score++;
            }
        }

        return score;
    }

    private static bool TryGetTexturePixels(Texture2D texture, out Color[] pixels) {
        if (TexturePixelCache.TryGetValue(texture, out pixels) && pixels.Length == texture.Width * texture.Height)
            return true;

        pixels = new Color[texture.Width * texture.Height];
        try {
            texture.GetData(pixels);
        }
        catch {
            pixels = null;
            return false;
        }

        TexturePixelCache[texture] = pixels;
        return true;
    }

    private static void RemoveDrawData(PlayerDrawSet drawInfo, Texture2D texture) {
        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
            if (drawInfo.DrawDataCache[i].texture == texture)
                drawInfo.DrawDataCache.RemoveAt(i);
        }
    }
}

public class TransformationCostumeLegLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return TransformationCostumeDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return new Between(ModContent.GetInstance<TransformationCostumeBackArmLayer>(), PlayerDrawLayers.Shoes);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        TransformationCostumeDrawHelper.RestoreLegs(ref drawInfo);
    }
}

public class TransformationCostumeBackArmLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return TransformationCostumeDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return new Between(PlayerDrawLayers.BalloonAcc, PlayerDrawLayers.Leggings);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        TransformationCostumeDrawHelper.RestoreBackArm(ref drawInfo);
    }
}

public class TransformationCostumeBodyLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return TransformationCostumeDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return new Between(PlayerDrawLayers.ArmorLongCoat, PlayerDrawLayers.OffhandAcc);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        TransformationCostumeDrawHelper.RestoreTorso(ref drawInfo);
    }
}

public class TransformationCostumeFrontArmLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return TransformationCostumeDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return new Between(ModContent.GetInstance<TransformationCostumeBodyLayer>(), PlayerDrawLayers.HandOnAcc);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        TransformationCostumeDrawHelper.RestoreFrontArm(ref drawInfo);
    }
}

public class TransformationCostumeHeadLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return TransformationCostumeDrawHelper.ShouldDraw(drawInfo);
    }

    public override Position GetDefaultPosition() {
        return new Between(PlayerDrawLayers.NeckAcc, PlayerDrawLayers.FinchNest);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        TransformationCostumeDrawHelper.DrawStableHead(ref drawInfo);
    }
}
