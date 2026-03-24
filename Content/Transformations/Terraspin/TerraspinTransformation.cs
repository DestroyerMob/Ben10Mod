using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Terraspin;

public class TerraspinTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Terraspin";
    public override string TransformationName => "Terraspin";
    public override int TransformationBuffId => ModContent.BuffType<Terraspin_Buff>();
    protected override string BasicDescription => "A simple turtle-like base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.SilverHelmet;
    protected override int BodySlot => ArmorIDs.Body.SilverChainmail;
    protected override int LegSlot => ArmorIDs.Legs.SilverGreaves;
}
