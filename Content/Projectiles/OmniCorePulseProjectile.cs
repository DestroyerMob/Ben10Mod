using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class OmniCorePulseProjectile : ModProjectile {
    private const float BaseRadius = 112f;
    private const float EnergyRefundPerHit = 6f;

    private float PowerScale => MathHelper.Clamp(Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0], 1f, 1.45f);
    private float Progress => 1f - Projectile.timeLeft / 42f;
    private float CurrentRadius => BaseRadius * PowerScale * EaseOutQuad(MathHelper.Clamp(Progress, 0f, 1f));

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 42;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation + 0.11f);
        Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.55f, 0.48f) * PowerScale);

        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
        Vector2 unit = angle.ToRotationVector2();
        Vector2 position = Projectile.Center + unit * Main.rand.NextFloat(CurrentRadius * 0.3f, CurrentRadius);
        Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.AncientLight : DustID.GemEmerald,
            unit.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(1f, 2.4f), 95,
            new Color(120, 255, 225), Main.rand.NextFloat(0.95f, 1.25f));
        dust.noGravity = true;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        omp.omnitrixEnergy = Math.Min(omp.omnitrixEnergyMax, omp.omnitrixEnergy + EnergyRefundPerHit);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float radius = CurrentRadius;
        float alpha = Utils.GetLerpValue(0f, 10f, Projectile.timeLeft, true);
        Color outer = new Color(75, 255, 225, 0) * (0.6f * alpha);
        Color inner = new Color(210, 255, 248, 0) * (0.92f * alpha);

        DrawRing(pixel, center, radius, 24, outer, 5.4f);
        DrawRing(pixel, center, radius * 0.72f, 18, inner, 3.4f);

        for (int i = 0; i < 6; i++) {
            float angle = Projectile.rotation + MathHelper.TwoPi * i / 6f;
            Vector2 offset = angle.ToRotationVector2() * radius * 0.82f;
            Main.EntitySpriteDraw(pixel, center + offset, null, inner * 0.9f, angle, Vector2.One * 0.5f,
                new Vector2(14f, 4f), SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(pixel, center, null, inner, Projectile.rotation, Vector2.One * 0.5f, new Vector2(18f, 5f),
            SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, Color.White * (0.85f * alpha), Projectile.rotation + MathHelper.PiOver2,
            Vector2.One * 0.5f, new Vector2(10f, 3f), SpriteEffects.None, 0);
        return false;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, int segments, Color color, float thickness) {
        float segmentLength = MathHelper.TwoPi * radius / segments * 0.76f;
        for (int i = 0; i < segments; i++) {
            float angle = MathHelper.TwoPi * i / segments;
            Vector2 offset = angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, center + offset, null, color, angle, Vector2.One * 0.5f,
                new Vector2(segmentLength, thickness), SpriteEffects.None, 0);
        }
    }

    private static float EaseOutQuad(float value) {
        return 1f - (1f - value) * (1f - value);
    }
}
