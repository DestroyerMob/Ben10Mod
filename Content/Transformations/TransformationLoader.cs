using System.Collections.Generic;
using System;
using Ben10Mod.Common.Systems;

namespace Ben10Mod.Content.Transformations
{
    public static class TransformationLoader
    {
        private static readonly Dictionary<string, Transformation> _transformations = new();

        internal static void Register(Transformation transformation)
        {
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

            if (_transformations.TryGetValue(fullID, out var trans))
                return Ben10FeatureBlacklistRegistry.IsTransformationBlacklisted(trans) ? null : trans;

            foreach (var pair in _transformations) {
                if (string.Equals(pair.Key, fullID, StringComparison.OrdinalIgnoreCase) &&
                    !Ben10FeatureBlacklistRegistry.IsTransformationBlacklisted(pair.Value))
                    return pair.Value;
            }

            return null;
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
