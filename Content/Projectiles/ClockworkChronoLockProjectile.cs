using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles.UltimateAttacks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ClockworkChronoLockProjectile : BurstFieldUltimateProjectile {
    protected override float RadiusGrowthPerTick => 18f;
    protected override float MaxRadius => 220f;
    protected override int LifetimeTicks => 36;
    protected override int DustType => DustID.GemTopaz;
    protected override Color FieldColor => new(242, 220, 125);
    protected override float LightStrength => 0.55f;

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    protected override void UpdateField(Player owner) {
        owner.velocity *= 0.92f;
        owner.noKnockback = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemyFrozen>(), 45);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 4 * 60);
        target.netUpdate = true;

        for (int i = 0; i < 10; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.6f, 2.6f);
            Dust dust = Dust.NewDustPerfect(target.Center, i % 2 == 0 ? DustID.GemTopaz : DustID.YellowTorch,
                velocity, 100, new Color(245, 225, 140), Main.rand.NextFloat(1f, 1.24f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }
}
