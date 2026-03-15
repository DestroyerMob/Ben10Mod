using System;
using System.Collections.Generic;
using Ben10Mod.Content.Items.Accessories;
using Terraria;

namespace Ben10Mod.Content.Transformations {
    public delegate bool TransformationBranchCondition(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
        Transformation parent, Transformation child, Transformation selectedTransformation);

    public delegate int TransformationBranchEnergyCost(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
        Transformation parent, Transformation child, Transformation selectedTransformation);

    public delegate bool TransformationBranchFailureBehavior(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
        Transformation parent, Transformation child, Transformation selectedTransformation);

    public sealed class RegisteredTransformationBranch {
        public string ParentTransformationId { get; init; } = "";
        public string ChildTransformationId { get; init; } = "";
        public int Priority { get; init; }
        public TransformationBranchCondition Condition { get; init; }
        public TransformationBranchEnergyCost EnergyCost { get; init; }
        public TransformationBranchFailureBehavior ShouldDetransformOnFailure { get; init; }

        public Transformation ResolveParent() => TransformationLoader.Get(ParentTransformationId);
        public Transformation ResolveChild() => TransformationLoader.Get(ChildTransformationId);

        public bool CanUse(Player player, OmnitrixPlayer omp, Omnitrix omnitrix, Transformation selectedTransformation) {
            Transformation parent = ResolveParent();
            Transformation child = ResolveChild();
            if (parent == null || child == null)
                return false;

            return Condition?.Invoke(player, omp, omnitrix, parent, child, selectedTransformation) ?? true;
        }

        public int ResolveEnergyCost(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
            Transformation selectedTransformation) {
            Transformation parent = ResolveParent();
            Transformation child = ResolveChild();
            if (parent == null || child == null)
                return 0;

            return Math.Max(0, EnergyCost?.Invoke(player, omp, omnitrix, parent, child, selectedTransformation) ?? 0);
        }

        public bool ShouldDetransform(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
            Transformation selectedTransformation) {
            Transformation parent = ResolveParent();
            Transformation child = ResolveChild();
            if (parent == null || child == null)
                return false;

            return ShouldDetransformOnFailure?.Invoke(player, omp, omnitrix, parent, child, selectedTransformation) ??
                   false;
        }
    }

    public static class TransformationBranchRegistry {
        private static readonly Dictionary<string, List<RegisteredTransformationBranch>> branchesByParent = new();

        public static void RegisterChildBranch(string parentTransformationId, string childTransformationId,
            TransformationBranchCondition condition = null,
            TransformationBranchEnergyCost energyCost = null,
            TransformationBranchFailureBehavior shouldDetransformOnFailure = null,
            int priority = 0) {
            if (string.IsNullOrWhiteSpace(parentTransformationId) || string.IsNullOrWhiteSpace(childTransformationId))
                throw new ArgumentException("Parent and child transformation IDs are required.");

            if (!branchesByParent.TryGetValue(parentTransformationId, out List<RegisteredTransformationBranch> branches)) {
                branches = new List<RegisteredTransformationBranch>();
                branchesByParent[parentTransformationId] = branches;
            }

            branches.Add(new RegisteredTransformationBranch {
                ParentTransformationId = parentTransformationId,
                ChildTransformationId = childTransformationId,
                Condition = condition,
                EnergyCost = energyCost,
                ShouldDetransformOnFailure = shouldDetransformOnFailure,
                Priority = priority
            });

            branches.Sort((left, right) => right.Priority.CompareTo(left.Priority));
        }

        public static IReadOnlyList<RegisteredTransformationBranch> GetChildBranches(string parentTransformationId) {
            return branchesByParent.TryGetValue(parentTransformationId, out List<RegisteredTransformationBranch> branches)
                ? branches
                : Array.Empty<RegisteredTransformationBranch>();
        }

        internal static void Clear() {
            branchesByParent.Clear();
        }
    }
}
