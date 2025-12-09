using Ben10Mod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Ben10Mod.Content.Interface {
    public class UISystem : ModSystem {
        internal UserInterface MyInterface;
        internal AlienSelectionScreen AS;

        private GameTime _lastUpdateUiGameTime;

        public override void Load() {
            base.Load();
            if (!Main.dedServ) {
                MyInterface = new UserInterface();
                AS = new AlienSelectionScreen();
                AS.Activate();
            }
        }

        public override void Unload() {
            base.Unload();
            AS = null;
        }

        public override void UpdateUI(GameTime gameTime) {
            _lastUpdateUiGameTime = gameTime;
            if (MyInterface?.CurrentState != null) {
                MyInterface.Update(gameTime);
            }
        }


        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MyMod: MyInterface",
                    delegate {
                        if (_lastUpdateUiGameTime != null && MyInterface?.CurrentState != null) {
                            MyInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        internal void ShowMyUI() {
            MyInterface?.SetState(AS);
        }

        internal void HideMyUI() {
            MyInterface?.SetState(null);
        }

    }

    public class AlienSelectionScreen : UIState {

        UIPanel panel;

        UIImage AlienOne;
        UIImage AlienTwo;
        UIImage AlienThree;
        UIImage AlienFour;
        UIImage AlienFive;
        UIImage AlienSix;
        UIImage AlienSeven;
        UIImage AlienEight;
        UIImage AlienNine;
        UIImage AlienTen;

    public override void OnInitialize() {
        panel = new UIPanel();
        panel.Width.Set(512, 0);
        panel.Height.Set(256, 0);
        panel.HAlign = panel.VAlign = 0.5f;
        Append(panel);

        UIText text = new UIText("Alien Selection Screen");
        text.HAlign = 0.5f;
        text.Top.Set(10f, 0f);
        panel.Append(text);

        int drawHeight = 60;
        int drawWidth  = 50;
        int padding    = 20; // space between icons

        AlienOne   = new(TransformationEnum.None.GetTransformationIcon());
        AlienTwo   = new(TransformationEnum.None.GetTransformationIcon());
        AlienThree = new(TransformationEnum.None.GetTransformationIcon());
        AlienFour  = new(TransformationEnum.None.GetTransformationIcon());
        AlienFive  = new(TransformationEnum.None.GetTransformationIcon());

        // Hook up events
        AlienOne.OnLeftClick   += NextAlienOne;
        AlienOne.OnRightClick  += PrevAlienOne;
        AlienTwo.OnLeftClick   += NextAlienTwo;
        AlienTwo.OnRightClick  += PrevAlienTwo;
        AlienThree.OnLeftClick += NextAlienThree;
        AlienThree.OnRightClick+= PrevAlienThree;
        AlienFour.OnLeftClick  += NextAlienFour;
        AlienFour.OnRightClick += PrevAlienFour;
        AlienFive.OnLeftClick  += NextAlienFive;
        AlienFive.OnRightClick += PrevAlienFive;

        var aliens = new[] { AlienOne, AlienTwo, AlienThree, AlienFour, AlienFive };
        int count = aliens.Length;

        // spacing between icons (center-to-center)
        float spacing = drawWidth + padding;
        // middle index (2 for 5 items, 1.5 for 4 items, etc.)
        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++) {
            var alien = aliens[i];

            alien.Width.Set(drawWidth, 0f);
            alien.Height.Set(drawHeight, 0f);

            // vertically: some fixed offset below the title
            alien.Top.Set(80f, 0f);

            // horizontally: center aligned, then shifted left/right
            alien.HAlign = 0.5f;
            float offsetFromCenter = (i - centerIndex) * spacing;
            alien.Left.Set(offsetFromCenter, 0f);

            panel.Append(alien);
        }
    }


        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);
            // If this code is in the panel or container element, check it directly
            if (ContainsPoint(Main.MouseScreen)) {
                Main.LocalPlayer.mouseInterface = true;
            }
            // Otherwise, we can check a child element instead
            if (panel.ContainsPoint(Main.MouseScreen)) {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (IsMouseHovering) {
                PlayerInput.LockVanillaMouseScroll("MyMod/ScrollListA"); // The passed in string can be anything.
            }
        }

        private void NextAlienOne(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.NextTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[0]);
        }
        private void NextAlienTwo(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.NextTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[1]);
        }
        private void NextAlienThree(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.NextTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[2]);
        }
        private void NextAlienFour(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.NextTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[3]);
        }
        private void NextAlienFive(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.NextTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[4]);
        }
        private void PrevAlienOne(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.PrevTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[0]);
        }
        private void PrevAlienTwo(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.PrevTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[1]);
        }
        private void PrevAlienThree(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.PrevTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[2]);
        }
        private void PrevAlienFour(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.PrevTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[3]);
        }
        private void PrevAlienFive(UIMouseEvent evt, UIElement listeningElement) {
            TransformationHandler.PrevTransformation(Main.LocalPlayer, ref Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[4]);
        }


        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            AlienOne.SetImage(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[0].GetTransformationIcon());
            AlienTwo.SetImage(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[1].GetTransformationIcon());
            AlienThree.SetImage(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[2].GetTransformationIcon());
            AlienFour.SetImage(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[3].GetTransformationIcon());
            AlienFive.SetImage(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[4].GetTransformationIcon());

            if (AlienOne.IsMouseHovering) {
                Main.instance.MouseText(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[0].GetName());
            }
            if (AlienTwo.IsMouseHovering) {
                Main.instance.MouseText(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[1].GetName());
            }
            if (AlienThree.IsMouseHovering) {
                Main.instance.MouseText(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[2].GetName());
            }
            if (AlienFour.IsMouseHovering) {
                Main.instance.MouseText(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[3].GetName());
            }
            if (AlienFive.IsMouseHovering) {
                Main.instance.MouseText(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().transformations[4].GetName());
            }
        }
    }
}
