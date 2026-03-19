using Microsoft.Xna.Framework;
using System;
using Ben10Mod.Content.Buffs.Summons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoCloneProjectile : ModProjectile {
    public override string Texture => "Ben10Mod/Content/Projectiles/BuzzShockMinionProjectile";

    private ref float AttackTimer => ref Projectile.ai[0];
    private ref float State => ref Projectile.localAI[0];
    private ref float StateTimer => ref Projectile.localAI[1];

    private const int StateIdle = 0;
    private const int StateAttack = 1;
    private const float IdleInertia = 30f;
    private const float IdleSpeed = 8f;
    private const float AttackInertia = 14f;
    private const float AttackSpeed = 12f;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
        Main.projFrames[Type] = 1;
    }

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 52;
        Projectile.friendly = false;
        Projectile.minion = true;
        Projectile.minionSlots = 1f;
        Projectile.netImportant = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            owner.ClearBuff(ModContent.BuffType<EchoEchoCloneBuff>());
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:EchoEcho") {
            Projectile.Kill();
            return;
        }

        if (owner.HasBuff(ModContent.BuffType<EchoEchoCloneBuff>()))
            Projectile.timeLeft = 2;

        int cloneIndex = GetCloneIndex();
        float bob = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4.2f + cloneIndex * 0.65f) * 7f;
        NPC target = FindTarget(owner, 620f);
        Vector2 idleCenter = GetIdlePosition(owner, cloneIndex) + new Vector2(0f, bob);

        if (target == null) {
            State = StateIdle;
            DoIdleMovement(idleCenter);
        }
        else {
            State = StateAttack;
            Vector2 screamCenter = target.Center + new Vector2(target.direction * 54f, -18f + bob);
            DoAttackMovement(screamCenter);
        }

        Projectile.rotation = Projectile.velocity.X * 0.02f;
        Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Firework_Red,
                Main.rand.NextVector2Circular(1f, 1f), 120, new Color(255, 140, 140), 0.9f);
            dust.noGravity = true;
        }

        int attackRate = omp.PrimaryAbilityEnabled ? 22 : 34;
        AttackTimer++;

        if (target != null && AttackTimer >= attackRate && Main.myPlayer == Projectile.owner) {
            AttackTimer = Main.rand.Next(4);
            Vector2 direction = Projectile.Center.DirectionTo(target.Center);
            if (direction != Vector2.Zero) {
                StateTimer = 10f;
                Vector2 velocity = direction * 11f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                    ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(), Projectile.damage, 0f, Projectile.owner);
            }
        }

        if (StateTimer > 0f)
            StateTimer--;
    }

    private int GetCloneIndex() {
        int index = 0;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile other = Main.projectile[i];
            if (!other.active || other.owner != Projectile.owner || other.type != Projectile.type || other.whoAmI == Projectile.whoAmI)
                continue;

            if (other.whoAmI < Projectile.whoAmI)
                index++;
        }

        return index;
    }

    private Vector2 GetIdlePosition(Player owner, int cloneIndex) {
        float spacing = 30f;
        float start = -(owner.ownedProjectileCounts[Type] - 1) * 0.5f;
        float slot = start + cloneIndex;
        float behindDirection = -owner.direction;
        return owner.Center + new Vector2(slot * spacing, -14f) + new Vector2(behindDirection * 44f, 0f);
    }

    private void DoIdleMovement(Vector2 idleCenter) {
        Vector2 toIdle = idleCenter - Projectile.Center;
        float distance = toIdle.Length();

        if (distance > 1600f) {
            Projectile.Center = idleCenter;
            Projectile.velocity *= 0.1f;
            Projectile.netUpdate = true;
            return;
        }

        if (distance > 14f) {
            toIdle.Normalize();
            toIdle *= IdleSpeed;
            Projectile.velocity = (Projectile.velocity * (IdleInertia - 1f) + toIdle) / IdleInertia;
        }
        else if (Projectile.velocity.Length() > 1f) {
            Projectile.velocity *= 0.92f;
        }
    }

    private void DoAttackMovement(Vector2 attackCenter) {
        Vector2 toAttack = attackCenter - Projectile.Center;
        float distance = toAttack.Length();

        if (distance > 8f) {
            toAttack.Normalize();
            toAttack *= AttackSpeed;
            Projectile.velocity = (Projectile.velocity * (AttackInertia - 1f) + toAttack) / AttackInertia;
        }
        else if (Projectile.velocity.Length() > 1f) {
            Projectile.velocity *= 0.88f;
        }
    }

    private NPC FindTarget(Player owner, float maxDistance) {
        NPC closestTarget = null;
        float closestDistance = maxDistance;

        if (owner.HasMinionAttackTargetNPC) {
            NPC forcedTarget = Main.npc[owner.MinionAttackTargetNPC];
            if (forcedTarget.CanBeChasedBy(Projectile) && Projectile.Center.Distance(forcedTarget.Center) < closestDistance)
                return forcedTarget;
        }

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
