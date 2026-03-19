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
    private const float IdleWalkSpeed = 4.6f;
    private const float AttackWalkSpeed = 5.4f;
    private const float Gravity = 0.4f;
    private const float MaxFallSpeed = 10f;
    private const float TeleportCatchupDistance = 520f;

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
        Projectile.tileCollide = true;
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
        NPC target = FindTarget(owner, 620f);
        Vector2 idleCenter = GetIdlePosition(owner, cloneIndex);

        if (target == null && Projectile.Center.Distance(idleCenter) > TeleportCatchupDistance) {
            Projectile.Center = idleCenter;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
        }

        if (target == null) {
            State = StateIdle;
            MoveGrounded(idleCenter, IdleWalkSpeed);
        }
        else {
            State = StateAttack;
            Vector2 screamCenter = GetAttackPosition(target);
            MoveGrounded(screamCenter, AttackWalkSpeed);
        }

        ApplyGroundPhysics();
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
        Vector2 bottom = owner.Bottom + new Vector2(behindDirection * 44f + slot * spacing, 0f);
        return new Vector2(bottom.X, bottom.Y - Projectile.height * 0.5f);
    }

    private Vector2 GetAttackPosition(NPC target) {
        float side = Projectile.Center.X <= target.Center.X ? -1f : 1f;
        Vector2 bottom = target.Bottom + new Vector2(side * 52f, 0f);
        return new Vector2(bottom.X, bottom.Y - Projectile.height * 0.5f);
    }

    private void MoveGrounded(Vector2 targetCenter, float walkSpeed) {
        float horizontalDistance = targetCenter.X - Projectile.Center.X;

        if (Vector2.DistanceSquared(Projectile.Center, targetCenter) > 1600f * 1600f) {
            Projectile.Center = targetCenter;
            Projectile.velocity *= 0.1f;
            Projectile.netUpdate = true;
            return;
        }

        if (Math.Abs(horizontalDistance) > 12f) {
            float desiredX = Math.Sign(horizontalDistance) * walkSpeed;
            Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, desiredX, 0.18f);
        }
        else {
            Projectile.velocity.X *= 0.8f;
        }

        bool grounded = IsGrounded();
        if (grounded && targetCenter.Y + 18f < Projectile.Center.Y && Math.Abs(horizontalDistance) < 28f) {
            Projectile.velocity.Y = -7f;
        }
    }

    private void ApplyGroundPhysics() {
        if (!IsGrounded()) {
            Projectile.velocity.Y = Math.Min(MaxFallSpeed, Projectile.velocity.Y + Gravity);
        }
        else if (Projectile.velocity.Y > 0f) {
            Projectile.velocity.Y = 0f;
        }
    }

    private bool IsGrounded() {
        return Collision.SolidCollision(new Vector2(Projectile.position.X, Projectile.position.Y + 2f),
            Projectile.width, Projectile.height);
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

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (oldVelocity.X != Projectile.velocity.X)
            Projectile.velocity.X = oldVelocity.X * -0.2f;

        if (oldVelocity.Y > 0f)
            Projectile.velocity.Y = 0f;

        return false;
    }
}
