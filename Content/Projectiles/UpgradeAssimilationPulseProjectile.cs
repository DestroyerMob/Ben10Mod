using System;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UpgradeAssimilationPulseProjectile : ModProjectile {
    private const int PulseLifetimeTicks = 24;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    private UpgradeTechProfile Profile => (UpgradeTechProfile)Utils.Clamp((int)Math.Round(Projectile.ai[0]), 0, 4);
    private bool SuccessfulScan => Projectile.ai[1] > 0.5f;

    private float CurrentRadius {
        get {
            float progress = 1f - Projectile.timeLeft / (float)PulseLifetimeTicks;
            float baseRadius = SuccessfulScan ? 116f : 84f;
            return MathHelper.Lerp(baseRadius * 0.5f, baseRadius, progress);
        }
    }

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = PulseLifetimeTicks;
        Projectile.hide = true;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        Color profileColor = UpgradeTransformation.GetTechColor(Profile);
        Lighting.AddLight(Projectile.Center, profileColor.ToVector3() * (SuccessfulScan ? 0.0042f : 0.0028f));

        if (!Main.dedServ && Main.rand.NextBool()) {
            Vector2 offset = Main.rand.NextVector2CircularEdge(CurrentRadius, CurrentRadius) * Main.rand.NextFloat(0.45f, 1f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? DustID.Electric : DustID.GreenTorch,
                offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.5f, 1.5f),
                95, profileColor, Main.rand.NextFloat(0.85f, 1.16f));
            dust.noGravity = true;
        }
    }
}
