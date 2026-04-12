using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using Ben10Mod.Content.Items.Armour;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Transformations.BigChill;

namespace Ben10Mod.Content.Interface {
    public class FittedTransformationIcon : UIElement {
        private const float IconPadding = 8f;
        private Asset<Texture2D> texture;
        private readonly Func<bool> isFavoriteProvider;
        private readonly Func<bool> isNewProvider;
        private readonly bool showHoverOutline;

        public FittedTransformationIcon(Asset<Texture2D> texture, Func<bool> isFavoriteProvider,
            Func<bool> isNewProvider = null, bool showHoverOutline = true) {
            this.texture = texture;
            this.isFavoriteProvider = isFavoriteProvider;
            this.isNewProvider = isNewProvider;
            this.showHoverOutline = showHoverOutline;
        }

        public void SetTexture(Asset<Texture2D> texture) {
            this.texture = texture;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);

            Texture2D iconTexture = texture?.Value;
            if (iconTexture == null)
                return;

            Rectangle iconRect = GetIconDrawRectangle(iconTexture);
            spriteBatch.Draw(iconTexture, iconRect, Color.White);

            if (showHoverOutline && ContainsPoint(Main.MouseScreen)) {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                spriteBatch.Draw(pixel, new Rectangle(iconRect.X, iconRect.Y, iconRect.Width, 1), Color.White);
                spriteBatch.Draw(pixel, new Rectangle(iconRect.X, iconRect.Bottom - 1, iconRect.Width, 1), Color.White);
                spriteBatch.Draw(pixel, new Rectangle(iconRect.X, iconRect.Y, 1, iconRect.Height), Color.White);
                spriteBatch.Draw(pixel, new Rectangle(iconRect.Right - 1, iconRect.Y, 1, iconRect.Height), Color.White);
            }

            if (isFavoriteProvider?.Invoke() == true) {
                Vector2 favoritePosition = new(iconRect.Right - 2f, iconRect.Y - 2f);
                Utils.DrawBorderString(spriteBatch, "★", favoritePosition, new Color(255, 220, 110), 0.92f, 1f, 0f);
            }

            if (isNewProvider?.Invoke() == true) {
                Vector2 newPosition = new(iconRect.X + 2f, iconRect.Y - 2f);
                Utils.DrawBorderString(spriteBatch, "NEW", newPosition, new Color(255, 110, 110), 0.7f, 0f, 0f);
            }
        }

        private Rectangle GetIconDrawRectangle(Texture2D iconTexture) {
            CalculatedStyle dims = GetDimensions();
            float availableWidth = Math.Max(1f, dims.Width - IconPadding * 2f);
            float availableHeight = Math.Max(1f, dims.Height - IconPadding * 2f);
            float scale = Math.Min(availableWidth / iconTexture.Width, availableHeight / iconTexture.Height);
            int drawWidth = Math.Max(1, (int)Math.Round(iconTexture.Width * scale));
            int drawHeight = Math.Max(1, (int)Math.Round(iconTexture.Height * scale));
            int drawX = (int)Math.Round(dims.X + (dims.Width - drawWidth) * 0.5f);
            int drawY = (int)Math.Round(dims.Y + (dims.Height - drawHeight) * 0.5f);
            return new Rectangle(drawX, drawY, drawWidth, drawHeight);
        }
    }

    public class UISystem : ModSystem {
        private readonly record struct HeroTrackerEntry(string Label, string ValueText, float Progress, Color AccentColor);
        private readonly record struct HeroTrackerPanel(string Title, List<HeroTrackerEntry> Entries);

        internal UserInterface               MyInterface;
        internal AlienSelectionScreen        AS;
        internal TransformationCodexScreen   TCS;
        internal TransformationPaletteScreen TPS;
        internal TransformationRadialMenu    TRM;
        private  GameTime                    _lastUpdateUiGameTime;

        private void EnsureInterfaceInitialized() {
            if (Main.dedServ || global::Ben10Mod.Ben10Mod.IsUnloading)
                return;

            MyInterface ??= new UserInterface();

            if (AS == null) {
                AS = new AlienSelectionScreen();
                AS.Activate();
            }

            if (TCS == null) {
                TCS = new TransformationCodexScreen();
                TCS.Activate();
            }

            if (TPS == null) {
                TPS = new TransformationPaletteScreen();
                TPS.Activate();
            }

            TRM ??= new TransformationRadialMenu();
        }

        public override void Load() {
            EnsureInterfaceInitialized();
        }

        public override void Unload() {
            // Avoid firing UI deactivation callbacks during mod teardown; they can touch player/mod state while
            // tModLoader is unloading content on a worker thread.
            TransformationPaletteScreen.ClearSharedState();
            MyInterface = null;
            AS          = null;
            TCS         = null;
            TPS         = null;
            TRM         = null;
            _lastUpdateUiGameTime = null;
        }

        public override void UpdateUI(GameTime gameTime) {
            if (global::Ben10Mod.Ben10Mod.IsUnloading)
                return;

            EnsureInterfaceInitialized();
            _lastUpdateUiGameTime = gameTime;
            TRM?.Update(this);
            if (MyInterface?.CurrentState != null)
                MyInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            if (global::Ben10Mod.Ben10Mod.IsUnloading)
                return;

            EnsureInterfaceInitialized();
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Ben10Mod: OmnitrixEnergyBar",
                    delegate {
                        DrawOmnitrixEnergyBar();
                        return true;
                    },
                    InterfaceScaleType.UI));
                mouseTextIndex++;

                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Ben10Mod: AlienSelection",
                    delegate {
                        if (_lastUpdateUiGameTime != null && MyInterface?.CurrentState != null)
                            MyInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        return true;
                    },
                    InterfaceScaleType.UI));
                mouseTextIndex++;

                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Ben10Mod: TransformationRadialMenu",
                    delegate {
                        TRM?.Draw(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        private void DrawOmnitrixEnergyBar() {
            Player player            = Main.LocalPlayer;
            var    omp               = player.GetModPlayer<OmnitrixPlayer>();
            var    clientConfig      = ModContent.GetInstance<Ben10ClientConfig>();
            bool   showEnergyBar     = clientConfig.ShowHeroEnergyBar && omp.omnitrixEquipped;
            bool   showMoveInterface = clientConfig.ShowHeroMoveInterface && omp.IsTransformed;
            bool   showAttackHudOnly = !showEnergyBar && showMoveInterface;
            bool   simplifyEnergyBar = clientConfig.UseSimplifiedHeroEnergyBar;
            bool   simplifyMoveHud   = clientConfig.UseSimplifiedHeroMoveInterface;
            List<HeroTrackerEntry> trackerEntries = BuildHeroTrackerEntries(player, omp);
            HeroTrackerPanel targetTracker = BuildFocusedTargetTracker(player);
            bool showTracker = trackerEntries.Count > 0;
            bool showTargetTracker = targetTracker.Entries.Count > 0;
            if (!showEnergyBar && !showMoveInterface && !showTracker && !showTargetTracker)
                return;

            int uiMargin   = 20;
            int gap        = 26;
            int hpBarWidth = 252;
            int y          = 30;
            int hpLeftX    = Main.screenWidth - uiMargin - hpBarWidth;
            int compactHudWidth = 220;
            int fullHudWidth = 252;
            int moveHudWidth = simplifyMoveHud ? compactHudWidth : fullHudWidth;
            int moveHudX = hpLeftX - gap - moveHudWidth;

            if (showAttackHudOnly) {
                int nextTrackerY = y;
                int attackHudHeight = DrawCurrentAttackIndicator(player, omp, moveHudX, nextTrackerY, moveHudWidth);
                if (attackHudHeight > 0)
                    nextTrackerY += attackHudHeight + 8;

                int activeAbilityHeight = DrawActiveAbilityIndicator(player, omp, moveHudX, nextTrackerY, moveHudWidth);
                if (activeAbilityHeight > 0)
                    nextTrackerY += activeAbilityHeight + 8;

                if (showTracker) {
                    int trackerHeight = DrawHeroTrackerPanel(trackerEntries, moveHudX, nextTrackerY, moveHudWidth);
                    if (trackerHeight > 0)
                        nextTrackerY += trackerHeight + 8;
                }

                if (showTargetTracker)
                    DrawHeroTrackerPanel(targetTracker.Entries, moveHudX, nextTrackerY, moveHudWidth, targetTracker.Title);
                return;
            }

            if (!showEnergyBar) {
                int nextTrackerY = y;
                if (showTracker) {
                    int trackerHeight = DrawHeroTrackerPanel(trackerEntries, moveHudX, nextTrackerY, moveHudWidth);
                    if (trackerHeight > 0)
                        nextTrackerY += trackerHeight + 8;
                }

                if (showTargetTracker)
                    DrawHeroTrackerPanel(targetTracker.Entries, moveHudX, nextTrackerY, moveHudWidth, targetTracker.Title);
                return;
            }

            float fillPercent = omp.omnitrixEnergyMax > 0f
                ? MathHelper.Clamp(omp.omnitrixEnergy / omp.omnitrixEnergyMax, 0f, 1f)
                : 0f;

            if (simplifyEnergyBar) {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                const int compactWidth = 220;
                const int compactBarHeight = 16;
                int compactX = hpLeftX - gap - compactWidth;
                Rectangle compactBarRect = new Rectangle(compactX, y + 8, compactWidth, compactBarHeight);
                Rectangle fillRect = new Rectangle(compactBarRect.X + 2, compactBarRect.Y + 2,
                    Math.Max(0, (int)((compactBarRect.Width - 4) * fillPercent)), Math.Max(1, compactBarRect.Height - 4));
                Color barBorder = new Color(88, 198, 138);
                Color barFill = new Color(92, 255, 148);

                Main.spriteBatch.Draw(pixel, compactBarRect, new Color(10, 18, 24, 190));
                Main.spriteBatch.Draw(pixel, new Rectangle(compactBarRect.X, compactBarRect.Y, compactBarRect.Width, 2), barBorder);
                Main.spriteBatch.Draw(pixel, new Rectangle(compactBarRect.X, compactBarRect.Bottom - 2, compactBarRect.Width, 2), barBorder);
                Main.spriteBatch.Draw(pixel, new Rectangle(compactBarRect.X, compactBarRect.Y, 2, compactBarRect.Height), barBorder);
                Main.spriteBatch.Draw(pixel, new Rectangle(compactBarRect.Right - 2, compactBarRect.Y, 2, compactBarRect.Height), barBorder);

                if (fillRect.Width > 0)
                    Main.spriteBatch.Draw(pixel, fillRect, barFill);

                DrawOmnitrixEnergyText(player, omp, compactBarRect, true, clientConfig.AlwaysShowOmnitrixEnergyText);

                int nextTrackerY = compactBarRect.Bottom + 8;
                if (showMoveInterface) {
                    int attackHudHeight = DrawCurrentAttackIndicator(player, omp, moveHudX, nextTrackerY, moveHudWidth);
                    if (attackHudHeight > 0)
                        nextTrackerY += attackHudHeight + 8;

                    int activeAbilityHeight = DrawActiveAbilityIndicator(player, omp, moveHudX, nextTrackerY, moveHudWidth);
                    if (activeAbilityHeight > 0)
                        nextTrackerY += activeAbilityHeight + 8;
                }

                if (showTracker) {
                    int trackerHeight = DrawHeroTrackerPanel(trackerEntries, moveHudX, nextTrackerY, moveHudWidth);
                    if (trackerHeight > 0)
                        nextTrackerY += trackerHeight + 8;
                }

                if (showTargetTracker)
                    DrawHeroTrackerPanel(targetTracker.Entries, moveHudX, nextTrackerY, moveHudWidth, targetTracker.Title);
                return;
            }

            Texture2D panelLeft  = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Left").Value;
            Texture2D panelMid   = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Middle").Value;
            Texture2D panelRight = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Right").Value;
            Texture2D fillTex    = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Fill").Value;

            int midCount = 20;
            int barWidth = panelLeft.Width + panelMid.Width * midCount + panelRight.Width;

            int x = hpLeftX - gap - barWidth;

            int barHeight = Math.Max(panelLeft.Height, Math.Max(panelMid.Height, panelRight.Height));

            int yLeft  = y + (barHeight - panelLeft.Height) / 2;
            int yMid   = y + (barHeight - panelMid.Height) / 2;
            int yRight = y + (barHeight - panelRight.Height) / 2;

            Main.spriteBatch.Draw(panelLeft, new Vector2(x, yLeft), Color.White);

            int midStartX = x + panelLeft.Width;
            int midEndX   = x + barWidth - panelRight.Width;
            for (int drawX = midStartX; drawX < midEndX; drawX += panelMid.Width) {
                int       w   = Math.Min(panelMid.Width, midEndX - drawX);
                Rectangle src = new Rectangle(0, 0, w, panelMid.Height);
                Main.spriteBatch.Draw(panelMid, new Vector2(drawX, yMid), src, Color.White);
            }

            Main.spriteBatch.Draw(panelRight, new Vector2(x + barWidth - panelRight.Width, yRight), Color.White);

            int padLeft   = 6;
            int padRight  = 6;
            int padTop    = 6;
            int padBottom = 6;

            int innerX      = x + padLeft;
            int innerY      = y + padTop;
            int innerWidth  = barWidth - padLeft - padRight;
            int innerHeight = barHeight - padTop - padBottom;

            if (innerWidth < 1 || innerHeight < 1) return;

            int fillWidth                                    = (int)(innerWidth * fillPercent);
            if (fillPercent > 0f && fillWidth < 1) fillWidth = 1;

            if (fillWidth > 0) {
                Rectangle fillRect = new Rectangle(innerX, innerY, fillWidth, innerHeight);
                Main.spriteBatch.Draw(fillTex, fillRect, Color.White);
            }

            Rectangle barRect = new Rectangle(x, y, barWidth, barHeight);
            DrawOmnitrixEnergyText(player, omp, barRect, false, clientConfig.AlwaysShowOmnitrixEnergyText);

            int fullHudNextY = y + barHeight + 18;
            if (showMoveInterface) {
                int fullAttackHudHeight = DrawCurrentAttackIndicator(player, omp, moveHudX, fullHudNextY, moveHudWidth);
                if (fullAttackHudHeight > 0)
                    fullHudNextY += fullAttackHudHeight + 8;

                int activeAbilityHeight = DrawActiveAbilityIndicator(player, omp, moveHudX, fullHudNextY, moveHudWidth);
                if (activeAbilityHeight > 0)
                    fullHudNextY += activeAbilityHeight + 8;
            }

            if (showTracker) {
                int trackerHeight = DrawHeroTrackerPanel(trackerEntries, moveHudX, fullHudNextY, moveHudWidth);
                if (trackerHeight > 0)
                    fullHudNextY += trackerHeight + 8;
            }

            if (showTargetTracker)
                DrawHeroTrackerPanel(targetTracker.Entries, moveHudX, fullHudNextY, moveHudWidth, targetTracker.Title);
        }

        private void DrawOmnitrixEnergyText(Player player, OmnitrixPlayer omp, Rectangle barRect, bool simplified, bool alwaysShow) {
            if (alwaysShow) {
                DrawOmnitrixEnergyInlineText(omp, barRect, simplified);
                return;
            }

            DrawOmnitrixEnergyHoverText(player, omp, barRect, simplified);
        }

        private void DrawOmnitrixEnergyInlineText(OmnitrixPlayer omp, Rectangle barRect, bool simplified) {
            string energyText = $"OE {(int)omp.omnitrixEnergy}/{(int)omp.omnitrixEnergyMax}";

            if (simplified) {
                Utils.DrawBorderString(Main.spriteBatch, energyText,
                    new Vector2(barRect.Center.X, barRect.Y - 16f), Color.White, 0.78f, 0.5f, 0f);
                return;
            }

            Utils.DrawBorderString(Main.spriteBatch,
                $"{(int)omp.omnitrixEnergy}/{(int)omp.omnitrixEnergyMax}",
                new Vector2(barRect.Center.X, barRect.Y - 12f),
                Color.White,
                0.9f,
                0.5f,
                0.5f
            );
        }

        private void DrawOmnitrixEnergyHoverText(Player player, OmnitrixPlayer omp, Rectangle barRect, bool simplified) {
            if (!barRect.Contains(Main.MouseScreen.ToPoint()))
                return;

            player.mouseInterface = true;

            string energyText = simplified
                ? $"OE {(int)omp.omnitrixEnergy}/{(int)omp.omnitrixEnergyMax}"
                : $"{(int)omp.omnitrixEnergy}/{(int)omp.omnitrixEnergyMax}";
            float textScale = simplified ? 0.78f : 0.9f;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(energyText) * textScale;
            Vector2 drawPosition = Main.MouseScreen + new Vector2(16f, 20f);

            if (drawPosition.X + textSize.X > Main.screenWidth - 8f)
                drawPosition.X = Main.screenWidth - textSize.X - 8f;

            if (drawPosition.Y + textSize.Y > Main.screenHeight - 8f)
                drawPosition.Y = Main.screenHeight - textSize.Y - 8f;

            drawPosition.X = Math.Max(8f, drawPosition.X);
            drawPosition.Y = Math.Max(8f, drawPosition.Y);

            Utils.DrawBorderString(Main.spriteBatch, energyText, drawPosition, Color.White, textScale);
        }

        private int DrawCurrentAttackIndicator(Player player, OmnitrixPlayer omp, int x, int y, int width) {
            bool showAttackHud    = omp.IsTransformed;
            bool showSelectionHud = !omp.IsTransformed && omp.GetActiveOmnitrix() != null;
            var  clientConfig     = ModContent.GetInstance<Ben10ClientConfig>();

            if (!showAttackHud && !showSelectionHud)
                return 0;

            Transformation trans = omp.CurrentTransformation;
            if (showAttackHud && trans == null)
                return 0;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            string cooldownSummary =
                showAttackHud ? omp.GetAttackHudCooldownSummary() : omp.GetSelectionHudCooldownSummary();
            string detailLabel = showAttackHud
                ? omp.GetCurrentAttackModeSummary()
                : omp.GetSelectedTransformationStatusSummary();
            string selectionSummary = showAttackHud
                ? string.Empty
                : cooldownSummary;
            bool simplifiedMoveHud = clientConfig.UseSimplifiedHeroMoveInterface;
            string resourceSummary = showAttackHud
                ? omp.GetCurrentAttackResourceSummary(compact: true)
                : string.Empty;
            int energyCost = showAttackHud ? trans.GetEnergyCost(omp) : 0;
            bool affordabilityWarning = clientConfig.ShowHeroAffordabilityTinting &&
                                        showAttackHud && trans != null && !omp.CanAffordCurrentAttackForHud();
            string compactFooter = affordabilityWarning
                ? $"Need {energyCost} OE"
                : showAttackHud && !string.IsNullOrWhiteSpace(resourceSummary)
                    ? resourceSummary
                    : !showAttackHud && !string.IsNullOrWhiteSpace(selectionSummary)
                        ? selectionSummary
                        : !string.IsNullOrWhiteSpace(cooldownSummary)
                    ? cooldownSummary
                    : energyCost > 0
                        ? $"{energyCost} OE"
                        : detailLabel;
            Rectangle panelRect = new Rectangle(x, y, width, simplifiedMoveHud
                ? (string.IsNullOrWhiteSpace(compactFooter) ? 42 : 58)
                : string.IsNullOrWhiteSpace(showAttackHud ? cooldownSummary : selectionSummary) ? 66 : 92);
            bool      holdingBadge = player.HeldItem.ModItem is PlumbersBadge;
            float     pulse        = MathHelper.Clamp(omp.AttackSelectionPulseProgress, 0f, 1f);
            float     ultimateReadyPulse = showAttackHud ? MathHelper.Clamp(omp.UltimateReadyCueProgress, 0f, 1f) : 0f;
            Color accent = showAttackHud
                ? omp.GetCurrentAttackAccentColor()
                : omp.GetSelectedTransformationAccentColor();
            if (affordabilityWarning)
                accent = new Color(235, 96, 72);
            Color borderColor = Color.Lerp(new Color(70, 90, 110), accent, 0.55f + pulse * 0.45f);
            Color fillColor = holdingBadge
                ? new Color(10, 18, 24, 205)
                : new Color(10, 18, 24, 155);

            if (pulse > 0f) {
                Rectangle glowRect = new Rectangle(panelRect.X - 2, panelRect.Y - 2, panelRect.Width + 4,
                    panelRect.Height + 4);
                Main.spriteBatch.Draw(pixel, glowRect, accent * (0.16f * pulse));
            }

            if (ultimateReadyPulse > 0f) {
                Rectangle ultimateGlowRect = new Rectangle(panelRect.X - 4, panelRect.Y - 4, panelRect.Width + 8,
                    panelRect.Height + 8);
                Main.spriteBatch.Draw(pixel, ultimateGlowRect, new Color(255, 220, 110) * (0.14f * ultimateReadyPulse));
                Utils.DrawBorderString(Main.spriteBatch, "ULT READY",
                    new Vector2(panelRect.Center.X, panelRect.Y - 14f), new Color(255, 232, 145), 0.8f, 0.5f, 0f);
            }

            Main.spriteBatch.Draw(pixel, panelRect, fillColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2),
                borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height),
                borderColor);

            string slotLabel = showAttackHud
                ? omp.GetCurrentAttackSelectionLabel()
                : omp.GetSelectedTransformationHudLabel();
            string attackName = showAttackHud
                ? omp.GetCurrentAttackDisplayName()
                : omp.GetSelectedTransformationDisplayName();

            if (simplifiedMoveHud) {
                float innerWidth = panelRect.Width - 20f;
                string fittedSlotLabel = FitHudText(slotLabel, innerWidth * 0.34f, 0.72f);
                float slotWidth = MeasureHudTextWidth(fittedSlotLabel, 0.72f);
                string fittedAttackName = FitHudText(attackName, Math.Max(48f, innerWidth - slotWidth - 10f), 0.88f);
                string fittedFooter = FitHudText(compactFooter, innerWidth, 0.68f);

                Utils.DrawBorderString(Main.spriteBatch, fittedAttackName,
                    new Vector2(panelRect.X + 10, panelRect.Y + 9), Color.White, 0.88f);
                Utils.DrawBorderString(Main.spriteBatch, fittedSlotLabel,
                    new Vector2(panelRect.Right - 10, panelRect.Y + 10), borderColor, 0.72f, 1f, 0f);

                if (!string.IsNullOrWhiteSpace(fittedFooter)) {
                    Utils.DrawBorderString(Main.spriteBatch, fittedFooter,
                        new Vector2(panelRect.X + 10, panelRect.Bottom - 19),
                        affordabilityWarning ? new Color(255, 170, 145) : new Color(170, 190, 208), 0.68f);
                }
                return panelRect.Height;
            }

            string title = showAttackHud ? "Attack" : "Selection";
            float fullInnerWidth = panelRect.Width - 20f;
            string fittedSlotLabelFull = FitHudText(slotLabel, fullInnerWidth * 0.34f, 0.82f);
            float slotWidthFull = MeasureHudTextWidth(fittedSlotLabelFull, 0.82f);
            string fittedTitle = FitHudText(title, Math.Max(48f, fullInnerWidth - slotWidthFull - 10f), 0.8f);
            string fittedAttackNameFull = FitHudText(attackName, fullInnerWidth, 0.96f);
            string fittedResourceSummary = FitHudText(
                affordabilityWarning ? $"Need {energyCost} OE" : resourceSummary,
                Math.Max(72f, fullInnerWidth * 0.42f), 0.86f);
            float resourceWidth = MeasureHudTextWidth(fittedResourceSummary, 0.86f);
            string fittedDetailLabel = FitHudText(detailLabel,
                string.IsNullOrWhiteSpace(fittedResourceSummary) ? fullInnerWidth : Math.Max(48f, fullInnerWidth - resourceWidth - 12f),
                0.72f);
            string lowerSummary = showAttackHud ? cooldownSummary : selectionSummary;
            string fittedLowerSummary = FitHudText(lowerSummary, fullInnerWidth, 0.72f);

            Utils.DrawBorderString(Main.spriteBatch, fittedTitle, new Vector2(panelRect.X + 10, panelRect.Y + 7),
                new Color(220, 230, 240), 0.8f);
            Utils.DrawBorderString(Main.spriteBatch, fittedSlotLabelFull, new Vector2(panelRect.Right - 10, panelRect.Y + 7),
                borderColor, 0.82f, 1f, 0f);
            Utils.DrawBorderString(Main.spriteBatch, fittedAttackNameFull, new Vector2(panelRect.X + 10, panelRect.Y + 25),
                Color.White, 0.96f);

            if (!string.IsNullOrWhiteSpace(fittedDetailLabel)) {
                Utils.DrawBorderString(Main.spriteBatch, fittedDetailLabel,
                    new Vector2(panelRect.X + 10, panelRect.Y + 45), new Color(180, 195, 210), 0.72f);
            }

            if (!string.IsNullOrWhiteSpace(fittedResourceSummary)) {
                Utils.DrawBorderString(Main.spriteBatch, fittedResourceSummary,
                    new Vector2(panelRect.Right - 10, panelRect.Y + 43),
                    affordabilityWarning ? new Color(255, 170, 145) : accent, 0.86f, 1f, 0f);
            }

            if (!string.IsNullOrWhiteSpace(fittedLowerSummary)) {
                Utils.DrawBorderString(Main.spriteBatch, fittedLowerSummary,
                    new Vector2(panelRect.X + 10, panelRect.Bottom - 22), new Color(170, 190, 208), 0.72f);
            }

            return panelRect.Height;
        }

        private static string CombineHudSummary(params string[] parts) {
            if (parts == null || parts.Length == 0)
                return string.Empty;

            List<string> visibleParts = new();
            for (int i = 0; i < parts.Length; i++) {
                if (!string.IsNullOrWhiteSpace(parts[i]))
                    visibleParts.Add(parts[i]);
            }

            return visibleParts.Count == 0 ? string.Empty : string.Join("  |  ", visibleParts);
        }

        private static string NormalizeHudText(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            StringBuilder builder = new(text.Length);
            bool lastWasWhitespace = false;
            for (int i = 0; i < text.Length; i++) {
                char character = text[i];
                if (char.IsWhiteSpace(character)) {
                    if (lastWasWhitespace)
                        continue;

                    builder.Append(' ');
                    lastWasWhitespace = true;
                    continue;
                }

                builder.Append(character);
                lastWasWhitespace = false;
            }

            return builder.ToString().Trim();
        }

        private static float MeasureHudTextWidth(string text, float scale) {
            string normalized = NormalizeHudText(text);
            if (string.IsNullOrWhiteSpace(normalized))
                return 0f;

            return FontAssets.MouseText.Value.MeasureString(normalized).X * scale;
        }

        private static string FitHudText(string text, float maxWidth, float scale) {
            string normalized = NormalizeHudText(text);
            if (string.IsNullOrWhiteSpace(normalized) || maxWidth <= 8f)
                return string.Empty;

            if (MeasureHudTextWidth(normalized, scale) <= maxWidth)
                return normalized;

            const string ellipsis = "...";
            if (MeasureHudTextWidth(ellipsis, scale) > maxWidth)
                return string.Empty;

            int low = 0;
            int high = normalized.Length;
            while (low < high) {
                int mid = (low + high + 1) / 2;
                string candidate = normalized[..mid].TrimEnd(' ', ',', ';', ':', '|', '-') + ellipsis;
                if (MeasureHudTextWidth(candidate, scale) <= maxWidth)
                    low = mid;
                else
                    high = mid - 1;
            }

            if (low <= 0)
                return ellipsis;

            int cutIndex = normalized.LastIndexOf(' ', Math.Min(low - 1, normalized.Length - 1));
            if (cutIndex < low / 2)
                cutIndex = low;

            return normalized[..cutIndex].TrimEnd(' ', ',', ';', ':', '|', '-') + ellipsis;
        }

        private int DrawActiveAbilityIndicator(Player player, OmnitrixPlayer omp, int x, int y, int width) {
            if (!omp.IsTransformed)
                return 0;

            List<OmnitrixPlayer.ActiveAbilityStatus> activeAbilities = omp.GetActiveAbilityStatuses();
            if (activeAbilities.Count == 0)
                return 0;

            var clientConfig = ModContent.GetInstance<Ben10ClientConfig>();
            bool simplified = clientConfig.UseSimplifiedHeroMoveInterface;
            int lineHeight = simplified ? 15 : 18;
            int headerHeight = simplified ? 18 : 22;
            int panelHeight = headerHeight + 10 + activeAbilities.Count * lineHeight;
            Rectangle panelRect = new Rectangle(x, y, width, panelHeight);
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Color accent = activeAbilities[0].AccentColor;
            Color borderColor = Color.Lerp(new Color(70, 90, 110), accent, 0.58f);
            Color fillColor = new Color(10, 18, 24, 185);

            Main.spriteBatch.Draw(pixel, panelRect, fillColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height), borderColor);

            string title = activeAbilities.Count == 1 ? "Active Ability" : "Active Abilities";
            float titleScale = simplified ? 0.68f : 0.78f;
            float panelInnerWidth = panelRect.Width - 20f;
            string fittedTitle = FitHudText(title, panelInnerWidth, titleScale);
            Utils.DrawBorderString(Main.spriteBatch, fittedTitle, new Vector2(panelRect.X + 10, panelRect.Y + 6),
                new Color(220, 230, 240), titleScale);

            int lineY = panelRect.Y + headerHeight;
            for (int i = 0; i < activeAbilities.Count; i++) {
                OmnitrixPlayer.ActiveAbilityStatus status = activeAbilities[i];
                float lineScale = simplified ? 0.7f : 0.78f;
                float valueScale = lineScale + 0.02f;
                string fittedRemaining = FitHudText(status.RemainingText, panelInnerWidth * 0.34f, valueScale);
                float remainingWidth = MeasureHudTextWidth(fittedRemaining, valueScale);
                string fittedDisplayName = FitHudText(status.DisplayName,
                    string.IsNullOrWhiteSpace(fittedRemaining) ? panelInnerWidth : Math.Max(48f, panelInnerWidth - remainingWidth - 10f),
                    lineScale);

                Utils.DrawBorderString(Main.spriteBatch, fittedDisplayName,
                    new Vector2(panelRect.X + 10, lineY + i * lineHeight), status.AccentColor, lineScale);
                Utils.DrawBorderString(Main.spriteBatch, fittedRemaining,
                    new Vector2(panelRect.Right - 10, lineY + i * lineHeight),
                    Color.Lerp(status.AccentColor, Color.White, 0.2f), valueScale, 1f, 0f);
            }

            return panelRect.Height;
        }

        private List<HeroTrackerEntry> BuildHeroTrackerEntries(Player player, OmnitrixPlayer omp) {
            List<HeroTrackerEntry> entries = new();
            AlienIdentityPlayer identityPlayer = player.GetModPlayer<AlienIdentityPlayer>();
            HeroPlumberArmorPlayer armorPlayer = player.GetModPlayer<HeroPlumberArmorPlayer>();
            var fourArmsPlayer = player.GetModPlayer<global::Ben10Mod.Content.Transformations.FourArms.FourArmsGroundSlamPlayer>();
            var cannonboltPlayer = player.GetModPlayer<global::Ben10Mod.Content.Transformations.Cannonbolt.CannonboltStatePlayer>();
            var echoEchoPlayer = player.GetModPlayer<global::Ben10Mod.Content.Transformations.EchoEcho.EchoEchoStatePlayer>();
            var chromaStonePlayer = player.GetModPlayer<global::Ben10Mod.Content.Transformations.ChromaStone.ChromaStoneStatePlayer>();
            int cannonboltMaxBounces = global::Ben10Mod.Content.Transformations.Cannonbolt.CannonboltStatePlayer.MaxBounceCount;

            if (omp.heroConvergenceEmblemEquipped || omp.HeroConvergenceHitCount > 0 || omp.HeroConvergenceCooldownTicks > 0) {
                bool burstCoolingDown = omp.HeroConvergenceCooldownTicks > 0;
                float convergenceProgress = burstCoolingDown
                    ? 1f - omp.HeroConvergenceCooldownTicks / (float)Math.Max(1, omp.HeroConvergenceCooldownMaxTicks)
                    : MathHelper.Clamp(omp.HeroConvergenceHitCount / (float)Math.Max(1, omp.HeroConvergenceRequiredHits), 0f, 1f);
                string convergenceValue = burstCoolingDown
                    ? $"CD {FormatTrackerSeconds(omp.HeroConvergenceCooldownTicks)}"
                    : $"{omp.HeroConvergenceHitCount}/{omp.HeroConvergenceRequiredHits}";
                Color convergenceAccent = burstCoolingDown ? new Color(255, 218, 120) : new Color(132, 255, 176);
                entries.Add(new HeroTrackerEntry("Convergence", convergenceValue, convergenceProgress, convergenceAccent));
            }

            if (omp.omniCoreReactorEquipped && (omp.IsTransformed || omp.OmniCoreReactorChargeValue > 0f)) {
                float threshold = Math.Max(1f, omp.OmniCoreReactorChargeThresholdValue);
                float chargeProgress = MathHelper.Clamp(omp.OmniCoreReactorChargeValue / threshold, 0f, 1f);
                string chargeValue = omp.OmniCoreReactorChargeValue >= threshold
                    ? "Ready"
                    : $"{(int)Math.Round(omp.OmniCoreReactorChargeValue)}/{(int)Math.Round(threshold)}";
                Color reactorAccent = omp.OmniCoreReactorChargeValue >= threshold
                    ? new Color(170, 255, 225)
                    : new Color(118, 238, 210);
                entries.Add(new HeroTrackerEntry("Omni-Core", chargeValue, chargeProgress, reactorAccent));
            }

            switch (omp.currentTransformationId) {
                case global::Ben10Mod.Content.Transformations.FourArms.FourArmsGroundSlamPlayer.TransformationId:
                    if (fourArmsPlayer.BerserkActive) {
                        entries.Add(new HeroTrackerEntry("Berserk",
                            FormatTrackerSeconds(fourArmsPlayer.BerserkTicksRemaining),
                            MathHelper.Clamp(fourArmsPlayer.BerserkProgress, 0f, 1f), new Color(255, 158, 104)));
                    }
                    else {
                        string rageValue = fourArmsPlayer.HasBerserkThreshold
                            ? "Ready"
                            : $"{(int)Math.Round(fourArmsPlayer.RageRatio * 100f)}%";
                        Color rageAccent = fourArmsPlayer.HasBerserkThreshold
                            ? new Color(255, 215, 140)
                            : new Color(255, 142, 102);
                        entries.Add(new HeroTrackerEntry("Rage", rageValue,
                            MathHelper.Clamp(fourArmsPlayer.RageRatio, 0f, 1f), rageAccent));
                    }
                    break;
                case global::Ben10Mod.Content.Transformations.Cannonbolt.CannonboltStatePlayer.TransformationId:
                    if (cannonboltPlayer.SiegeActive) {
                        entries.Add(new HeroTrackerEntry("Siege",
                            FormatTrackerSeconds(cannonboltPlayer.SiegeTicksRemaining),
                            MathHelper.Clamp(cannonboltPlayer.SiegeProgress, 0f, 1f), new Color(255, 210, 132)));
                    }
                    else if (cannonboltPlayer.IsRolled || cannonboltPlayer.RollSpeedRatio > 0f) {
                        string rollValue = cannonboltPlayer.IsRolled
                            ? $"{cannonboltPlayer.RollStateLabel} {(int)Math.Round(cannonboltPlayer.RollSpeedRatio * 100f)}%"
                            : $"{(int)Math.Round(cannonboltPlayer.RollSpeedRatio * 100f)}%";
                        entries.Add(new HeroTrackerEntry("Roll", rollValue,
                            MathHelper.Clamp(cannonboltPlayer.RollSpeedRatio, 0f, 1f), new Color(232, 196, 128)));
                    }

                    if (cannonboltPlayer.IsRolled || cannonboltPlayer.ImpactChargeRatio > 0f) {
                        entries.Add(new HeroTrackerEntry("Impact",
                            $"{cannonboltPlayer.BounceCount}/{cannonboltMaxBounces}",
                            MathHelper.Clamp(cannonboltPlayer.ImpactChargeRatio, 0f, 1f), new Color(255, 182, 112)));
                    }

                    if (cannonboltPlayer.GyroShellActive) {
                        entries.Add(new HeroTrackerEntry("Gyro",
                            FormatTrackerSeconds(cannonboltPlayer.GyroTicksRemaining),
                            MathHelper.Clamp(cannonboltPlayer.GyroProgress, 0f, 1f), new Color(255, 232, 164)));
                    }
                    break;
                case global::Ben10Mod.Content.Transformations.EchoEcho.EchoEchoStatePlayer.TransformationId:
                    entries.Add(new HeroTrackerEntry("Echoes", $"{echoEchoPlayer.ActiveEchoCount}/{echoEchoPlayer.MaxEchoCount}",
                        MathHelper.Clamp(echoEchoPlayer.ActiveEchoCount / (float)Math.Max(1, echoEchoPlayer.MaxEchoCount), 0f, 1f),
                        new Color(150, 210, 255)));
                    if (echoEchoPlayer.ChorusActive) {
                        entries.Add(new HeroTrackerEntry("Chorus", FormatTrackerSeconds(echoEchoPlayer.ChorusTicksRemaining),
                            MathHelper.Clamp(echoEchoPlayer.ChorusProgress, 0f, 1f), new Color(205, 238, 255)));
                    }
                    break;
                case AlienIdentityPlayer.ChromaStoneTransformationId:
                    string radianceValue = chromaStonePlayer.HasDischargeThreshold
                        ? "Ready"
                        : $"{(int)Math.Round(chromaStonePlayer.RadianceRatio * 100f)}%";
                    Color radianceAccent = chromaStonePlayer.HasDischargeThreshold
                        ? new Color(255, 216, 150)
                        : new Color(190, 255, 230);
                    entries.Add(new HeroTrackerEntry("Radiance", radianceValue,
                        MathHelper.Clamp(chromaStonePlayer.RadianceRatio, 0f, 1f), radianceAccent));

                    if (chromaStonePlayer.DischargeActive) {
                        entries.Add(new HeroTrackerEntry("Discharge",
                            "Channeling",
                            MathHelper.Clamp(Math.Max(chromaStonePlayer.ActiveDischargeRadianceRatio,
                                chromaStonePlayer.ActiveDischargeFacetPower / 3f), 0f, 1f), new Color(255, 214, 132)));
                    }

                    entries.Add(new HeroTrackerEntry("Facets", $"{chromaStonePlayer.VisibleFacetCount}/3",
                        MathHelper.Clamp(chromaStonePlayer.VisibleFacetCount / 3f, 0f, 1f), new Color(188, 224, 255)));

                    if (chromaStonePlayer.Guarding) {
                        float guardProgress = Math.Max(chromaStonePlayer.GuardHoldRatio, chromaStonePlayer.GuardStoredRatio);
                        entries.Add(new HeroTrackerEntry("Guard", $"{(int)Math.Round(chromaStonePlayer.GuardStoredEnergy)}",
                            MathHelper.Clamp(guardProgress, 0f, 1f), new Color(166, 235, 255)));
                    }
                    break;
                case AlienIdentityPlayer.FasttrackTransformationId:
                    entries.Add(new HeroTrackerEntry("Momentum", $"{(int)Math.Round(identityPlayer.FasttrackMomentumRatio * 100f)}%",
                        MathHelper.Clamp(identityPlayer.FasttrackMomentumRatio, 0f, 1f), new Color(145, 255, 150)));
                    break;
                case AlienIdentityPlayer.AstrodactylTransformationId:
                    entries.Add(new HeroTrackerEntry("Air Supremacy", $"{(int)Math.Round(identityPlayer.AstrodactylAirSupremacyRatio * 100f)}%",
                        MathHelper.Clamp(identityPlayer.AstrodactylAirSupremacyRatio, 0f, 1f), new Color(150, 255, 220)));
                    break;
                case AlienIdentityPlayer.FrankenstrikeTransformationId:
                    entries.Add(new HeroTrackerEntry("Static Charge", $"{(int)Math.Round(identityPlayer.FrankenstrikeStaticChargeRatio * 100f)}%",
                        MathHelper.Clamp(identityPlayer.FrankenstrikeStaticChargeRatio, 0f, 1f), new Color(135, 175, 255)));
                    break;
                case AlienIdentityPlayer.WaterHazardTransformationId:
                    entries.Add(new HeroTrackerEntry("Pressure", $"{(int)Math.Round(identityPlayer.WaterHazardPressureRatio * 100f)}%",
                        MathHelper.Clamp(identityPlayer.WaterHazardPressureRatio, 0f, 1f), new Color(120, 220, 255)));
                    break;
            }

            if (armorPlayer.bulwarkSet && omp.IsTransformed) {
                entries.Add(new HeroTrackerEntry("Bulwark", $"{armorPlayer.BulwarkVisibleChargeHits}/8",
                    MathHelper.Clamp(armorPlayer.BulwarkVisibleChargeHits / 8f, 0f, 1f), new Color(245, 228, 155)));
            }

            return entries;
        }

        private HeroTrackerPanel BuildFocusedTargetTracker(Player player) {
            NPC targetNpc = FindTrackedTargetNpc(player);
            List<HeroTrackerEntry> entries = new();
            if (targetNpc == null)
                return new HeroTrackerPanel(null, entries);

            AlienIdentityGlobalNPC npcState = targetNpc.GetGlobalNPC<AlienIdentityGlobalNPC>();

            if (npcState.IsFasttrackComboActiveFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Combo", $"{npcState.FasttrackComboStacks}/6",
                    MathHelper.Clamp(npcState.FasttrackComboStacks / 6f, 0f, 1f), new Color(255, 182, 102)));
            }

            if (npcState.IsSkyMarkedFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Sky Mark", FormatTrackerSeconds(npcState.AstrodactylSkyMarkTime),
                    MathHelper.Clamp(npcState.AstrodactylSkyMarkTime / 360f, 0f, 1f), new Color(146, 255, 176)));
            }

            if (npcState.IsBlitzwolferResonantFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Resonance", $"{npcState.BlitzwolferResonanceStacks}/8",
                    MathHelper.Clamp(npcState.BlitzwolferResonanceStacks / 8f, 0f, 1f), new Color(126, 255, 154)));
            }

            if (npcState.IsEchoEchoResonantFor(player.whoAmI)) {
                string echoResonanceValue = npcState.IsEchoEchoResonancePrimedFor(player.whoAmI)
                    ? "Pop Ready"
                    : $"{npcState.EchoEchoResonanceStacks}/8";
                entries.Add(new HeroTrackerEntry("Resonance", echoResonanceValue,
                    MathHelper.Clamp(npcState.EchoEchoResonanceStacks / 8f, 0f, 1f), new Color(166, 224, 255)));
            }

            if (npcState.IsEchoEchoFracturedFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Fracture", FormatTrackerSeconds(npcState.EchoEchoFractureTime),
                    MathHelper.Clamp(npcState.EchoEchoFractureTime / 180f, 0f, 1f), new Color(210, 238, 255)));
            }

            if (npcState.IsUltimateEchoEchoFocusedFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Focused", FormatTrackerSeconds(npcState.UltimateEchoEchoFocusedTime),
                    MathHelper.Clamp(npcState.UltimateEchoEchoFocusedTime / 150f, 0f, 1f), new Color(166, 228, 255)));
            }

            if (npcState.IsFrankenstrikeConductiveFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Conductive", $"{npcState.FrankenstrikeConductiveStacks}/6",
                    MathHelper.Clamp(npcState.FrankenstrikeConductiveStacks / 6f, 0f, 1f), new Color(135, 175, 255)));
            }

            if (npcState.HasLodestarPolarityFor(player.whoAmI)) {
                string polarity = npcState.LodestarPolarityDirection >= 0 ? "Pull" : "Push";
                entries.Add(new HeroTrackerEntry("Polarity", polarity,
                    MathHelper.Clamp(npcState.LodestarPolarityTime / 300f, 0f, 1f), new Color(212, 140, 255)));
            }

            if (npcState.IsWaterHazardSoakedFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Soak", $"{npcState.WaterHazardSoak}/100",
                    MathHelper.Clamp(npcState.WaterHazardSoak / 100f, 0f, 1f), new Color(120, 220, 255)));
            }

            if (npcState.IsJetrayLockedFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Lock", FormatTrackerSeconds(npcState.JetrayLockTime),
                    MathHelper.Clamp(npcState.JetrayLockTime / 420f, 0f, 1f), new Color(118, 255, 224)));
            }

            if (npcState.IsBigChillDeepFrozenFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Deep Freeze", FormatTrackerSeconds(npcState.BigChillDeepFreezeTime),
                    MathHelper.Clamp(npcState.BigChillDeepFreezeTime / (float)BigChillTransformation.DeepFreezeDurationTicks, 0f, 1f),
                    new Color(188, 236, 255)));
            }
            else if (npcState.HasBigChillFrostbiteFor(player.whoAmI)) {
                float frostbiteRatio = npcState.GetBigChillFrostbiteStacks(player.whoAmI) /
                                       (float)BigChillTransformation.FrostbiteThreshold;
                entries.Add(new HeroTrackerEntry("Frostbite",
                    $"{npcState.GetBigChillFrostbiteStacks(player.whoAmI)}/{BigChillTransformation.FrostbiteThreshold}",
                    MathHelper.Clamp(frostbiteRatio, 0f, 1f), new Color(145, 215, 255)));
            }

            if (npcState.IsBigChillFrigidFracturedFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Fracture", FormatTrackerSeconds(npcState.BigChillFrigidFractureTime),
                    MathHelper.Clamp(npcState.BigChillFrigidFractureTime / 240f, 0f, 1f), new Color(225, 246, 255)));
            }

            if (npcState.IsWhampirePreyFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Prey", FormatTrackerSeconds(npcState.WhampirePreyTime),
                    MathHelper.Clamp(npcState.WhampirePreyTime / 420f, 0f, 1f), new Color(255, 128, 144)));
            }

            if (npcState.IsSnareOhCursedFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Curse", $"{npcState.SnareOhCurseStacks}/7",
                    MathHelper.Clamp(npcState.SnareOhCurseStacks / 7f, 0f, 1f), new Color(216, 194, 115)));
            }

            if (npcState.IsAlienXJudgedFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Judgement", $"{npcState.AlienXJudgementStacks}/6",
                    MathHelper.Clamp(npcState.AlienXJudgementStacks / 6f, 0f, 1f), new Color(215, 215, 255)));
            }

            if (npcState.IsDreamboundFor(player.whoAmI)) {
                entries.Add(new HeroTrackerEntry("Dreambound", FormatTrackerSeconds(npcState.PeskyDustDreamTime),
                    MathHelper.Clamp(npcState.PeskyDustDreamTime / 360f, 0f, 1f), new Color(255, 188, 244)));
            }

            return new HeroTrackerPanel(entries.Count > 0 ? $"Target: {ShortenTrackerText(targetNpc.GivenOrTypeName, 18)}" : null,
                entries);
        }

        private NPC FindTrackedTargetNpc(Player player) {
            NPC hoveredNpc = FindHoveredTrackedTargetNpc(player);
            if (hoveredNpc != null)
                return hoveredNpc;

            NPC bestNpc = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy())
                    continue;

                AlienIdentityGlobalNPC npcState = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
                if (!HasTrackedTargetStatus(npcState, player.whoAmI))
                    continue;

                float mouseDistance = Vector2.DistanceSquared(npc.Center, Main.MouseWorld);
                float playerDistance = Vector2.DistanceSquared(npc.Center, player.Center);
                float score = mouseDistance + playerDistance * 0.2f;
                if (score >= bestScore)
                    continue;

                bestScore = score;
                bestNpc = npc;
            }

            return bestNpc;
        }

        private NPC FindHoveredTrackedTargetNpc(Player player) {
            Point mousePoint = Main.MouseWorld.ToPoint();
            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy())
                    continue;

                Rectangle hoverRect = npc.Hitbox;
                hoverRect.Inflate(18, 18);
                if (!hoverRect.Contains(mousePoint))
                    continue;

                AlienIdentityGlobalNPC npcState = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
                if (HasTrackedTargetStatus(npcState, player.whoAmI))
                    return npc;
            }

            return null;
        }

        private static bool HasTrackedTargetStatus(AlienIdentityGlobalNPC npcState, int owner) {
            return npcState.IsFasttrackComboActiveFor(owner)
                || npcState.IsSkyMarkedFor(owner)
                || npcState.IsBlitzwolferResonantFor(owner)
                || npcState.IsEchoEchoResonantFor(owner)
                || npcState.IsEchoEchoFracturedFor(owner)
                || npcState.IsUltimateEchoEchoFocusedFor(owner)
                || npcState.IsFrankenstrikeConductiveFor(owner)
                || npcState.HasLodestarPolarityFor(owner)
                || npcState.IsWaterHazardSoakedFor(owner)
                || npcState.IsJetrayLockedFor(owner)
                || npcState.HasBigChillFrostbiteFor(owner)
                || npcState.IsBigChillDeepFrozenFor(owner)
                || npcState.IsWhampirePreyFor(owner)
                || npcState.IsSnareOhCursedFor(owner)
                || npcState.IsAlienXJudgedFor(owner)
                || npcState.IsDreamboundFor(owner);
        }

        private static string FormatTrackerSeconds(int ticks) {
            return $"{Math.Max(1, (int)Math.Ceiling(ticks / 60f))}s";
        }

        private static string ShortenTrackerText(string text, int maxLength) {
            if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
                return text;

            return text[..Math.Max(1, maxLength - 3)] + "...";
        }

        private int DrawHeroTrackerPanel(List<HeroTrackerEntry> entries, int x, int y, int width, string title = null) {
            if (entries == null || entries.Count == 0)
                return 0;

            var clientConfig = ModContent.GetInstance<Ben10ClientConfig>();
            bool simplified = clientConfig.UseSimplifiedHeroMoveInterface;
            int rowHeight = simplified ? 28 : 34;
            int headerHeight = simplified ? 20 : 24;
            int panelHeight = headerHeight + 6 + entries.Count * rowHeight;
            Rectangle panelRect = new Rectangle(x, y, width, panelHeight);
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Color borderColor = Color.Lerp(new Color(70, 90, 110), entries[0].AccentColor, 0.55f);
            Color fillColor = new Color(10, 18, 24, 185);
            Color subduedText = new Color(182, 196, 212);

            Main.spriteBatch.Draw(pixel, panelRect, fillColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height), borderColor);

            string panelTitle = string.IsNullOrWhiteSpace(title) ? entries.Count == 1 ? "Tracker" : "Trackers" : title;
            float titleScale = simplified ? 0.68f : 0.78f;
            float panelInnerWidth = panelRect.Width - 20f;
            string fittedPanelTitle = FitHudText(panelTitle, panelInnerWidth, titleScale);
            Utils.DrawBorderString(Main.spriteBatch, fittedPanelTitle,
                new Vector2(panelRect.X + 10, panelRect.Y + 6), new Color(220, 230, 240), titleScale);

            for (int i = 0; i < entries.Count; i++) {
                HeroTrackerEntry entry = entries[i];
                int rowY = panelRect.Y + headerHeight + i * rowHeight;
                float labelScale = simplified ? 0.68f : 0.76f;
                float valueScale = simplified ? 0.72f : 0.82f;
                int barHeight = simplified ? 6 : 8;
                int barY = rowY + (simplified ? 15 : 19);
                Rectangle barBackgroundRect = new Rectangle(panelRect.X + 10, barY, panelRect.Width - 20, barHeight);
                int barFillWidth = Math.Max(0, (int)Math.Round((barBackgroundRect.Width - 2) * MathHelper.Clamp(entry.Progress, 0f, 1f)));

                string fittedValue = FitHudText(entry.ValueText, panelInnerWidth * 0.36f, valueScale);
                float valueWidth = MeasureHudTextWidth(fittedValue, valueScale);
                string fittedLabel = FitHudText(entry.Label,
                    string.IsNullOrWhiteSpace(fittedValue) ? panelInnerWidth : Math.Max(48f, panelInnerWidth - valueWidth - 10f),
                    labelScale);

                Utils.DrawBorderString(Main.spriteBatch, fittedLabel, new Vector2(panelRect.X + 10, rowY), subduedText, labelScale);
                Utils.DrawBorderString(Main.spriteBatch, fittedValue, new Vector2(panelRect.Right - 10, rowY),
                    entry.AccentColor, valueScale, 1f, 0f);

                Main.spriteBatch.Draw(pixel, barBackgroundRect, new Color(26, 34, 42, 220));
                Main.spriteBatch.Draw(pixel, new Rectangle(barBackgroundRect.X, barBackgroundRect.Y, barBackgroundRect.Width, 1), entry.AccentColor * 0.85f);
                Main.spriteBatch.Draw(pixel, new Rectangle(barBackgroundRect.X, barBackgroundRect.Bottom - 1, barBackgroundRect.Width, 1), entry.AccentColor * 0.85f);
                Main.spriteBatch.Draw(pixel, new Rectangle(barBackgroundRect.X, barBackgroundRect.Y, 1, barBackgroundRect.Height), entry.AccentColor * 0.85f);
                Main.spriteBatch.Draw(pixel, new Rectangle(barBackgroundRect.Right - 1, barBackgroundRect.Y, 1, barBackgroundRect.Height), entry.AccentColor * 0.85f);

                if (barFillWidth > 0) {
                    Rectangle barFillRect = new Rectangle(barBackgroundRect.X + 1, barBackgroundRect.Y + 1,
                        barFillWidth, Math.Max(1, barBackgroundRect.Height - 2));
                    Main.spriteBatch.Draw(pixel, barFillRect, entry.AccentColor);
                }
            }

            return panelRect.Height;
        }

        internal void ShowMyUI() {
            EnsureInterfaceInitialized();
            MyInterface?.SetState(AS);
        }

        internal void ShowCodexUI() {
            EnsureInterfaceInitialized();
            CloseVanillaMenusForCodex();
            TCS?.PrepareToOpen();
            MyInterface?.SetState(TCS);
        }

        internal bool IsCodexUIOpen() {
            EnsureInterfaceInitialized();
            return MyInterface?.CurrentState == TCS;
        }

        internal void ShowPaletteUI() {
            EnsureInterfaceInitialized();
            MyInterface?.SetState(TPS);
        }

        internal void HideMyUI() => MyInterface?.SetState(null);

        private static void CloseVanillaMenusForCodex() {
            Player player = Main.LocalPlayer;
            if (player == null)
                return;

            Main.playerInventory = false;
            Main.npcChatText = string.Empty;
            Main.InGuideCraftMenu = false;
            Main.recBigList = false;
            player.chest = -1;
            Main.CloseNPCChatOrSign();
            Main.editSign = false;
            Main.editChest = false;
            Main.blockInput = false;
            Recipe.FindRecipes();
        }
    }

    public class AlienSelectionScreen : UIState {
        private const int CompactDescriptionMaxCharacters = 180;
        private const int CompactAbilityMaxCharacters = 84;
        private const int CompactAbilityMaxCount = 5;
        private const float RosterAbilityEntryWidth = 400f;
        internal const float CodexEntryWidth = 526f;
        private          UIPanel                 mainPanel;
        private readonly List<FittedTransformationIcon> rosterSlots = new();
        private          UIGrid                  unlockedGrid;
        private          CustomNameTextInputPanel unlockedSearchInput;
        private          UIPanel                 infoPanel;
        private          UIText                  nameText;
        private          UIText                  descriptionText;
        private          UIList                  abilityList;
        private          UIText                  abilitiesHeader;
        private          UIText                  unlockedHintText;

        private string currentlySelectedId     = "";
        private string unlockedRosterSignature = "";
        private string unlockedSearchText      = string.Empty;
        private bool   unlockedGridDirty       = true;
        private string lastUnlockedClickId     = string.Empty;
        private ulong  lastUnlockedClickTime;
        private const ulong UnlockedAlienDoubleClickWindow = 24;

        internal static Asset<Texture2D> GetSafeTransformationIcon(Transformation trans) {
            try {
                Asset<Texture2D> icon = trans?.GetTransformationIcon();
                if (icon != null)
                    return icon;
            }
            catch { }

            return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien");
        }

        internal static string GetSafeTransformationName(Transformation trans) {
            var player = Main.LocalPlayer?.GetModPlayer<OmnitrixPlayer>();
            if (player != null && trans != null)
                return player.GetTransformationBaseName(trans);

            return string.IsNullOrWhiteSpace(trans?.TransformationName) ? "Unknown Alien" : trans.TransformationName;
        }

        internal static string GetSafeTransformationDescription(Transformation trans, OmnitrixPlayer player) {
            string description = null;

            try {
                description = trans?.GetDescription(player);
            }
            catch { }

            description = CompactTransformationDescription(description);
            return string.IsNullOrWhiteSpace(description) ? "No description available." : description;
        }

        internal static IReadOnlyList<string>
            GetSafeTransformationAbilities(Transformation trans, OmnitrixPlayer player) {
            try {
                List<string> abilities = trans?.GetAbilities(player);
                IReadOnlyList<string> compactAbilities = CompactTransformationAbilities(abilities);
                if (compactAbilities.Count > 0)
                    return compactAbilities;
            }
            catch { }

            return new[] { "No abilities listed." };
        }

        internal static UIElement CreateWrappedListEntry(string entryText, float width, float scale, bool framed) {
            string cleanText = NormalizeUiText(entryText);
            string displayText = string.IsNullOrWhiteSpace(cleanText) ? "Unknown entry" : cleanText;

            if (!framed) {
                UIElement container = new UIElement();
                container.Width.Set(width, 0f);

                UIText text = new UIText(displayText, scale);
                text.Left.Set(0f, 0f);
                text.Top.Set(0f, 0f);
                text.Width.Set(width, 0f);
                text.IsWrapped = true;
                container.Append(text);
                text.Recalculate();

                float textHeight = text.MinHeight.Pixels > 0f ? text.MinHeight.Pixels : 20f;
                container.Height.Set(Math.Max(20f, textHeight + 2f), 0f);
                return container;
            }

            UIPanel row = new UIPanel();
            row.Width.Set(width, 0f);
            row.BackgroundColor = new Color(28, 38, 54, 220);
            row.BorderColor = new Color(82, 104, 132, 220);

            UIText wrappedText = new UIText(displayText, scale);
            wrappedText.Left.Set(10f, 0f);
            wrappedText.Top.Set(9f, 0f);
            wrappedText.Width.Set(width - 20f, 0f);
            wrappedText.IsWrapped = true;
            row.Append(wrappedText);
            wrappedText.Recalculate();

            float wrappedHeight = wrappedText.MinHeight.Pixels > 0f ? wrappedText.MinHeight.Pixels : 20f;
            row.Height.Set(Math.Max(44f, wrappedHeight + 18f), 0f);
            return row;
        }

        internal static float MeasureEntryListHeight(IReadOnlyList<UIElement> entries, float listPadding,
            float minimumHeight = 56f) {
            if (entries == null || entries.Count == 0)
                return minimumHeight;

            float totalHeight = 0f;
            for (int i = 0; i < entries.Count; i++) {
                UIElement entry = entries[i];
                totalHeight += entry?.Height.Pixels ?? 0f;
            }

            totalHeight += Math.Max(0, entries.Count - 1) * listPadding;
            return Math.Max(minimumHeight, totalHeight);
        }

        private static string CompactTransformationDescription(string description) {
            string normalized = NormalizeUiText(description);
            if (string.IsNullOrWhiteSpace(normalized))
                return string.Empty;

            string firstSentence = ExtractPrimarySentence(normalized);
            if (string.IsNullOrWhiteSpace(firstSentence))
                firstSentence = normalized;

            return CompactUiLine(firstSentence, CompactDescriptionMaxCharacters);
        }

        private static IReadOnlyList<string> CompactTransformationAbilities(IReadOnlyList<string> abilities) {
            List<string> compactAbilities = new();
            if (abilities == null)
                return compactAbilities;

            for (int i = 0; i < abilities.Count; i++) {
                string ability = NormalizeUiText(abilities[i]);
                if (string.IsNullOrWhiteSpace(ability) || ShouldHideUiAbilityLine(ability))
                    continue;

                if (ability.StartsWith("• ", StringComparison.Ordinal))
                    ability = ability.Substring(2).Trim();

                ability = CompactUiLine(ability, CompactAbilityMaxCharacters);
                if (string.IsNullOrWhiteSpace(ability) || ContainsText(compactAbilities, ability))
                    continue;

                compactAbilities.Add(ability);
                if (compactAbilities.Count >= CompactAbilityMaxCount)
                    break;
            }

            return compactAbilities;
        }

        private static string ExtractPrimarySentence(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            for (int i = 0; i < text.Length; i++) {
                char character = text[i];
                if (character is not ('.' or '!' or '?'))
                    continue;

                string sentence = text.Substring(0, i + 1).Trim();
                if (sentence.Length >= 48 || i >= text.Length - 1)
                    return sentence;
            }

            int currentStateIndex = text.IndexOf(" Current ", StringComparison.OrdinalIgnoreCase);
            if (currentStateIndex > 0)
                return text.Substring(0, currentStateIndex).Trim();

            return text;
        }

        private static string CompactUiLine(string text, int maxCharacters) {
            string normalized = NormalizeUiText(text);
            if (string.IsNullOrWhiteSpace(normalized) || normalized.Length <= maxCharacters)
                return normalized;

            int cutIndex = normalized.LastIndexOf(' ', Math.Min(maxCharacters, normalized.Length - 1));
            if (cutIndex < maxCharacters / 2)
                cutIndex = Math.Min(maxCharacters, normalized.Length);

            return normalized.Substring(0, cutIndex).TrimEnd(' ', ',', ';', ':', '-') + "…";
        }

        private static string NormalizeUiText(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            StringBuilder builder = new(text.Length);
            bool lastWasWhitespace = false;
            for (int i = 0; i < text.Length; i++) {
                char character = text[i];
                if (char.IsWhiteSpace(character)) {
                    if (lastWasWhitespace)
                        continue;

                    builder.Append(' ');
                    lastWasWhitespace = true;
                    continue;
                }

                builder.Append(character);
                lastWasWhitespace = false;
            }

            return builder.ToString().Trim();
        }

        private static bool ShouldHideUiAbilityLine(string text) {
            return text.StartsWith("Current sync:", StringComparison.OrdinalIgnoreCase) ||
                   text.StartsWith("Source weapon:", StringComparison.OrdinalIgnoreCase) ||
                   text.StartsWith("Remembered weapon:", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsText(IReadOnlyList<string> values, string candidate) {
            if (values == null || string.IsNullOrWhiteSpace(candidate))
                return false;

            for (int i = 0; i < values.Count; i++) {
                if (string.Equals(values[i], candidate, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public override void OnInitialize() {
            mainPanel = new UIPanel();
            mainPanel.Width.Set(1220f, 0f);
            mainPanel.Height.Set(680f, 0f);
            mainPanel.HAlign = mainPanel.VAlign = 0.5f;
            Append(mainPanel);

            var title = new UIText("Omnitrix - Alien Roster", 1.45f);
            title.HAlign = 0.5f;
            title.Top.Set(18f, 0f);
            mainPanel.Append(title);

            var rosterHeader = new UIText("Active Roster", 1.25f);
            rosterHeader.Left.Set(65f, 0f);
            rosterHeader.Top.Set(68f, 0f);
            mainPanel.Append(rosterHeader);

            int slotSize     = 92;
            int rosterStartX = 65;
            int rosterY      = 105;

            for (int i = 0; i < 5; i++) {
                var slot = new FittedTransformationIcon(
                    ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien"), () => false);
                slot.Width.Set(slotSize, 0f);
                slot.Height.Set(slotSize, 0f);
                slot.Left.Set(rosterStartX + i * (slotSize + 26f), 0f);
                slot.Top.Set(rosterY, 0f);
                int index = i;
                slot.OnLeftClick  += (_, _) => AssignToSlot(index);
                slot.OnRightClick += (_, _) => ClearSlot(index);
                slot.OnMouseOver += (_, _) => {
                    var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
                    string transformationId = index < player.transformationSlots.Length
                        ? player.transformationSlots[index]
                        : string.Empty;
                    UpdateInfoPanelFromTransformationId(transformationId);
                };

                mainPanel.Append(slot);
                rosterSlots.Add(slot);
            }

            var divider = new UIPanel();
            divider.Width.Set(610f, 0f);
            divider.Height.Set(4f, 0f);
            divider.Left.Set(65f, 0f);
            divider.Top.Set(rosterY + slotSize + 22f, 0f);
            divider.BackgroundColor = new Color(80, 120, 255, 180);
            mainPanel.Append(divider);

            var unlockedHeader = new UIText("Unlocked Aliens", 1.25f);
            unlockedHeader.Left.Set(65f, 0f);
            unlockedHeader.Top.Set(rosterY + slotSize + 52f, 0f);
            mainPanel.Append(unlockedHeader);

            UIPanel searchPanel = new UIPanel();
            searchPanel.Width.Set(304f, 0f);
            searchPanel.Height.Set(34f, 0f);
            searchPanel.Left.Set(325f, 0f);
            searchPanel.Top.Set(rosterY + slotSize + 46f, 0f);
            searchPanel.PaddingTop = 0f;
            searchPanel.PaddingBottom = 0f;
            searchPanel.PaddingLeft = 0f;
            searchPanel.PaddingRight = 0f;
            mainPanel.Append(searchPanel);

            unlockedSearchInput = new CustomNameTextInputPanel("Search unlocked aliens") {
                MaxLength = 40
            };
            unlockedSearchInput.Width.Set(-12f, 1f);
            unlockedSearchInput.Height.Set(-10f, 1f);
            unlockedSearchInput.Left.Set(6f, 0f);
            unlockedSearchInput.Top.Set(5f, 0f);
            unlockedSearchInput.TextChanged += text => HandleUnlockedSearchChanged(text);
            unlockedSearchInput.Submitted += text => HandleUnlockedSearchChanged(text);
            searchPanel.Append(unlockedSearchInput);

            unlockedHintText = new UIText(string.Empty, 0.78f);
            unlockedHintText.Left.Set(65f, 0f);
            unlockedHintText.Top.Set(rosterY + slotSize + 76f, 0f);
            mainPanel.Append(unlockedHintText);

            unlockedGrid = new UIGrid();
            unlockedGrid.Width.Set(564f, 0f);
            unlockedGrid.Height.Set(315f, 0f);
            unlockedGrid.Left.Set(65f, 0f);
            unlockedGrid.Top.Set(rosterY + slotSize + 102f, 0f);
            unlockedGrid.ListPadding = 26f;
            unlockedGrid.ManualSortMethod = _ => { };
            mainPanel.Append(unlockedGrid);

            var gridScrollbar = new UIScrollbar();
            gridScrollbar.Height.Set(315f, 0f);
            gridScrollbar.Left.Set(641f, 0f);
            gridScrollbar.Top.Set(rosterY + slotSize + 102f, 0f);
            mainPanel.Append(gridScrollbar);
            unlockedGrid.SetScrollbar(gridScrollbar);

            infoPanel = new UIPanel();
            infoPanel.Width.Set(460f, 0f);
            infoPanel.Height.Set(545f, 0f);
            infoPanel.Left.Set(740f, 0f);
            infoPanel.Top.Set(92f, 0f);
            mainPanel.Append(infoPanel);

            nameText        = new UIText("Select an alien", 1.3f);
            nameText.HAlign = 0f;
            nameText.Left.Set(32f, 0f);
            nameText.Top.Set(28f, 0f);
            infoPanel.Append(nameText);

            descriptionText        = new UIText("Click any unlocked alien", 0.95f);
            descriptionText.HAlign = 0f;
            descriptionText.Left.Set(32f, 0f);
            descriptionText.Top.Set(72f, 0f);
            descriptionText.Width.Set(400f, 0f);
            descriptionText.IsWrapped = true;
            infoPanel.Append(descriptionText);

            abilitiesHeader = new UIText("Kit Highlights:", 1.15f);
            abilitiesHeader.Left.Set(32f, 0f);
            abilitiesHeader.Top.Set(180f, 0f);
            infoPanel.Append(abilitiesHeader);

            abilityList = new UIList();
            abilityList.Width.Set(400f, 0f);
            abilityList.Height.Set(300f, 0f);
            abilityList.Left.Set(32f, 0f);
            abilityList.Top.Set(210f, 0f);
            abilityList.ListPadding = 8f;
            infoPanel.Append(abilityList);

            var closeBtn = new UITextPanel<string>("Close Roster");
            closeBtn.HAlign = 0.5f;
            closeBtn.Top.Set(-34f, 1f);
            closeBtn.OnLeftClick += (_, _) => {
                ModContent.GetInstance<UISystem>().HideMyUI();
                Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().showingUI = false;
            };
            mainPanel.Append(closeBtn);
        }

        public override void OnActivate() {
            base.OnActivate();

            unlockedSearchText = string.Empty;
            unlockedGridDirty = true;
            unlockedRosterSignature = string.Empty;
            lastUnlockedClickId = string.Empty;
            lastUnlockedClickTime = 0;
            unlockedSearchInput?.SetText(string.Empty, invoke: false);
            unlockedSearchInput?.SetFocused(false);
            RefreshUnlockedHint(0, 0);
        }

        private void AssignToSlot(int slotIndex) {
            if (string.IsNullOrEmpty(currentlySelectedId)) return;

            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            if (player.unlockedTransformations.Contains(currentlySelectedId)) {
                player.transformationSlots[slotIndex] = currentlySelectedId;
                player.SyncTransformationStateToServer();
                UpdateInfoPanelFromTransformationId(player.transformationSlots[slotIndex]);
                currentlySelectedId = "";
            }
        }

        private void ClearSlot(int slotIndex) {
            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            player.transformationSlots[slotIndex] = "";
            player.SyncTransformationStateToServer();
            UpdateInfoPanelFromTransformationId(player.transformationSlots[slotIndex]);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();

            for (int i = 0; i < rosterSlots.Count; i++) {
                string id    = i < player.transformationSlots.Length ? player.transformationSlots[i] : string.Empty;
                var    trans = TransformationLoader.Get(id);
                rosterSlots[i].SetTexture(GetSafeTransformationIcon(trans));
            }

            RefreshUnlockedGrid(player);

            if (mainPanel.ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }

        private void RefreshUnlockedGrid(OmnitrixPlayer player) {
            string signature = BuildUnlockedRosterSignature(player);
            if (!unlockedGridDirty && signature == unlockedRosterSignature)
                return;

            unlockedGridDirty       = false;
            unlockedRosterSignature = signature;
            unlockedGrid.Clear();

            int totalUnlockedCount = 0;
            int visibleUnlockedCount = 0;

            foreach (string id in player.GetUnlockedTransformationsForDisplay()) {
                if (string.IsNullOrEmpty(id))
                    continue;

                var trans = TransformationLoader.Get(id);
                if (trans == null)
                    continue;

                totalUnlockedCount++;
                if (!UnlockedTransformationMatchesSearch(trans, player))
                    continue;

                visibleUnlockedCount++;

                var icon = GetSafeTransformationIcon(trans);
                var slot = new UIElement();
                slot.Width.Set(92f, 0f);
                slot.Height.Set(92f, 0f);

                string transformationId = id;
                var btn = new FittedTransformationIcon(icon,
                    () => player.IsFavoriteTransformation(transformationId),
                    () => player.IsNewlyUnlockedTransformation(transformationId));
                btn.Width.Set(92f, 0f);
                btn.Height.Set(92f, 0f);
                btn.IgnoresMouseInteraction = true;
                slot.OnLeftClick += (_, _) => HandleUnlockedTransformationLeftClick(transformationId);

                slot.OnRightClick += (_, _) => {
                    OmnitrixPlayer localPlayer = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
                    localPlayer.ToggleFavoriteTransformation(transformationId);
                    unlockedRosterSignature = string.Empty;
                    unlockedGridDirty = true;
                    UpdateInfoPanel(TransformationLoader.Get(transformationId));
                };

                slot.OnMouseOver += (_, _) => UpdateInfoPanel(TransformationLoader.Get(transformationId));

                slot.Append(btn);

                unlockedGrid.Add(slot);
            }

            unlockedGrid.Recalculate();
            unlockedGrid.RecalculateChildren();
            RefreshUnlockedHint(visibleUnlockedCount, totalUnlockedCount);
        }

        private static string BuildUnlockedRosterSignature(OmnitrixPlayer player) {
            if (player == null)
                return string.Empty;

            List<string> displayIds = new(player.GetUnlockedTransformationsForDisplay());
            if (displayIds.Count == 0)
                return string.Empty;

            System.Text.StringBuilder builder = new();
            builder.Append("search=").Append(MainMenuSafeSearchText()).Append('|');
            for (int i = 0; i < displayIds.Count; i++) {
                string transformationId = displayIds[i];
                Transformation transformation = TransformationLoader.Get(transformationId);
                builder.Append(transformationId)
                    .Append('=')
                    .Append(player.IsFavoriteTransformation(transformationId) ? '1' : '0')
                    .Append('=')
                    .Append(player.IsNewlyUnlockedTransformation(transformationId) ? '1' : '0')
                    .Append('=')
                    .Append(transformation?.GetDisplayName(player) ?? string.Empty)
                    .Append('|');
            }

            return builder.ToString();
        }

        private static string MainMenuSafeSearchText() {
            AlienSelectionScreen state = ModContent.GetInstance<UISystem>()?.AS;
            return state?.unlockedSearchText ?? string.Empty;
        }

        private void UpdateInfoPanel(Transformation trans) {
            if (trans == null) {
                nameText.SetText("Select an alien");
                descriptionText.SetText("Click any unlocked alien");
                abilitiesHeader.Top.Set(180f, 0f);
                abilityList.Top.Set(210f, 0f);
                abilityList.Clear();
                return;
            }

            OmnitrixPlayer player      = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            string         displayName = GetSafeTransformationName(trans);
            nameText.SetText(player.IsFavoriteTransformation(trans) ? $"★ {displayName}" : displayName);
            descriptionText.SetText(GetSafeTransformationDescription(trans, player));

            // Give wrapped descriptions enough room before the abilities section starts.
            var   descriptionHeight = descriptionText.MinHeight.Pixels > 0f ? descriptionText.MinHeight.Pixels : 72f;
            float abilitiesTop      = Math.Max(180f, descriptionText.Top.Pixels + descriptionHeight + 26f);
            abilitiesHeader.Top.Set(abilitiesTop, 0f);
            abilityList.Top.Set(abilitiesTop + 30f, 0f);

            abilityList.Clear();
            var abilities = GetSafeTransformationAbilities(trans, player);
            foreach (var ability in abilities)
                abilityList.Add(CreateWrappedListEntry("• " + (ability ?? "Unknown ability"),
                    RosterAbilityEntryWidth, 0.95f, framed: false));
        }

        private void UpdateInfoPanelFromTransformationId(string transformationId) {
            UpdateInfoPanel(TransformationLoader.Get(transformationId));
        }

        private void SelectUnlockedTransformation(string transformationId) {
            currentlySelectedId = transformationId ?? string.Empty;

            OmnitrixPlayer player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            if (player.MarkTransformationAsSeen(currentlySelectedId)) {
                unlockedRosterSignature = string.Empty;
                unlockedGridDirty = true;
            }

            UpdateInfoPanel(TransformationLoader.Get(currentlySelectedId));
        }

        private void HandleUnlockedTransformationLeftClick(string transformationId) {
            ulong currentTime = Main.GameUpdateCount;
            bool isDoubleClick = !string.IsNullOrEmpty(transformationId) &&
                                 string.Equals(lastUnlockedClickId, transformationId, StringComparison.OrdinalIgnoreCase) &&
                                 currentTime - lastUnlockedClickTime <= UnlockedAlienDoubleClickWindow;

            SelectUnlockedTransformation(transformationId);

            if (isDoubleClick)
                AssignAndTransformUnlockedTransformation(transformationId);

            lastUnlockedClickId = transformationId ?? string.Empty;
            lastUnlockedClickTime = currentTime;
        }

        private void AssignAndTransformUnlockedTransformation(string transformationId) {
            if (string.IsNullOrWhiteSpace(transformationId))
                return;

            OmnitrixPlayer player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            if (!player.IsTransformationUnlocked(transformationId))
                return;

            Content.Items.Accessories.Omnitrix activeOmnitrix = player.GetActiveOmnitrix();
            if (activeOmnitrix == null) {
                player.ShowTransformFailureFeedback("Equip the Omnitrix to transform.");
                return;
            }

            int selectedSlotIndex = player.GetSelectedTransformationSlotIndex();
            if (selectedSlotIndex < 0 || selectedSlotIndex >= player.transformationSlots.Length) {
                player.ShowTransformFailureFeedback("Select an Omnitrix slot first.");
                return;
            }

            player.transformationSlots[selectedSlotIndex] = transformationId;
            activeOmnitrix.transformationSlots = player.transformationSlots;
            player.SyncTransformationStateToServer();

            currentlySelectedId = transformationId;
            UpdateInfoPanelFromTransformationId(player.transformationSlots[selectedSlotIndex]);
            if (activeOmnitrix.TryTransformToSlot(Main.LocalPlayer, player, selectedSlotIndex)) {
                ModContent.GetInstance<UISystem>().HideMyUI();
                player.showingUI = false;
            }
        }

        private void HandleUnlockedSearchChanged(string text) {
            string normalizedText = text?.Trim() ?? string.Empty;
            if (string.Equals(unlockedSearchText, normalizedText, StringComparison.Ordinal))
                return;

            unlockedSearchText = normalizedText;
            unlockedRosterSignature = string.Empty;
            unlockedGridDirty = true;
        }

        private bool UnlockedTransformationMatchesSearch(Transformation transformation, OmnitrixPlayer player) {
            if (transformation == null || string.IsNullOrWhiteSpace(unlockedSearchText))
                return transformation != null;

            string query = unlockedSearchText.Trim();
            if (query.Length == 0)
                return true;

            string displayName = transformation.GetDisplayName(player) ?? string.Empty;
            string baseName = transformation.TransformationName ?? string.Empty;
            string fullId = transformation.FullID ?? string.Empty;

            return displayName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   baseName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   fullId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void RefreshUnlockedHint(int visibleUnlockedCount, int totalUnlockedCount) {
            if (unlockedHintText == null)
                return;

            if (string.IsNullOrWhiteSpace(unlockedSearchText)) {
                unlockedHintText.SetText(string.Empty);
                return;
            }

            if (visibleUnlockedCount <= 0) {
                unlockedHintText.SetText($"No unlocked aliens match \"{unlockedSearchText}\"");
                return;
            }

            unlockedHintText.SetText($"Showing {visibleUnlockedCount} of {totalUnlockedCount} unlocked aliens");
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }

    public class TransformationCodexScreen : UIState {
        private UIPanel mainPanel;
        private UIList codexList;
        private UIPanel infoPanel;
        private UIPanel headerPanel;
        private UIPanel overviewPanel;
        private UIPanel unlockPanel;
        private UIPanel abilitiesPanel;
        private UIPanel combatPanel;
        private FittedTransformationIcon previewIcon;
        private UIText nameText;
        private UIText statusText;
        private UIText overviewHeader;
        private UIText descriptionText;
        private UIText unlockHeader;
        private UIText unlockConditionText;
        private UIText abilitiesHeader;
        private UIList abilityList;
        private UIText combatHeader;
        private UIList combatSlotList;
        private UIList detailSectionList;
        private string currentlySelectedId = string.Empty;
        private string codexListSignature = string.Empty;
        private bool codexListDirty = true;

        public void PrepareToOpen() {
            codexListDirty = true;
            OmnitrixPlayer player = Main.LocalPlayer?.GetModPlayer<OmnitrixPlayer>();
            if (player == null)
                return;

            string preferredSelection = FindFirstNewTransformation(player);
            if (!string.IsNullOrEmpty(preferredSelection))
                currentlySelectedId = preferredSelection;
        }

        public override void OnInitialize() {
            mainPanel = new UIPanel();
            mainPanel.Width.Set(1180f, 0f);
            mainPanel.Height.Set(680f, 0f);
            mainPanel.HAlign = mainPanel.VAlign = 0.5f;
            mainPanel.BackgroundColor = new Color(12, 18, 28, 238);
            mainPanel.BorderColor = new Color(76, 96, 126, 235);
            Append(mainPanel);

            var title = new UIText("Transformation Codex", 1.45f);
            title.HAlign = 0.5f;
            title.Top.Set(18f, 0f);
            mainPanel.Append(title);

            var subtitle = new UIText("Alien forms, unlock conditions, and kit details.", 0.85f);
            subtitle.HAlign = 0.5f;
            subtitle.Top.Set(54f, 0f);
            mainPanel.Append(subtitle);

            var listHeader = new UIText("Alien Forms", 1.18f);
            listHeader.Left.Set(40f, 0f);
            listHeader.Top.Set(92f, 0f);
            mainPanel.Append(listHeader);

            codexList = new UIList();
            codexList.Width.Set(382f, 0f);
            codexList.Height.Set(492f, 0f);
            codexList.Left.Set(40f, 0f);
            codexList.Top.Set(124f, 0f);
            codexList.ListPadding = 12f;
            codexList.ManualSortMethod = _ => { };
            mainPanel.Append(codexList);

            var listScrollbar = new UIScrollbar();
            listScrollbar.Height.Set(492f, 0f);
            listScrollbar.Left.Set(430f, 0f);
            listScrollbar.Top.Set(124f, 0f);
            mainPanel.Append(listScrollbar);
            codexList.SetScrollbar(listScrollbar);

            infoPanel = new UIPanel();
            infoPanel.Width.Set(650f, 0f);
            infoPanel.Height.Set(560f, 0f);
            infoPanel.Left.Set(490f, 0f);
            infoPanel.Top.Set(92f, 0f);
            infoPanel.BackgroundColor = new Color(16, 22, 32, 232);
            infoPanel.BorderColor = new Color(78, 98, 128, 225);
            mainPanel.Append(infoPanel);

            headerPanel = new UIPanel();
            headerPanel.Width.Set(590f, 0f);
            headerPanel.Height.Set(152f, 0f);
            headerPanel.Left.Set(30f, 0f);
            headerPanel.Top.Set(24f, 0f);
            headerPanel.BackgroundColor = new Color(24, 32, 46, 220);
            headerPanel.BorderColor = new Color(74, 92, 120);
            infoPanel.Append(headerPanel);

            previewIcon = new FittedTransformationIcon(
                ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien"),
                () => false,
                () => false,
                showHoverOutline: false);
            previewIcon.Width.Set(120f, 0f);
            previewIcon.Height.Set(120f, 0f);
            previewIcon.Left.Set(18f, 0f);
            previewIcon.Top.Set(16f, 0f);
            previewIcon.IgnoresMouseInteraction = true;
            headerPanel.Append(previewIcon);

            nameText = new UIText("Select a form", 1.3f);
            nameText.Left.Set(160f, 0f);
            nameText.Top.Set(22f, 0f);
            headerPanel.Append(nameText);

            statusText = new UIText("Choose a form to view its unlock or access details and kit breakdown.", 0.78f);
            statusText.Left.Set(160f, 0f);
            statusText.Top.Set(64f, 0f);
            statusText.Width.Set(390f, 0f);
            statusText.IsWrapped = true;
            headerPanel.Append(statusText);

            detailSectionList = new UIList();
            detailSectionList.Width.Set(562f, 0f);
            detailSectionList.Height.Set(342f, 0f);
            detailSectionList.Left.Set(30f, 0f);
            detailSectionList.Top.Set(194f, 0f);
            detailSectionList.ListPadding = 14f;
            detailSectionList.ManualSortMethod = _ => { };
            infoPanel.Append(detailSectionList);

            var detailScrollbar = new UIScrollbar();
            detailScrollbar.Height.Set(342f, 0f);
            detailScrollbar.Left.Set(600f, 0f);
            detailScrollbar.Top.Set(194f, 0f);
            infoPanel.Append(detailScrollbar);
            detailSectionList.SetScrollbar(detailScrollbar);

            overviewPanel = CreateCodexSectionPanel();
            overviewPanel.Height.Set(116f, 0f);
            detailSectionList.Add(overviewPanel);

            overviewHeader = new UIText("Overview", 1.08f);
            overviewHeader.Left.Set(18f, 0f);
            overviewHeader.Top.Set(12f, 0f);
            overviewPanel.Append(overviewHeader);

            descriptionText = new UIText("View lore-facing kit info and ability details here.", 0.95f);
            descriptionText.Left.Set(18f, 0f);
            descriptionText.Top.Set(44f, 0f);
            descriptionText.Width.Set(-36f, 1f);
            descriptionText.IsWrapped = true;
            overviewPanel.Append(descriptionText);

            unlockPanel = CreateCodexSectionPanel();
            unlockPanel.Height.Set(94f, 0f);
            detailSectionList.Add(unlockPanel);

            unlockHeader = new UIText("Unlock / Access", 1.08f);
            unlockHeader.Left.Set(18f, 0f);
            unlockHeader.Top.Set(12f, 0f);
            unlockPanel.Append(unlockHeader);

            unlockConditionText = new UIText("Select a form to inspect how it is unlocked or accessed.", 0.9f);
            unlockConditionText.Left.Set(18f, 0f);
            unlockConditionText.Top.Set(44f, 0f);
            unlockConditionText.Width.Set(-36f, 1f);
            unlockConditionText.IsWrapped = true;
            unlockPanel.Append(unlockConditionText);

            abilitiesPanel = CreateCodexSectionPanel();
            abilitiesPanel.Height.Set(96f, 0f);
            detailSectionList.Add(abilitiesPanel);

            abilitiesHeader = new UIText("Kit Highlights", 1.1f);
            abilitiesHeader.Left.Set(18f, 0f);
            abilitiesHeader.Top.Set(12f, 0f);
            abilitiesPanel.Append(abilitiesHeader);

            abilityList = new UIList();
            abilityList.Width.Set(-36f, 1f);
            abilityList.Height.Set(40f, 0f);
            abilityList.Left.Set(18f, 0f);
            abilityList.Top.Set(44f, 0f);
            abilityList.ListPadding = 8f;
            abilitiesPanel.Append(abilityList);

            combatPanel = CreateCodexSectionPanel();
            combatPanel.Height.Set(96f, 0f);
            detailSectionList.Add(combatPanel);

            combatHeader = new UIText("Combat Slots", 1.1f);
            combatHeader.Left.Set(18f, 0f);
            combatHeader.Top.Set(12f, 0f);
            combatPanel.Append(combatHeader);

            combatSlotList = new UIList();
            combatSlotList.Width.Set(-36f, 1f);
            combatSlotList.Height.Set(40f, 0f);
            combatSlotList.Left.Set(18f, 0f);
            combatSlotList.Top.Set(44f, 0f);
            combatSlotList.ListPadding = 8f;
            combatPanel.Append(combatSlotList);

            var closeBtn = new UITextPanel<string>("Close Codex");
            closeBtn.Width.Set(148f, 0f);
            closeBtn.Height.Set(40f, 0f);
            closeBtn.Left.Set(-178f, 1f);
            closeBtn.Top.Set(18f, 0f);
            closeBtn.OnLeftClick += (_, _) => {
                ModContent.GetInstance<UISystem>().HideMyUI();
                Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().showingUI = false;
            };
            mainPanel.Append(closeBtn);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            ForceCloseVanillaMenus();
            OmnitrixPlayer player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            EnsureSelection(player);
            RefreshCodexList(player);

            if (mainPanel.ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }

        private static void ForceCloseVanillaMenus() {
            Player player = Main.LocalPlayer;
            if (player == null)
                return;

            Main.playerInventory = false;
            Main.npcChatText = string.Empty;
            Main.InGuideCraftMenu = false;
            Main.recBigList = false;
            Main.editSign = false;
            Main.editChest = false;
            Main.blockInput = false;
            player.chest = -1;
        }

        private void EnsureSelection(OmnitrixPlayer player) {
            IReadOnlyList<string> displayList = player.GetTransformationsForCodexDisplay();
            if (displayList.Count == 0) {
                currentlySelectedId = string.Empty;
                UpdateInfoPanel(null, player);
                return;
            }

            if (ContainsTransformation(displayList, currentlySelectedId))
                return;

            string preferredSelection = FindFirstNewTransformation(player);
            if (string.IsNullOrEmpty(preferredSelection))
                preferredSelection = displayList[0];

            SelectTransformation(player, preferredSelection);
        }

        private void RefreshCodexList(OmnitrixPlayer player) {
            string signature = BuildCodexSignature(player);
            if (!codexListDirty && signature == codexListSignature)
                return;

            codexListDirty = false;
            codexListSignature = signature;
            codexList.Clear();

            foreach (string transformationId in player.GetTransformationsForCodexDisplay()) {
                if (string.IsNullOrEmpty(transformationId))
                    continue;

                Transformation transformation = TransformationLoader.Get(transformationId);
                if (transformation == null)
                    continue;

                bool isUnlocked = player.IsTransformationUnlocked(transformation);
                bool isSelected = string.Equals(currentlySelectedId, transformationId, StringComparison.OrdinalIgnoreCase);
                UIPanel row = new UIPanel();
                row.Width.Set(0f, 1f);
                row.Height.Set(74f, 0f);
                row.BackgroundColor = isSelected
                    ? (isUnlocked ? new Color(44, 60, 86, 225) : new Color(58, 50, 68, 225))
                    : (isUnlocked ? new Color(24, 32, 42, 210) : new Color(30, 28, 36, 208));
                row.BorderColor = isSelected
                    ? (isUnlocked ? new Color(120, 175, 255) : new Color(180, 156, 235))
                    : (isUnlocked ? new Color(70, 88, 108) : new Color(90, 82, 108));

                string capturedId = transformationId;
                row.OnLeftClick += (_, _) => SelectTransformation(player, capturedId);
                var icon = new FittedTransformationIcon(
                    AlienSelectionScreen.GetSafeTransformationIcon(transformation),
                    () => player.IsFavoriteTransformation(capturedId),
                    () => player.IsNewlyUnlockedTransformation(capturedId),
                    showHoverOutline: false);
                icon.Width.Set(58f, 0f);
                icon.Height.Set(58f, 0f);
                icon.Left.Set(8f, 0f);
                icon.Top.Set(8f, 0f);
                icon.IgnoresMouseInteraction = true;
                row.Append(icon);

                string displayName = transformation.GetDisplayName(player);
                if (player.IsFavoriteTransformation(capturedId))
                    displayName = $"★ {displayName}";

                var rowName = new UIText(displayName, 0.92f);
                rowName.Left.Set(78f, 0f);
                rowName.Top.Set(12f, 0f);
                row.Append(rowName);

                string rowSubtitle = player.GetTransformationCodexSubtitle(transformation);
                var subtitle = new UIText(rowSubtitle, 0.68f);
                subtitle.Left.Set(78f, 0f);
                subtitle.Top.Set(42f, 0f);
                row.Append(subtitle);

                codexList.Add(row);
            }

            codexList.Recalculate();
            codexList.RecalculateChildren();
        }

        private string BuildCodexSignature(OmnitrixPlayer player) {
            if (player == null)
                return string.Empty;

            IReadOnlyList<string> displayIds = player.GetTransformationsForCodexDisplay();
            if (displayIds.Count == 0)
                return $"selected={currentlySelectedId}";

            System.Text.StringBuilder builder = new();
            builder.Append("selected=").Append(currentlySelectedId).Append('|');
            for (int i = 0; i < displayIds.Count; i++) {
                string transformationId = displayIds[i];
                Transformation transformation = TransformationLoader.Get(transformationId);
                builder.Append(transformationId)
                    .Append('=')
                    .Append(player.IsTransformationUnlocked(transformationId) ? '1' : '0')
                    .Append('=')
                    .Append(player.IsFavoriteTransformation(transformationId) ? '1' : '0')
                    .Append('=')
                    .Append(player.IsNewlyUnlockedTransformation(transformationId) ? '1' : '0')
                    .Append('=')
                    .Append(transformation?.GetDisplayName(player) ?? string.Empty)
                    .Append('|');
            }

            return builder.ToString();
        }

        private void SelectTransformation(OmnitrixPlayer player, string transformationId) {
            currentlySelectedId = transformationId ?? string.Empty;

            if (player.MarkTransformationAsSeen(currentlySelectedId)) {
                codexListSignature = string.Empty;
                codexListDirty = true;
            }

            UpdateInfoPanel(TransformationLoader.Get(currentlySelectedId), player);
        }

        private void UpdateInfoPanel(Transformation transformation, OmnitrixPlayer player) {
            if (transformation == null) {
                previewIcon.SetTexture(ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien"));
                nameText.SetText("Select a form");
                statusText.SetText("Choose a form to view its unlock or access details and kit breakdown.");
                descriptionText.SetText("View lore-facing kit info and ability details here.");
                overviewHeader.SetText("Overview");
                unlockHeader.SetText("Unlock / Access");
                unlockConditionText.SetText("Select a form to inspect how it is unlocked or accessed.");
                overviewPanel.Height.Set(116f, 0f);
                unlockPanel.Height.Set(94f, 0f);
                abilitiesPanel.Height.Set(96f, 0f);
                abilityList.Height.Set(40f, 0f);
                abilityList.Clear();
                combatHeader.SetText("Combat Slots");
                combatPanel.Height.Set(96f, 0f);
                combatSlotList.Height.Set(40f, 0f);
                combatSlotList.Clear();
                detailSectionList.Recalculate();
                detailSectionList.RecalculateChildren();
                return;
            }

            previewIcon.SetTexture(AlienSelectionScreen.GetSafeTransformationIcon(transformation));
            nameText.SetText(transformation.GetDisplayName(player));

            List<string> statusParts = new() {
                player.GetTransformationAvailabilityStateText(transformation),
                player.GetTransformationUnlockCategoryText(transformation)
            };
            if (player.IsTransformationUnlocked(transformation) && player.IsFavoriteTransformation(transformation))
                statusParts.Add("Favorite");
            if (player.IsTransformationUnlocked(transformation) && player.IsNewlyUnlockedTransformation(transformation))
                statusParts.Add("Newly unlocked");
            statusText.SetText(string.Join("  |  ", statusParts));

            overviewHeader.SetText("Overview");
            descriptionText.SetText(AlienSelectionScreen.GetSafeTransformationDescription(transformation, player));
            descriptionText.Recalculate();

            unlockHeader.SetText(player.GetTransformationAccessHeaderText(transformation));
            string unlockConditionTextValue = player.GetTransformationUnlockConditionText(transformation);
            string unlockProgressText = player.GetTransformationUnlockProgressText(transformation);
            unlockConditionText.SetText(string.IsNullOrWhiteSpace(unlockProgressText)
                ? unlockConditionTextValue
                : $"{unlockConditionTextValue}\n\nStatus: {unlockProgressText}");
            unlockConditionText.Recalculate();

            float descriptionHeight = descriptionText.MinHeight.Pixels > 0f ? descriptionText.MinHeight.Pixels : 56f;
            float overviewHeight = Math.Max(112f, descriptionText.Top.Pixels + descriptionHeight + 18f);
            overviewPanel.Height.Set(overviewHeight, 0f);

            float unlockHeight = unlockConditionText.MinHeight.Pixels > 0f ? unlockConditionText.MinHeight.Pixels : 36f;
            float unlockPanelHeight = Math.Max(88f, unlockConditionText.Top.Pixels + unlockHeight + 18f);
            unlockPanel.Height.Set(unlockPanelHeight, 0f);

            IReadOnlyList<string> abilities = AlienSelectionScreen.GetSafeTransformationAbilities(transformation, player);
            List<UIElement> abilityEntries = new();
            abilityList.Clear();
            for (int i = 0; i < abilities.Count; i++) {
                UIElement entry = AlienSelectionScreen.CreateWrappedListEntry("• " + (abilities[i] ?? "Unknown ability"),
                    AlienSelectionScreen.CodexEntryWidth, 0.88f, framed: true);
                abilityEntries.Add(entry);
                abilityList.Add(entry);
            }

            float abilitiesContentHeight = AlienSelectionScreen.MeasureEntryListHeight(abilityEntries, abilityList.ListPadding);
            float abilitiesPanelHeight = Math.Max(112f, 44f + abilitiesContentHeight + 18f);
            abilitiesPanel.Height.Set(abilitiesPanelHeight, 0f);
            abilityList.Height.Set(abilitiesContentHeight, 0f);

            IReadOnlyList<string> combatSlotSummaries = transformation.GetCombatSlotSummaries(player);
            List<UIElement> combatEntries = new();
            combatSlotList.Clear();
            for (int i = 0; i < combatSlotSummaries.Count; i++) {
                UIElement entry = AlienSelectionScreen.CreateWrappedListEntry(
                    combatSlotSummaries[i] ?? "Unknown combat slot",
                    AlienSelectionScreen.CodexEntryWidth, 0.88f, framed: true);
                combatEntries.Add(entry);
                combatSlotList.Add(entry);
            }

            float combatContentHeight = AlienSelectionScreen.MeasureEntryListHeight(combatEntries, combatSlotList.ListPadding);
            float combatPanelHeight = Math.Max(112f, 44f + combatContentHeight + 18f);
            combatPanel.Height.Set(combatPanelHeight, 0f);
            combatSlotList.Height.Set(combatContentHeight, 0f);

            detailSectionList.Recalculate();
            detailSectionList.RecalculateChildren();
        }

        private string FindFirstNewTransformation(OmnitrixPlayer player) {
            IReadOnlyList<string> displayList = player.GetTransformationsForCodexDisplay();
            for (int i = 0; i < displayList.Count; i++) {
                string transformationId = displayList[i];
                if (player.IsNewlyUnlockedTransformation(transformationId))
                    return transformationId;
            }

            return string.Empty;
        }

        private static bool ContainsTransformation(IReadOnlyList<string> displayList, string transformationId) {
            if (displayList == null || string.IsNullOrEmpty(transformationId))
                return false;

            for (int i = 0; i < displayList.Count; i++) {
                if (string.Equals(displayList[i], transformationId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static UIPanel CreateCodexSectionPanel() {
            UIPanel panel = new UIPanel();
            panel.Width.Set(0f, 1f);
            panel.BackgroundColor = new Color(20, 28, 40, 215);
            panel.BorderColor = new Color(70, 88, 116, 215);
            return panel;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }
}
