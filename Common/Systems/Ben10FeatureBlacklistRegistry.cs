using System;
using System.Collections.Generic;
using Ben10Mod.Content.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Systems;

public enum Ben10FeatureType {
    Transformation,
    Omnitrix,
    PlumbersBadge,
    WorldGen
}

public static class Ben10FeatureBlacklistRegistry {
    public const string BaseModId = "Ben10Mod";

    private static readonly HashSet<string> BlacklistedTransformationIds =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<Ben10FeatureType, HashSet<string>> BlacklistedModIds = new() {
        [Ben10FeatureType.Transformation] = new(StringComparer.OrdinalIgnoreCase),
        [Ben10FeatureType.Omnitrix] = new(StringComparer.OrdinalIgnoreCase),
        [Ben10FeatureType.PlumbersBadge] = new(StringComparer.OrdinalIgnoreCase),
        [Ben10FeatureType.WorldGen] = new(StringComparer.OrdinalIgnoreCase)
    };

    public static void BlacklistTransformation(string transformationIdOrModId) {
        string normalizedIdentifier = NormalizeIdentifier(transformationIdOrModId);
        if (string.IsNullOrEmpty(normalizedIdentifier))
            throw new ArgumentException("Transformation blacklist entries must be non-empty strings.",
                nameof(transformationIdOrModId));

        if (IsTransformationIdentifier(normalizedIdentifier))
            BlacklistedTransformationIds.Add(normalizedIdentifier);
        else
            BlacklistedModIds[Ben10FeatureType.Transformation].Add(normalizedIdentifier);
    }

    public static void BlacklistFeature(Ben10FeatureType featureType, string modId) {
        string normalizedModId = NormalizeIdentifier(modId);
        if (string.IsNullOrEmpty(normalizedModId))
            throw new ArgumentException("Feature blacklist entries must be non-empty strings.", nameof(modId));

        if (featureType == Ben10FeatureType.Transformation) {
            BlacklistTransformation(normalizedModId);
            return;
        }

        BlacklistedModIds[featureType].Add(normalizedModId);
    }

    public static bool IsTransformationBlacklisted(Transformation transformation) {
        if (transformation == null)
            return false;

        return IsTransformationBlacklistedInternal(transformation.FullID, transformation.Mod?.Name);
    }

    public static bool IsTransformationBlacklisted(string transformationIdOrModId) {
        string normalizedIdentifier = NormalizeIdentifier(transformationIdOrModId);
        if (string.IsNullOrEmpty(normalizedIdentifier))
            return false;

        if (!IsTransformationIdentifier(normalizedIdentifier))
            return IsFeatureBlacklisted(Ben10FeatureType.Transformation, normalizedIdentifier);

        string ownerModId = TryGetTransformationOwnerMod(normalizedIdentifier, out string parsedOwnerModId)
            ? parsedOwnerModId
            : string.Empty;
        return IsTransformationBlacklistedInternal(normalizedIdentifier, ownerModId);
    }

    public static bool IsFeatureBlacklisted(Ben10FeatureType featureType, Mod mod) {
        return mod != null && IsFeatureBlacklisted(featureType, mod.Name);
    }

    public static bool IsFeatureBlacklisted(Ben10FeatureType featureType, string modId) {
        string normalizedModId = NormalizeIdentifier(modId);
        if (string.IsNullOrEmpty(normalizedModId))
            return false;

        if (ShouldAllowBaseModContent(featureType, normalizedModId))
            return false;

        return BlacklistedModIds[featureType].Contains(normalizedModId);
    }

    public static bool TryParseFeatureType(string featureKey, out Ben10FeatureType featureType) {
        string normalizedFeatureKey = NormalizeIdentifier(featureKey);
        if (string.IsNullOrEmpty(normalizedFeatureKey)) {
            featureType = default;
            return false;
        }

        switch (normalizedFeatureKey.ToLowerInvariant()) {
            case "transformation":
            case "transformations":
                featureType = Ben10FeatureType.Transformation;
                return true;
            case "omnitrix":
            case "omnitrixes":
            case "omnitrices":
                featureType = Ben10FeatureType.Omnitrix;
                return true;
            case "plumbersbadge":
            case "plumbersbadges":
            case "plumberbadge":
            case "plumberbadges":
            case "badge":
            case "badges":
                featureType = Ben10FeatureType.PlumbersBadge;
                return true;
            case "worldgen":
            case "worldgeneration":
            case "generation":
                featureType = Ben10FeatureType.WorldGen;
                return true;
            default:
                featureType = default;
                return false;
        }
    }

    internal static void Clear() {
        BlacklistedTransformationIds.Clear();
        foreach (HashSet<string> blacklistedIds in BlacklistedModIds.Values)
            blacklistedIds.Clear();
    }

    private static bool IsTransformationBlacklistedInternal(string transformationId, string ownerModId) {
        if (ShouldAllowBaseModContent(Ben10FeatureType.Transformation, ownerModId))
            return false;

        return BlacklistedTransformationIds.Contains(transformationId) ||
               BlacklistedModIds[Ben10FeatureType.Transformation].Contains(ownerModId);
    }

    private static bool ShouldAllowBaseModContent(Ben10FeatureType featureType, string ownerModId) {
        if (!string.Equals(ownerModId, BaseModId, StringComparison.OrdinalIgnoreCase))
            return false;

        Ben10ServerConfig config = ModContent.GetInstance<Ben10ServerConfig>();
        return featureType switch {
            Ben10FeatureType.Transformation => config.AllowBlacklistedBaseTransformations,
            Ben10FeatureType.Omnitrix => config.AllowBlacklistedBaseOmnitrixes,
            Ben10FeatureType.PlumbersBadge => config.AllowBlacklistedBasePlumbersBadges,
            Ben10FeatureType.WorldGen => config.AllowBlacklistedBaseWorldGen,
            _ => false
        };
    }

    private static bool TryGetTransformationOwnerMod(string transformationId, out string ownerModId) {
        ownerModId = string.Empty;
        if (string.IsNullOrWhiteSpace(transformationId))
            return false;

        int separatorIndex = transformationId.IndexOf(':');
        if (separatorIndex <= 0)
            return false;

        ownerModId = transformationId[..separatorIndex].Trim();
        return !string.IsNullOrEmpty(ownerModId);
    }

    private static bool IsTransformationIdentifier(string value) {
        return !string.IsNullOrEmpty(value) && value.Contains(':');
    }

    private static string NormalizeIdentifier(string value) {
        return value?.Trim() ?? string.Empty;
    }
}
