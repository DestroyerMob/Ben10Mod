using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Ben10Mod.Content.Projectiles;

public class XLR8DashProjectile : RathPounceProjectile {
    protected override float DashSpeed => 18f;
    protected override float DashLift => -0.4f;
    protected override float ForwardOffset => 30f;
    protected override int HitDebuffType => 0;
    protected override int HitDebuffDuration => 0;
    protected override Color OuterColor => new(16, 24, 42, 220);
    protected override Color InnerColor => new(120, 220, 255, 180);
    protected override int TrailDustType => DustID.BlueCrystalShard;
    protected override Color TrailDustColor => new(95, 190, 255);
    protected override int PrimaryImpactDustType => DustID.BlueCrystalShard;
    protected override int SecondaryImpactDustType => DustID.BlueCrystalShard;
    protected override Color ImpactDustColor => new(145, 225, 255);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead) {
            Vector2 dashDirection = Projectile.rotation.ToRotationVector2();
            owner.velocity = -dashDirection * 6.5f + new Vector2(0f, -0.9f);
            owner.immune = true;
            owner.immuneNoBlink = true;
            owner.immuneTime = System.Math.Max(owner.immuneTime, 12);
            owner.noKnockback = false;
        }

        Projectile.Kill();
    }
}
