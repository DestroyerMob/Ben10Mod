using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Ben10Mod;

public sealed class Ben10ClientConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Browsable(false)]
    [DefaultValue(true)]
    public bool ShowHeroInterface {
        get => ShowHeroEnergyBar || ShowHeroMoveInterface;
        set {
            ShowHeroEnergyBar = value;
            ShowHeroMoveInterface = value;
        }
    }

    [DefaultValue(true)]
    public bool ShowHeroEnergyBar { get; set; } = true;

    [DefaultValue(true)]
    public bool ShowHeroMoveInterface { get; set; } = true;

    [DefaultValue(false)]
    public bool UseSimplifiedHeroInterface { get; set; } = false;

    [DefaultValue(true)]
    public bool ShowHeroAffordabilityTinting { get; set; } = true;

    [DefaultValue(false)]
    public bool AlwaysShowOmnitrixEnergyText { get; set; } = false;

    [DefaultValue(18)]
    public int TransformWheelDeadzonePixels { get; set; } = 18;
}
