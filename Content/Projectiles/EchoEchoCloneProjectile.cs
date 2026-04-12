using System;
using System.Collections.Generic;
using Ben10Mod.Content.Transformations.EchoEcho;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoCloneProjectile : ModProjectile {
    private const int Width = 30;
    private const int Height = 40;
    private const int LifetimeTicks = 12 * 60;
    private const int TemporaryLifetimeTicks = EchoEchoStatePlayer.ChorusOverloadDurationTicks + 90;

    private bool Temporary => Projectile.ai[0] >= 0.5f;
    private float SpawnOrder => Projectile.ai[1];

    public override string Texture => "Ben10Mod/Content/Projectiles/BuzzShockMinionProjectile";

    public override void SetDefaults() {
        Projectile.width = Width;
        Projectile.height = Height;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = false;
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

        Projectile.velocity *= 0.9f;
        Projectile.rotation = 0f;
        Projectile.spriteDirection = owner.Center.X <= Projectile.Center.X ? -1 : 1;

        if (Temporary && !owner.GetModPlayer<EchoEchoStatePlayer>().ChorusActive)
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, 15);

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
}
