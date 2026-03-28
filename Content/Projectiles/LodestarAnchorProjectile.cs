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

public class LodestarAnchorProjectile : ModProjectile {
    private const float PullRadius = 112f;
    private const float DamageRadius = 68f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 12 * 60;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 24;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != "Ben10Mod:Lodestar") {
            Projectile.Kill();
            return;
        }

        Projectile.velocity = Vector2.Zero;
        Projectile.localAI[0]++;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            PullNearbyNPCs();

        Lighting.AddLight(Projectile.Center, new Vector3(0.88f, 0.34f, 0.3f) * 0.4f);

        if (Main.rand.NextBool(3)) {
            Vector2 offset = Main.rand.NextVector2Circular(PullRadius * 0.55f, PullRadius * 0.55f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? DustID.Iron : DustID.Firework_Red,
                offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * 0.35f, 110, new Color(235, 120, 108),
                Main.rand.NextFloat(0.9f, 1.08f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= DamageRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = Projectile.localAI[0] * 0.028f;
        float pulse = 0.86f + 0.14f * MathF.Sin(Projectile.localAI[0] * 0.1f);

        DrawRing(pixel, center, PullRadius * 0.62f * pulse, 3.4f, new Color(175, 185, 205, 48), rotation);
        DrawRing(pixel, center, PullRadius * 0.4f * pulse, 4f, new Color(225, 95, 82, 82), -rotation * 1.2f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(225, 95, 82, 210), rotation, Vector2.One * 0.5f,
            new Vector2(20f, 6f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(225, 95, 82, 210), rotation + MathHelper.PiOver2, Vector2.One * 0.5f,
            new Vector2(20f, 6f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(245, 245, 250, 225), 0f, Vector2.One * 0.5f,
            new Vector2(8f, 8f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 180);
        target.AddBuff(BuffID.BrokenArmor, 180);
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
            float pullStrength = MathHelper.Lerp(1.5f, 7.2f, distanceFactor);
            if (npc.boss)
                pullStrength *= 0.5f;
            else if (npc.knockBackResist > 0f)
                pullStrength *= MathHelper.Lerp(0.65f, 1.08f, npc.knockBackResist);

            Vector2 desiredVelocity = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero) * pullStrength;
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, npc.boss ? 0.08f : 0.22f);
            npc.netUpdate = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotation) {
        const int Segments = 22;
        for (int i = 0; i < Segments; i++) {
            float angle = rotation + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.6f), SpriteEffects.None, 0);
        }
    }
}
