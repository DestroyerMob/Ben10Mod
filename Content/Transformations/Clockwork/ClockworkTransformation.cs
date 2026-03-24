using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Clockwork;

public class ClockworkTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Clockwork";
    public override string TransformationName => "Clockwork";
    public override int TransformationBuffId => ModContent.BuffType<Clockwork_Buff>();
    protected override string BasicDescription => "A simple time-themed base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.TungstenHelmet;
    protected override int BodySlot => ArmorIDs.Body.TungstenChainmail;
    protected override int LegSlot => ArmorIDs.Legs.TungstenGreaves;
}
