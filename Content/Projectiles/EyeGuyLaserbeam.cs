using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EyeGuyLaserbeam : ModProjectile, IMagistrataOutlineProvider {
    public const int VariantPrimary = 0;
    public const int VariantCrossfire = 1;
    public const int VariantOmniBurst = 2;

    private int Variant => Math.Clamp((int)Math.Round(Projectile.ai[0]), VariantPrimary, VariantOmniBurst);
    private bool Focused => Projectile.ai[1] >= 0.5f;

    public override void SetDefaults() {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 3;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 2;
        Projectile.timeLeft = 96;
        Projectile.alpha = 18;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            ApplyVariantDefaults();
        }

        if (Variant == VariantOmniBurst)
            Projectile.velocity *= 0.986f;
        else if (Projectile.velocity.LengthSquared() < (Focused ? 1600f : 1369f))
            Projectile.velocity *= Focused ? 1.015f : 1.01f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, GetLightColor());

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, GetDustType(),
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.12f), 100, GetDustColor(),
                Main.rand.NextFloat(0.88f, 1.22f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() * 0.5f;
        float rotation = Projectile.rotation + MathHelper.PiOver2;
        Color outerColor = GetOuterColor() * Projectile.Opacity;
        Color innerColor = GetInnerColor() * Projectile.Opacity;

        Main.EntitySpriteDraw(texture, drawPosition, null, outerColor, rotation, origin,
            Projectile.scale * (Variant == VariantOmniBurst ? 1.14f : 1.02f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, drawPosition, null, innerColor, rotation, origin,
            Projectile.scale * 0.68f, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Focused || Variant != VariantPrimary)
            target.AddBuff(BuffID.BrokenArmor, Focused ? 180 : 120);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 7; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, GetDustType(),
                Main.rand.NextVector2Circular(2.2f, 2.2f), 95, GetDustColor(),
                Main.rand.NextFloat(0.92f, 1.26f));
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
        switch (Variant) {
            case VariantCrossfire:
                Projectile.scale = Focused ? 1.18f : 1.08f;
                Projectile.penetrate = Focused ? 3 : 2;
                Projectile.timeLeft = Focused ? 92 : 84;
                Projectile.extraUpdates = 2;
                Projectile.localNPCHitCooldown = 9;
                Projectile.alpha = 10;
                break;
            case VariantOmniBurst:
                Projectile.scale = Focused ? 1.28f : 1.16f;
                Projectile.penetrate = 1;
                Projectile.timeLeft = Focused ? 32 : 28;
                Projectile.extraUpdates = 1;
                Projectile.localNPCHitCooldown = 13;
                Projectile.tileCollide = false;
                Projectile.alpha = 0;
                break;
            default:
                Projectile.scale = Focused ? 1.08f : 1f;
                Projectile.penetrate = Focused ? 4 : 3;
                Projectile.timeLeft = Focused ? 104 : 96;
                Projectile.extraUpdates = 2;
                Projectile.localNPCHitCooldown = 8;
                Projectile.alpha = 18;
                break;
        }
    }

    private Vector3 GetLightColor() {
        return Variant switch {
            VariantCrossfire => Focused ? new Vector3(0.24f, 1.1f, 0.3f) : new Vector3(0.14f, 0.98f, 0.22f),
            VariantOmniBurst => Focused ? new Vector3(0.32f, 1.15f, 0.36f) : new Vector3(0.2f, 0.92f, 0.24f),
            _ => Focused ? new Vector3(0.16f, 1f, 0.22f) : new Vector3(0f, 0.95f, 0.25f)
        };
    }

    private int GetDustType() {
        return Variant == VariantOmniBurst
            ? (Focused ? DustID.GreenTorch : DustID.GreenFairy)
            : (Main.rand.NextBool() ? DustID.GreenFairy : DustID.GreenTorch);
    }

    private Color GetDustColor() {
        return Variant switch {
            VariantCrossfire => Focused ? new Color(185, 255, 170) : new Color(130, 255, 150),
            VariantOmniBurst => Focused ? new Color(205, 255, 180) : new Color(155, 255, 165),
            _ => Focused ? new Color(165, 255, 160) : new Color(110, 245, 140)
        };
    }

    private Color GetOuterColor() {
        return Variant switch {
            VariantCrossfire => Focused ? new Color(120, 255, 150, 220) : new Color(72, 240, 125, 220),
            VariantOmniBurst => Focused ? new Color(160, 255, 175, 220) : new Color(95, 240, 145, 220),
            _ => Focused ? new Color(100, 255, 145, 220) : new Color(60, 220, 110, 220)
        };
    }

    private Color GetInnerColor() {
        return Variant switch {
            VariantCrossfire => Focused ? new Color(240, 255, 220, 220) : new Color(220, 255, 210, 215),
            VariantOmniBurst => Focused ? new Color(245, 255, 225, 220) : new Color(232, 255, 220, 215),
            _ => Focused ? new Color(235, 255, 220, 220) : new Color(210, 255, 215, 210)
        };
    }
}
