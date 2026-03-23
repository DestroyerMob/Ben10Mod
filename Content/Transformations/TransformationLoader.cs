using System.Collections.Generic;
using System;

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

            return _transformations.TryGetValue(fullID, out var trans) ? trans : null;
        }

        public static Transformation Resolve(string fullID)
        {
            if (string.IsNullOrWhiteSpace(fullID))
                return null;

            if (_transformations.TryGetValue(fullID, out var trans))
                return trans;

            foreach (var pair in _transformations) {
                if (string.Equals(pair.Key, fullID, StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }

            return null;
        }

        public static IEnumerable<Transformation> All => _transformations.Values;
    }
}
