using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Ben10Mod;

public sealed class Ben10ServerConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(false)]
    public bool AllowBlacklistedBaseTransformations { get; set; } = false;

    [DefaultValue(false)]
    public bool AllowBlacklistedBaseOmnitrixes { get; set; } = false;

    [DefaultValue(false)]
    public bool AllowBlacklistedBasePlumbersBadges { get; set; } = false;

    [DefaultValue(false)]
    public bool AllowBlacklistedBaseWorldGen { get; set; } = false;
}
