using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneLanceEchoProjectile : ModProjectile {
    private const int DelayTicks = 10;
    private const int LifetimeTicks = 20;

    private float PowerRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private float BeamLength => Math.Max(80f, Projectile.ai[1]);
    private bool IsDetonating => LifetimeTicks - Projectile.timeLeft >= DelayTicks;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool? CanDamage() => IsDetonating;

    public override void AI() {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(PowerRatio * 2.4f + Projectile.timeLeft * 0.05f, 1.08f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * (IsDetonating ? 0.64f : 0.3f));

        if (!Main.dedServ && Main.rand.NextBool(IsDetonating ? 1 : 2)) {
            float beamProgress = Main.rand.NextFloat();
            Vector2 dustPosition = Projectile.Center + direction * MathHelper.Lerp(-BeamLength * 0.5f, BeamLength * 0.5f, beamProgress);
            Dust dust = Dust.NewDustPerfect(dustPosition + Main.rand.NextVector2Circular(5f, 5f), DustID.WhiteTorch,
                Main.rand.NextVector2Circular(1.6f, 1.6f), 95, prismColor, Main.rand.NextFloat(0.85f, 1.15f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (!IsDetonating)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 start = Projectile.Center - direction * BeamLength * 0.5f;
        Vector2 end = Projectile.Center + direction * BeamLength * 0.5f;
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end,
            20f, ref collisionPoint);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.SourceDamage *= 1f + PowerRatio * 0.16f;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 start = Projectile.Center - direction * BeamLength * 0.5f;
        Vector2 end = Projectile.Center + direction * BeamLength * 0.5f;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(PowerRatio * 2.1f + 0.22f, 1.04f) * (IsDetonating ? 0.62f : 0.24f);
        Color inner = ChromaStonePrismHelper.GetSpectrumColor(PowerRatio * 1.4f + 0.84f, 1.12f) * (IsDetonating ? 0.92f : 0.42f);
        Color core = new Color(245, 250, 255, 230) * (IsDetonating ? 0.92f : 0.42f);

        ChromaStonePrismHelper.DrawBeam(pixel, start - Main.screenPosition, end - Main.screenPosition, 20f, outer);
        ChromaStonePrismHelper.DrawBeam(pixel, start - Main.screenPosition, end - Main.screenPosition, 11f, inner);
        ChromaStonePrismHelper.DrawBeam(pixel, start - Main.screenPosition, end - Main.screenPosition, 4f, core);
        return false;
    }
}
