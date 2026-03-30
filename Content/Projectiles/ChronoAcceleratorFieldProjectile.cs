using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChronoAcceleratorFieldProjectile : ModProjectile {
    private const float BaseRadius = 96f;

    private float PowerScale => MathHelper.Clamp(Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0], 1f, 1.65f);
    private float LifetimeProgress => 1f - Projectile.timeLeft / 96f;

    private float CurrentRadius {
        get {
            float appear = Utils.GetLerpValue(0f, 0.12f, LifetimeProgress, true);
            float fade = Utils.GetLerpValue(0f, 14f, Projectile.timeLeft, true);
            return BaseRadius * PowerScale * Math.Min(appear, fade);
        }
    }

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
        Projectile.timeLeft = 96;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 16;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation + 0.028f * PowerScale);
        Lighting.AddLight(Projectile.Center, new Vector3(0.18f, 0.44f, 0.46f) * PowerScale);

        DragEnemiesTowardField();
        EmitFieldDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 120);
        target.velocity *= 0.84f;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float radius = CurrentRadius;
        float alpha = MathHelper.Clamp(Utils.GetLerpValue(0f, 0.12f, LifetimeProgress, true) *
                                       Utils.GetLerpValue(0f, 16f, Projectile.timeLeft, true), 0f, 1f);
        float spin = Projectile.rotation;

        Color outer = new Color(80, 255, 235, 0) * (0.6f * alpha);
        Color inner = new Color(190, 255, 250, 0) * (0.85f * alpha);
        Color core = new Color(240, 255, 255, 0) * (0.95f * alpha);

        DrawRing(pixel, center, radius, 18, outer, 4.6f);
        DrawRing(pixel, center, radius * 0.72f, 14, inner, 3.2f);
        DrawRing(pixel, center, radius * 0.45f, 10, outer * 0.75f, 2.4f);

        for (int i = 0; i < 4; i++) {
            float angle = spin + MathHelper.PiOver2 * i;
            float handLength = radius * (i % 2 == 0 ? 0.94f : 0.66f);
            DrawBeam(pixel, center, angle, handLength, 4.6f, inner * (i % 2 == 0 ? 1f : 0.8f));
        }

        DrawCross(pixel, center, 14f, 4.5f, -spin * 1.3f, core * 0.9f);
        DrawCross(pixel, center, 8f, 8f, 0f, Color.White * alpha);
        return false;
    }

    private void DragEnemiesTowardField() {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        float radius = CurrentRadius;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance > radius || distance <= 8f)
                continue;

            Vector2 toCenter = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
            float pullStrength = MathHelper.Lerp(0.16f, 0.05f, distance / radius);
            npc.velocity = Vector2.Lerp(npc.velocity, toCenter * (2.6f + PowerScale) + new Vector2(0f, -0.45f), pullStrength);
        }
    }

    private void EmitFieldDust() {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        float radius = CurrentRadius;
        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
        Vector2 unit = angle.ToRotationVector2();
        Vector2 position = Projectile.Center + unit * Main.rand.NextFloat(radius * 0.35f, radius);
        Vector2 velocity = unit.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.8f, 1.8f);
        Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.GemDiamond : DustID.AncientLight,
            velocity, 105, new Color(160, 255, 245), Main.rand.NextFloat(0.9f, 1.18f));
        dust.noGravity = true;
    }

    private static void DrawBeam(Texture2D pixel, Vector2 center, float rotation, float length, float thickness, Color color) {
        Main.EntitySpriteDraw(pixel, center, null, color, rotation, new Vector2(0f, 0.5f),
            new Vector2(length, thickness), SpriteEffects.None, 0);
    }

    private static void DrawCross(Texture2D pixel, Vector2 center, float length, float thickness, float rotation, Color color) {
        Main.EntitySpriteDraw(pixel, center, null, color, rotation, Vector2.One * 0.5f, new Vector2(length, thickness),
            SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, color, rotation + MathHelper.PiOver2, Vector2.One * 0.5f,
            new Vector2(length, thickness), SpriteEffects.None, 0);
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, int segments, Color color, float thickness) {
        float segmentLength = MathHelper.TwoPi * radius / segments * 0.78f;
        for (int i = 0; i < segments; i++) {
            float angle = MathHelper.TwoPi * i / segments;
            Vector2 offset = angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, center + offset, null, color, angle, Vector2.One * 0.5f,
                new Vector2(segmentLength, thickness), SpriteEffects.None, 0);
        }
    }
}
