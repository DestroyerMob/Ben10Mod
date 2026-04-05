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
    public bool ShouldSerializeShowHeroInterface() => false;

    [DefaultValue(true)]
    public bool ShowHeroEnergyBar { get; set; } = true;

    [DefaultValue(true)]
    public bool ShowHeroMoveInterface { get; set; } = true;

    [Browsable(false)]
    [DefaultValue(false)]
    public bool UseSimplifiedHeroInterface {
        get => UseSimplifiedHeroEnergyBar || UseSimplifiedHeroMoveInterface;
        set {
            _useSimplifiedHeroEnergyBar = value;
            _useSimplifiedHeroMoveInterface = value;
        }
    }
    public bool ShouldSerializeUseSimplifiedHeroInterface() => false;

    [DefaultValue(false)]
    public bool UseSimplifiedHeroEnergyBar {
        get => _useSimplifiedHeroEnergyBar;
        set => _useSimplifiedHeroEnergyBar = value;
    }

    [DefaultValue(false)]
    public bool UseSimplifiedHeroMoveInterface {
        get => _useSimplifiedHeroMoveInterface;
        set => _useSimplifiedHeroMoveInterface = value;
    }

    [DefaultValue(false)]
    public bool EnableTransformationRandomizer { get; set; } = false;

    [DefaultValue(true)]
    public bool ShowHeroAffordabilityTinting { get; set; } = true;

    [DefaultValue(false)]
    public bool AlwaysShowOmnitrixEnergyText { get; set; } = false;

    [DefaultValue(18)]
    public int TransformWheelDeadzonePixels { get; set; } = 18;

    private bool _useSimplifiedHeroEnergyBar;
    private bool _useSimplifiedHeroMoveInterface;
}
