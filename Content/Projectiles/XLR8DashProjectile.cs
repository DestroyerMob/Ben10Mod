using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Ben10Mod.Content.Projectiles;

public class XLR8DashProjectile : RathPounceProjectile {
    private const int MaxPierces = 4;
    private const int PotisMaxPierces = 7;
    private const int PotisDashLifetime = 22;

    private bool PotisInfused => Projectile.ai[0] >= 0.5f;
    private float DashPowerRatio => MathHelper.Clamp(Projectile.ai[1], 0f, 1f);
    private int CurrentMaxPierces => PotisInfused ? PotisMaxPierces : MaxPierces;

    protected override int DashWidth => (int)Math.Round((PotisInfused ? 86 : 62) * (1f + DashPowerRatio * 0.12f));
    protected override int DashHeight => (int)Math.Round((PotisInfused ? 50 : 40) * (1f + DashPowerRatio * 0.08f));
    protected override float DashSpeed => (PotisInfused ? 23.5f : 20f) + DashPowerRatio * (PotisInfused ? 3.4f : 2.6f);
    protected override float DashLift => PotisInfused ? -0.08f : -0.2f;
    protected override float ForwardOffset => (PotisInfused ? 38f : 30f) + DashPowerRatio * 6f;
    protected override int HitDebuffType => 0;
    protected override int HitDebuffDuration => 0;
    protected override Color OuterColor => Color.Lerp(
        PotisInfused ? new Color(34, 110, 130, 230) : new Color(16, 24, 42, 220),
        new Color(152, 244, 255, 235),
        DashPowerRatio * 0.35f);
    protected override Color InnerColor => Color.Lerp(
        PotisInfused ? new Color(210, 255, 246, 210) : new Color(120, 220, 255, 180),
        new Color(255, 252, 178, 220),
        DashPowerRatio * 0.28f);
    protected override int TrailDustType => DustID.BlueCrystalShard;
    protected override Color TrailDustColor => PotisInfused ? new Color(150, 250, 255) : new Color(95, 190, 255);
    protected override int PrimaryImpactDustType => DustID.BlueCrystalShard;
    protected override int SecondaryImpactDustType => PotisInfused ? DustID.WhiteTorch : DustID.BlueCrystalShard;
    protected override Color ImpactDustColor => PotisInfused ? new Color(188, 255, 238) : new Color(145, 225, 255);
    protected override bool UsesRathPounceImpact => false;

    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        if (PotisInfused && Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;
            Projectile.timeLeft = Math.Max(Projectile.timeLeft, PotisDashLifetime);
            Projectile.localNPCHitCooldown = 6;
        }

        base.AI();

        if (PotisInfused) {
            Lighting.AddLight(Projectile.Center, new Vector3(0.08f, 0.82f, 0.94f) * (0.28f + DashPowerRatio * 0.12f));

            if (Main.rand.NextBool(2)) {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextBool(4) ? DustID.WhiteTorch : DustID.BlueCrystalShard,
                    -Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(1.2f, 3.8f + DashPowerRatio * 2.2f),
                    115, new Color(160, 250, 255), Main.rand.NextFloat(1f, 1.36f + DashPowerRatio * 0.16f));
                dust.noGravity = true;
            }
        }
        else if (DashPowerRatio > 0.55f && Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                DustID.BlueCrystalShard,
                -Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(1.1f, 2.8f + DashPowerRatio * 1.5f),
                120, new Color(120, 220, 255), Main.rand.NextFloat(0.9f, 1.18f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float xScale = (PotisInfused ? 38f : 24f) + DashPowerRatio * (PotisInfused ? 8f : 5f);
        float yScale = (PotisInfused ? 24f : 18f) + DashPowerRatio * (PotisInfused ? 4f : 3f);

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), OuterColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(xScale, yScale), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), InnerColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f),
            new Vector2((PotisInfused ? 16f : 11f) + DashPowerRatio * 3f, (PotisInfused ? 10f : 8f) + DashPowerRatio * 2f),
            SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead) {
            owner.immune = true;
            owner.immuneNoBlink = true;
            owner.immuneTime = Math.Max(owner.immuneTime, (PotisInfused ? 16 : 12) + (int)Math.Round(DashPowerRatio * 4f));
            owner.noKnockback = true;
        }

        Projectile.localAI[0]++;
        if (Projectile.localAI[0] >= CurrentMaxPierces)
            Projectile.Kill();
    }
}
