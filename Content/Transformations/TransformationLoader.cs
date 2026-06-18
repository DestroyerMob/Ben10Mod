using System.Collections.Generic;
using System;
using Ben10Mod.Common.Systems;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations
{
    public static class TransformationLoader
    {
        private static readonly Dictionary<string, Transformation> _transformations =
            new(StringComparer.OrdinalIgnoreCase);

        internal static void Register(Transformation transformation)
        {
            if (transformation == null || string.IsNullOrWhiteSpace(transformation.FullID))
                return;

            if (_transformations.TryGetValue(transformation.FullID, out Transformation existing) &&
                !ReferenceEquals(existing, transformation)) {
                ModContent.GetInstance<global::Ben10Mod.Ben10Mod>().Logger.Warn(
                    $"Duplicate transformation ID registered: {transformation.FullID}");
            }

            _transformations[transformation.FullID] = transformation;
        }

        public static Transformation Get(string fullID)
        {
            if (string.IsNullOrWhiteSpace(fullID))
                return null;

            if (!_transformations.TryGetValue(fullID, out var trans))
                return null;

            return Ben10FeatureBlacklistRegistry.IsTransformationBlacklisted(trans) ? null : trans;
        }

        public static Transformation Resolve(string fullID)
        {
            if (string.IsNullOrWhiteSpace(fullID))
                return null;

            return _transformations.TryGetValue(fullID, out var trans) &&
                   !Ben10FeatureBlacklistRegistry.IsTransformationBlacklisted(trans)
                ? trans
                : null;
        }

        public static IEnumerable<Transformation> All {
            get {
                foreach (Transformation transformation in _transformations.Values) {
                    if (!Ben10FeatureBlacklistRegistry.IsTransformationBlacklisted(transformation))
                        yield return transformation;
                }
            }
        }

        internal static void Clear()
        {
            _transformations.Clear();
        }
    }
}
