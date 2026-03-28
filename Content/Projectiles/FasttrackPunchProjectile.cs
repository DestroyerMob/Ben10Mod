using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FasttrackPunchProjectile : PunchProjectile {
    private float MomentumRatio => MathHelper.Clamp(Projectile.ai[1], 0f, 1f);
    private bool HighMomentum => MomentumRatio >= 0.65f;

    protected override Color Background => Color.Lerp(new Color(10, 14, 18, 230), new Color(14, 24, 30, 240), MomentumRatio);
    protected override Color Foreground => Color.Lerp(new Color(105, 220, 190, 190), new Color(185, 255, 230, 210), MomentumRatio);
    protected override int SpawnDustType => DustID.GreenFairy;
    protected override Color SpawnDustColor => Color.Lerp(new Color(110, 225, 195), new Color(170, 255, 225), MomentumRatio);
    protected override int TrailDustType => DustID.GemEmerald;
    protected override Color TrailDustColor => Color.Lerp(new Color(95, 220, 180), new Color(150, 255, 220), MomentumRatio);
    protected override Vector3 LightEmission => Vector3.Lerp(new Vector3(0.07f, 0.34f, 0.24f), new Vector3(0.12f, 0.58f, 0.38f), MomentumRatio);
    protected override int ImpactDustType => DustID.GemEmerald;
    protected override Color ImpactDustColor => Color.Lerp(new Color(120, 235, 195), new Color(170, 255, 225), MomentumRatio);
    protected override int SpawnDustBurstCount => HighMomentum ? 8 : 5;
    protected override int TrailDustChance => HighMomentum ? 3 : 5;
    protected override int ImpactDustBurstCount => HighMomentum ? 10 : 7;

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    protected override Vector2 GetShoulderOffset(Player owner, Vector2 direction, float scale) {
        return new Vector2(owner.direction * 10f * scale, -3f * scale);
    }

    protected override float GetExtension(float progress, float scale) {
        float extensionCurve = progress < 0.34f ? progress / 0.34f : 1f - (progress - 0.34f) / 0.66f * 0.5f;
        float maxExtension = MathHelper.Lerp(35f, 43f, MomentumRatio);
        return MathHelper.Lerp(13f, maxExtension * scale, MathHelper.Clamp(extensionCurve, 0f, 1f));
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);
        if (HighMomentum)
            target.AddBuff(BuffID.BrokenArmor, 90);
        target.netUpdate = true;
    }
}
