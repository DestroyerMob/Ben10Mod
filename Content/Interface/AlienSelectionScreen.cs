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

namespace Ben10Mod.Content.Interface
{
    public class HoverOutlineImage : UIImage
    {
        public HoverOutlineImage(Asset<Texture2D> texture) : base(texture) { }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            if (!ContainsPoint(Main.MouseScreen))
                return;

            var dims = GetDimensions();
            Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            rect.Width = Math.Max(1, rect.Width);
            rect.Height = Math.Max(1, rect.Height);

            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), Color.White);
        }
    }

    public class UISystem : ModSystem
    {
        internal UserInterface MyInterface;
        internal AlienSelectionScreen AS;
        internal TransformationPaletteScreen TPS;
        private GameTime _lastUpdateUiGameTime;

        private void EnsureInterfaceInitialized()
        {
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
        }

        public override void Load()
        {
            EnsureInterfaceInitialized();
        }

        public override void Unload() {
            MyInterface = null;
            AS = null;
            TPS = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            EnsureInterfaceInitialized();
            _lastUpdateUiGameTime = gameTime;
            if (MyInterface?.CurrentState != null)
                MyInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            EnsureInterfaceInitialized();
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Ben10Mod: AlienSelection",
                    delegate {
                        if (_lastUpdateUiGameTime != null && MyInterface?.CurrentState != null)
                            MyInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        return true;
                    },
                    InterfaceScaleType.UI));

                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Ben10Mod: OmnitrixEnergyBar",
                    delegate {
                        DrawOmnitrixEnergyBar();
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        private void DrawOmnitrixEnergyBar()
        {
            Player player = Main.LocalPlayer;
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (!omp.omnitrixEquipped) return;

            float fillPercent = MathHelper.Clamp(omp.omnitrixEnergy / (float)omp.omnitrixEnergyMax, 0f, 1f);

            Texture2D panelLeft  = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Left").Value;
            Texture2D panelMid   = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Middle").Value;
            Texture2D panelRight = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Right").Value;
            Texture2D fillTex    = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Fill").Value;

            int midCount = 20;
            int barWidth = panelLeft.Width + panelMid.Width * midCount + panelRight.Width;
            
            int uiMargin   = 20;
            int gap        = 26;
            int hpBarWidth = 252;
            int y          = 30;

            int hpLeftX = Main.screenWidth - uiMargin - hpBarWidth;
            int x = hpLeftX - gap - barWidth;

            int barHeight = Math.Max(panelLeft.Height, Math.Max(panelMid.Height, panelRight.Height));

            int yLeft  = y + (barHeight - panelLeft.Height) / 2;
            int yMid   = y + (barHeight - panelMid.Height) / 2;
            int yRight = y + (barHeight - panelRight.Height) / 2;

            Main.spriteBatch.Draw(panelLeft, new Vector2(x, yLeft), Color.White);

            int midStartX = x + panelLeft.Width;
            int midEndX   = x + barWidth - panelRight.Width;
            for (int drawX = midStartX; drawX < midEndX; drawX += panelMid.Width)
            {
                int w = Math.Min(panelMid.Width, midEndX - drawX);
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

            int fillWidth = (int)(innerWidth * fillPercent);
            if (fillPercent > 0f && fillWidth < 1) fillWidth = 1;

            if (fillWidth > 0)
            {
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

        private void DrawCurrentAttackIndicator(Player player, OmnitrixPlayer omp, int x, int y, int width)
        {
            bool showAttackHud = omp.IsTransformed;
            bool showSelectionHud = !omp.IsTransformed && omp.GetActiveOmnitrix() != null;

            if (!showAttackHud && !showSelectionHud)
                return;

            Transformation trans = omp.CurrentTransformation;
            if (showAttackHud && trans == null)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Rectangle panelRect = new Rectangle(x, y, width, 66);
            bool holdingBadge = player.HeldItem.ModItem is PlumbersBadge;
            float pulse = MathHelper.Clamp(omp.AttackSelectionPulseProgress, 0f, 1f);
            Color accent = showAttackHud ? omp.GetCurrentAttackAccentColor() : omp.GetSelectedTransformationAccentColor();
            Color borderColor = Color.Lerp(new Color(70, 90, 110), accent, 0.55f + pulse * 0.45f);
            Color fillColor = holdingBadge
                ? new Color(10, 18, 24, 205)
                : new Color(10, 18, 24, 155);

            if (pulse > 0f)
            {
                Rectangle glowRect = new Rectangle(panelRect.X - 2, panelRect.Y - 2, panelRect.Width + 4, panelRect.Height + 4);
                Main.spriteBatch.Draw(pixel, glowRect, accent * (0.16f * pulse));
            }

            Main.spriteBatch.Draw(pixel, panelRect, fillColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), borderColor);
            Main.spriteBatch.Draw(pixel, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height), borderColor);

            string title = showAttackHud ? "Attack" : "Selection";
            string slotLabel = showAttackHud ? omp.GetCurrentAttackSelectionLabel() : omp.GetSelectedTransformationHudLabel();
            string attackName = showAttackHud ? omp.GetCurrentAttackDisplayName() : omp.GetSelectedTransformationDisplayName();
            int energyCost = showAttackHud ? trans.GetEnergyCost(omp) : 0;

            Utils.DrawBorderString(Main.spriteBatch, title, new Vector2(panelRect.X + 10, panelRect.Y + 7),
                new Color(220, 230, 240), 0.8f);
            Utils.DrawBorderString(Main.spriteBatch, slotLabel, new Vector2(panelRect.Right - 10, panelRect.Y + 7),
                borderColor, 0.82f, 1f, 0f);
            Utils.DrawBorderString(Main.spriteBatch, attackName, new Vector2(panelRect.X + 10, panelRect.Y + 25),
                Color.White, 0.96f);

            if (energyCost > 0)
            {
                Utils.DrawBorderString(Main.spriteBatch, "Energy",
                    new Vector2(panelRect.X + 10, panelRect.Y + 45), new Color(180, 195, 210), 0.72f);
                Utils.DrawBorderString(Main.spriteBatch, $"{energyCost} OE",
                    new Vector2(panelRect.Right - 10, panelRect.Y + 43), accent, 0.86f, 1f, 0f);
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

    public class AlienSelectionScreen : UIState
    {
        private UIPanel mainPanel;
        private readonly List<HoverOutlineImage> rosterSlots = new();
        private UIGrid unlockedGrid;
        private UIPanel infoPanel;
        private UIText nameText;
        private UIText descriptionText;
        private UIList abilityList;
        private UIText abilitiesHeader;

        private string currentlySelectedId = "";

        private static Asset<Texture2D> GetSafeTransformationIcon(Transformation trans)
        {
            try
            {
                Asset<Texture2D> icon = trans?.GetTransformationIcon();
                if (icon != null)
                    return icon;
            }
            catch
            {
            }

            return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien");
        }

        private static string GetSafeTransformationName(Transformation trans)
        {
            return string.IsNullOrWhiteSpace(trans?.TransformationName) ? "Unknown Alien" : trans.TransformationName;
        }

        private static string GetSafeTransformationDescription(Transformation trans, OmnitrixPlayer player)
        {
            string description = null;

            try
            {
                description = trans?.GetDescription(player);
            }
            catch
            {
            }

            return string.IsNullOrWhiteSpace(description) ? "No description available." : description;
        }

        private static IReadOnlyList<string> GetSafeTransformationAbilities(Transformation trans, OmnitrixPlayer player)
        {
            try
            {
                List<string> abilities = trans?.GetAbilities(player);
                if (abilities != null && abilities.Count > 0)
                    return abilities;
            }
            catch
            {
            }

            return new[] { "No abilities listed." };
        }

        public override void OnInitialize()
        {
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

            int slotSize = 92;
            int rosterStartX = 65;
            int rosterY = 105;

            for (int i = 0; i < 5; i++)
            {
                var slot = new HoverOutlineImage(ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien"));
                slot.Width.Set(slotSize, 0f);
                slot.Height.Set(slotSize, 0f);
                slot.Left.Set(rosterStartX + i * (slotSize + 26f), 0f);
                slot.Top.Set(rosterY, 0f);
                int index = i;
                slot.OnLeftClick += (_, _) => AssignToSlot(index);
                slot.OnRightClick += (_, _) => ClearSlot(index);
                slot.OnMouseOver += (_, _) =>
                {
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

            unlockedGrid = new UIGrid();
            unlockedGrid.Width.Set(564f, 0f);
            unlockedGrid.Height.Set(315f, 0f);
            unlockedGrid.Left.Set(65f, 0f);
            unlockedGrid.Top.Set(rosterY + slotSize + 85f, 0f);
            unlockedGrid.ListPadding = 26f;
            mainPanel.Append(unlockedGrid);

            var gridScrollbar = new UIScrollbar();
            gridScrollbar.Height.Set(315f, 0f);
            gridScrollbar.Left.Set(641f, 0f);
            gridScrollbar.Top.Set(rosterY + slotSize + 85f, 0f);
            mainPanel.Append(gridScrollbar);
            unlockedGrid.SetScrollbar(gridScrollbar);

            infoPanel = new UIPanel();
            infoPanel.Width.Set(460f, 0f);
            infoPanel.Height.Set(545f, 0f);
            infoPanel.Left.Set(740f, 0f);
            infoPanel.Top.Set(92f, 0f);
            mainPanel.Append(infoPanel);

            nameText = new UIText("Select an alien", 1.3f);
            nameText.HAlign = 0f;
            nameText.Left.Set(32f, 0f);
            nameText.Top.Set(28f, 0f);
            infoPanel.Append(nameText);

            descriptionText = new UIText("Click any unlocked alien", 0.95f);
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
            closeBtn.OnLeftClick += (_, _) =>
            {
                ModContent.GetInstance<UISystem>().HideMyUI();
                Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().showingUI = false;
            };
            mainPanel.Append(closeBtn);
        }

        private void AssignToSlot(int slotIndex)
        {
            if (string.IsNullOrEmpty(currentlySelectedId)) return;

            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            if (player.unlockedTransformations.Contains(currentlySelectedId))
            {
                player.transformationSlots[slotIndex] = currentlySelectedId;
                player.SyncTransformationStateToServer();
                currentlySelectedId = "";
            }
        }

        private void ClearSlot(int slotIndex)
        {
            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            player.transformationSlots[slotIndex] = "";
            player.SyncTransformationStateToServer();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();

            for (int i = 0; i < rosterSlots.Count; i++)
            {
                string id = i < player.transformationSlots.Length ? player.transformationSlots[i] : string.Empty;
                var trans = TransformationLoader.Get(id);
                rosterSlots[i].SetImage(GetSafeTransformationIcon(trans));
            }

            unlockedGrid.Clear();
            foreach (var id in player.unlockedTransformations)
            {
                if (string.IsNullOrEmpty(id)) continue;

                var trans = TransformationLoader.Get(id);
                if (trans == null) continue;

                var icon = GetSafeTransformationIcon(trans);
                var slot = new UIElement();
                slot.Width.Set(92f, 0f);
                slot.Height.Set(92f, 0f);

                var btn = new HoverOutlineImage(icon);
                btn.Width.Set(icon.Width(), 0f);
                btn.Height.Set(icon.Height(), 0f);
                btn.Left.Set(0f, 0f);
                btn.VAlign = 0.5f;

                btn.OnLeftClick += (_, _) =>
                {
                    currentlySelectedId = id;
                    UpdateInfoPanel(trans);
                };

                btn.OnMouseOver += (_, _) => UpdateInfoPanel(trans);

                slot.Append(btn);
                unlockedGrid.Add(slot);
            }

            unlockedGrid.Recalculate();
            unlockedGrid.RecalculateChildren();

            if (mainPanel.ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }

        private void UpdateInfoPanel(Transformation trans)
        {
            if (trans == null)
            {
                nameText.SetText("Select an alien");
                descriptionText.SetText("Click any unlocked alien");
                abilitiesHeader.Top.Set(180f, 0f);
                abilityList.Top.Set(210f, 0f);
                abilityList.Clear();
                return;
            }

            OmnitrixPlayer player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            nameText.SetText(GetSafeTransformationName(trans));
            descriptionText.SetText(GetSafeTransformationDescription(trans, player));

            // Give wrapped descriptions enough room before the abilities section starts.
            var descriptionHeight = descriptionText.MinHeight.Pixels > 0f ? descriptionText.MinHeight.Pixels : 72f;
            float abilitiesTop = Math.Max(180f, descriptionText.Top.Pixels + descriptionHeight + 26f);
            abilitiesHeader.Top.Set(abilitiesTop, 0f);
            abilityList.Top.Set(abilitiesTop + 30f, 0f);

            abilityList.Clear();
            var abilities = GetSafeTransformationAbilities(trans, player);
            foreach (var ability in abilities)
                abilityList.Add(new UIText("• " + (ability ?? "Unknown ability"), 0.95f));
        }

        private void UpdateInfoPanelFromTransformationId(string transformationId)
        {
            UpdateInfoPanel(TransformationLoader.Get(transformationId));
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }
}
