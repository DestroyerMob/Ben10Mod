using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WildVineAnchorProjectile : ModProjectile {
    public const float ModeSeed = 0f;
    public const float ModeSnare = 1f;
    public const float ModeBloom = 2f;
    public const int BaseLifetime = 5 * 60;
    public const int MaxOwnedAnchors = 7;

    private const float MergeRange = 42f;
    private const float BaseRadius = 108f;
    private const float SnareRadius = 132f;
    private const float BloomRadius = 164f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = BaseLifetime;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        if (Projectile.ai[1] <= 0f)
            Projectile.ai[1] = 1f;

        Projectile.rotation += 0.018f + GetMode(Projectile) * 0.006f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.08f, 0.2f, 0.05f));

        if (Main.netMode != NetmodeID.MultiplayerClient)
            PullNearbyEnemies();

        if (Main.dedServ)
            return;

        float radius = GetControlRadius(Projectile);
        int dustChance = GetMode(Projectile) >= ModeBloom ? 1 : 2;
        if (Main.rand.NextBool(dustChance)) {
            Vector2 offset = Main.rand.NextVector2Circular(radius * 0.28f, radius * 0.16f);
            Dust vineDust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool(3) ? DustID.Grass : DustID.JunglePlants,
                new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), Main.rand.NextFloat(-0.65f, -0.15f)),
                100, new Color(110, 205, 80), Main.rand.NextFloat(0.9f, 1.18f));
            vineDust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= GetDamageRadius(Projectile);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<WildVineTethered>(), GetMode(Projectile) >= ModeBloom ? 90 : 60);
        PullTargetTowardAnchor(target, GetMode(Projectile) >= ModeBloom ? 0.55f : 0.35f);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float lifetimeProgress = 1f - Projectile.timeLeft / (float)BaseLifetime;
        float fadeIn = Utils.GetLerpValue(0f, 0.12f, lifetimeProgress, true);
        float fadeOut = Utils.GetLerpValue(0f, 0.22f, Projectile.timeLeft / (float)BaseLifetime, true);
        float opacity = fadeIn * fadeOut;
        float radius = GetControlRadius(Projectile);
        Color ringColor = GetMode(Projectile) >= ModeBloom
            ? new Color(170, 232, 110, 92)
            : new Color(98, 178, 78, 78);

        DrawRing(pixel, center, radius * 0.34f, 3.4f, ringColor * opacity, Projectile.rotation);
        DrawRing(pixel, center, radius * 0.18f, 4.6f, new Color(192, 244, 128, 96) * opacity,
            -Projectile.rotation * 1.3f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(94, 170, 62, 170) * opacity, Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(12f, 34f) * Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(190, 238, 116, 140) * opacity, -Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(8f, 20f) * Projectile.scale, SpriteEffects.None, 0);
        return false;
    }

    public static int CreateOrRefresh(IEntitySource source, Vector2 center, int damage, int owner, float mode = ModeSeed,
        int lifetime = BaseLifetime, float radiusScale = 1f) {
        Projectile existing = FindClosestOwnedAnchor(owner, center, MergeRange);
        if (existing != null) {
            existing.ai[0] = Math.Max(existing.ai[0], mode);
            existing.ai[1] = MathHelper.Clamp(Math.Max(existing.ai[1], radiusScale), 1f, 1.55f);
            existing.damage = Math.Max(existing.damage, damage);
            existing.timeLeft = Math.Max(existing.timeLeft, lifetime);
            existing.netUpdate = true;
            EmitAnchorPulse(existing.Center, GetControlRadius(existing));
            return existing.whoAmI;
        }

        EnforceAnchorLimit(owner);
        int projectileIndex = Projectile.NewProjectile(source, center, Vector2.Zero,
            ModContent.ProjectileType<WildVineAnchorProjectile>(), damage, 0f, owner, mode,
            MathHelper.Clamp(radiusScale, 1f, 1.55f));

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;

        return projectileIndex;
    }

    public static Projectile FindClosestOwnedAnchor(int owner, Vector2 point, float maxDistance) {
        Projectile bestAnchor = null;
        float bestDistance = maxDistance;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile anchor = Main.projectile[i];
            if (!IsOwnedActiveAnchor(anchor, owner))
                continue;

            float distance = Vector2.Distance(anchor.Center, point);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestAnchor = anchor;
        }

        return bestAnchor;
    }

    public static List<Vector2> CollectOwnedAnchorCenters(int owner, Vector2 point, float maxDistance, int maxCount) {
        List<Vector2> centers = new();

        for (int pass = 0; pass < maxCount; pass++) {
            Projectile bestAnchor = null;
            float bestDistance = maxDistance;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile anchor = Main.projectile[i];
                if (!IsOwnedActiveAnchor(anchor, owner) || centers.Contains(anchor.Center))
                    continue;

                float distance = Vector2.Distance(anchor.Center, point);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestAnchor = anchor;
            }

            if (bestAnchor == null)
                break;

            centers.Add(bestAnchor.Center);
        }

        return centers;
    }

    public static bool IsOwnedActiveAnchor(Projectile projectile, int owner) {
        return projectile != null && projectile.active &&
               projectile.type == ModContent.ProjectileType<WildVineAnchorProjectile>() &&
               projectile.owner == owner;
    }

    public static Vector2 ResolveControlPoint(Player owner, Vector2 from, float anchorRange) {
        Projectile anchor = FindClosestOwnedAnchor(owner.whoAmI, from, anchorRange);
        return anchor?.Center ?? owner.MountedCenter;
    }

    private static float GetMode(Projectile anchor) {
        return MathHelper.Clamp(anchor.ai[0], ModeSeed, ModeBloom);
    }

    private static float GetControlRadius(Projectile anchor) {
        float modeRadius = GetMode(anchor) >= ModeBloom ? BloomRadius : GetMode(anchor) >= ModeSnare ? SnareRadius : BaseRadius;
        float scale = MathHelper.Clamp(anchor.ai[1] <= 0f ? 1f : anchor.ai[1], 1f, 1.55f);
        return modeRadius * scale;
    }

    private static float GetDamageRadius(Projectile anchor) {
        return GetControlRadius(anchor) * 0.42f;
    }

    private void PullNearbyEnemies() {
        float radius = GetControlRadius(Projectile);
        float radiusSq = radius * radius;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc == null || !npc.active || !npc.CanBeChasedBy(Projectile))
                continue;

            if (Vector2.DistanceSquared(npc.Center, Projectile.Center) > radiusSq)
                continue;

            if (!Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width,
                    npc.height))
                continue;

            npc.AddBuff(ModContent.BuffType<WildVineTethered>(), 24);
            PullTargetTowardAnchor(npc, GetMode(Projectile) >= ModeBloom ? 0.44f : 0.28f);
        }
    }

    private void PullTargetTowardAnchor(NPC target, float baseStrength) {
        Vector2 toAnchor = Projectile.Center - target.Center;
        float distance = toAnchor.Length();
        if (distance <= 8f)
            return;

        Vector2 pullDirection = toAnchor / distance;
        float resistFactor = target.boss ? 0.18f : MathHelper.Clamp(target.knockBackResist + 0.35f, 0.2f, 1.2f);
        float falloff = MathHelper.Clamp(distance / GetControlRadius(Projectile), 0.25f, 1f);
        target.velocity += pullDirection * baseStrength * resistFactor * falloff;

        float maxPullSpeed = target.boss ? 2.2f : 7.5f;
        float alongPull = Vector2.Dot(target.velocity, pullDirection);
        if (alongPull > maxPullSpeed)
            target.velocity -= pullDirection * (alongPull - maxPullSpeed);

        target.netUpdate = true;
    }

    private static void EnforceAnchorLimit(int owner) {
        int activeAnchors = 0;
        Projectile oldest = null;
        int lowestTimeLeft = int.MaxValue;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile anchor = Main.projectile[i];
            if (!IsOwnedActiveAnchor(anchor, owner))
                continue;

            activeAnchors++;
            if (anchor.timeLeft >= lowestTimeLeft)
                continue;

            lowestTimeLeft = anchor.timeLeft;
            oldest = anchor;
        }

        if (activeAnchors >= MaxOwnedAnchors)
            oldest?.Kill();
    }

    private static void EmitAnchorPulse(Vector2 center, float radius) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 12; i++) {
            Dust pulse = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(radius * 0.22f, radius * 0.12f),
                DustID.JunglePlants, Main.rand.NextVector2Circular(1.2f, 0.8f), 95, new Color(125, 220, 88),
                Main.rand.NextFloat(0.9f, 1.2f));
            pulse.noGravity = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 14;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.1f), SpriteEffects.None, 0f);
        }
    }
}
