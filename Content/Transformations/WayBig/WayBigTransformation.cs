using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WayBig;

public class WayBigTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:WayBig";
    public override string TransformationName => "Way Big";
    public override int TransformationBuffId => ModContent.BuffType<WayBig_Buff>();
    protected override string CostumeItemName => "FourArms";
    protected override string BasicDescription => "A simple giant base-form implementation with a basic projectile primary attack.";
}
