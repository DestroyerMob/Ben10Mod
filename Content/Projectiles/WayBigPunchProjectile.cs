using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.Projectiles;

public class WayBigPunchProjectile : PunchProjectile {
    protected override Color Foreground => new(220, 255, 245, 220);
    protected override Color Background => new(48, 182, 210, 235);
    protected override int SpawnDustType => Terraria.ID.DustID.GemSapphire;
    protected override Color SpawnDustColor => new(170, 245, 255);
    protected override int TrailDustType => Terraria.ID.DustID.MartianSaucerSpark;
    protected override Color TrailDustColor => new(145, 255, 250);
    protected override Vector3 LightEmission => new(0.18f, 0.7f, 0.8f);
    protected override int ImpactDustType => Terraria.ID.DustID.GemDiamond;
    protected override Color ImpactDustColor => new(210, 255, 255);

    protected override Vector2 GetPunchAnchor(Player owner, Vector2 direction, float scale) {
        float horizontalOffset = 18f * scale;
        float shoulderHeight = owner.height * 0.62f;
        return owner.Bottom + new Vector2(direction.X * horizontalOffset, -shoulderHeight);
    }

    protected override float GetExtension(float progress, float scale) {
        float extension = base.GetExtension(progress, scale);
        return extension + 8f * scale;
    }
}
