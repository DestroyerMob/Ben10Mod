using System.Collections.Generic;

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
            return _transformations.TryGetValue(fullID, out var trans) ? trans : null;
        }

        public static IEnumerable<Transformation> All => _transformations.Values;
    }
}