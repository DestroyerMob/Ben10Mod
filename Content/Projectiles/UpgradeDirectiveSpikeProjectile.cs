using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UpgradeDirectiveSpikeProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MartianTurretBolt}";

    private int FlagMask => (int)Math.Round(Projectile.ai[1]);
    private bool Overclocked => (FlagMask & 1) != 0;
    private bool FullyIntegrated => (FlagMask & 2) != 0;
    private UpgradeAttackVariant Variant => (UpgradeAttackVariant)((FlagMask >> 2) & 0x3);

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 100;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        ApplyHoming(FullyIntegrated ? 0.1f : 0.07f, FullyIntegrated ? 540f : 430f);
        Projectile.velocity = Projectile.velocity.RotatedBy(Math.Sin((Main.GameUpdateCount + Projectile.identity) * 0.06f) * 0.012f);
        Lighting.AddLight(Projectile.Center, 0.24f, 1f, 0.62f);

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.Electric : DustID.GreenTorch,
                -Projectile.velocity * 0.08f, 95, new Color(155, 255, 190), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), FullyIntegrated ? 165 : 120);
        if (Overclocked || Variant != UpgradeAttackVariant.Primary)
            target.AddBuff(BuffID.Electrified, 90);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.Electric,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 100, new Color(155, 255, 190), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }

    private void ApplyHoming(float homingStrength, float maxDistance) {
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

        if (bestTarget == null)
            return;

        float speed = Projectile.velocity.Length();
        if (speed <= 0.01f)
            speed = 16f;

        Vector2 desiredVelocity = Projectile.DirectionTo(bestTarget.Center) * speed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
    }
}
