using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Content.Transformations;

namespace Ben10Mod.Content.Interface {
    public class FittedTransformationIcon : UIElement {
        private const float IconPadding = 8f;
        private Asset<Texture2D> texture;
        private readonly Func<bool> isFavoriteProvider;

        public FittedTransformationIcon(Asset<Texture2D> texture, Func<bool> isFavoriteProvider) {
            this.texture = texture;
            this.isFavoriteProvider = isFavoriteProvider;
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

            if (ContainsPoint(Main.MouseScreen)) {
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
        internal UserInterface               MyInterface;
        internal AlienSelectionScreen        AS;
        internal TransformationPaletteScreen TPS;
        internal TransformationRadialMenu    TRM;
        private  GameTime                    _lastUpdateUiGameTime;

        private void EnsureInterfaceInitialized() {
            if (Main.dedServ)
                return;

            MyInterface ??= new UserInterface();

            if (AS == null) {
                AS = new AlienSelectionScreen();
                AS.Activate();
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
            MyInterface = null;
            AS          = null;
            TPS         = null;
            TRM         = null;
        }

        public override void UpdateUI(GameTime gameTime) {
            EnsureInterfaceInitialized();
            _lastUpdateUiGameTime = gameTime;
            TRM?.Update(this);
            if (MyInterface?.CurrentState != null)
                MyInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
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
            if (!clientConfig.ShowHeroInterface)
                return;
            bool   showEnergyBar     = omp.omnitrixEquipped;
            bool   showAttackHudOnly = !showEnergyBar && omp.IsTransformed;
            if (!showEnergyBar && !showAttackHudOnly)
                return;

            int uiMargin   = 20;
            int gap        = 26;
            int hpBarWidth = 252;
            int y          = 30;
            int hpLeftX    = Main.screenWidth - uiMargin - hpBarWidth;

            if (showAttackHudOnly) {
                int hudWidth = clientConfig.UseSimplifiedHeroInterface ? 220 : 252;
                int hudX     = hpLeftX - gap - hudWidth;
                DrawCurrentAttackIndicator(player, omp, hudX, y, hudWidth);
                return;
            }

            float fillPercent = MathHelper.Clamp(omp.omnitrixEnergy / (float)omp.omnitrixEnergyMax, 0f, 1f);

            if (clientConfig.UseSimplifiedHeroInterface) {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                const int compactWidth = 220;
                const int compactBarHeight = 16;
                int compactX = hpLeftX - gap - compactWidth;
                Rectangle barRect = new Rectangle(compactX, y + 8, compactWidth, compactBarHeight);
                Rectangle fillRect = new Rectangle(barRect.X + 2, barRect.Y + 2,
                    Math.Max(0, (int)((barRect.Width - 4) * fillPercent)), Math.Max(1, barRect.Height - 4));
                Color barBorder = new Color(88, 198, 138);
                Color barFill = new Color(92, 255, 148);

                Main.spriteBatch.Draw(pixel, barRect, new Color(10, 18, 24, 190));
                Main.spriteBatch.Draw(pixel, new Rectangle(barRect.X, barRect.Y, barRect.Width, 2), barBorder);
                Main.spriteBatch.Draw(pixel, new Rectangle(barRect.X, barRect.Bottom - 2, barRect.Width, 2), barBorder);
                Main.spriteBatch.Draw(pixel, new Rectangle(barRect.X, barRect.Y, 2, barRect.Height), barBorder);
                Main.spriteBatch.Draw(pixel, new Rectangle(barRect.Right - 2, barRect.Y, 2, barRect.Height), barBorder);

                if (fillRect.Width > 0)
                    Main.spriteBatch.Draw(pixel, fillRect, barFill);

                Utils.DrawBorderString(Main.spriteBatch, $"OE {(int)omp.omnitrixEnergy}/{(int)omp.omnitrixEnergyMax}",
                    new Vector2(barRect.Center.X, y - 8), Color.White, 0.78f, 0.5f, 0f);

                DrawCurrentAttackIndicator(player, omp, compactX, barRect.Bottom + 8, compactWidth);
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

            string text = $"{(int)omp.omnitrixEnergy}/{(int)omp.omnitrixEnergyMax}";
            Utils.DrawBorderString(
                Main.spriteBatch,
                text,
                new Vector2(x + barWidth * 0.5f, y - 12),
                Color.White,
                0.9f,
                0.5f,
                0.5f
            );

            DrawCurrentAttackIndicator(player, omp, x, y + barHeight + 18, barWidth);
        }

        private void DrawCurrentAttackIndicator(Player player, OmnitrixPlayer omp, int x, int y, int width) {
            bool showAttackHud    = omp.IsTransformed;
            bool showSelectionHud = !omp.IsTransformed && omp.GetActiveOmnitrix() != null;
            var  clientConfig     = ModContent.GetInstance<Ben10ClientConfig>();

            if (!showAttackHud && !showSelectionHud)
                return;

            Transformation trans = omp.CurrentTransformation;
            if (showAttackHud && trans == null)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            string cooldownSummary =
                showAttackHud ? omp.GetAttackHudCooldownSummary() : omp.GetSelectionHudCooldownSummary();
            string paletteStatus = showAttackHud
                ? omp.GetCurrentTransformationPaletteStatusText()
                : omp.GetSelectedTransformationPaletteStatusText();
            int energyCost = showAttackHud ? trans.GetEnergyCost(omp) : 0;
            bool affordabilityWarning = clientConfig.ShowHeroAffordabilityTinting &&
                                        showAttackHud && trans != null && !omp.CanAffordCurrentAttackForHud();
            string compactFooter = affordabilityWarning
                ? $"Need {energyCost} OE"
                : !string.IsNullOrWhiteSpace(cooldownSummary)
                    ? cooldownSummary
                    : energyCost > 0
                        ? $"{energyCost} OE"
                        : paletteStatus;
            Rectangle panelRect = new Rectangle(x, y, width, clientConfig.UseSimplifiedHeroInterface
                ? (string.IsNullOrWhiteSpace(compactFooter) ? 42 : 58)
                : string.IsNullOrWhiteSpace(cooldownSummary) ? 66 : 92);
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

            if (clientConfig.UseSimplifiedHeroInterface) {
                Utils.DrawBorderString(Main.spriteBatch, attackName,
                    new Vector2(panelRect.X + 10, panelRect.Y + 9), Color.White, 0.88f);
                Utils.DrawBorderString(Main.spriteBatch, slotLabel,
                    new Vector2(panelRect.Right - 10, panelRect.Y + 10), borderColor, 0.72f, 1f, 0f);

                if (!string.IsNullOrWhiteSpace(compactFooter)) {
                    Utils.DrawBorderString(Main.spriteBatch, compactFooter,
                        new Vector2(panelRect.X + 10, panelRect.Bottom - 19),
                        affordabilityWarning ? new Color(255, 170, 145) : new Color(170, 190, 208), 0.68f);
                }
                return;
            }

            string title = showAttackHud ? "Attack" : "Selection";
            Utils.DrawBorderString(Main.spriteBatch, title, new Vector2(panelRect.X + 10, panelRect.Y + 7),
                new Color(220, 230, 240), 0.8f);
            Utils.DrawBorderString(Main.spriteBatch, slotLabel, new Vector2(panelRect.Right - 10, panelRect.Y + 7),
                borderColor, 0.82f, 1f, 0f);
            Utils.DrawBorderString(Main.spriteBatch, attackName, new Vector2(panelRect.X + 10, panelRect.Y + 25),
                Color.White, 0.96f);

            string detailLabel = paletteStatus;
            if (string.IsNullOrWhiteSpace(detailLabel) && energyCost > 0)
                detailLabel = "Energy";

            if (!string.IsNullOrWhiteSpace(detailLabel)) {
                Utils.DrawBorderString(Main.spriteBatch, detailLabel,
                    new Vector2(panelRect.X + 10, panelRect.Y + 45), new Color(180, 195, 210), 0.72f);
            }

            if (energyCost > 0) {
                Utils.DrawBorderString(Main.spriteBatch, affordabilityWarning ? $"Need {energyCost} OE" : $"{energyCost} OE",
                    new Vector2(panelRect.Right - 10, panelRect.Y + 43),
                    affordabilityWarning ? new Color(255, 170, 145) : accent, 0.86f, 1f, 0f);
            }

            if (!string.IsNullOrWhiteSpace(cooldownSummary)) {
                Utils.DrawBorderString(Main.spriteBatch, cooldownSummary,
                    new Vector2(panelRect.X + 10, panelRect.Bottom - 22), new Color(170, 190, 208), 0.72f);
            }
        }

        internal void ShowMyUI() {
            EnsureInterfaceInitialized();
            MyInterface?.SetState(AS);
        }

        internal void ShowPaletteUI() {
            EnsureInterfaceInitialized();
            MyInterface?.SetState(TPS);
        }

        internal void HideMyUI() => MyInterface?.SetState(null);
    }

    public class AlienSelectionScreen : UIState {
        private          UIPanel                 mainPanel;
        private readonly List<FittedTransformationIcon> rosterSlots = new();
        private          UIGrid                  unlockedGrid;
        private          UIPanel                 infoPanel;
        private          UIText                  nameText;
        private          UIText                  descriptionText;
        private          UIList                  abilityList;
        private          UIText                  abilitiesHeader;

        private string currentlySelectedId     = "";
        private string unlockedRosterSignature = "";
        private bool   unlockedGridDirty       = true;

        private static Asset<Texture2D> GetSafeTransformationIcon(Transformation trans) {
            try {
                Asset<Texture2D> icon = trans?.GetTransformationIcon();
                if (icon != null)
                    return icon;
            }
            catch { }

            return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien");
        }

        private static string GetSafeTransformationName(Transformation trans) {
            var player = Main.LocalPlayer?.GetModPlayer<OmnitrixPlayer>();
            if (player != null && trans != null)
                return player.GetTransformationBaseName(trans);

            return string.IsNullOrWhiteSpace(trans?.TransformationName) ? "Unknown Alien" : trans.TransformationName;
        }

        private static string GetSafeTransformationDescription(Transformation trans, OmnitrixPlayer player) {
            string description = null;

            try {
                description = trans?.GetDescription(player);
            }
            catch { }

            return string.IsNullOrWhiteSpace(description) ? "No description available." : description;
        }

        private static IReadOnlyList<string>
            GetSafeTransformationAbilities(Transformation trans, OmnitrixPlayer player) {
            try {
                List<string> abilities = trans?.GetAbilities(player);
                if (abilities != null && abilities.Count > 0)
                    return abilities;
            }
            catch { }

            return new[] { "No abilities listed." };
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

            var unlockedHint = new UIText("Right click an unlocked alien to favorite it", 0.78f);
            unlockedHint.Left.Set(65f, 0f);
            unlockedHint.Top.Set(rosterY + slotSize + 76f, 0f);
            mainPanel.Append(unlockedHint);

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

            abilitiesHeader = new UIText("Abilities:", 1.15f);
            abilitiesHeader.Left.Set(32f, 0f);
            abilitiesHeader.Top.Set(180f, 0f);
            infoPanel.Append(abilitiesHeader);

            abilityList = new UIList();
            abilityList.Width.Set(400f, 0f);
            abilityList.Height.Set(300f, 0f);
            abilityList.Left.Set(32f, 0f);
            abilityList.Top.Set(210f, 0f);
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

        private void AssignToSlot(int slotIndex) {
            if (string.IsNullOrEmpty(currentlySelectedId)) return;

            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            if (player.unlockedTransformations.Contains(currentlySelectedId)) {
                player.transformationSlots[slotIndex] = currentlySelectedId;
                player.SyncTransformationStateToServer();
                currentlySelectedId = "";
            }
        }

        private void ClearSlot(int slotIndex) {
            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            player.transformationSlots[slotIndex] = "";
            player.SyncTransformationStateToServer();
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

            foreach (string id in player.GetUnlockedTransformationsForDisplay()) {
                if (string.IsNullOrEmpty(id))
                    continue;

                var trans = TransformationLoader.Get(id);
                if (trans == null)
                    continue;

                var icon = GetSafeTransformationIcon(trans);
                var slot = new UIElement();
                slot.Width.Set(92f, 0f);
                slot.Height.Set(92f, 0f);

                string transformationId = id;
                var btn = new FittedTransformationIcon(icon, () => player.IsFavoriteTransformation(transformationId));
                btn.Width.Set(92f, 0f);
                btn.Height.Set(92f, 0f);
                btn.IgnoresMouseInteraction = true;
                slot.OnLeftClick += (_, _) => {
                    currentlySelectedId = transformationId;
                    UpdateInfoPanel(TransformationLoader.Get(transformationId));
                };

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
        }

        private static string BuildUnlockedRosterSignature(OmnitrixPlayer player) {
            if (player == null)
                return string.Empty;

            List<string> displayIds = new(player.GetUnlockedTransformationsForDisplay());
            if (displayIds.Count == 0)
                return string.Empty;

            System.Text.StringBuilder builder = new();
            for (int i = 0; i < displayIds.Count; i++) {
                string transformationId = displayIds[i];
                Transformation transformation = TransformationLoader.Get(transformationId);
                builder.Append(transformationId)
                    .Append('=')
                    .Append(player.IsFavoriteTransformation(transformationId) ? '1' : '0')
                    .Append('=')
                    .Append(transformation?.GetDisplayName(player) ?? string.Empty)
                    .Append('|');
            }

            return builder.ToString();
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
                abilityList.Add(new UIText("• " + (ability ?? "Unknown ability"), 0.95f));
        }

        private void UpdateInfoPanelFromTransformationId(string transformationId) {
            UpdateInfoPanel(TransformationLoader.Get(transformationId));
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }
}
