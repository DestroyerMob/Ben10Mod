using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Ben10Mod.Content.Projectiles;

public class XLR8PunchProjectile : PunchProjectile {
    protected override Color Background => new(8, 8, 12, 235);
    protected override Color Foreground => new(105, 195, 255, 200);
    protected override int SpawnDustType => DustID.BlueCrystalShard;
    protected override Color SpawnDustColor => new(85, 170, 255);
    protected override int TrailDustType => DustID.BlueCrystalShard;
    protected override Color TrailDustColor => new(110, 205, 255);
    protected override Vector3 LightEmission => new(0.08f, 0.28f, 0.75f);
    protected override int ImpactDustType => DustID.BlueCrystalShard;
    protected override Color ImpactDustColor => new(135, 210, 255);
    protected override int SpawnDustBurstCount => 4;
    protected override int TrailDustChance => 5;
    protected override int ImpactDustBurstCount => 7;
}
