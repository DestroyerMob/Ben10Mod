using System.Collections.Generic;
using Ben10Mod.Content.Transformations.EchoEcho;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoCloneProjectile : ModProjectile {
    private const int Width = 40;
    private const int Height = 52;
    private const int LifetimeTicks = 12 * 60;
    private const int TemporaryLifetimeTicks = EchoEchoStatePlayer.ChorusOverloadDurationTicks + 90;
    private const float IdleWalkSpeed = 4.6f;
    private const float AttackWalkSpeed = 5.4f;
    private const float Gravity = 0.4f;
    private const float MaxFallSpeed = 10f;
    private const float TeleportCatchupDistance = 520f;

    private bool Temporary => Projectile.ai[0] >= 0.5f;
    private ref float SpawnOrder => ref Projectile.ai[1];
    private ref float AttackTimer => ref Projectile.localAI[0];
    private ref float StateTimer => ref Projectile.localAI[1];

    public override string Texture => "Ben10Mod/Content/Projectiles/BuzzShockMinionProjectile";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
        Main.projFrames[Type] = 1;
    }

    public override void SetDefaults() {
        Projectile.width = Width;
        Projectile.height = Height;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.minion = true;
        Projectile.minionSlots = 0f;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = false;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != EchoEchoStatePlayer.TransformationId) {
            Projectile.Kill();
            return;
        }

        if (Temporary && !owner.GetModPlayer<EchoEchoStatePlayer>().ChorusActive)
            Projectile.timeLeft = System.Math.Min(Projectile.timeLeft, 15);

        int cloneIndex = GetCloneIndex();
        NPC target = FindTarget(owner, 620f);
        Vector2 idleCenter = GetIdlePosition(owner, cloneIndex);

        if (target == null && Projectile.Center.Distance(idleCenter) > TeleportCatchupDistance) {
            Projectile.Center = idleCenter;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
        }

        if (target == null) {
            MoveGrounded(idleCenter, IdleWalkSpeed);
        }
        else {
            Vector2 attackCenter = GetAttackPosition(target);
            MoveGrounded(attackCenter, AttackWalkSpeed);
        }

        ApplyGroundPhysics();
        Projectile.rotation = Projectile.velocity.X * 0.02f;
        Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

        EchoEchoStatePlayer state = owner.GetModPlayer<EchoEchoStatePlayer>();
        int attackRate = state.ChorusActive ? 18 : 30;
        AttackTimer++;

        if (target != null && AttackTimer >= attackRate && Main.myPlayer == Projectile.owner) {
            AttackTimer = Main.rand.Next(5);
            Vector2 direction = Projectile.Center.DirectionTo(target.Center);
            if (direction != Vector2.Zero) {
                StateTimer = 10f;
                int projectileDamage = ResolveSonicDamage(owner, cloneIndex, state);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 11f,
                    ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(), projectileDamage, 0f, Projectile.owner,
                    cloneIndex + 1, 0f);
            }
        }

        if (StateTimer > 0f)
            StateTimer--;

        Lighting.AddLight(Projectile.Center, new Vector3(0.28f, 0.42f, 0.65f) * 0.42f);
        if (Main.dedServ || Main.rand.NextBool(3))
            return;

        Vector2 dustVelocity = Main.rand.NextVector2Circular(0.25f, 0.55f) + new Vector2(0f, -0.45f);
        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 15f), DustID.GemSapphire,
            dustVelocity, 110, Temporary ? new Color(165, 240, 255) : new Color(188, 220, 255),
            Temporary ? 0.95f : 0.82f);
        dust.noGravity = true;
    }

    public static bool TryPlaceOrMoveEcho(Player owner, IEntitySource source, Vector2 desiredCenter, int damage,
        float knockback, int maxEchoes, bool temporary) {
        List<Projectile> echoes = GetOwnedEchoProjectiles(owner);
        if (echoes.Count >= maxEchoes && echoes.Count > 0)
            echoes[0].Kill();

        return SpawnEcho(owner, source, desiredCenter, damage, knockback, temporary) >= 0;
    }

    public static int SpawnEcho(Player owner, IEntitySource source, Vector2 desiredCenter, int damage,
        float knockback, bool temporary) {
        if (Main.netMode == NetmodeID.MultiplayerClient && owner.whoAmI != Main.myPlayer)
            return -1;

        Vector2 center = ResolvePlacementCenter(owner, desiredCenter);
        float spawnOrder = GetNextSpawnOrder(owner);
        int projectileIndex = Projectile.NewProjectile(source, center, Vector2.Zero,
            ModContent.ProjectileType<EchoEchoCloneProjectile>(), damage, knockback, owner.whoAmI,
            temporary ? 1f : 0f, spawnOrder);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Projectile projectile = Main.projectile[projectileIndex];
            projectile.timeLeft = temporary ? TemporaryLifetimeTicks : LifetimeTicks;
            projectile.originalDamage = damage;
            projectile.netUpdate = true;
        }

        return projectileIndex;
    }

    public static int CountOwnedEchoes(Player owner) {
        int count = 0;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (IsOwnedEcho(projectile, owner))
                count++;
        }

        return count;
    }

    public static Projectile FindNearestOwnedEcho(Player owner) {
        Projectile bestEcho = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsOwnedEcho(projectile, owner))
                continue;

            float distance = Vector2.DistanceSquared(owner.Center, projectile.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestEcho = projectile;
        }

        return bestEcho;
    }

    public static List<Projectile> GetOwnedEchoProjectiles(Player owner) {
        List<Projectile> echoes = new();
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (IsOwnedEcho(projectile, owner))
                echoes.Add(projectile);
        }

        echoes.Sort(static (left, right) => left.ai[1].CompareTo(right.ai[1]));
        return echoes;
    }

    public static void CollapseTemporaryEchoes(Player owner, bool spawnCollapseBursts) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsOwnedEcho(projectile, owner) || projectile.ai[0] < 0.5f)
                continue;

            if (spawnCollapseBursts) {
                int burstDamage = EchoEchoTransformation.ResolveHeroDamage(owner, 0.52f);
                EchoEchoTransformation.SpawnResonancePop(owner, owner.GetSource_FromThis(), projectile.Center,
                    burstDamage, 3.2f, 0.82f, allowChorusChain: false);
            }

            projectile.Kill();
        }
    }

    public static void PruneOwnedEchoes(Player owner, int maxEchoes) {
        List<Projectile> echoes = GetOwnedEchoProjectiles(owner);
        while (echoes.Count > maxEchoes) {
            echoes[0].Kill();
            echoes.RemoveAt(0);
        }
    }

    public static Vector2 ResolvePlacementCenter(Player owner, Vector2 desiredCenter) {
        Vector2[] offsets = {
            Vector2.Zero,
            new Vector2(0f, -28f),
            new Vector2(32f, 0f),
            new Vector2(-32f, 0f),
            new Vector2(0f, 28f),
            new Vector2(48f, -24f),
            new Vector2(-48f, -24f),
            new Vector2(56f, 18f),
            new Vector2(-56f, 18f)
        };

        for (int i = 0; i < offsets.Length; i++) {
            Vector2 candidateCenter = desiredCenter + offsets[i];
            Vector2 topLeft = candidateCenter - new Vector2(Width * 0.5f, Height * 0.5f);
            if (!Collision.SolidCollision(topLeft, Width, Height))
                return candidateCenter;
        }

        return owner.Center + new Vector2(owner.direction * 58f, -12f);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (oldVelocity.X != Projectile.velocity.X)
            Projectile.velocity.X = oldVelocity.X * -0.2f;

        if (oldVelocity.Y > 0f)
            Projectile.velocity.Y = 0f;

        return false;
    }

    private static bool IsOwnedEcho(Projectile projectile, Player owner) {
        return projectile.active &&
               projectile.owner == owner.whoAmI &&
               projectile.type == ModContent.ProjectileType<EchoEchoCloneProjectile>();
    }

    private static float GetNextSpawnOrder(Player owner) {
        float nextOrder = 1f;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsOwnedEcho(projectile, owner))
                continue;

            nextOrder = System.Math.Max(nextOrder, projectile.ai[1] + 1f);
        }

        return nextOrder;
    }

    private int GetCloneIndex() {
        int index = 0;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile other = Main.projectile[i];
            if (!other.active || other.owner != Projectile.owner || other.type != Projectile.type || other.whoAmI == Projectile.whoAmI)
                continue;

            if (other.ai[1] < SpawnOrder || (other.ai[1] == SpawnOrder && other.whoAmI < Projectile.whoAmI))
                index++;
        }

        return index;
    }

    private Vector2 GetIdlePosition(Player owner, int cloneIndex) {
        float spacing = 30f;
        float start = -(CountOwnedEchoes(owner) - 1) * 0.5f;
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

        if (System.Math.Abs(horizontalDistance) > 12f) {
            float desiredX = System.Math.Sign(horizontalDistance) * walkSpeed;
            Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, desiredX, 0.18f);
        }
        else {
            Projectile.velocity.X *= 0.8f;
        }

        bool grounded = IsGrounded();
        bool blockedForward = Collision.SolidCollision(
            new Vector2(Projectile.position.X + System.Math.Sign(Projectile.velocity.X) * 8f, Projectile.position.Y),
            Projectile.width, Projectile.height);
        if (grounded && blockedForward && System.Math.Abs(Projectile.velocity.X) > 0.2f) {
            Projectile.position.Y -= 14f;
            Projectile.velocity.Y = -5f;
            Projectile.netUpdate = true;
        }

        if (grounded && targetCenter.Y + 18f < Projectile.Center.Y && System.Math.Abs(horizontalDistance) < 28f)
            Projectile.velocity.Y = -7f;
    }

    private void ApplyGroundPhysics() {
        if (!IsGrounded()) {
            Projectile.velocity.Y = System.Math.Min(MaxFallSpeed, Projectile.velocity.Y + Gravity);
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

    private int ResolveSonicDamage(Player owner, int cloneIndex, EchoEchoStatePlayer state) {
        float multiplier = EchoEchoTransformation.FirstEchoDamageMultiplier;
        if (cloneIndex == 1)
            multiplier = EchoEchoTransformation.SecondEchoDamageMultiplier;
        else if (cloneIndex >= 2)
            multiplier = EchoEchoTransformation.ChorusEchoDamageMultiplier;

        if (state.ChorusActive)
            multiplier *= 1.18f;

        return EchoEchoTransformation.ResolveHeroDamage(owner, 0.74f * multiplier);
    }
}
