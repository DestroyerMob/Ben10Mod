using System;
using System.Collections.Generic;
using Ben10Mod.Content.Transformations;

namespace Ben10Mod.Common.Omnitrix;

internal sealed class PalettePresetData {
    public List<TransformationPaletteColorEntry> Entries { get; } = new();
    public List<string> EnabledChannelKeys { get; } = new();
}

public sealed class TransformationCustomizationStore {
    internal Dictionary<string, Dictionary<string, TransformationPaletteChannelSettings>> TransformationPaletteOverrides { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, TransformationPaletteChannelSettings> OmnitrixVisualPaletteOverrides { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal HashSet<string> PaletteEnabledChannels { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, string> SelectedTransformationCostumes { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, string> CustomTransformationNames { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, PalettePresetData[]> PalettePresets { get; } =
        new(StringComparer.OrdinalIgnoreCase);
}
