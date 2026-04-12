using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.EyeGuy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EyeGuyLaserbeam : ModProjectile, IMagistrataOutlineProvider {
    public const int FlagWatcherEcho = 1 << 0;
    public const int FlagOmniGaze = 1 << 1;
    public const int FlagOverload = 1 << 2;
    public const int FlagFinalPulse = 1 << 3;
    public const int FlagDisableShockChain = 1 << 4;

    private EyeGuyElement Element => (EyeGuyElement)Utils.Clamp((int)Math.Round(Projectile.ai[0]), 0, 2);
    private int FlagMask => (int)Math.Round(Projectile.ai[1]);
    private bool WatcherEcho => (FlagMask & FlagWatcherEcho) != 0;
    private bool OmniGaze => (FlagMask & FlagOmniGaze) != 0;
    private bool Overload => (FlagMask & FlagOverload) != 0;
    private bool FinalPulse => (FlagMask & FlagFinalPulse) != 0;

    public override void SetDefaults() {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 3;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 2;
        Projectile.timeLeft = 90;
        Projectile.alpha = 12;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 9;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            ApplyVariantDefaults();
        }

        if (OmniGaze || FinalPulse) {
            Projectile.velocity *= FinalPulse ? 0.982f : 0.988f;
        }
        else if (Projectile.velocity.LengthSquared() < (Overload ? 1600f : 1369f)) {
            Projectile.velocity *= Overload ? 1.016f : 1.011f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, GetLightColor());

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, GetDustType(),
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.12f), 100, GetDustColor(),
                Main.rand.NextFloat(0.88f, Overload ? 1.28f : 1.12f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() * 0.5f;
        float rotation = Projectile.rotation + MathHelper.PiOver2;
        float outerScale = Projectile.scale * (OmniGaze || FinalPulse ? 1.16f : 1.02f);
        float innerScale = Projectile.scale * (WatcherEcho ? 0.6f : 0.68f);

        Main.EntitySpriteDraw(texture, drawPosition, null, GetOuterColor() * Projectile.Opacity, rotation, origin,
            outerScale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, drawPosition, null, GetInnerColor() * Projectile.Opacity, rotation, origin,
            innerScale, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        EyeGuyTransformation.ResolveElementalHit(Projectile, target, damageDone, Element, FlagMask);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 7; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, GetDustType(),
                Main.rand.NextVector2Circular(2.2f, 2.2f), 95, GetDustColor(),
                Main.rand.NextFloat(0.9f, Overload ? 1.3f : 1.15f));
            dust.noGravity = true;
        }
    }

    public bool TryGetMagistrataOutlineDrawData(out MagistrataOutlineDrawData drawData) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        drawData = new MagistrataOutlineDrawData(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            texture.Size() * 0.5f,
            Projectile.rotation + MathHelper.PiOver2,
            Projectile.scale,
            SpriteEffects.None
        );
        return true;
    }

    private void ApplyVariantDefaults() {
        Projectile.scale = WatcherEcho ? 0.9f : 1f;
        Projectile.penetrate = WatcherEcho ? 2 : 3;
        Projectile.timeLeft = 90;
        Projectile.extraUpdates = OmniGaze || FinalPulse ? 1 : 2;
        Projectile.localNPCHitCooldown = OmniGaze || FinalPulse ? 14 : (WatcherEcho ? 11 : 9);
        Projectile.tileCollide = !(OmniGaze || FinalPulse);

        if (Overload)
            Projectile.scale *= 1.08f;

        if (OmniGaze) {
            Projectile.scale *= 1.16f;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 34;
            Projectile.alpha = 0;
        }

        if (FinalPulse) {
            Projectile.scale *= 1.24f;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 30;
            Projectile.alpha = 0;
        }

        if ((FlagMask & FlagDisableShockChain) != 0 && Element == EyeGuyElement.Shock) {
            Projectile.penetrate = Math.Min(Projectile.penetrate, 1);
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, 44);
        }
    }

    private Vector3 GetLightColor() {
        Color color = GetOuterColor();
        float scale = Overload ? 0.005f : 0.0038f;
        return color.ToVector3() * scale;
    }

    private int GetDustType() {
        return Element switch {
            EyeGuyElement.Fire => DustID.GoldFlame,
            EyeGuyElement.Frost => Main.rand.NextBool() ? DustID.GemDiamond : DustID.IceTorch,
            _ => Main.rand.NextBool() ? DustID.Electric : DustID.BlueTorch
        };
    }

    private Color GetDustColor() {
        return Element switch {
            EyeGuyElement.Fire => Overload ? new Color(255, 190, 140) : new Color(255, 150, 100),
            EyeGuyElement.Frost => Overload ? new Color(215, 255, 255) : new Color(155, 240, 255),
            _ => Overload ? new Color(205, 225, 255) : new Color(145, 190, 255)
        };
    }

    private Color GetOuterColor() {
        Color baseColor = Element switch {
            EyeGuyElement.Fire => new Color(255, 120, 80, 220),
            EyeGuyElement.Frost => new Color(110, 225, 255, 220),
            _ => new Color(120, 165, 255, 220)
        };

        if (WatcherEcho)
            baseColor = Color.Lerp(baseColor, Color.White, 0.14f);
        if (Overload)
            baseColor = Color.Lerp(baseColor, new Color(255, 240, 190, 220), 0.2f);
        if (FinalPulse)
            baseColor = Color.Lerp(baseColor, Color.White, 0.3f);

        return baseColor;
    }

    private Color GetInnerColor() {
        Color highlight = Element switch {
            EyeGuyElement.Fire => new Color(255, 245, 225, 220),
            EyeGuyElement.Frost => new Color(240, 255, 255, 220),
            _ => new Color(235, 245, 255, 220)
        };

        if (Overload || OmniGaze || FinalPulse)
            highlight = Color.Lerp(highlight, Color.White, 0.15f);

        return highlight;
    }
}
