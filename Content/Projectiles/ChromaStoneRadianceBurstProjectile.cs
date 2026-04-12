using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneRadianceBurstProjectile : ModProjectile {
    private const int LifetimeTicks = 20;
    private const float StartRadius = 20f;
    private const float BaseMaxRadius = 132f;

    private float RadianceRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private bool CrystalGuard => Projectile.ai[1] >= 0.5f;

    private float CurrentRadius {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    private float PreviousRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        if (Projectile.timeLeft == LifetimeTicks)
            SpawnIgnition();

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.7f);
        float maxRadius = BaseMaxRadius + RadianceRatio * 84f + (CrystalGuard ? 20f : 0f);
        float radius = MathHelper.Lerp(StartRadius, maxRadius, easedProgress);
        SpawnBurstDust(radius, PreviousRadius);
        PreviousRadius = radius;
        CurrentRadius = radius;

        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(0.45f + RadianceRatio * 1.6f, 1.1f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * 0.5f);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.SourceDamage *= 1f + RadianceRatio * 0.28f + (CrystalGuard ? 0.08f : 0f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 blastDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
        float blastForce = MathHelper.Lerp(7.5f, 12.5f, RadianceRatio);
        target.velocity = Vector2.Lerp(target.velocity, blastDirection * blastForce, target.boss ? 0.12f : 0.42f);
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float pulse = 1f - MathF.Abs(0.5f - progress) * 1.3f;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.12f + progress * 2.4f, 1.06f) * (0.42f + pulse * 0.18f);
        Color inner = ChromaStonePrismHelper.GetSpectrumColor(0.75f + progress * 3f, 1.08f) * (0.5f + pulse * 0.28f);

        ChromaStonePrismHelper.DrawRing(pixel, center, CurrentRadius * 0.86f, 4.2f, outer, progress * 2.2f, 24);
        ChromaStonePrismHelper.DrawRing(pixel, center, CurrentRadius * 0.48f, 3f, inner, -progress * 2.8f, 18);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, progress * MathHelper.TwoPi,
            new Vector2(CurrentRadius * 0.64f, 5.6f), outer * 0.75f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, progress * MathHelper.TwoPi + MathHelper.PiOver2,
            new Vector2(CurrentRadius * 0.64f, 5.6f), outer * 0.75f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(245, 250, 255, 235) * (0.68f + pulse * 0.2f), 0f,
            Vector2.One * 0.5f, new Vector2(16f, 16f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 18; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.18f + 0.22f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), DustID.WhiteTorch,
                Main.rand.NextVector2Circular(4.2f, 4.2f), 95, prismColor, Main.rand.NextFloat(1f, 1.4f));
            dust.noGravity = true;
        }
    }

    private void SpawnIgnition() {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.28f, Volume = 0.78f }, Projectile.Center);
        for (int i = 0; i < 20; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.25f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.WhiteTorch,
                Main.rand.NextVector2Circular(3.4f, 3.4f), 95, prismColor, Main.rand.NextFloat(1.05f, 1.5f));
            dust.noGravity = true;
        }
    }

    private void SpawnBurstDust(float radius, float previousRadius) {
        if (Main.dedServ)
            return;

        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - 22f));
        int points = Math.Max(16, (int)Math.Round(radius / 7f));
        float rotation = Main.GlobalTimeWrappedHourly * 2.6f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Dust dust = Dust.NewDustPerfect(Projectile.Center + direction * shellOffset, DustID.WhiteTorch,
                direction * Main.rand.NextFloat(0.8f, 3.4f), 95,
                ChromaStonePrismHelper.GetSpectrumColor(i * 0.17f + shellOffset * 0.01f),
                Main.rand.NextFloat(0.9f, 1.28f));
            dust.noGravity = true;
        }
    }
}
