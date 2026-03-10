using Ben10Mod.Enums;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations;

public abstract class TransformationPlayer : ModPlayer {
    public virtual     int    PrimaryAttack           => -1;
    public virtual     int    SecondaryAttack         => -1;
    public virtual     int    UltimateAttack          => -1;
    public virtual     float  PrimaryAttackModifier   => 1f;
    public virtual     float  SecondaryAttackModifier => 1f;
    public virtual     float  UltimateAttackModifier  => 1f;
    public virtual     int    TransformationBuffId    => -1;
    public new virtual string Name                    => "None";
    public virtual     string IconPath                => "Ben10Mod/Content/Interface/EmptyAlien";
    public virtual     string Description             => "A mysterious alien from the Omnitrix database.";



}