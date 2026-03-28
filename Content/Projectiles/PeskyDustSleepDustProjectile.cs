using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class PeskyDustSleepDustProjectile : ModProjectile {
    private bool Drifting => Projectile.ai[0] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 90;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        NPC target = FindTarget(Drifting ? 260f : 210f);
        if (target != null) {
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * (Drifting ? 14.5f : 12.5f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, Drifting ? 0.12f : 0.08f);
        }
        else {
            Projectile.velocity *= 0.992f;
        }

        Projectile.rotation += 0.18f * Math.Sign(Projectile.velocity.X == 0f ? 1f : Projectile.velocity.X);
        Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.72f, 0.95f) * 0.32f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                Main.rand.NextBool() ? DustID.GoldFlame : DustID.PinkTorch,
                Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f) + Main.rand.NextVector2Circular(0.4f, 0.4f),
                105, new Color(255, 230, 150), Main.rand.NextFloat(0.7f, 1f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 244, 180, 190), Projectile.rotation, Vector2.One * 0.5f,
            new Vector2(12f, 12f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(5f, -2f).RotatedBy(Projectile.rotation), null,
            new Color(255, 175, 220, 170), Projectile.rotation, Vector2.One * 0.5f, new Vector2(8f, 8f),
            SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(-4f, 3f).RotatedBy(Projectile.rotation), null,
            new Color(200, 245, 255, 150), Projectile.rotation, Vector2.One * 0.5f, new Vector2(6f, 6f),
            SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity *= Drifting ? 0.92f : 0.95f;
        target.netUpdate = true;
    }

    private NPC FindTarget(float maxDistance) {
        NPC bestTarget = null;
        float bestDistanceSq = maxDistance * maxDistance;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distanceSq = Vector2.DistanceSquared(npc.Center, Projectile.Center);
            if (distanceSq >= bestDistanceSq)
                continue;

            bestDistanceSq = distanceSq;
            bestTarget = npc;
        }

        return bestTarget;
    }
}
