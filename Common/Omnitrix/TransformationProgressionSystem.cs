using System.Collections.Generic;

namespace Ben10Mod.Common.Omnitrix;

public sealed class TransformationProgressionSystem {
    internal HashSet<int> ParticipatedEvents { get; } = new();
    internal HashSet<int> ActiveEvents { get; } = new();
}
