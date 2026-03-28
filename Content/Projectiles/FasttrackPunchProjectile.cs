using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FasttrackPunchProjectile : PunchProjectile {
    private bool Empowered => Projectile.ai[1] >= 0.5f;

    protected override Color Background => Empowered ? new(14, 20, 26, 240) : new(10, 14, 18, 230);
    protected override Color Foreground => Empowered ? new(165, 255, 220, 205) : new(105, 220, 190, 190);
    protected override int SpawnDustType => DustID.GreenFairy;
    protected override Color SpawnDustColor => Empowered ? new(170, 255, 225) : new(110, 225, 195);
    protected override int TrailDustType => DustID.GemEmerald;
    protected override Color TrailDustColor => Empowered ? new(150, 255, 220) : new(95, 220, 180);
    protected override Vector3 LightEmission => Empowered ? new(0.12f, 0.58f, 0.38f) : new(0.07f, 0.34f, 0.24f);
    protected override int ImpactDustType => DustID.GemEmerald;
    protected override Color ImpactDustColor => Empowered ? new(170, 255, 225) : new(120, 235, 195);
    protected override int SpawnDustBurstCount => Empowered ? 8 : 5;
    protected override int TrailDustChance => Empowered ? 3 : 5;
    protected override int ImpactDustBurstCount => Empowered ? 10 : 7;

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    protected override Vector2 GetShoulderOffset(Player owner, Vector2 direction, float scale) {
        return new Vector2(owner.direction * 10f * scale, -3f * scale);
    }

    protected override float GetExtension(float progress, float scale) {
        float extensionCurve = progress < 0.34f ? progress / 0.34f : 1f - (progress - 0.34f) / 0.66f * 0.5f;
        float maxExtension = Empowered ? 40f : 35f;
        return MathHelper.Lerp(13f, maxExtension * scale, MathHelper.Clamp(extensionCurve, 0f, 1f));
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), Empowered ? 105 : 75);
        if (Empowered)
            target.AddBuff(BuffID.BrokenArmor, 90);
        target.netUpdate = true;
    }
}
