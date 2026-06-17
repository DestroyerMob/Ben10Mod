using Ben10Mod.Content.Interface;
using Ben10Mod.Keybinds;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Omnitrix;

public sealed class OmnitrixInputController {
    public void HandleUiInput(OmnitrixPlayer owner) {
        if (KeybindSystem.OpenTransformationScreen.JustPressed && owner.omnitrixEquipped) {
            UISystem uiSystem = ModContent.GetInstance<UISystem>();
            if (!owner.showingUI) {
                uiSystem.ShowMyUI();
                owner.showingUI = true;
            }
            else {
                uiSystem.HideMyUI();
                owner.showingUI = false;
            }
        }

        if (KeybindSystem.OpenTransformationCodex.JustPressed && owner.omnitrixEquipped &&
            owner.unlockedTransformations.Count > 0) {
            UISystem uiSystem = ModContent.GetInstance<UISystem>();
            if (uiSystem.IsCodexUIOpen()) {
                uiSystem.HideMyUI();
                owner.showingUI = false;
            }
            else {
                uiSystem.ShowCodexUI();
                owner.showingUI = true;
            }
        }
    }
}
