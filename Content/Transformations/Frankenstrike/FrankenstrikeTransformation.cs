using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Frankenstrike;

public class FrankenstrikeTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Frankenstrike";
    public override string TransformationName => "Frankenstrike";
    public override int TransformationBuffId => ModContent.BuffType<Frankenstrike_Buff>();
    protected override string CostumeItemName => "BuzzShock";
    protected override string BasicDescription => "A simple storm-powered base-form implementation with a basic projectile primary attack.";
}
