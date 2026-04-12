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
    private bool initialized;
    private bool echoSpawned;

    private int FacetCount => Math.Clamp((int)Math.Round(Projectile.ai[0]), 0, 3);
    private float PowerRatio => MathHelper.Clamp(Projectile.ai[1], 0f, 1f);

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
        Projectile.timeLeft = 74;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        if (!initialized) {
            initialized = true;
            Projectile.scale = 1f + FacetCount * 0.1f + PowerRatio * 0.08f;
            Projectile.penetrate = 5 + FacetCount;
            Projectile.localNPCHitCooldown = Math.Max(7, 12 - FacetCount);
        }

        float desiredSpeed = 20f + FacetCount * 1.5f + PowerRatio * 2f;
        if (Projectile.velocity.LengthSquared() < desiredSpeed * desiredSpeed)
            Projectile.velocity *= 1.01f + FacetCount * 0.002f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(PowerRatio * 2.8f + Projectile.identity * 0.09f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * (0.46f + FacetCount * 0.08f));

        if (!Main.dedServ && Main.rand.NextBool(FacetCount >= 2 ? 1 : 2)) {
            Vector2 dustOffset = Main.rand.NextVector2Circular(7f, 7f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.WhiteTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.03f, 0.1f), 95, prismColor,
                Main.rand.NextFloat(0.95f, 1.28f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 lineStart = Projectile.Center - direction * (24f + FacetCount * 4f);
        Vector2 lineEnd = Projectile.Center + direction * MathHelper.Lerp(52f, 76f, FacetCount / 3f);
        float collisionPoint = 0f;
        float width = MathHelper.Lerp(16f, 24f, FacetCount / 3f) * Projectile.scale;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
            width, ref collisionPoint);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.SourceDamage *= 1f + FacetCount * 0.18f + PowerRatio * 0.08f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.owner != Main.myPlayer)
            return;

        Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);

        if (FacetCount >= 2) {
            int splinterCount = 3 + FacetCount;
            int splinterDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.36f));
            for (int i = 0; i < splinterCount; i++) {
                float spread = splinterCount <= 1 ? 0f : MathHelper.Lerp(-0.5f, 0.5f, i / (float)(splinterCount - 1));
                Vector2 velocity = baseDirection.RotatedBy(spread) * Main.rand.NextFloat(13f, 17f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, velocity,
                    ModContent.ProjectileType<ChromaStoneProjectile>(), splinterDamage, Projectile.knockBack * 0.72f,
                    Projectile.owner, ChromaStoneProjectile.ModeBurstShard, PowerRatio);
            }
        }

        if (FacetCount >= 3 && !echoSpawned) {
            echoSpawned = true;
            int echoDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.48f));
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, baseDirection,
                ModContent.ProjectileType<ChromaStoneLanceEchoProjectile>(), echoDamage, Projectile.knockBack * 0.8f,
                Projectile.owner, PowerRatio, 124f);
        }

        target.velocity = Vector2.Lerp(target.velocity,
            (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * (7.5f + FacetCount * 1.2f),
            target.boss ? 0.14f : 0.32f);
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
                new Vector2(MathHelper.Lerp(56f + FacetCount * 6f, 18f, progress), MathHelper.Lerp(10f, 3f, progress)) *
                Projectile.scale, trailColor);
        }

        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.16f + PowerRatio * 1.6f, 1.08f) * 0.62f;
        Color middle = ChromaStonePrismHelper.GetSpectrumColor(0.5f + PowerRatio * 1.1f, 1.1f) * 0.88f;
        Color core = new Color(245, 250, 255, 235) * Projectile.Opacity;
        float widthBoost = 1f + FacetCount * 0.1f;

        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(70f, 12f) * Projectile.scale * widthBoost, outer);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + direction * 16f, rotation,
            new Vector2(30f, 18f) * Projectile.scale * widthBoost, middle);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(46f, 5.4f) * Projectile.scale * widthBoost, core);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + normal * 7f, rotation + 0.5f,
            new Vector2(18f, 3.2f) * Projectile.scale, middle * 0.72f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center - normal * 7f, rotation - 0.5f,
            new Vector2(18f, 3.2f) * Projectile.scale, middle * 0.72f);
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 16; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.22f + PowerRatio * 0.6f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                Main.rand.NextVector2Circular(3.2f, 3.2f), 90, prismColor, Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }
}
