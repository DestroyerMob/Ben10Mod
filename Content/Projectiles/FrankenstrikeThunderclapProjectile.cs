using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FrankenstrikeThunderclapProjectile : ModProjectile {
    private const int LifetimeTicks = 24;
    private const float StartRadius = 28f;
    private const float EndRadius = 108f;
    private float ChargeRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);

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
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.75f }, owner.Center);
        }

        Projectile.Center = owner.Center;
        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2f);
        CurrentRadius = MathHelper.Lerp(StartRadius, EndRadius + 56f * ChargeRatio, easedProgress);
        Lighting.AddLight(Projectile.Center, new Vector3(0.28f, 0.48f, 0.95f));

        if (Main.rand.NextBool(2)) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 direction = angle.ToRotationVector2();
            Dust dust = Dust.NewDustPerfect(Projectile.Center + direction * Main.rand.NextFloat(CurrentRadius * 0.3f, CurrentRadius),
                Main.rand.NextBool(3) ? DustID.Electric : DustID.BlueTorch,
                direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1.8f, 1.8f), 110,
                new Color(170, 225, 255), Main.rand.NextFloat(0.95f, 1.25f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        float rotation = Projectile.localAI[0] * 0.04f;
        float opacity = Utils.GetLerpValue(0f, 0.14f, 1f - Projectile.timeLeft / (float)LifetimeTicks, true) *
            Utils.GetLerpValue(0f, 0.32f, Projectile.timeLeft / (float)LifetimeTicks, true);

        DrawRing(pixel, Projectile.Center - Main.screenPosition, CurrentRadius, 4.2f, new Color(105, 150, 255, 75) * opacity, rotation);
        DrawRing(pixel, Projectile.Center - Main.screenPosition, CurrentRadius * 0.68f, 3.3f, new Color(235, 245, 255, 125) * opacity, -rotation * 1.2f);
        DrawRing(pixel, Projectile.Center - Main.screenPosition, CurrentRadius * 0.44f, 2.6f, new Color(255, 255, 255, 155) * opacity, rotation * 1.45f);
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int conductiveStacks = identity.GetFrankenstrikeConductiveStacks(Projectile.owner);
        if (conductiveStacks > 0)
            modifiers.SourceDamage *= 1f + conductiveStacks * 0.1f + ChargeRatio * 0.12f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 push = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 6.6f;
        target.velocity = Vector2.Lerp(target.velocity, push + new Vector2(0f, -1.8f), 0.6f);
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyFrankenstrikeConductive(Projectile.owner, 2, 240);
        target.AddBuff(BuffID.Electrified, 180);
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
