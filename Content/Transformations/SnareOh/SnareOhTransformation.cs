using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.SnareOh;

public class SnareOhTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:SnareOh";
    public override string TransformationName => "Snare-Oh";
    public override int TransformationBuffId => ModContent.BuffType<SnareOh_Buff>();
    protected override string CostumeItemName => "GhostFreak";
    protected override string BasicDescription => "A simple mummy-like base-form implementation with a basic projectile primary attack.";
}
