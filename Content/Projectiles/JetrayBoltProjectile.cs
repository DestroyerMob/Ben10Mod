using System;
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
    private int FocusTargetIndex => (int)Math.Round(Projectile.ai[1]) - 1;
    private float PathQuality => MathHelper.Clamp(Projectile.ai[2], 0f, 1f);

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
        Lighting.AddLight(Projectile.Center, 0.18f + PathQuality * 0.06f, 0.92f, 1f);

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                Main.rand.NextBool(4) ? DustID.Electric : DustID.BlueTorch, 0f, 0f, 100, new Color(135, 255, 255), 1.1f);
            dust.noGravity = true;
            dust.velocity *= 0.2f;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        bool locked = target.GetGlobalNPC<AlienIdentityGlobalNPC>().IsJetrayLockedFor(Projectile.owner);
        if (locked) {
            modifiers.SourceDamage *= 1.04f + PathQuality * 0.18f;
            modifiers.ArmorPenetration += 5 + (int)Math.Round(PathQuality * 5f);
        }
        else {
            modifiers.SourceDamage *= StrafeLock ? 0.66f + PathQuality * 0.1f : 0.72f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        int lockTime = StrafeLock
            ? 240 + (int)MathHelper.Lerp(0f, 100f, PathQuality)
            : 190;
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyJetrayLock(Projectile.owner, lockTime);
        target.AddBuff(BuffID.Electrified, 160 + (int)MathHelper.Lerp(0f, 50f, PathQuality));
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
        float homingStrength = HomingStrength + (target.GetGlobalNPC<AlienIdentityGlobalNPC>().IsJetrayLockedFor(Projectile.owner)
            ? 0.08f + PathQuality * 0.05f
            : 0f);
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength)
            .SafeNormalize(desiredVelocity.SafeNormalize(Vector2.UnitX)) * speed;
    }

    private NPC FindClosestTarget() {
        NPC focusedTarget = ResolveFocusedTarget();
        if (focusedTarget != null)
            return focusedTarget;

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

    private NPC ResolveFocusedTarget() {
        if (FocusTargetIndex < 0 || FocusTargetIndex >= Main.maxNPCs)
            return null;

        NPC target = Main.npc[FocusTargetIndex];
        if (!target.CanBeChasedBy(Projectile))
            return null;

        if (!target.GetGlobalNPC<AlienIdentityGlobalNPC>().IsJetrayLockedFor(Projectile.owner))
            return null;

        if (Vector2.DistanceSquared(Projectile.Center, target.Center) > HomingRange * HomingRange * 4f)
            return null;

        return target;
    }
}
