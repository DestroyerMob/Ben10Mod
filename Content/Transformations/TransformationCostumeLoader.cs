using System;
using System.Collections.Generic;

namespace Ben10Mod.Content.Transformations {
    public static class TransformationCostumeLoader {
        private static readonly Dictionary<string, TransformationCostume> Costumes =
            new(StringComparer.OrdinalIgnoreCase);

        internal static void Register(TransformationCostume costume) {
            if (costume == null || string.IsNullOrWhiteSpace(costume.FullID))
                return;

            Costumes[costume.FullID] = costume;
        }

        public static TransformationCostume Resolve(string fullID) {
            if (string.IsNullOrWhiteSpace(fullID))
                return null;

            return Costumes.TryGetValue(fullID, out TransformationCostume costume)
                ? costume
                : null;
        }

        public static IReadOnlyList<TransformationCostume> GetForTransformation(string transformationId) {
            if (string.IsNullOrWhiteSpace(transformationId))
                return Array.Empty<TransformationCostume>();

            List<TransformationCostume> costumes = new();
            foreach (TransformationCostume costume in Costumes.Values) {
                if (costume == null ||
                    !string.Equals(costume.TargetTransformationId, transformationId, StringComparison.OrdinalIgnoreCase))
                    continue;

                costumes.Add(costume);
            }

            costumes.Sort(static (left, right) => {
                int sortCompare = left.SortOrder.CompareTo(right.SortOrder);
                if (sortCompare != 0)
                    return sortCompare;

                int modCompare = string.Compare(left.Mod.Name, right.Mod.Name, StringComparison.OrdinalIgnoreCase);
                if (modCompare != 0)
                    return modCompare;

                return string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
            });
            return costumes;
        }

        internal static void Clear() {
            Costumes.Clear();
        }
    }
}
