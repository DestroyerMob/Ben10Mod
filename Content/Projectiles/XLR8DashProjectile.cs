using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Ben10Mod.Content.Projectiles;

public class XLR8DashProjectile : RathPounceProjectile {
    protected override int HitDebuffType => 0;
    protected override int HitDebuffDuration => 0;
    protected override Color OuterColor => new(16, 24, 42, 220);
    protected override Color InnerColor => new(120, 220, 255, 180);
    protected override int TrailDustType => DustID.BlueCrystalShard;
    protected override Color TrailDustColor => new(95, 190, 255);
    protected override int PrimaryImpactDustType => DustID.BlueCrystalShard;
    protected override int SecondaryImpactDustType => DustID.BlueCrystalShard;
    protected override Color ImpactDustColor => new(145, 225, 255);
}
