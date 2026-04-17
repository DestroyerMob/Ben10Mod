using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class NRGHomingEnergyBallProjectile : ModProjectile {
    private const float MaxSearchDistance = 620f;
    private const float HomingSpeed = 16f;
    private const float HomingInertia = 12f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 2;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        NPC target = FindTarget();
        if (target != null)
            HomeTowards(target);
        else
            Projectile.velocity *= 0.995f;

        SpawnEnergyDust();
        Lighting.AddLight(Projectile.Center, 1.1f, 0.2f, 0.08f);
        Projectile.localAI[0] += 0.22f;
    }

    private NPC FindTarget() {
        NPC selectedTarget = null;
        float closestDistance = MaxSearchDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Projectile.Center.Distance(npc.Center);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            selectedTarget = npc;
        }

        return selectedTarget;
    }

    private void HomeTowards(NPC target) {
        Vector2 desiredVelocity = Projectile.Center.DirectionTo(target.Center) * HomingSpeed;
        Projectile.velocity = (Projectile.velocity * (HomingInertia - 1f) + desiredVelocity) / HomingInertia;
    }

    private void SpawnEnergyDust() {
        if (Main.dedServ)
            return;

        Vector2 velocityDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 tangent = velocityDirection.RotatedBy(MathHelper.PiOver2);
        float spin = Projectile.localAI[0];

        Dust core = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.RedTorch,
            Projectile.velocity * 0.08f, 90, new Color(255, 80, 45), Main.rand.NextFloat(1.15f, 1.45f));
        core.noGravity = true;

        for (int i = 0; i < 2; i++) {
            float angle = spin + MathHelper.Pi * i;
            Vector2 orbitOffset = velocityDirection * (float)Math.Cos(angle) * 6f + tangent * (float)Math.Sin(angle) * 6f;
            Vector2 dustVelocity = tangent * (i == 0 ? 0.7f : -0.7f) + Projectile.velocity * 0.04f;

            Dust orbitDust = Dust.NewDustPerfect(Projectile.Center + orbitOffset, i == 0 ? DustID.Torch : DustID.RedTorch,
                dustVelocity, 110, new Color(255, 175, 95), Main.rand.NextFloat(0.95f, 1.25f));
            orbitDust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 240);
        SpawnImpactDust();
    }

    public override void OnKill(int timeLeft) {
        SpawnImpactDust();
    }

    private void SpawnImpactDust() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 12; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.8f, 2.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.Torch : DustID.RedTorch,
                velocity, 100, new Color(255, 145, 80), Main.rand.NextFloat(1.05f, 1.45f));
            dust.noGravity = true;
        }
    }
}
