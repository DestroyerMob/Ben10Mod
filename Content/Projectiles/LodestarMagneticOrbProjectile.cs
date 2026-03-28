using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class LodestarMagneticOrbProjectile : ModProjectile {
    private const float PullRadius = 92f;
    private const float DamageRadius = 54f;
    private bool Repel => Projectile.ai[0] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override void AI() {
        Projectile.velocity *= 0.97f;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            PullNearbyNPCs();

        Lighting.AddLight(Projectile.Center, new Vector3(0.86f, 0.36f, 0.32f) * 0.42f);

        if (Main.rand.NextBool(2)) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(12f, PullRadius * 0.5f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? DustID.Firework_Red : DustID.Iron,
                offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * 0.25f, 110, new Color(235, 125, 115),
                Main.rand.NextFloat(0.9f, 1.1f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= DamageRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outerRing = Repel ? new Color(118, 165, 240, 85) : new Color(225, 95, 82, 85);
        Color coreColor = Repel ? new Color(120, 165, 255, 210) : new Color(225, 95, 82, 210);

        DrawRing(pixel, center, PullRadius * 0.62f, 3f, new Color(175, 185, 205, 50), Projectile.rotation * 0.4f);
        DrawRing(pixel, center, PullRadius * 0.4f, 3.6f, outerRing, -Projectile.rotation * 0.7f);
        Main.EntitySpriteDraw(pixel, center, null, coreColor, 0f, Vector2.One * 0.5f,
            new Vector2(15f, 15f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(245, 245, 250, 225), 0f, Vector2.One * 0.5f,
            new Vector2(7f, 7f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 forceDirection = Repel
            ? (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX)
            : (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitX);
        target.velocity = Vector2.Lerp(target.velocity, forceDirection * 7.5f, 0.42f);
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyLodestarPolarity(Projectile.owner, 210, Repel ? 1 : -1);
        target.netUpdate = true;
    }

    private void PullNearbyNPCs() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance > PullRadius || distance <= 6f)
                continue;

            float distanceFactor = 1f - distance / PullRadius;
            float pullStrength = MathHelper.Lerp(1.1f, 5.2f, distanceFactor);
            if (npc.boss)
                pullStrength *= 0.55f;
            else if (npc.knockBackResist > 0f)
                pullStrength *= MathHelper.Lerp(0.65f, 1.05f, npc.knockBackResist);

            Vector2 desiredVelocity = (Repel
                ? (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX)
                : (Projectile.Center - npc.Center).SafeNormalize(Vector2.UnitX)) * pullStrength;
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, npc.boss ? 0.08f : 0.18f);
            npc.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyLodestarPolarity(Projectile.owner, 60, Repel ? 1 : -1);
            npc.netUpdate = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotation) {
        const int Segments = 18;
        for (int i = 0; i < Segments; i++) {
            float angle = rotation + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.8f), SpriteEffects.None, 0);
        }
    }
}
