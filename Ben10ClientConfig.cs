using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Ben10Mod;

public sealed class Ben10ClientConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(true)]
    public bool ShowHeroInterface { get; set; } = true;

    [DefaultValue(false)]
    public bool UseSimplifiedHeroInterface { get; set; } = false;

    [DefaultValue(true)]
    public bool ShowHeroAffordabilityTinting { get; set; } = true;

    [DefaultValue(18)]
    public int TransformWheelDeadzonePixels { get; set; } = 18;
}
