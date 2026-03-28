using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class LodestarPolarVortexProjectile : ModProjectile {
    private const float BaseRadius = 148f;
    private const float MaxRadius = 220f;
    private const float InnerRadius = 76f;
    private bool Repel => Projectile.ai[0] >= 0.5f;

    private float CurrentRadius {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != "Ben10Mod:Lodestar") {
            Projectile.Kill();
            return;
        }

        Projectile.velocity = Vector2.Zero;
        Projectile.localAI[0]++;

        float pulse = 0.5f + 0.5f * MathF.Sin(Projectile.localAI[0] * 0.09f);
        CurrentRadius = MathHelper.Lerp(BaseRadius, MaxRadius, pulse);

        if (Main.netMode != NetmodeID.MultiplayerClient)
            PullNPCs();

        Lighting.AddLight(Projectile.Center, new Vector3(0.92f, 0.35f, 0.3f) * 0.52f);

        if (Main.rand.NextBool(2)) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(InnerRadius, CurrentRadius);
            Vector2 tangential = offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1f, 1f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool(3) ? DustID.Firework_Red : DustID.Iron,
                tangential, 110, new Color(235, 118, 104), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = Projectile.localAI[0] * 0.025f;
        Color ringColor = Repel ? new Color(115, 160, 250, 72) : new Color(225, 90, 78, 72);
        Color coreColor = Repel ? new Color(85, 125, 225, 225) : new Color(145, 28, 24, 225);
        Color centerGlow = Repel ? new Color(120, 180, 255, 220) : new Color(245, 110, 96, 220);

        DrawRing(pixel, center, CurrentRadius, 4.2f, new Color(165, 175, 198, 44), rotation * 0.55f);
        DrawRing(pixel, center, CurrentRadius * 0.72f, 5f, ringColor, -rotation);
        DrawRing(pixel, center, CurrentRadius * 0.44f, 5.4f, new Color(245, 235, 235, 90), rotation * 1.3f);
        Main.EntitySpriteDraw(pixel, center, null, coreColor, 0f, Vector2.One * 0.5f,
            new Vector2(InnerRadius * 0.9f, InnerRadius * 0.9f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, centerGlow, 0f, Vector2.One * 0.5f,
            new Vector2(18f, 18f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 forceDirection = Repel
            ? (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX)
            : (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitX);
        target.velocity = Vector2.Lerp(target.velocity, forceDirection * (Repel ? 10f : 8.5f), 0.5f);
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyLodestarPolarity(Projectile.owner, 300, Repel ? 1 : -1);
        target.netUpdate = true;
    }

    private void PullNPCs() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance > CurrentRadius || distance <= 6f)
                continue;

            float distanceFactor = 1f - distance / CurrentRadius;
            float pullStrength = MathHelper.Lerp(2.2f, 12.5f, distanceFactor);
            if (distance <= InnerRadius)
                pullStrength *= MathHelper.Lerp(1.2f, 1.7f, 1f - distance / InnerRadius);

            if (npc.boss)
                pullStrength *= 0.45f;
            else if (npc.knockBackResist > 0f)
                pullStrength *= MathHelper.Lerp(0.7f, 1.15f, npc.knockBackResist);

            Vector2 desiredVelocity = (Repel
                ? (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX)
                : (Projectile.Center - npc.Center).SafeNormalize(Vector2.UnitX)) * pullStrength;
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, npc.boss ? 0.09f : 0.28f);
            npc.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyLodestarPolarity(Projectile.owner, 60, Repel ? 1 : -1);
            npc.netUpdate = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 28;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.3f), SpriteEffects.None, 0);
        }
    }
}
