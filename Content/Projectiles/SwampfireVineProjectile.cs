using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SwampfireVineProjectile : ModProjectile {
    public const int DefaultHeight = 126;
    public const int BaseLifetime = 12 * 60;
    public const int MaxOwnedPods = 7;

    private const float VineWaveAmplitude = 16f;
    private const float VineWaveFrequency = 1.65f;
    private const float VineWaveSecondaryAmplitude = 5f;
    private const float VineWaveSpeed = 0.14f;
    private const float BaseGrowthPerTick = 1f / (2.4f * 60f);
    private const float MergeRadius = 58f;

    public override string Texture => "Terraria/Images/Projectile_0";

    private bool IsIgnited => Projectile.ai[1] >= 1f;
    private bool UltimateIgnition => Projectile.ai[1] >= 2f;
    private float Growth => MathHelper.Clamp(Projectile.ai[0], 0.12f, 1.2f);
    private float GasRadius => MathHelper.Lerp(52f, 132f, MathHelper.Clamp(Growth, 0f, 1f));
    private float IgnitionRadius => MathHelper.Lerp(64f, UltimateIgnition ? 176f : 126f, MathHelper.Clamp(Growth, 0f, 1f));

    public override void SetDefaults() {
        Projectile.width = 68;
        Projectile.height = DefaultHeight;
        Projectile.friendly = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = BaseLifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        Projectile.frameCounter++;

        if (Projectile.ai[0] <= 0f) {
            Projectile.ai[0] = 0.18f;
            Projectile.netUpdate = true;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SpawnSproutDust();
        }

        if (IsIgnited) {
            Projectile.friendly = true;
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.48f, 0.12f) * (UltimateIgnition ? 1.35f : 0.95f));
            SpawnIgnitionDust();
            return;
        }

        Projectile.friendly = false;
        float previousGrowth = Projectile.ai[0];
        Projectile.ai[0] = MathHelper.Clamp(Projectile.ai[0] + BaseGrowthPerTick, 0.12f, 1f);
        if (Math.Abs(Projectile.ai[0] - previousGrowth) > 0.08f)
            Projectile.netUpdate = true;

        Lighting.AddLight(Projectile.Center, new Vector3(0.34f, 0.85f, 0.18f) * MathHelper.Lerp(0.45f, 0.85f, Growth));
        SpawnGrowthDust();

        if (Projectile.frameCounter % 10 == 0)
            ApplyFuelVapourAura();
    }

    private void SpawnSproutDust() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 18; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Bottom + new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-12f, 4f)),
                i % 3 == 0 ? DustID.Torch : DustID.Grass, Main.rand.NextVector2Circular(1.4f, 1.4f), 90,
                Color.White, 1.15f);
            dust.noGravity = true;
        }
    }

    private void SpawnGrowthDust() {
        if (Main.dedServ)
            return;

        int vineDustCount = Growth >= 0.72f ? 6 : 4;
        for (int i = 0; i < vineDustCount; i++)
            SpawnVineDust(Main.rand.NextFloat(), Main.rand.NextBool() ? DustID.Grass : DustID.JunglePlants,
                new Color(130, 230, 90), MathHelper.Lerp(0.75f, 1.08f, Growth), 6f);

        if (Main.rand.NextBool())
            SpawnVineDust(Main.rand.NextFloat(), DustID.JungleGrass, new Color(110, 215, 90), 0.95f, 9f,
                phaseOffset: 0.55f, amplitudeScale: 1.08f);

        if (!Main.rand.NextBool(3))
            return;

        float emberProgress = Main.rand.NextFloat(0.02f, 0.35f);
        Vector2 emberPos = GetVineCurvePoint(emberProgress, phaseOffset: 0.25f, amplitudeScale: 0.7f)
            + Main.rand.NextVector2Circular(4f, 6f);
        Dust ember = Dust.NewDustPerfect(emberPos, DustID.Torch,
            GetVineCurveTangent(emberProgress, phaseOffset: 0.25f, amplitudeScale: 0.7f) * Main.rand.NextFloat(0.15f, 0.45f)
            + Main.rand.NextVector2Circular(0.22f, 0.22f), 120, new Color(255, 150, 70), 0.9f);
        ember.noGravity = true;
    }

    private void SpawnIgnitionDust() {
        if (Main.dedServ)
            return;

        int count = UltimateIgnition ? 12 : 8;
        for (int i = 0; i < count; i++) {
            Vector2 offset = Main.rand.NextVector2Circular(IgnitionRadius * 0.45f, DefaultHeight * 0.48f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, i % 3 == 0 ? DustID.Grass : DustID.Torch,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 95,
                i % 3 == 0 ? new Color(130, 235, 85) : new Color(255, 135, 45),
                Main.rand.NextFloat(0.9f, UltimateIgnition ? 1.45f : 1.18f));
            dust.noGravity = true;
        }
    }

    private void ApplyFuelVapourAura() {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int fuelType = ModContent.BuffType<FuelVapour>();
        float radius = GasRadius;
        Vector2 gasCenter = Projectile.Center + new Vector2(0f, -DefaultHeight * 0.08f);
        int buffTime = Growth >= 0.82f ? 150 : 90;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.friendly || npc.dontTakeDamage || !npc.CanBeChasedBy(Projectile))
                continue;

            float allowedDistance = radius + Math.Max(npc.width, npc.height) * 0.45f;
            if (Vector2.DistanceSquared(gasCenter, npc.Center) > allowedDistance * allowedDistance)
                continue;

            npc.AddBuff(fuelType, buffTime);
        }
    }

    private Vector2 GetVineCurvePoint(float progress, float phaseOffset = 0f, float amplitudeScale = 1f) {
        progress = MathHelper.Clamp(progress, 0f, 1f);

        float waveTime = Projectile.frameCounter * VineWaveSpeed + phaseOffset;
        float vineHeight = Projectile.height - 8f;
        float swayStrength = 0.2f + 0.8f * progress;
        float primaryOffset = MathF.Sin(progress * MathHelper.TwoPi * VineWaveFrequency + waveTime) *
            VineWaveAmplitude * amplitudeScale * swayStrength;
        float secondaryOffset = MathF.Sin(progress * MathHelper.TwoPi * 0.75f - waveTime * 0.7f) *
            VineWaveSecondaryAmplitude * amplitudeScale * progress;

        return Projectile.Bottom + new Vector2(primaryOffset + secondaryOffset, -progress * vineHeight);
    }

    private Vector2 GetVineCurveTangent(float progress, float phaseOffset = 0f, float amplitudeScale = 1f) {
        float step = 0.02f;
        Vector2 start = GetVineCurvePoint(progress - step, phaseOffset, amplitudeScale);
        Vector2 end = GetVineCurvePoint(progress + step, phaseOffset, amplitudeScale);
        Vector2 tangent = end - start;

        return tangent.LengthSquared() > 0.001f ? Vector2.Normalize(tangent) : -Vector2.UnitY;
    }

    private void SpawnVineDust(float progress, int dustType, Color color, float scale, float width, float phaseOffset = 0f, float amplitudeScale = 1f) {
        Vector2 curvePoint = GetVineCurvePoint(progress, phaseOffset, amplitudeScale);
        Vector2 tangent = GetVineCurveTangent(progress, phaseOffset, amplitudeScale);
        Vector2 normal = new(-tangent.Y, tangent.X);
        Vector2 dustPos = curvePoint + normal * Main.rand.NextFloat(-width, width);
        Vector2 drift = tangent * Main.rand.NextFloat(0.12f, 0.35f) + normal * Main.rand.NextFloat(-0.16f, 0.16f);

        Dust dust = Dust.NewDustPerfect(dustPos, dustType, drift, 120, color, scale);
        dust.noGravity = true;
    }

    public override bool? CanHitNPC(NPC target) {
        return IsIgnited && !target.friendly && target.CanBeChasedBy(Projectile);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (!IsIgnited)
            return false;

        Vector2 closest = new(
            MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
            MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));

        return Vector2.DistanceSquared(Projectile.Center, closest) <= IgnitionRadius * IgnitionRadius;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (target.HasBuff(ModContent.BuffType<FuelVapour>()))
            modifiers.FinalDamage *= UltimateIgnition ? 1.32f : 1.16f;

        modifiers.ArmorPenetration += UltimateIgnition ? 12 : 6;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 basePos = Projectile.Bottom - Main.screenPosition;
        float growth = MathHelper.Clamp(Growth, 0f, 1f);
        float fade = IsIgnited ? Utils.GetLerpValue(0f, 10f, Projectile.timeLeft, true) : Utils.GetLerpValue(0f, 30f, Projectile.timeLeft, true);
        float opacity = Utils.GetLerpValue(0.05f, 0.25f, growth, true) * fade;
        Color outer = IsIgnited ? new Color(190, 80, 28, 235) : new Color(40, 120, 28, 225);
        Color inner = IsIgnited ? new Color(255, 170, 60, 230) : new Color(145, 255, 120, 205);
        float heightScale = MathHelper.Lerp(0.45f, 1f, growth);

        for (int i = -4; i <= 4; i++) {
            float bend = i * 8f;
            float width = (i == 0 ? 18f : (Math.Abs(i) == 1 ? 14f : 11f)) * MathHelper.Lerp(0.72f, 1.18f, growth);
            float height = (i == 0 ? 118f : (Math.Abs(i) == 1 ? 110f : 102f)) * heightScale;
            Vector2 center = basePos + new Vector2(bend, -height * 0.5f);

            Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), outer * opacity,
                0.08f * i, new Vector2(0.5f, 0.5f), new Vector2(width, height), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(pixel, center + new Vector2(0f, -6f), new Rectangle(0, 0, 1, 1), inner * opacity,
                0.08f * i, new Vector2(0.5f, 0.5f), new Vector2(width * 0.42f, height * 0.82f), SpriteEffects.None, 0f);
        }

        for (int i = -3; i <= 3; i++) {
            Vector2 leafCenter = basePos + new Vector2(i * 12f, (-58f - Math.Abs(i) * 8f) * heightScale);
            Main.spriteBatch.Draw(pixel, leafCenter, new Rectangle(0, 0, 1, 1), new Color(90, 200, 70, 180) * opacity,
                0.65f * i, new Vector2(0.5f, 0.5f), new Vector2(8f, 22f) * MathHelper.Lerp(0.75f, 1.08f, growth), SpriteEffects.None, 0f);
        }

        Color podColor = IsIgnited ? new Color(255, 120, 35, 210) : new Color(255, 135, 50, 170);
        Main.spriteBatch.Draw(pixel, basePos + new Vector2(0f, -10f), new Rectangle(0, 0, 1, 1), podColor * opacity,
            0f, new Vector2(0.5f, 0.5f), new Vector2(38f, 14f) * MathHelper.Lerp(0.85f, 1.32f, growth), SpriteEffects.None, 0f);

        if (!IsIgnited && growth > 0.55f) {
            float radius = GasRadius * 0.55f;
            Color gasColor = new Color(120, 205, 80, 42) * Utils.GetLerpValue(0.55f, 1f, growth, true) * fade;
            Main.spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition + new Vector2(0f, -DefaultHeight * 0.08f),
                new Rectangle(0, 0, 1, 1), gasColor, 0f, new Vector2(0.5f, 0.5f),
                new Vector2(radius * 2f, radius * 0.72f), SpriteEffects.None, 0f);
        }

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, UltimateIgnition ? 420 : 260);
        target.AddBuff(ModContent.BuffType<FuelVapour>(), 90);

        if (UltimateIgnition)
            target.AddBuff(BuffID.Poisoned, 210);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        int count = IsIgnited ? 24 : 14;
        for (int i = 0; i < count; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 50f),
                i % 2 == 0 ? DustID.Grass : DustID.Torch, Main.rand.NextVector2Circular(IsIgnited ? 3.2f : 2.2f, IsIgnited ? 3.2f : 2.2f),
                120, Color.White, IsIgnited ? 1.22f : 1.05f);
            dust.noGravity = true;
        }
    }

    public static int CreateOrGrow(IEntitySource source, Vector2 center, int damage, int owner, float startingGrowth, int refreshTime) {
        Projectile nearbyPod = FindOwnedPodNear(owner, center, MergeRadius);
        if (nearbyPod != null) {
            GrowProjectile(nearbyPod, startingGrowth, refreshTime, damage);
            return nearbyPod.whoAmI;
        }

        if (CountOwnedPods(owner) >= MaxOwnedPods) {
            Projectile weakestPod = FindWeakestOwnedPod(owner);
            if (weakestPod != null) {
                GrowProjectile(weakestPod, startingGrowth * 0.85f, refreshTime, damage);
                return weakestPod.whoAmI;
            }
        }

        int index = Projectile.NewProjectile(source, center, Vector2.Zero,
            ModContent.ProjectileType<SwampfireVineProjectile>(), damage, 0f, owner,
            MathHelper.Clamp(startingGrowth, 0.12f, 1f), 0f);

        if (index >= 0 && index < Main.maxProjectiles) {
            Projectile pod = Main.projectile[index];
            pod.originalDamage = damage;
            pod.timeLeft = Math.Max(pod.timeLeft, refreshTime);
            pod.netUpdate = true;
        }

        return index;
    }

    public static int GrowOwnedPods(int owner, float growth, int refreshTime = 180) {
        int grown = 0;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsActiveOwnedPod(projectile, owner) || projectile.ai[1] >= 1f)
                continue;

            GrowProjectile(projectile, growth, refreshTime, projectile.damage);
            grown++;
        }

        return grown;
    }

    public static int CountOwnedPods(int owner) {
        int count = 0;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (IsActiveOwnedPod(projectile, owner) && projectile.ai[1] < 1f)
                count++;
        }

        return count;
    }

    public static int IgniteOwnedPods(int owner, IEntitySource source, int damage, bool ultimate) {
        int ignited = 0;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsActiveOwnedPod(projectile, owner) || projectile.ai[1] >= 1f)
                continue;

            IgniteProjectile(projectile, source, damage, ultimate);
            ignited++;
        }

        return ignited;
    }

    private static bool IsActiveOwnedPod(Projectile projectile, int owner) {
        return projectile.active &&
               projectile.owner == owner &&
               projectile.type == ModContent.ProjectileType<SwampfireVineProjectile>();
    }

    private static Projectile FindOwnedPodNear(int owner, Vector2 center, float radius) {
        float radiusSq = radius * radius;
        Projectile nearest = null;
        float nearestDistance = radiusSq;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsActiveOwnedPod(projectile, owner) || projectile.ai[1] >= 1f)
                continue;

            float distance = Vector2.DistanceSquared(projectile.Center, center);
            if (distance > nearestDistance)
                continue;

            nearest = projectile;
            nearestDistance = distance;
        }

        return nearest;
    }

    private static Projectile FindWeakestOwnedPod(int owner) {
        Projectile weakest = null;
        float weakestScore = float.MaxValue;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsActiveOwnedPod(projectile, owner) || projectile.ai[1] >= 1f)
                continue;

            float score = projectile.ai[0] + projectile.timeLeft / (float)BaseLifetime;
            if (score >= weakestScore)
                continue;

            weakest = projectile;
            weakestScore = score;
        }

        return weakest;
    }

    private static void GrowProjectile(Projectile projectile, float growth, int refreshTime, int damage) {
        projectile.ai[0] = MathHelper.Clamp(projectile.ai[0] + Math.Max(0f, growth), 0.12f, 1f);
        projectile.timeLeft = Math.Max(projectile.timeLeft, refreshTime);
        projectile.damage = Math.Max(projectile.damage, damage);
        projectile.originalDamage = Math.Max(projectile.originalDamage, damage);
        projectile.netUpdate = true;
    }

    private static void IgniteProjectile(Projectile projectile, IEntitySource source, int damage, bool ultimate) {
        projectile.ai[1] = ultimate ? 2f : 1f;
        projectile.damage = Math.Max(projectile.damage, damage);
        projectile.originalDamage = Math.Max(projectile.originalDamage, damage);
        projectile.timeLeft = Math.Min(projectile.timeLeft, ultimate ? 42 : 34);
        projectile.netUpdate = true;

        float growth = MathHelper.Clamp(projectile.ai[0], 0.2f, 1f);
        float radiusScale = ultimate
            ? MathHelper.Lerp(1.55f, 2.7f, growth)
            : MathHelper.Lerp(0.95f, 1.75f, growth);
        int burstDamage = Math.Max(1, (int)Math.Round(damage * (ultimate ? 0.92f : 0.68f)));
        int burstIndex = Projectile.NewProjectile(source, projectile.Center + new Vector2(0f, -DefaultHeight * 0.08f),
            Vector2.Zero, ModContent.ProjectileType<SwampfireIgnitionBurstProjectile>(), burstDamage,
            ultimate ? 3.4f : 2.1f, projectile.owner, radiusScale, ultimate ? 1f : 0f);

        if (burstIndex >= 0 && burstIndex < Main.maxProjectiles)
            Main.projectile[burstIndex].netUpdate = true;
    }
}
