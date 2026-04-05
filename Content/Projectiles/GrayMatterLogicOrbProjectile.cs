using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GrayMatterLogicOrbProjectile : ModProjectile {
    private bool Hyperfocused => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 4;
        Projectile.timeLeft = 180;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.rotation += Hyperfocused ? 0.18f : 0.12f;
        Projectile.velocity *= Hyperfocused ? 0.994f : 0.988f;

        NPC target = FindClosestNPC(Hyperfocused ? 520f : 380f);
        if (target != null) {
            float desiredSpeed = Hyperfocused ? 8.8f : 7.2f;
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * desiredSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, Hyperfocused ? 0.09f : 0.055f);
        }

        Lighting.AddLight(Projectile.Center, Hyperfocused ? 0.14f : 0.08f, 0.34f, 0.1f);

        for (int i = 0; i < 2; i++) {
            Vector2 offset = Main.rand.NextVector2CircularEdge(10f, 10f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, i == 0 ? DustID.GreenTorch : DustID.Electric,
                -offset * 0.045f, 95, Hyperfocused ? new Color(175, 255, 175) : new Color(135, 220, 145),
                Main.rand.NextFloat(0.8f, 1.05f));
            dust.noGravity = true;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.penetrate--;
        if (Projectile.penetrate <= 0)
            return true;

        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X * 0.92f;

        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y * 0.92f;

        Projectile.netUpdate = true;
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), Hyperfocused ? 240 : 150);
        target.AddBuff(BuffID.Confused, Hyperfocused ? 150 : 90);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 12; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.Electric,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 100,
                Hyperfocused ? new Color(185, 255, 185) : new Color(145, 225, 155),
                Main.rand.NextFloat(0.9f, 1.2f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float pulse = 1f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * (Hyperfocused ? 7f : 5f) + Projectile.identity) * 0.08f;
        float outerRadius = Hyperfocused ? 14f : 12f;

        DrawRing(pixel, center, outerRadius * pulse, 3.6f, new Color(85, 200, 110, 130), Projectile.rotation);
        DrawRing(pixel, center, (outerRadius - 4f) * pulse, 2.6f, new Color(190, 255, 200, 185), -Projectile.rotation * 1.25f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(120, 220, 140, 135), 0f, Vector2.One * 0.5f,
            new Vector2(14f * pulse, 14f * pulse), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(240, 255, 245, 205), 0f, Vector2.One * 0.5f,
            new Vector2(6.5f * pulse, 6.5f * pulse), SpriteEffects.None, 0);
        return false;
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
        const int Segments = 20;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 1.85f), SpriteEffects.None, 0);
        }
    }
}
