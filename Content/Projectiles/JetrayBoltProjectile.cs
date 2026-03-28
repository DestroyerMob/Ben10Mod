using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class JetrayBoltProjectile : ModProjectile {
    private const float HomingRange = 620f;
    private const float HomingStrength = 0.12f;
    private bool StrafeLock => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MartianTurretBolt}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 4;
        Projectile.timeLeft = 120;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        HomeTowardTarget();
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.18f, 0.92f, 1f);

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                Main.rand.NextBool(4) ? DustID.Electric : DustID.BlueTorch, 0f, 0f, 100, new Color(135, 255, 255), 1.1f);
            dust.noGravity = true;
            dust.velocity *= 0.2f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyJetrayLock(Projectile.owner, StrafeLock ? 300 : 220);
        target.AddBuff(BuffID.Electrified, 180);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(2.6f, 2.6f), 100, new Color(170, 255, 255), Main.rand.NextFloat(0.95f, 1.3f));
            dust.noGravity = true;
        }
    }

    private void HomeTowardTarget() {
        NPC target = FindClosestTarget();
        if (target == null)
            return;

        float speed = Projectile.velocity.Length();
        if (speed <= 0.01f)
            speed = 18f;

        Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * speed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, HomingStrength)
            .SafeNormalize(desiredVelocity.SafeNormalize(Vector2.UnitX)) * speed;
    }

    private NPC FindClosestTarget() {
        NPC closestTarget = null;
        float closestDistance = HomingRange;
        bool foundLockedTarget = false;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            bool isLockedTarget = StrafeLock && identity.IsJetrayLockedFor(Projectile.owner);
            if (isLockedTarget) {
                float lockedDistance = Vector2.Distance(Projectile.Center, npc.Center);
                if (lockedDistance < closestDistance) {
                    closestDistance = lockedDistance;
                    closestTarget = npc;
                    foundLockedTarget = true;
                }
                continue;
            }

            if (foundLockedTarget)
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = npc;
        }

        return closestTarget;
    }
}
