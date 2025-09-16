using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Command {
    public class MasterControl : ModCommand {
        public override string Command => "mastercontrol";

        public override CommandType Type => CommandType.Chat;

        public override string Usage => "/mastercontrol <parameter>";

        public override void Action(CommandCaller caller, string input, string[] args) {
            if (args.Length > 0) {
                if (args[0] == "give") {
                    caller.Player.GetModPlayer<OmnitrixPlayer>().masterControl = true;
                } else if (args[0] == "remove") {
                    caller.Player.GetModPlayer<OmnitrixPlayer>().masterControl = false;
                }
            } else {
                Main.NewText(caller.Player.GetModPlayer<OmnitrixPlayer>().masterControl);
            }
        }
    }
}
