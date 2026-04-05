using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GrayMatterNeuronProjectile : ModProjectile {
    private const float BaseHomingRange = 380f;
    private const float FocusedHomingRange = 520f;
    private const float BaseHomingStrength = 0.06f;
    private const float FocusedHomingStrength = 0.11f;

    private bool Hyperfocused => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MartianTurretBolt}";

    public override void SetDefaults() {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 100;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.scale = Hyperfocused ? 1.08f : 0.95f;
        }

        ApplyHoming();
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, Hyperfocused ? 0.18f : 0.1f, 0.42f, 0.18f);

        if (Main.rand.NextBool(Hyperfocused ? 2 : 3)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center,
                Main.rand.NextBool(4) ? DustID.Electric : DustID.GreenTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.1f), 100,
                Hyperfocused ? new Color(175, 255, 170) : new Color(145, 235, 155),
                Main.rand.NextFloat(0.82f, 1.08f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), Hyperfocused ? 180 : 120);
        if (Hyperfocused)
            target.AddBuff(BuffID.Confused, 60);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center,
                i % 2 == 0 ? DustID.GreenTorch : DustID.Electric,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 95,
                Hyperfocused ? new Color(190, 255, 180) : new Color(155, 235, 160),
                Main.rand.NextFloat(0.85f, 1.1f));
            dust.noGravity = true;
        }
    }

    private void ApplyHoming() {
        NPC target = FindClosestNPC(Hyperfocused ? FocusedHomingRange : BaseHomingRange);
        if (target == null)
            return;

        float speed = Projectile.velocity.Length();
        if (speed <= 0.01f)
            speed = Hyperfocused ? 18f : 16f;

        Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * speed;
        float turnSpeed = Hyperfocused ? FocusedHomingStrength : BaseHomingStrength;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, turnSpeed)
            .SafeNormalize(desiredVelocity.SafeNormalize(Vector2.UnitX)) * speed;
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
}
