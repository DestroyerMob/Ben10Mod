using Ben10Mod.Content;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Command;

public class TransformationCommand : ModCommand {
    public override string Command => "transformation";
    public override string Usage => "/transformation <add|remove> <transformation|all>";
    public override string Description => "Adds or removes a transformation unlock by full id, short id, or display name, or uses 'all' for the whole roster.";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (args.Length < 2) {
            Main.NewText(Usage, Color.Orange);
            return;
        }

        string mode = args[0].ToLowerInvariant();
        string transformationQuery = string.Join(" ", args.Skip(1));
        var omp = caller.Player.GetModPlayer<OmnitrixPlayer>();

        if (mode != "add" && mode != "remove") {
            Main.NewText(Usage, Color.Orange);
            return;
        }

        if (string.Equals(transformationQuery, "all", StringComparison.OrdinalIgnoreCase)) {
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient) {
                if (mode == "add") {
                    foreach (Transformation registeredTransformation in TransformationLoader.All
                                 .OrderBy(transformation => transformation.FullID, StringComparer.OrdinalIgnoreCase))
                        TransformationHandler.AddTransformation(caller.Player, registeredTransformation.FullID);

                    Main.NewText("Requested unlock for all registered transformations.", Color.LimeGreen);
                }
                else {
                    foreach (string unlockedId in omp.unlockedTransformations.ToList())
                        TransformationHandler.RemoveTransformation(caller.Player, unlockedId);

                    Main.NewText("Requested removal for all unlocked transformations.", Color.OrangeRed);
                }

                return;
            }

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

        var matches = FindTransformationsForQuery(transformationQuery);
        if (matches.Count > 1) {
            string matchList = string.Join(", ", matches
                .OrderBy(match => match.FullID, StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .Select(match => match.FullID));
            string suffix = matches.Count > 5 ? ", ..." : string.Empty;
            Main.NewText($"'{transformationQuery}' matches multiple transformations: {matchList}{suffix}", Color.Orange);
            return;
        }

        var transformation = matches.Count == 1 ? matches[0] : null;

        if (transformation == null) {
            string suggestionText = BuildSuggestionText(transformationQuery);
            Main.NewText(
                string.IsNullOrEmpty(suggestionText)
                    ? $"Unknown transformation: {transformationQuery}"
                    : $"Unknown transformation: {transformationQuery}. {suggestionText}",
                Color.Red);
            return;
        }

        string canonicalTransformationId = transformation.FullID;
        if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient) {
            if (mode == "add") {
                TransformationHandler.AddTransformation(caller.Player, canonicalTransformationId);
                Main.NewText($"Requested unlock for {canonicalTransformationId}.", Color.LimeGreen);
            }
            else {
                TransformationHandler.RemoveTransformation(caller.Player, canonicalTransformationId);
                Main.NewText($"Requested removal for {canonicalTransformationId}.", Color.OrangeRed);
            }

            return;
        }

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

    private static List<Transformation> FindTransformationsForQuery(string query) {
        List<Transformation> exactMatches = new();
        HashSet<string> seenTransformationIds = new(StringComparer.OrdinalIgnoreCase);

        Transformation strictMatch = TransformationLoader.Resolve(query);
        if (strictMatch != null) {
            exactMatches.Add(strictMatch);
            seenTransformationIds.Add(strictMatch.FullID);
        }

        string normalizedQuery = NormalizeTransformationQuery(query);
        if (string.IsNullOrEmpty(normalizedQuery))
            return exactMatches;

        foreach (Transformation transformation in TransformationLoader.All) {
            if (transformation == null || seenTransformationIds.Contains(transformation.FullID))
                continue;

            if (!GetTransformationSearchKeys(transformation).Contains(normalizedQuery))
                continue;

            exactMatches.Add(transformation);
            seenTransformationIds.Add(transformation.FullID);
        }

        return exactMatches;
    }

    private static string BuildSuggestionText(string query) {
        string normalizedQuery = NormalizeTransformationQuery(query);
        if (string.IsNullOrEmpty(normalizedQuery))
            return string.Empty;

        List<Transformation> suggestions = new();
        foreach (Transformation transformation in TransformationLoader.All) {
            if (transformation == null)
                continue;

            bool matches = GetTransformationSearchKeys(transformation)
                .Any(searchKey => searchKey.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase));
            if (!matches)
                continue;

            suggestions.Add(transformation);
        }

        if (suggestions.Count == 0)
            return "Try the full id, like Ben10Mod:HeatBlast.";

        string suggestionList = string.Join(", ", suggestions
            .OrderBy(transformation => transformation.FullID, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .Select(transformation => transformation.FullID));
        string suffix = suggestions.Count > 5 ? ", ..." : string.Empty;
        return $"Closest matches: {suggestionList}{suffix}";
    }

    private static HashSet<string> GetTransformationSearchKeys(Transformation transformation) {
        HashSet<string> searchKeys = new(StringComparer.OrdinalIgnoreCase);
        if (transformation == null)
            return searchKeys;

        AddSearchKey(searchKeys, transformation.FullID);
        AddSearchKey(searchKeys, transformation.TransformationName);

        int separatorIndex = transformation.FullID.IndexOf(':');
        if (separatorIndex >= 0 && separatorIndex < transformation.FullID.Length - 1)
            AddSearchKey(searchKeys, transformation.FullID[(separatorIndex + 1)..]);

        return searchKeys;
    }

    private static void AddSearchKey(HashSet<string> searchKeys, string value) {
        string normalizedValue = NormalizeTransformationQuery(value);
        if (!string.IsNullOrEmpty(normalizedValue))
            searchKeys.Add(normalizedValue);
    }

    private static string NormalizeTransformationQuery(string value) {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        StringBuilder builder = new();
        foreach (char character in value) {
            if (char.IsLetterOrDigit(character))
                builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}

public sealed class TransformationsCommand : TransformationCommand {
    public override string Command => "transformations";
}
