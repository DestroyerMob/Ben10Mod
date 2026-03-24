using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Goop;

public class GoopTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Goop";
    public override string TransformationName => "Goop";
    public override int TransformationBuffId => ModContent.BuffType<Goop_Buff>();
    protected override string BasicDescription => "A simple slime-like base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.CopperHelmet;
    protected override int BodySlot => ArmorIDs.Body.CopperChainmail;
    protected override int LegSlot => ArmorIDs.Legs.CopperGreaves;
}
