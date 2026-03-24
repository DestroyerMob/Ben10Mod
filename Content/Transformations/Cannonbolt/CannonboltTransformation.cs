using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Cannonbolt;

public class CannonboltTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Cannonbolt";
    public override string TransformationName => "Cannonbolt";
    public override int TransformationBuffId => ModContent.BuffType<Cannonbolt_Buff>();
    protected override string CostumeItemName => "DiamondHead";
    protected override string BasicDescription => "A simple armored base-form implementation with a basic projectile primary attack.";
}
