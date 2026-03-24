using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Whampire;

public class WhampireTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Whampire";
    public override string TransformationName => "Whampire";
    public override int TransformationBuffId => ModContent.BuffType<Whampire_Buff>();
    protected override string CostumeItemName => "GhostFreak";
    protected override string BasicDescription => "A simple vampire-like base-form implementation with a basic projectile primary attack.";
}
