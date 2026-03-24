using Ben10Mod.Content.Buffs.Transformations;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Astrodactyl;

public class AstrodactylTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Astrodactyl";
    public override string TransformationName => "Astrodactyl";
    public override int TransformationBuffId => ModContent.BuffType<Astrodactyl_Buff>();
    protected override string CostumeItemName => "StinkFly";
    protected override string BasicDescription => "A simple aerial base-form implementation with a basic projectile primary attack.";
}
