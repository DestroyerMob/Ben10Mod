using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

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
    protected override int SpawnDustBurstCount => Surged ? 30 : 20;
    protected override int TrailDustChance => Surged ? 1 : 2;
    protected override int ImpactDustBurstCount => Surged ? 34 : 24;

    protected override Vector2 GetPunchAnchor(Player owner, Vector2 direction, float scale) {
        float horizontalOffset = 18f * scale;
        float shoulderHeight = owner.height * 0.62f;
        return owner.Bottom + new Vector2(direction.X * horizontalOffset, -shoulderHeight);
    }

    protected override float GetExtension(float progress, float scale) {
        float extension = base.GetExtension(progress, scale);
        return extension + (Surged ? 13f : 8f) * scale;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        target.AddBuff(BuffID.BrokenArmor, Surged ? 210 : 135);
        if (Surged)
            target.AddBuff(BuffID.Slow, 90);

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(Projectile.spriteDirection, 0f));
        target.velocity.X += direction.X * (Surged ? 5.4f : 3.6f);
        if (!target.noGravity)
            target.velocity.Y -= Surged ? 3.2f : 2.2f;
    }

    private bool Surged => Projectile.ai[1] > 0.5f;
}
