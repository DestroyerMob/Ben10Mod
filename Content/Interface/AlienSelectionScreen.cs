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

            panel.Append(text);

            AlienOne = new(TransformationEnum.None.GetTransformationIcon());
            AlienOne.OnLeftClick += NextAlienOne;
            AlienOne.OnRightClick += PrevAlienOne;
            AlienOne.Height.Set(32, 0);
            AlienOne.Width.Set(32, 0);
            AlienOne.VAlign = 0.5f;
            AlienOne.Left.Set(0, 0);
            panel.Append(AlienOne);   
            
            AlienTwo = new(TransformationEnum.None.GetTransformationIcon());
            AlienTwo.OnLeftClick += NextAlienTwo;
            AlienTwo.OnRightClick += PrevAlienTwo;
            AlienTwo.Height.Set(32, 0);
            AlienTwo.Width.Set(32, 0);
            AlienTwo.VAlign = 0.5f;
            AlienTwo.Left.Set(AlienTwo.Width.Pixels * 2.25f + (18 * 2.25f), 0);;
            panel.Append(AlienTwo);    
            
            AlienThree = new(TransformationEnum.None.GetTransformationIcon());
            AlienThree.OnLeftClick += NextAlienThree;
            AlienThree.OnRightClick += PrevAlienThree;
            AlienThree.Height.Set(32, 0);
            AlienThree.Width.Set(32, 0);
            AlienThree.VAlign = 0.5f;
            AlienThree.Left.Set(AlienThree.Width.Pixels * 4.5f + (18 * 4.5f), 0);
            panel.Append(AlienThree); 
            
            AlienFour = new(TransformationEnum.None.GetTransformationIcon());
            AlienFour.OnLeftClick += NextAlienFour;
            AlienFour.OnRightClick += PrevAlienFour;
            AlienFour.Height.Set(32, 0);
            AlienFour.Width.Set(32, 0);
            AlienFour.VAlign = 0.5f;
            AlienFour.Left.Set(AlienFour.Width.Pixels * 6.75f + (18 * 6.75f), 0);
            panel.Append(AlienFour);
            
            AlienFive = new(TransformationEnum.None.GetTransformationIcon());
            AlienFive.OnLeftClick += NextAlienFive;
            AlienFive.OnRightClick += PrevAlienFive;
            AlienFive.Height.Set(32, 0);
            AlienFive.Width.Set(32, 0);
            AlienFive.VAlign = 0.5f;
            AlienFive.Left.Set(AlienFive.Width.Pixels * 9 + (18 * 9), 0);
            panel.Append(AlienFive);  
            
            //AlienSix = new(TransformationEnum.None.GetTransformationIcon());
            //AlienSix.OnLeftClick += NextAlienSix;
            //AlienSix.OnRightClick += PrevAlienSix;
            //AlienSix.Height.Set(32, 0);
            //AlienSix.Width.Set(32, 0);
            //AlienSix.VAlign = 0.5f;
            //AlienSix.Left.Set(AlienSix.Width.Pixels * 5 + (18 * 5), 0);
            //panel.Append(AlienSix);      
            
            //AlienSeven = new(TransformationEnum.None.GetTransformationIcon());
            //AlienSeven.OnLeftClick += NextAlienSeven;
            //AlienSeven.OnRightClick += PrevAlienSeven;
            //AlienSeven.Height.Set(32, 0);
            //AlienSeven.Width.Set(32, 0);
            //AlienSeven.VAlign = 0.5f;
            //AlienSeven.Left.Set(AlienSeven.Width.Pixels * 6 + (18 * 6), 0);
            //panel.Append(AlienSeven);   
            
            //AlienEight = new(TransformationEnum.None.GetTransformationIcon());
            //AlienEight.OnLeftClick += NextAlienEight;
            //AlienEight.OnRightClick += PrevAlienEight;
            //AlienEight.Height.Set(32, 0);
            //AlienEight.Width.Set(32, 0);
            //AlienEight.VAlign = 0.5f;
            //AlienEight.Left.Set(AlienEight.Width.Pixels * 7 + (18 * 7), 0);
            //panel.Append(AlienEight);

            //AlienNine = new(TransformationEnum.None.GetTransformationIcon());
            //AlienNine.OnLeftClick += NextAlienNine;
            //AlienNine.OnRightClick += PrevAlienNine;
            //AlienNine.Height.Set(32, 0);
            //AlienNine.Width.Set(32, 0);
            //AlienNine.VAlign = 0.5f;
            //AlienNine.Left.Set(AlienNine.Width.Pixels * 8 + (18 * 8), 0);
            //panel.Append(AlienNine);

            //AlienTen = new(TransformationEnum.None.GetTransformationIcon());
            //AlienTen.OnLeftClick += NextAlienTen;
            //AlienTen.OnRightClick += PrevAlienTen;
            //AlienTen.Height.Set(32, 0);
            //AlienTen.Width.Set(32, 0);
            //AlienTen.VAlign = 0.5f;
            //AlienTen.Left.Set(AlienTen.Width.Pixels * 9 + (18 * 9), 0);
            //panel.Append(AlienTen);
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
