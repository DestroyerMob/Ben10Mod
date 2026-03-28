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

public class WhampireCorrupturaBoltProjectile : ModProjectile {
    private bool Cloaked => Projectile.ai[0] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 96;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        NPC target = FindTarget(Cloaked ? 280f : 220f);
        if (target != null) {
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * (Cloaked ? 15.5f : 13.5f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, Cloaked ? 0.12f : 0.08f);
        }
        else {
            Projectile.velocity *= 0.994f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.72f, 0.12f, 0.16f) * 0.45f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                Main.rand.NextBool() ? DustID.Shadowflame : DustID.Blood,
                Projectile.velocity * Main.rand.NextFloat(0.03f, 0.1f), 120, new Color(165, 35, 48),
                Main.rand.NextFloat(0.8f, 1.08f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.EntitySpriteDraw(pixel, center, null, new Color(38, 10, 16, 220), Projectile.rotation, Vector2.One * 0.5f,
            new Vector2(18f, 7f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(130, 18, 28, 210), Projectile.rotation, Vector2.One * 0.5f,
            new Vector2(12f, 4.4f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(240, 88, 106, 190), Projectile.rotation, Vector2.One * 0.5f,
            new Vector2(6f, 2.5f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool alreadyDazed = target.HasBuff(BuffID.Confused);
        target.AddBuff(BuffID.Confused, Cloaked ? 105 : 75);
        target.AddBuff(BuffID.Weak, Cloaked ? 180 : 120);
        if (alreadyDazed)
            target.AddBuff(BuffID.BrokenArmor, Cloaked ? 150 : 105);

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
