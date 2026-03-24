using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Lodestar;

public class LodestarTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Lodestar";
    public override string TransformationName => "Lodestar";
    public override int TransformationBuffId => ModContent.BuffType<Lodestar_Buff>();
    protected override string CostumeItemName => "ChromaStone";
    protected override string BasicDescription => "A simple magnetic base-form implementation with a basic projectile primary attack.";
}
