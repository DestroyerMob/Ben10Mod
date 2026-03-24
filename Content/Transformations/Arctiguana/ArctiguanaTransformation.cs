using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Arctiguana;

public class ArctiguanaTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Arctiguana";
    public override string TransformationName => "Arctiguana";
    public override int TransformationBuffId => ModContent.BuffType<Arctiguana_Buff>();
    protected override string CostumeItemName => "BigChill";
    protected override string BasicDescription => "A simple cold-themed base-form implementation with a basic projectile primary attack.";
}
