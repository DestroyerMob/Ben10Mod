using Ben10Mod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace Ben10Mod.Content.Interface
{
    public class UISystem : ModSystem
    {
        internal UserInterface MyInterface;
        internal AlienSelectionScreen AS;
        private GameTime _lastUpdateUiGameTime;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                MyInterface = new UserInterface();
                AS = new AlienSelectionScreen();
                AS.Activate();
            }
        }

        public override void Unload() => AS = null;

        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (MyInterface?.CurrentState != null)
                MyInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                // Your existing Alien Roster layer
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Ben10Mod: AlienSelection",
                    delegate {
                        if (_lastUpdateUiGameTime != null && MyInterface?.CurrentState != null)
                            MyInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        return true;
                    },
                    InterfaceScaleType.UI));

                // NEW: Omnitrix Energy Bar (always visible when transformed)
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Ben10Mod: OmnitrixEnergyBar",
                    delegate {
                        DrawOmnitrixEnergyBar();
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        private void DrawOmnitrixEnergyBar() {
            Player player = Main.LocalPlayer;
            var    omp    = player.GetModPlayer<OmnitrixPlayer>();

            if (!omp.omnitrixEquipped) return;

            float fillPercent = MathHelper.Clamp(omp.omnitrixEnergy / (float)omp.omnitrixEnergyMax, 0f, 1f);

            Texture2D panelLeft  = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Left").Value;
            Texture2D panelMid   = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Middle").Value;
            Texture2D panelRight = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Panel_Right").Value;
            Texture2D fillTex    = ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/OE_Fill").Value;

            // Width: left + (mid repeated N times) + right
            int midCount = 20;
            int barWidth = panelLeft.Width + panelMid.Width * midCount + panelRight.Width;
            
            int uiMargin   = 20;     // distance from screen edge (matches vanilla feel)
            int gap        = 26;          // space between your bar and the HP bar
            int hpBarWidth = 252;  // horizontal bars HP width (works with your screenshot)
            int y          = 30;            // top padding (match vanilla horizontal bars baseline)

            // left edge of the vanilla HP bar area
            int hpLeftX = Main.screenWidth - uiMargin - hpBarWidth;

            // your bar goes immediately to the left of that
            int x = hpLeftX - gap - barWidth;

            // Height: unify by using the tallest piece, then center the others vertically
            int barHeight = Math.Max(panelLeft.Height, Math.Max(panelMid.Height, panelRight.Height));

            int yLeft  = y + (barHeight - panelLeft.Height) / 2;
            int yMid   = y + (barHeight - panelMid.Height) / 2;
            int yRight = y + (barHeight - panelRight.Height) / 2;

            // Draw left
            Main.spriteBatch.Draw(panelLeft, new Vector2(x, yLeft), Color.White);

            // Draw tiled middle
            int midStartX = x + panelLeft.Width;
            int midEndX   = x + barWidth - panelRight.Width;

            for (int drawX = midStartX; drawX < midEndX; drawX += panelMid.Width) {
                int       w   = Math.Min(panelMid.Width, midEndX - drawX);
                Rectangle src = new Rectangle(0, 0, w, panelMid.Height);
                Main.spriteBatch.Draw(panelMid, new Vector2(drawX, yMid), src, Color.White);
            }

            // Draw right (now vertically aligned)
            Main.spriteBatch.Draw(panelRight, new Vector2(x + barWidth - panelRight.Width, yRight), Color.White);

            // ===== Fill inset tuned for your art =====
            int padLeft   = 6;
            int padRight  = 6;
            int padTop    = 6;
            int padBottom = 6;

            int innerX      = x + padLeft;
            int innerY      = y + padTop;
            int innerWidth  = barWidth - padLeft - padRight;
            int innerHeight = barHeight - padTop - padBottom; // 12

            if (innerWidth < 1 || innerHeight < 1)
                return;

            int fillWidth                                    = (int)(innerWidth * fillPercent);
            if (fillPercent > 0f && fillWidth < 1) fillWidth = 1;

            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle(innerX, innerY, fillWidth, innerHeight);
                Main.spriteBatch.Draw(fillTex, fillRect, Color.White);
            }

            // Energy text above the bar
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
        }

        internal void ShowMyUI() => MyInterface?.SetState(AS);
        internal void HideMyUI() => MyInterface?.SetState(null);
    }

    public class AlienSelectionScreen : UIState
    {
        private UIPanel mainPanel;
        private readonly List<UIImage> rosterSlots = new();
        private UIGrid unlockedGrid;
        private UIPanel infoPanel;
        private UIImage previewImage;
        private UIText nameText;
        private UIText descriptionText;
        private UIList abilityList;

        private TransformationEnum currentlySelected = TransformationEnum.None;

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
                var slot = new UIImage(TransformationEnum.None.GetTransformationIcon());
                slot.Width.Set(slotSize, 0f);
                slot.Height.Set(slotSize, 0f);
                slot.Left.Set(rosterStartX + i * (slotSize + 26f), 0f);
                slot.Top.Set(rosterY, 0f);

                int index = i;
                slot.OnLeftClick += (_, _) => AssignToSlot(index);
                slot.OnRightClick += (_, _) => ClearSlot(index);

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
            unlockedGrid.Width.Set(610f, 0f);
            unlockedGrid.Height.Set(315f, 0f);        // ← SHORTENED so it ends before Close button
            unlockedGrid.Left.Set(65f, 0f);
            unlockedGrid.Top.Set(rosterY + slotSize + 85f, 0f);
            unlockedGrid.ListPadding = 10f;
            mainPanel.Append(unlockedGrid);

            var gridScrollbar = new UIScrollbar();
            gridScrollbar.Height.Set(315f, 0f);       // ← matches new grid height
            gridScrollbar.Left.Set(685f, 0f);
            gridScrollbar.Top.Set(rosterY + slotSize + 85f, 0f);
            mainPanel.Append(gridScrollbar);
            unlockedGrid.SetScrollbar(gridScrollbar);

            infoPanel = new UIPanel();
            infoPanel.Width.Set(460f, 0f);
            infoPanel.Height.Set(545f, 0f);
            infoPanel.Left.Set(740f, 0f);
            infoPanel.Top.Set(92f, 0f);
            mainPanel.Append(infoPanel);

            previewImage = new UIImage(TransformationEnum.None.GetTransformationIcon());
            previewImage.Width.Set(158f, 0f);
            previewImage.Height.Set(158f, 0f);
            previewImage.HAlign = 0.5f;
            previewImage.Top.Set(28f, 0f);
            infoPanel.Append(previewImage);

            nameText = new UIText("Select an alien", 1.3f);
            nameText.HAlign = 0.5f;
            nameText.Top.Set(205f, 0f);
            infoPanel.Append(nameText);

            descriptionText = new UIText("Click any unlocked alien", 0.95f);
            descriptionText.HAlign = 0.5f;
            descriptionText.Top.Set(240f, 0f);
            descriptionText.Width.Set(400f, 0f);
            descriptionText.IsWrapped = true;
            infoPanel.Append(descriptionText);

            var abilitiesHeader = new UIText("Abilities:", 1.15f);
            abilitiesHeader.Left.Set(32f, 0f);
            abilitiesHeader.Top.Set(295f, 0f);
            infoPanel.Append(abilitiesHeader);

            abilityList = new UIList();
            abilityList.Width.Set(400f, 0f);
            abilityList.Height.Set(190f, 0f);
            abilityList.Left.Set(32f, 0f);
            abilityList.Top.Set(325f, 0f);
            infoPanel.Append(abilityList);

            var closeBtn = new UITextPanel<string>("Close Roster");
            closeBtn.HAlign = 0.5f;
            closeBtn.Top.Set(-58f, 1f);
            closeBtn.OnLeftClick += (_, _) => ModContent.GetInstance<UISystem>().HideMyUI();
            mainPanel.Append(closeBtn);
        }

        private void AssignToSlot(int slotIndex)
        {
            if (currentlySelected == TransformationEnum.None) return;

            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            if (player.unlockedTransformation.Contains(currentlySelected))
                player.transformations[slotIndex] = currentlySelected;
        }

        private void ClearSlot(int slotIndex)
        {
            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            player.transformations[slotIndex] = TransformationEnum.None;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var player = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();

            for (int i = 0; i < rosterSlots.Count; i++)
                rosterSlots[i].SetImage(player.transformations[i].GetTransformationIcon());

            unlockedGrid.Clear();
            foreach (var trans in player.unlockedTransformation)
            {
                if (trans == TransformationEnum.None) continue;

                var btn = new UIImage(trans.GetTransformationIcon());
                btn.Width.Set(80f, 0f);
                btn.Height.Set(80f, 0f);

                btn.OnLeftClick += (_, _) =>
                {
                    currentlySelected = trans;
                    UpdateInfoPanel(trans);
                };

                btn.OnMouseOver += (_, _) => UpdateInfoPanel(trans);

                unlockedGrid.Add(btn);
            }

            unlockedGrid.Recalculate();
            unlockedGrid.RecalculateChildren();

            if (mainPanel.ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }

        private void UpdateInfoPanel(TransformationEnum trans)
        {
            previewImage.SetImage(trans.GetTransformationIcon());
            nameText.SetText(trans.GetName());
            descriptionText.SetText(trans.GetDescription());

            abilityList.Clear();
            var abilities = trans.GetAbilities();
            foreach (var ability in abilities)
                abilityList.Add(new UIText("• " + ability, 0.95f));
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }
}