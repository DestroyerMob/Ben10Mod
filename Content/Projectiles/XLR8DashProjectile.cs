using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Ben10Mod.Content.Projectiles;

public class XLR8DashProjectile : RathPounceProjectile {
    private const int MaxPierces = 4;

    protected override float DashSpeed => 20f;
    protected override float DashLift => -0.2f;
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

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead) {
            owner.immune = true;
            owner.immuneNoBlink = true;
            owner.immuneTime = System.Math.Max(owner.immuneTime, 12);
            owner.noKnockback = true;
        }

        Projectile.localAI[0]++;
        if (Projectile.localAI[0] >= MaxPierces)
            Projectile.Kill();
    }
}
