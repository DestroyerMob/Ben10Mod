using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Content.Interface;

namespace Ben10Mod.Common.Command;

public class PaletteCommand : ModCommand {
    public override string Command => "palette";
    public override string Usage => "/palette";
    public override string Description => "Opens the alien customization menu for the current or selected form.";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (Main.dedServ || caller.Player.whoAmI != Main.myPlayer) {
            Main.NewText("Alien customization can only be opened from a local client.", Color.Orange);
            return;
        }

        UISystem uiSystem = ModContent.GetInstance<UISystem>();
        if (uiSystem?.MyInterface == null || uiSystem.TPS == null) {
            Main.NewText("The customization menu is not available right now.", Color.Red);
            return;
        }

        uiSystem.ShowPaletteUI();
        caller.Player.GetModPlayer<OmnitrixPlayer>().showingUI = true;
    }
}
