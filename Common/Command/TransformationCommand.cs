using Ben10Mod.Content;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Command;

public class TransformationCommand : ModCommand {
    public override string Command => "transformation";
    public override string Usage => "/transformation <add|remove> <ModName:TransformationId|all>";
    public override string Description => "Adds or removes a transformation unlock by full transformation id, or uses 'all' for the whole roster.";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (args.Length < 2) {
            Main.NewText(Usage, Color.Orange);
            return;
        }

        string mode = args[0].ToLowerInvariant();
        string transformationId = args[1];
        var omp = caller.Player.GetModPlayer<OmnitrixPlayer>();

        if (mode != "add" && mode != "remove") {
            Main.NewText(Usage, Color.Orange);
            return;
        }

        if (string.Equals(transformationId, "all", StringComparison.OrdinalIgnoreCase)) {
            int changedCount = mode == "add"
                ? TransformationLoader.All
                    .OrderBy(transformation => transformation.FullID, StringComparer.OrdinalIgnoreCase)
                    .Count(transformation => omp.UnlockTransformation(transformation.FullID, showEffects: false))
                : omp.unlockedTransformations
                    .ToList()
                    .Count(unlockedId => omp.RemoveTransformation(unlockedId, showEffects: false));

            if (changedCount == 0) {
                string noChangeMessage = mode == "add"
                    ? "All registered transformations are already unlocked."
                    : "The transformation roster is already empty.";
                Main.NewText(noChangeMessage, Color.Yellow);
                return;
            }

            string action = mode == "add" ? "Unlocked" : "Removed";
            Color messageColor = mode == "add" ? Color.LimeGreen : Color.OrangeRed;
            Main.NewText($"{action} {changedCount} transformation{(changedCount == 1 ? string.Empty : "s")}.", messageColor);
            return;
        }

        var transformation = TransformationLoader.Resolve(transformationId);

        if (transformation == null) {
            Main.NewText($"Unknown transformation id: {transformationId}", Color.Red);
            return;
        }

        string canonicalTransformationId = transformation.FullID;
        bool changed = mode switch {
            "add" => omp.UnlockTransformation(canonicalTransformationId),
            "remove" => omp.RemoveTransformation(canonicalTransformationId),
            _ => false
        };

        if (!changed) {
            string status = mode == "add" ? "already unlocked" : "not unlocked";
            Main.NewText($"{canonicalTransformationId} is {status}.", Color.Yellow);
        }
    }
}
