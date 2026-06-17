using System;
using System.Collections.Generic;

namespace Ben10Mod.Common.Omnitrix;

public sealed class TransformationRosterState {
    public string[] Slots { get; set; } = { "Ben10Mod:HeatBlast", "", "", "", "" };
    public List<string> Unlocked { get; } = new() { "Ben10Mod:HeatBlast" };
    public HashSet<string> Favorites { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> NewlyUnlocked { get; } = new(StringComparer.OrdinalIgnoreCase);
}
