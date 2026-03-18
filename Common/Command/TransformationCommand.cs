using Ben10Mod.Content;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Command;

public class TransformationCommand : ModCommand {
    public override string Command => "transformation";
    public override string Usage => "/transformation <add|remove> <ModName:TransformationId>";
    public override string Description => "Adds or removes a transformation unlock by full transformation id.";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (args.Length < 2) {
            Main.NewText(Usage, Color.Orange);
            return;
        }

        string mode = args[0].ToLowerInvariant();
        string transformationId = args[1];
        var transformation = TransformationLoader.Get(transformationId);

        if (transformation == null) {
            Main.NewText($"Unknown transformation id: {transformationId}", Color.Red);
            return;
        }

        var omp = caller.Player.GetModPlayer<OmnitrixPlayer>();
        bool changed = mode switch {
            "add" => omp.UnlockTransformation(transformationId),
            "remove" => omp.RemoveTransformation(transformationId),
            _ => false
        };

        if (mode != "add" && mode != "remove") {
            Main.NewText(Usage, Color.Orange);
            return;
        }

        if (!changed) {
            string status = mode == "add" ? "already unlocked" : "not unlocked";
            Main.NewText($"{transformationId} is {status}.", Color.Yellow);
        }
    }
}
