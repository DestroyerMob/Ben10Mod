using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.AmpFibian;

public class AmpFibianTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:AmpFibian";
    public override string TransformationName => "AmpFibian";
    public override int TransformationBuffId => ModContent.BuffType<AmpFibian_Buff>();
    protected override string BasicDescription => "A simple electrical base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.GoldHelmet;
    protected override int BodySlot => ArmorIDs.Body.GoldChainmail;
    protected override int LegSlot => ArmorIDs.Legs.GoldGreaves;
}
