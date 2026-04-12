using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneLanceProjectile : ModProjectile {
    private float RadianceRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private bool CrystalGuard => Projectile.ai[1] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 5;
        Projectile.timeLeft = 64;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.scale = 1f + RadianceRatio * 0.16f + (CrystalGuard ? 0.1f : 0f);
            Projectile.penetrate = Math.Max(Projectile.penetrate, CrystalGuard ? 6 : 5);
        }

        float desiredSpeed = 18f + RadianceRatio * 4f + (CrystalGuard ? 1.5f : 0f);
        if (Projectile.velocity.LengthSquared() < desiredSpeed * desiredSpeed)
            Projectile.velocity *= CrystalGuard ? 1.014f : 1.01f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(RadianceRatio * 2.8f + Projectile.identity * 0.09f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * (0.46f + RadianceRatio * 0.22f));

        if (!Main.dedServ && Main.rand.NextBool(CrystalGuard ? 1 : 2)) {
            Vector2 dustOffset = Main.rand.NextVector2Circular(7f, 7f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.WhiteTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.03f, 0.1f), 95, prismColor,
                Main.rand.NextFloat(0.95f, 1.28f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 lineStart = Projectile.Center - direction * 24f;
        Vector2 lineEnd = Projectile.Center + direction * MathHelper.Lerp(48f, 64f, RadianceRatio);
        float collisionPoint = 0f;
        float width = MathHelper.Lerp(16f, 22f, RadianceRatio) * Projectile.scale;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
            width, ref collisionPoint);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.SourceDamage *= 1f + RadianceRatio * 0.18f + (CrystalGuard ? 0.08f : 0f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.owner != Main.myPlayer)
            return;

        int splinterCount = (CrystalGuard ? 4 : 3) + (RadianceRatio >= 0.72f ? 1 : 0);
        float baseRotation = Projectile.velocity.ToRotation();
        int splinterDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.42f));

        for (int i = 0; i < splinterCount; i++) {
            float spread = splinterCount <= 1 ? 0f : MathHelper.Lerp(-0.46f, 0.46f, i / (float)(splinterCount - 1));
            Vector2 velocity = (baseRotation + spread).ToRotationVector2() * Main.rand.NextFloat(13f, 16.5f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, velocity,
                ModContent.ProjectileType<ChromaStoneProjectile>(), splinterDamage, Projectile.knockBack * 0.75f,
                Projectile.owner, MathHelper.Clamp(RadianceRatio * 0.85f + 0.12f, 0f, 1f), CrystalGuard ? 1f : 0f);
        }

        target.velocity = Vector2.Lerp(target.velocity,
            (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 7.5f, target.boss ? 0.14f : 0.32f);
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float rotation = direction.ToRotation();

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float progress = i / (float)Projectile.oldPos.Length;
            Vector2 trailCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Color trailColor = ChromaStonePrismHelper.GetSpectrumColor(progress * 2.4f + i * 0.11f) *
                ((1f - progress) * 0.35f);
            ChromaStonePrismHelper.DrawRotatedRect(pixel, trailCenter, rotation,
                new Vector2(MathHelper.Lerp(56f, 18f, progress), MathHelper.Lerp(10f, 3f, progress)) * Projectile.scale,
                trailColor);
        }

        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.16f + RadianceRatio * 1.6f, 1.08f) * 0.62f;
        Color middle = ChromaStonePrismHelper.GetSpectrumColor(0.5f + RadianceRatio * 1.1f, 1.1f) * 0.88f;
        Color core = new Color(245, 250, 255, 235) * Projectile.Opacity;

        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(70f, 12f) * Projectile.scale, outer);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + direction * 16f, rotation,
            new Vector2(30f, 18f) * Projectile.scale, middle);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(46f, 5.4f) * Projectile.scale, core);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + normal * 7f, rotation + 0.5f,
            new Vector2(18f, 3.2f) * Projectile.scale, middle * 0.72f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center - normal * 7f, rotation - 0.5f,
            new Vector2(18f, 3.2f) * Projectile.scale, middle * 0.72f);
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 14; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.22f + RadianceRatio * 0.6f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                Main.rand.NextVector2Circular(3.2f, 3.2f), 90, prismColor, Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }
}
