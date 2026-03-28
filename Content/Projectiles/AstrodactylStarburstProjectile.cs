using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AstrodactylStarburstProjectile : ModProjectile {
    private bool Hyperflight => Projectile.ai[0] >= 0.5f;
    private float AirSupremacyRatio => MathHelper.Clamp(Projectile.ai[1], 0f, 1f);

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 62;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 14;
    }

    public override void AI() {
        HomeTowardTarget(MathHelper.Lerp(Hyperflight ? 0.12f : 0.08f, 0.18f, AirSupremacyRatio),
            MathHelper.Lerp(Hyperflight ? 520f : 420f, 620f, AirSupremacyRatio));
        Projectile.rotation += Hyperflight ? 0.34f : 0.24f;
        Lighting.AddLight(Projectile.Center,
            Vector3.Lerp(Hyperflight ? new Vector3(0.25f, 1f, 0.72f) : new Vector3(0.18f, 0.92f, 0.62f),
                new Vector3(0.32f, 1f, 0.86f), AirSupremacyRatio));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                Main.rand.NextBool(3) ? DustID.GreenTorch : DustID.GemEmerald,
                Main.rand.NextVector2Circular(0.5f, 0.5f), 100, new Color(185, 255, 225),
                Main.rand.NextFloat(0.9f, Hyperflight ? 1.18f : 1.05f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float pulse = 1f + 0.14f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f + Projectile.identity);
        float size = MathHelper.Lerp(1f, 1.24f, AirSupremacyRatio);

        Main.EntitySpriteDraw(pixel, center, null, new Color(70, 225, 135, 120), 0f, Vector2.One * 0.5f,
            new Vector2(22f, 22f) * pulse * size, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(235, 255, 240, 215), Projectile.rotation, Vector2.One * 0.5f,
            new Vector2(12f, 12f) * pulse * size, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, Hyperflight ? 240 : 180);
        target.AddBuff(BuffID.Oiled, 90);
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            int shardDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.4f));
            const int ShardCount = 4;

            for (int i = 0; i < ShardCount; i++) {
                Vector2 velocity = (MathHelper.TwoPi * i / ShardCount).ToRotationVector2() * Main.rand.NextFloat(10f, 13f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                        ModContent.ProjectileType<AstrodactylPlasmaBoltProjectile>(), shardDamage, Projectile.knockBack * 0.6f,
                        Projectile.owner, Hyperflight ? 1f : 0f, AirSupremacyRatio);
            }
        }

        if (Main.dedServ)
            return;

        for (int i = 0; i < 12; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.GemEmerald,
                Main.rand.NextVector2Circular(3f, 3f), 100, new Color(185, 255, 225), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }

    private void HomeTowardTarget(float homingStrength, float maxDistance) {
        NPC target = FindTarget(maxDistance);
        if (target == null)
            return;

        float speed = Projectile.velocity.Length();
        if (speed <= 0.01f)
            speed = Hyperflight ? 15f : 13f;

        Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity) * speed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
    }

    private NPC FindTarget(float maxDistance) {
        NPC bestTarget = null;
        float bestDistanceSquared = maxDistance * maxDistance;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distanceSquared = Vector2.DistanceSquared(Projectile.Center, npc.Center);
            if (distanceSquared >= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            bestTarget = npc;
        }

        return bestTarget;
    }
}
