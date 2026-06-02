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
    private int CurrentMaxPierces => PotisInfused ? PotisMaxPierces : MaxPierces;

    protected override int DashWidth => PotisInfused ? 86 : 62;
    protected override int DashHeight => PotisInfused ? 50 : 40;
    protected override float DashSpeed => PotisInfused ? 23.5f : 20f;
    protected override float DashLift => PotisInfused ? -0.08f : -0.2f;
    protected override float ForwardOffset => PotisInfused ? 38f : 30f;
    protected override int HitDebuffType => 0;
    protected override int HitDebuffDuration => 0;
    protected override Color OuterColor => PotisInfused ? new Color(34, 110, 130, 230) : new Color(16, 24, 42, 220);
    protected override Color InnerColor => PotisInfused ? new Color(210, 255, 246, 210) : new Color(120, 220, 255, 180);
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
            Lighting.AddLight(Projectile.Center, new Vector3(0.08f, 0.82f, 0.94f) * 0.28f);

            if (Main.rand.NextBool(2)) {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextBool(4) ? DustID.WhiteTorch : DustID.BlueCrystalShard,
                    -Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(1.2f, 3.8f),
                    115, new Color(160, 250, 255), Main.rand.NextFloat(1f, 1.36f));
                dust.noGravity = true;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float xScale = PotisInfused ? 38f : 24f;
        float yScale = PotisInfused ? 24f : 18f;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), OuterColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(xScale, yScale), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), InnerColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(PotisInfused ? 16f : 11f, PotisInfused ? 10f : 8f),
            SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead) {
            owner.immune = true;
            owner.immuneNoBlink = true;
            owner.immuneTime = Math.Max(owner.immuneTime, PotisInfused ? 16 : 12);
            owner.noKnockback = true;
        }

        Projectile.localAI[0]++;
        if (Projectile.localAI[0] >= CurrentMaxPierces)
            Projectile.Kill();
    }
}
