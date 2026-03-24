using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.PeskyDust;

public class PeskyDustTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:PeskyDust";
    public override string TransformationName => "Pesky Dust";
    public override int TransformationBuffId => ModContent.BuffType<PeskyDust_Buff>();
    protected override string BasicDescription => "A simple fairy-like base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.JungleHat;
    protected override int BodySlot => ArmorIDs.Body.JungleShirt;
    protected override int LegSlot => ArmorIDs.Legs.JunglePants;
}
