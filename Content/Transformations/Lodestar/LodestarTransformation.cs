using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Lodestar;

public class LodestarTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Lodestar";
    public override string TransformationName => "Lodestar";
    public override int TransformationBuffId => ModContent.BuffType<Lodestar_Buff>();
    protected override string BasicDescription => "A simple magnetic base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.LeadHelmet;
    protected override int BodySlot => ArmorIDs.Body.LeadChainmail;
    protected override int LegSlot => ArmorIDs.Legs.LeadGreaves;
}
