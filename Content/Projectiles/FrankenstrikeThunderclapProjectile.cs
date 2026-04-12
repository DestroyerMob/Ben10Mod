using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Frankenstrike;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FrankenstrikeThunderclapProjectile : ModProjectile {
    private const int LifetimeTicks = 22;
    private const float StartRadius = 20f;

    private float RadiusScale => Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0];
    private bool Empowered => Projectile.ai[1] >= 0.5f;

    private float CurrentRadius {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => "Terraria/Images/Projectile_0";

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
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.22f, Volume = 0.68f }, Projectile.Center);
        }

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - (float)Math.Pow(1f - progress, 2f);
        float endRadius = (Empowered ? 122f : 98f) * RadiusScale;
        CurrentRadius = MathHelper.Lerp(StartRadius, endRadius, easedProgress);
        Lighting.AddLight(Projectile.Center, Empowered ? new Vector3(0.34f, 0.56f, 1f) : new Vector3(0.24f, 0.44f, 0.86f));

        if (Main.rand.NextBool(2)) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 direction = angle.ToRotationVector2();
            Dust dust = Dust.NewDustPerfect(Projectile.Center + direction * Main.rand.NextFloat(CurrentRadius * 0.3f, CurrentRadius),
                Main.rand.NextBool(3) ? DustID.Electric : DustID.BlueTorch,
                direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1.8f, 1.8f), 110,
                Empowered ? new Color(185, 228, 255) : new Color(165, 215, 255),
                Main.rand.NextFloat(0.95f, Empowered ? 1.28f : 1.16f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        float rotation = Projectile.timeLeft * 0.06f;
        float opacity = Utils.GetLerpValue(0f, 0.16f, 1f - Projectile.timeLeft / (float)LifetimeTicks, true) *
            Utils.GetLerpValue(0f, 0.3f, Projectile.timeLeft / (float)LifetimeTicks, true);
        Vector2 center = Projectile.Center - Main.screenPosition;

        DrawRing(pixel, center, CurrentRadius, Empowered ? 4.8f : 4f,
            (Empowered ? new Color(135, 188, 255, 88) : new Color(105, 160, 255, 76)) * opacity, rotation);
        DrawRing(pixel, center, CurrentRadius * 0.68f, Empowered ? 3.6f : 3f,
            new Color(235, 245, 255, 132) * opacity, -rotation * 1.2f);
        DrawRing(pixel, center, CurrentRadius * 0.44f, Empowered ? 3f : 2.4f,
            new Color(255, 255, 255, 160) * opacity, rotation * 1.4f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return;

        Vector2 push = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * (Empowered ? 7.2f : 5.6f);
        target.velocity = Vector2.Lerp(target.velocity, push + new Vector2(0f, -1.6f), 0.6f);
        FrankenstrikeTransformation.ApplyConductiveHit(owner, target, Empowered ? 2 : 1, 180);
        target.netUpdate = true;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotation) {
        const int Segments = 22;
        for (int i = 0; i < Segments; i++) {
            float angle = rotation + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.5f), SpriteEffects.None, 0);
        }
    }
}
