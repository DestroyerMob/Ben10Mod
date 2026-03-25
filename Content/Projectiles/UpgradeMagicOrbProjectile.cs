using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UpgradeMagicOrbProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private int FlagMask => (int)Math.Round(Projectile.ai[1]);
    private bool Overclocked => (FlagMask & 1) != 0;
    private bool FullyIntegrated => (FlagMask & 2) != 0;
    private UpgradeAttackVariant Variant => (UpgradeAttackVariant)((FlagMask >> 2) & 0x3);

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 115;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.rotation += 0.045f;
        Projectile.velocity *= 0.992f;

        NPC target = FindClosestNPC(FullyIntegrated ? 520f : 440f);
        if (target != null) {
            float desiredSpeed = Variant == UpgradeAttackVariant.Construct ? 7.2f : 8.4f;
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * desiredSpeed;
            float turnSpeed = FullyIntegrated ? 0.08f : 0.055f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, turnSpeed);
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.25f, 1.15f, 0.78f) * 0.85f);

        for (int i = 0; i < 2; i++) {
            Vector2 offset = Main.rand.NextVector2CircularEdge(14f, 14f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, i == 0 ? DustID.GreenTorch : DustID.Electric,
                -offset * 0.035f, 100, new Color(140, 255, 220), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Electrified, FullyIntegrated ? 210 : 150);
        if (Overclocked || Variant != UpgradeAttackVariant.Primary)
            target.AddBuff(BuffID.Confused, FullyIntegrated ? 120 : 75);

        EmitBurst(8);
    }

    public override void OnKill(int timeLeft) {
        EmitBurst(14);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + Projectile.identity) * 0.08f;
        float outerRadius = Variant == UpgradeAttackVariant.Construct ? 16f : 18f;

        DrawRing(pixel, center, outerRadius * pulse, 4.2f, new Color(105, 255, 200, 135), Projectile.rotation);
        DrawRing(pixel, center, (outerRadius - 5f) * pulse, 3f, new Color(185, 255, 230, 185), -Projectile.rotation * 1.2f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(120, 255, 205, 145), 0f, Vector2.One * 0.5f,
            new Vector2(20f * pulse, 20f * pulse), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(245, 255, 250, 215), 0f, Vector2.One * 0.5f,
            new Vector2(10f * pulse, 10f * pulse), SpriteEffects.None, 0);
        return false;
    }

    private void EmitBurst(int count) {
        for (int i = 0; i < count; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.Electric,
                Main.rand.NextVector2Circular(3f, 3f), 100, new Color(140, 255, 220), Main.rand.NextFloat(0.95f, 1.3f));
            dust.noGravity = true;
        }
    }

    private NPC FindClosestNPC(float maxDistance) {
        NPC closestTarget = null;
        float closestDistance = maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Projectile.Center.Distance(npc.Center);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = npc;
        }

        return closestTarget;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 24;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.1f), SpriteEffects.None, 0);
        }
    }
}
