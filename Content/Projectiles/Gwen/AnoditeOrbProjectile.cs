using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.Gwen;

public class AnoditeOrbProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 38;
        Projectile.height = 38;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 4;
        Projectile.timeLeft = 240;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 16;
    }

    public override void AI() {
        Projectile.rotation += 0.03f;
        Projectile.velocity *= 0.988f;

        NPC target = FindClosestNPC(280f);
        if (target != null) {
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * 7f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.018f);
        }

        Lighting.AddLight(Projectile.Center, new Vector3(1.2f, 0.45f, 0.9f) * 0.9f);

        for (int i = 0; i < 2; i++) {
            Vector2 offset = Main.rand.NextVector2CircularEdge(18f, 18f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.PinkTorch,
                -offset * 0.03f, 100, new Color(255, 145, 225), Main.rand.NextFloat(1.05f, 1.3f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Confused, 75);
        EmitBurst(7);
    }

    public override void OnKill(int timeLeft) {
        EmitBurst(14);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float pulse = 1f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 5f + Projectile.identity) * 0.08f;

        DrawRing(pixel, center, 22f * pulse, 4.8f, new Color(255, 95, 185, 130), Projectile.rotation);
        DrawRing(pixel, center, 15f * pulse, 3.4f, new Color(255, 180, 230, 185), -Projectile.rotation * 1.4f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 125, 210, 155), 0f, Vector2.One * 0.5f,
            new Vector2(24f * pulse, 24f * pulse), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 245, 255, 215), 0f, Vector2.One * 0.5f,
            new Vector2(13f * pulse, 13f * pulse), SpriteEffects.None, 0);
        return false;
    }

    private void EmitBurst(int count) {
        for (int i = 0; i < count; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.PinkTorch : DustID.GemRuby,
                Main.rand.NextVector2Circular(3f, 3f), 100, new Color(255, 140, 220), Main.rand.NextFloat(1f, 1.4f));
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
