using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Blitzwolfer;

public class BlitzwolferTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Blitzwolfer";
    public override string TransformationName => "Blitzwolfer";
    public override int TransformationBuffId => ModContent.BuffType<Blitzwolfer_Buff>();
    protected override string CostumeItemName => "XLR8";
    protected override string BasicDescription => "A simple wolf-like base-form implementation with a basic projectile primary attack.";
}
