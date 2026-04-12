using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles.UltimateAttacks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EyeGuyUltimateBeam : ChannelBeamUltimateProjectile {
    private bool Focused => Projectile.ai[0] >= 0.5f;

    protected override float MaxLength => Focused ? 3000f : 2600f;
    protected override float BeamThickness => Focused ? 34f : 28f;
    protected override float StartOffset => 52f;
    protected override int MinEnergyToSustain => 10;
    protected override Vector2 StartScale => Focused ? new Vector2(1.75f, 1.04f) : new Vector2(1.55f, 1f);
    protected override Vector2 OuterScale => Focused ? new Vector2(2.85f, 1.08f) : new Vector2(2.55f, 1f);
    protected override Vector2 MidScale => Focused ? new Vector2(2.02f, 1.02f) : new Vector2(1.85f, 1f);
    protected override Vector2 InnerScale => Focused ? new Vector2(1.36f, 0.98f) : new Vector2(1.25f, 1f);
    protected override Color BeamColor => Focused ? new Color(110, 255, 175) : new Color(60, 255, 140);
    protected override Color BeamHighlightColor => Focused ? new Color(245, 255, 235) : Color.White;
    protected override int EndDustType => Focused ? DustID.GreenFairy : DustID.GreenTorch;
    protected override int EndDustCount => Focused ? 7 : 5;
    protected override float LightR => Focused ? 0.3f : 0.2f;
    protected override float LightG => Focused ? 1.9f : 1.6f;
    protected override float LightB => Focused ? 0.72f : 0.6f;

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = Focused ? 8 : 10;
    }

    protected override Vector2 GetBeamStart(Player owner, Vector2 direction) {
        return owner.MountedCenter + new Vector2(owner.direction * 12f, -12f) + direction * 14f;
    }

    protected override void OnBeamUpdated(Player owner, OmnitrixPlayer omp, Vector2 start, Vector2 direction) {
        if (!Main.rand.NextBool(2))
            return;

        Vector2 end = start + direction * BeamHitLength;
        Color dustColor = Focused ? new Color(190, 255, 185) : new Color(125, 255, 160);
        Dust startDust = Dust.NewDustPerfect(start + Main.rand.NextVector2Circular(10f, 10f), EndDustType,
            Main.rand.NextVector2Circular(0.8f, 0.8f), 100, dustColor, Main.rand.NextFloat(0.95f, 1.25f));
        startDust.noGravity = true;

        Dust endDust = Dust.NewDustPerfect(end + Main.rand.NextVector2Circular(20f, 20f), EndDustType,
            Main.rand.NextVector2Circular(1.8f, 1.8f), 100, Color.Lerp(dustColor, Color.White, 0.35f),
            Main.rand.NextFloat(1.05f, 1.45f));
        endDust.noGravity = true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Focused)
            modifiers.SourceDamage *= 1.14f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.BrokenArmor, Focused ? 210 : 150);
    }
}
