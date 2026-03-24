using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.AlienX;

public class AlienXTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:AlienX";
    public override string TransformationName => "Alien X";
    public override int TransformationBuffId => ModContent.BuffType<AlienX_Buff>();
    protected override string CostumeItemName => "ChromaStone";
    protected override string BasicDescription => "A simple Celestialsapien implementation with a basic projectile primary attack for future expansion.";
}
