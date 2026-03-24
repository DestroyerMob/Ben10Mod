using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Upgrade;

public class UpgradeTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Upgrade";
    public override string TransformationName => "Upgrade";
    public override int TransformationBuffId => ModContent.BuffType<Upgrade_Buff>();
    protected override string CostumeItemName => "BuzzShock";
    protected override string BasicDescription => "A simple techno-organic base-form implementation with a basic projectile primary attack.";
}
