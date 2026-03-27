using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles.UltimateAttacks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArctiguanaUltimateBeamProjectile : ChannelBeamUltimateProjectile {
    protected override float MaxLength => 2400f;
    protected override float BeamThickness => 34f;
    protected override float StartOffset => 24f;
    protected override int MinEnergyToSustain => 8;
    protected override Vector2 StartScale => new(1.7f, 1f);
    protected override Vector2 OuterScale => new(2.65f, 1.05f);
    protected override Vector2 MidScale => new(1.9f, 1f);
    protected override Vector2 InnerScale => new(1.25f, 0.94f);
    protected override Color BeamColor => new(105, 210, 255);
    protected override Color BeamHighlightColor => new(235, 250, 255);
    protected override int EndDustType => DustID.IceTorch;
    protected override int EndDustCount => 6;
    protected override float LightR => 0.2f;
    protected override float LightG => 0.8f;
    protected override float LightB => 1.2f;

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    protected override Vector2 GetBeamStart(Player owner, Vector2 direction) {
        return owner.MountedCenter + new Vector2(owner.direction * 10f, -12f) + direction * 12f;
    }

    protected override void OnBeamUpdated(Player owner, OmnitrixPlayer omp, Vector2 start, Vector2 direction) {
        if (!Main.rand.NextBool(2))
            return;

        Vector2 end = start + direction * BeamHitLength;
        Dust startDust = Dust.NewDustPerfect(start + Main.rand.NextVector2Circular(10f, 10f),
            Main.rand.NextBool(3) ? DustID.IceTorch : DustID.Frost, Main.rand.NextVector2Circular(0.8f, 0.8f), 110,
            new Color(185, 235, 255), Main.rand.NextFloat(1f, 1.28f));
        startDust.noGravity = true;

        Dust endDust = Dust.NewDustPerfect(end + Main.rand.NextVector2Circular(18f, 18f),
            Main.rand.NextBool(3) ? DustID.IceTorch : DustID.SnowflakeIce, Main.rand.NextVector2Circular(1.8f, 1.8f), 110,
            new Color(220, 250, 255), Main.rand.NextFloat(1.05f, 1.4f));
        endDust.noGravity = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Frostburn2, 240);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 240);
        target.AddBuff(ModContent.BuffType<EnemyFrozen>(), 32);
        target.netUpdate = true;
    }
}
