using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Keybinds;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Interface {
    internal sealed class TransformationRadialMenu {
        private const float SlotRadius = 132f;
        private const float SlotSize = 82f;
        private const float CenterPanelWidth = 280f;
        private const float CenterPanelHeight = 96f;
        private const float OpenMargin = 210f;
        private const float BackdropSize = 396f;

        private bool isOpen;
        private int previewSlotIndex = -1;
        private int lastHoveredSlotIndex = -1;
        private Vector2 menuCenter;

        public bool IsOpen => isOpen;

        public void Update(UISystem uiSystem) {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active || Main.gameMenu) {
                Close();
                return;
            }

            OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
            Omnitrix activeOmnitrix = omp.GetActiveOmnitrix();
            bool hasBlockingInterface = uiSystem?.MyInterface?.CurrentState != null || omp.showingUI;
            bool canOpen = activeOmnitrix != null && !hasBlockingInterface && !Main.playerInventory;

            if (!canOpen) {
                Close();
                return;
            }

            if (!isOpen && KeybindSystem.TransformWheel?.JustPressed == true)
                Open(omp);

            if (!isOpen)
                return;

            Main.LocalPlayer.mouseInterface = true;
            UpdatePreviewSlot();

            if (Main.mouseLeft && Main.mouseLeftRelease) {
                Main.mouseLeftRelease = false;
                ConfirmSelection(player, omp, activeOmnitrix);
                return;
            }

            if (KeybindSystem.TransformWheel?.JustReleased == true) {
                ConfirmSelection(player, omp, activeOmnitrix);
                return;
            }

            if (KeybindSystem.TransformWheel?.Current != true)
                Close();
        }

        public void Draw(SpriteBatch spriteBatch) {
            if (!isOpen)
                return;

            Player player = Main.LocalPlayer;
            OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
            Omnitrix activeOmnitrix = omp.GetActiveOmnitrix();
            if (activeOmnitrix == null)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;

            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(4, 8, 12, 82));

            Rectangle backdropRect = BuildRect(menuCenter, BackdropSize, BackdropSize);
            DrawPanel(spriteBatch, pixel, backdropRect, new Color(8, 14, 20, 220), new Color(78, 110, 122, 190), 2);

            for (int i = 0; i < OmnitrixPlayer.TransformationSlotCount; i++) {
                Vector2 slotCenter = GetSlotCenter(i);
                Rectangle slotRect = BuildRect(slotCenter, SlotSize, SlotSize);
                Color accent = GetSlotAccent(activeOmnitrix, i);

                DrawConnection(spriteBatch, pixel, menuCenter, slotCenter, accent * 0.32f, 2f);
                DrawPanel(spriteBatch, pixel, slotRect,
                    i == previewSlotIndex ? new Color(22, 34, 42, 245) : new Color(12, 18, 24, 220),
                    accent, i == previewSlotIndex ? 3 : 2);

                DrawSlotIcon(spriteBatch, slotRect, GetSafeTransformationIcon(activeOmnitrix.transformationSlots[i]));

                Utils.DrawBorderString(spriteBatch, (i + 1).ToString(),
                    new Vector2(slotRect.X + 8, slotRect.Y + 6), new Color(225, 235, 245), 0.72f);
            }

            Rectangle centerPanel = BuildRect(menuCenter, CenterPanelWidth, CenterPanelHeight);
            DrawPanel(spriteBatch, pixel, centerPanel, new Color(6, 10, 14, 235), GetSlotAccent(activeOmnitrix, previewSlotIndex), 2);

            string transformationId = GetPreviewTransformationId(activeOmnitrix);
            Transformation previewTransformation = TransformationLoader.Get(transformationId);
            string displayName = string.IsNullOrEmpty(transformationId)
                ? "Empty Slot"
                : previewTransformation?.GetDisplayName(omp) ?? "Unknown Form";
            string slotLabel = previewSlotIndex >= 0 ? $"Slot {previewSlotIndex + 1}" : "No Slot";
            string hintText = string.IsNullOrEmpty(transformationId)
                ? "No transformation assigned"
                : "Release Q or left click to transform";

            Utils.DrawBorderString(spriteBatch, slotLabel, new Vector2(centerPanel.Center.X, centerPanel.Y + 10),
                new Color(180, 210, 225), 0.78f, 0.5f, 0f);
            Utils.DrawBorderString(spriteBatch, displayName, new Vector2(centerPanel.Center.X, centerPanel.Y + 34),
                Color.White, 1f, 0.5f, 0f);
            Utils.DrawBorderString(spriteBatch, hintText, new Vector2(centerPanel.Center.X, centerPanel.Y + 62),
                new Color(150, 175, 190), 0.74f, 0.5f, 0f);
        }

        private void Open(OmnitrixPlayer omp) {
            isOpen = true;
            menuCenter = ClampToScreen(Main.MouseScreen);
            previewSlotIndex = ResolveInitialPreviewSlot(omp);
            lastHoveredSlotIndex = previewSlotIndex;
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        private void Close() {
            isOpen = false;
            previewSlotIndex = -1;
            lastHoveredSlotIndex = -1;
        }

        private void ConfirmSelection(Player player, OmnitrixPlayer omp, Omnitrix activeOmnitrix) {
            int slotToTransform = previewSlotIndex;
            Close();

            if (slotToTransform < 0)
                return;

            activeOmnitrix.TryTransformToSlot(player, omp, slotToTransform);
        }

        private void UpdatePreviewSlot() {
            for (int i = 0; i < OmnitrixPlayer.TransformationSlotCount; i++) {
                if (!BuildRect(GetSlotCenter(i), SlotSize, SlotSize).Contains(Main.MouseScreen.ToPoint()))
                    continue;

                previewSlotIndex = i;
                if (lastHoveredSlotIndex != i) {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    lastHoveredSlotIndex = i;
                }

                return;
            }
        }

        private int ResolveInitialPreviewSlot(OmnitrixPlayer omp) {
            int selectedSlot = omp.GetSelectedTransformationSlotIndex();
            if (selectedSlot >= 0 && selectedSlot < OmnitrixPlayer.TransformationSlotCount)
                return selectedSlot;

            for (int i = 0; i < omp.transformationSlots.Length && i < OmnitrixPlayer.TransformationSlotCount; i++) {
                if (!string.IsNullOrEmpty(omp.transformationSlots[i]))
                    return i;
            }

            return 0;
        }

        private string GetPreviewTransformationId(Omnitrix activeOmnitrix) {
            if (previewSlotIndex < 0 || previewSlotIndex >= activeOmnitrix.transformationSlots.Length)
                return string.Empty;

            return activeOmnitrix.transformationSlots[previewSlotIndex] ?? string.Empty;
        }

        private Vector2 GetSlotCenter(int slotIndex) {
            float angle = MathHelper.ToRadians(-90f + slotIndex * (360f / OmnitrixPlayer.TransformationSlotCount));
            return menuCenter + angle.ToRotationVector2() * SlotRadius;
        }

        private static Rectangle BuildRect(Vector2 center, float width, float height) {
            return new Rectangle((int)(center.X - width * 0.5f), (int)(center.Y - height * 0.5f), (int)width, (int)height);
        }

        private static Vector2 ClampToScreen(Vector2 desiredCenter) {
            float clampedX = MathHelper.Clamp(desiredCenter.X, OpenMargin, Main.screenWidth - OpenMargin);
            float clampedY = MathHelper.Clamp(desiredCenter.Y, OpenMargin, Main.screenHeight - OpenMargin);
            return new Vector2(clampedX, clampedY);
        }

        private static void DrawPanel(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color fillColor, Color borderColor,
            int borderThickness) {
            spriteBatch.Draw(pixel, rect, fillColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - borderThickness, rect.Width, borderThickness), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, borderThickness, rect.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - borderThickness, rect.Y, borderThickness, rect.Height), borderColor);
        }

        private static void DrawConnection(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color,
            float thickness) {
            Vector2 edge = end - start;
            float fullLength = edge.Length();
            if (fullLength <= 0.001f)
                return;

            Vector2 direction = edge / fullLength;
            Vector2 trimmedStart = start + direction * 74f;
            Vector2 trimmedEnd = end - direction * (SlotSize * 0.5f + 8f);
            Vector2 trimmedEdge = trimmedEnd - trimmedStart;
            float trimmedLength = trimmedEdge.Length();
            if (trimmedLength <= 0.001f)
                return;

            float rotation = trimmedEdge.ToRotation();
            Vector2 midpoint = trimmedStart + trimmedEdge * 0.5f;
            spriteBatch.Draw(pixel, midpoint, null, color, rotation, Vector2.One * 0.5f,
                new Vector2(trimmedLength, thickness), SpriteEffects.None, 0f);
        }

        private static void DrawSlotIcon(SpriteBatch spriteBatch, Rectangle slotRect, Asset<Texture2D> iconAsset) {
            Texture2D texture = iconAsset.Value;
            float availableWidth = slotRect.Width - 16f;
            float availableHeight = slotRect.Height - 16f;
            float scale = Math.Min(availableWidth / texture.Width, availableHeight / texture.Height);
            Vector2 drawPosition = new Vector2(slotRect.Center.X, slotRect.Center.Y);

            spriteBatch.Draw(texture, drawPosition, null, Color.White, 0f,
                new Vector2(texture.Width * 0.5f, texture.Height * 0.5f), scale, SpriteEffects.None, 0f);
        }

        private static Asset<Texture2D> GetSafeTransformationIcon(string transformationId) {
            try {
                Transformation transformation = TransformationLoader.Get(transformationId);
                Asset<Texture2D> icon = transformation?.GetTransformationIcon();
                if (icon != null)
                    return icon;
            }
            catch {
            }

            return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien");
        }

        private Color GetSlotAccent(Omnitrix activeOmnitrix, int slotIndex) {
            if (slotIndex < 0 || slotIndex >= activeOmnitrix.transformationSlots.Length)
                return new Color(90, 100, 112);

            if (slotIndex == previewSlotIndex)
                return new Color(120, 255, 170);

            if (slotIndex == activeOmnitrix.transformationNum)
                return new Color(120, 190, 255);

            return string.IsNullOrEmpty(activeOmnitrix.transformationSlots[slotIndex])
                ? new Color(108, 112, 124)
                : new Color(92, 122, 138);
        }
    }
}
