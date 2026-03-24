using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Fasttrack;

public class FasttrackTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Fasttrack";
    public override string TransformationName => "Fasttrack";
    public override int TransformationBuffId => ModContent.BuffType<Fasttrack_Buff>();
    protected override string BasicDescription => "A simple speedster base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.TinHelmet;
    protected override int BodySlot => ArmorIDs.Body.TinChainmail;
    protected override int LegSlot => ArmorIDs.Legs.TinGreaves;
}
