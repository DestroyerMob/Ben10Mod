using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WaterHazard;

public class WaterHazardTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:WaterHazard";
    public override string TransformationName => "Water Hazard";
    public override int TransformationBuffId => ModContent.BuffType<WaterHazard_Buff>();
    protected override string BasicDescription => "A simple armored aquatic base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.GoldHelmet;
    protected override int BodySlot => ArmorIDs.Body.GoldChainmail;
    protected override int LegSlot => ArmorIDs.Legs.GoldGreaves;
}
