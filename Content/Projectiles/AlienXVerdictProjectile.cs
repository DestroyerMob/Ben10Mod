using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AlienXVerdictProjectile : ModProjectile {
    private const int LifetimeTicks = 52;
    private const int ChargeTicks = 16;
    private const int BurnOnContactTime = 300;
    private const int BurnOnDetonationTime = 420;
    private const float StartRadius = 18f;
    private const float ChargeRadius = 54f;
    private const float BaseMaxRadius = 320f;
    private const float DeliberationRadiusBonus = 60f;
    private const int BaseDustPoints = 24;
    private const int MaxDustPoints = 68;
    private bool Deliberation => Projectile.ai[0] >= 0.5f;
    private bool IsCharging => Timer < ChargeTicks;

    private float CurrentRadius {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private float PreviousRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float Timer {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 44;
        Projectile.height = 44;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        Projectile.velocity = Vector2.Zero;
        owner.velocity *= IsCharging ? 0.82f : 0.9f;
        owner.noKnockback = true;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, 6);

        if (Timer == 0f) {
            SpawnIgnitionBurst(owner);
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.4f, Volume = 0.95f }, owner.Center);
        }
        Timer++;

        float maxRadius = BaseMaxRadius + (Deliberation ? DeliberationRadiusBonus : 0f);
        float radius;

        if (IsCharging) {
            float chargeProgress = Utils.GetLerpValue(0f, ChargeTicks, Timer, true);
            float easedCharge = 1f - MathF.Pow(1f - chargeProgress, 2f);
            radius = MathHelper.Lerp(StartRadius, ChargeRadius, easedCharge);
            SpawnChargingDust(radius);
        }
        else {
            if (Timer == ChargeTicks) {
                SpawnEruptionBurst(owner);
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.18f, Volume = 1.15f }, owner.Center);
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.25f, Volume = 0.9f }, owner.Center);
            }

            float eruptionProgress = Utils.GetLerpValue(ChargeTicks, LifetimeTicks, Timer, true);
            float easedEruption = 1f - MathF.Pow(1f - eruptionProgress, 3.2f);
            radius = MathHelper.Lerp(ChargeRadius, maxRadius, easedEruption);
            SpawnSupernovaDust(radius, PreviousRadius);
        }

        PreviousRadius = radius;
        CurrentRadius = radius;

        Lighting.AddLight(Projectile.Center, IsCharging
            ? new Vector3(1.15f, 0.7f, 0.28f)
            : new Vector3(1.85f, 1.05f, 0.36f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        int judgement = target.GetGlobalNPC<AlienIdentityGlobalNPC>().GetAlienXJudgementStacks(Projectile.owner);
        float eruptionBonus = IsCharging ? 0f : 0.45f;
        modifiers.SourceDamage *= 1f + judgement * 0.12f + (Deliberation ? 0.12f : 0f) + eruptionBonus;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int judgement = identity.GetAlienXJudgementStacks(Projectile.owner);
        identity.ApplyAlienXJudgement(Projectile.owner, judgement >= 2 ? 3 : 2, Deliberation ? 360 : 300);
        identity.ApplyAlienXStasis(Projectile.owner, Deliberation ? 86 : 68);
        target.AddBuff(ModContent.BuffType<AlienXSupernovaBurn>(), BurnOnContactTime);

        Vector2 blastDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
        float blastForce = (IsCharging ? 6.5f : 13.5f) + judgement * 1.35f;
        target.velocity = Vector2.Lerp(target.velocity, blastDirection * blastForce, target.boss ? 0.12f : 0.48f);
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (!Main.dedServ) {
            for (int i = 0; i < 72; i++) {
                Vector2 burstVelocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.2f, 8.2f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(CurrentRadius * 0.14f, CurrentRadius * 0.14f),
                    i % 4 == 0 ? DustID.WhiteTorch : i % 3 == 0 ? DustID.Flare : i % 2 == 0 ? DustID.GoldFlame : DustID.Torch,
                    burstVelocity,
                    100,
                    Color.Lerp(new Color(255, 170, 90), new Color(255, 248, 220), Main.rand.NextFloat()),
                    Main.rand.NextFloat(1.2f, 2f));
                dust.noGravity = true;
            }
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        float finalRadius = Math.Max(CurrentRadius, BaseMaxRadius + (Deliberation ? DeliberationRadiusBonus : 0f));
        Player owner = Main.player[Projectile.owner];
        int detonationBaseDamage = Math.Max(1, (int)Math.Round(Projectile.damage * (Deliberation ? 2.35f : 1.95f)));

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance > finalRadius || distance <= 8f)
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            identity.ApplyAlienXJudgement(Projectile.owner, 2, 360);
            identity.ApplyAlienXStasis(Projectile.owner, Deliberation ? 98 : 76);
            npc.AddBuff(ModContent.BuffType<AlienXSupernovaBurn>(), BurnOnDetonationTime);

            Vector2 blastDirection = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            float proximity = 1f - distance / finalRadius;
            float blastForce = MathHelper.Lerp(10f, Deliberation ? 28f : 22f, proximity);
            npc.velocity = Vector2.Lerp(npc.velocity, blastDirection * blastForce, npc.boss ? 0.1f : 0.42f);
            int detonationDamage = Math.Max(1, (int)Math.Round(detonationBaseDamage * MathHelper.Lerp(0.8f, 1.35f, proximity)));
            npc.SimpleStrikeNPC(detonationDamage, owner.direction, false, 0f, ModContent.GetInstance<HeroDamage>());
            npc.netUpdate = true;
        }
    }

    private void SpawnIgnitionBurst(Player owner) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 38; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(4.8f, 4.8f);
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.WhiteTorch,
                velocity, 90, new Color(255, 245, 230), Main.rand.NextFloat(1.3f, 1.95f));
            dust.noGravity = true;
        }

        for (int i = 0; i < 26; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(3.8f, 3.8f);
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(14f, 14f), i % 3 == 0 ? DustID.Torch : i % 2 == 0 ? DustID.Flare : DustID.GoldFlame,
                velocity, 100, new Color(255, 185, 105), Main.rand.NextFloat(1.05f, 1.55f));
            dust.noGravity = true;
        }

        SpawnFourPointStarDust(owner.Center, 42f, 0.7f, 4, 1.25f);
    }

    private void SpawnEruptionBurst(Player owner) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 54; i++) {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3.4f, 9f);
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(24f, 24f),
                i % 4 == 0 ? DustID.WhiteTorch : i % 3 == 0 ? DustID.Flare : i % 2 == 0 ? DustID.GoldFlame : DustID.Torch,
                velocity, 85, Color.Lerp(new Color(255, 160, 75), new Color(255, 250, 235), Main.rand.NextFloat()),
                Main.rand.NextFloat(1.25f, 2.1f));
            dust.noGravity = true;
        }

        SpawnFourPointStarDust(owner.Center, 128f, 1.15f, 8, 1.95f);
    }

    private void SpawnChargingDust(float radius) {
        if (Main.dedServ)
            return;

        float rotation = Timer * 0.16f;
        int sparks = 6 + (int)(Timer * 0.35f);
        for (int i = 0; i < sparks; i++) {
            float angle = rotation + MathHelper.TwoPi * i / sparks;
            Vector2 direction = angle.ToRotationVector2();
            Vector2 position = Projectile.Center + direction * Main.rand.NextFloat(radius * 0.35f, radius);
            Vector2 swirlVelocity = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.8f, 2.6f);

            Dust dust = Dust.NewDustPerfect(position,
                i % 3 == 0 ? DustID.WhiteTorch : i % 2 == 0 ? DustID.Flare : DustID.GoldFlame,
                swirlVelocity, 95, Color.Lerp(new Color(255, 180, 95), new Color(255, 248, 230), Main.rand.NextFloat()),
                Main.rand.NextFloat(1.05f, 1.6f));
            dust.noGravity = true;
        }

        if ((int)Timer % 3 == 0) {
            float starRadius = MathHelper.Lerp(radius * 0.72f, radius * 1.12f, 0.7f);
            float intensity = MathHelper.Lerp(0.35f, 0.8f, Utils.GetLerpValue(0f, ChargeTicks, Timer, true));
            SpawnFourPointStarDust(Projectile.Center, starRadius, intensity, 3, 1.2f);
        }
    }

    private void SpawnSupernovaDust(float radius, float previousRadius) {
        if (Main.dedServ)
            return;

        float shellThickness = 42f;
        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - shellThickness));
        float radiusProgress = MathHelper.Clamp(radius / (BaseMaxRadius + DeliberationRadiusBonus), 0f, 1f);
        int points = Math.Max(BaseDustPoints, (int)Math.Round(MathHelper.Lerp(BaseDustPoints, MaxDustPoints, radiusProgress)));
        float rotation = Main.GlobalTimeWrappedHourly * 3.2f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction * Main.rand.NextFloat(1.6f, 5.4f);

            int dustType = i % 6 == 0
                ? DustID.WhiteTorch
                : i % 4 == 0
                    ? DustID.Torch
                    : i % 2 == 0
                    ? DustID.Flare
                    : DustID.GoldFlame;
            Color dustColor = i % 6 == 0
                ? new Color(255, 250, 235)
                : Color.Lerp(new Color(255, 165, 80), new Color(255, 238, 170), Main.rand.NextFloat());

            Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 95, dustColor,
                Main.rand.NextFloat(1.15f, 1.9f));
            dust.noGravity = true;
        }

        for (int i = 0; i < 6; i++) {
            Dust coreDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(radius * 0.16f, radius * 0.16f),
                i % 3 == 0 ? DustID.WhiteTorch : i % 2 == 0 ? DustID.Flare : DustID.GoldFlame,
                Main.rand.NextVector2Circular(1.4f, 1.4f),
                90,
                new Color(255, 245, 230),
                Main.rand.NextFloat(1.2f, 1.7f));
            coreDust.noGravity = true;
        }

        if ((int)Timer % 4 == 0) {
            float starRadius = MathHelper.Lerp(radius * 0.42f, radius * 0.78f, 0.65f);
            SpawnFourPointStarDust(Projectile.Center, starRadius, 0.78f, 4, 1.45f);
        }
    }

    private void SpawnFourPointStarDust(Vector2 center, float armLength, float intensity, int segmentsPerArm, float baseScale) {
        if (Main.dedServ || armLength <= 0f || intensity <= 0f)
            return;

        int contourCount = Math.Max(2, segmentsPerArm);
        int samplesPerContour = Math.Max(24, segmentsPerArm * 12);

        for (int contour = 0; contour < contourCount; contour++) {
            float contourProgress = contourCount == 1 ? 0f : contour / (float)(contourCount - 1);
            float contourScale = MathHelper.Lerp(1f, 0.52f, contourProgress);
            float contourRadius = armLength * contourScale;
            float contourJitter = MathHelper.Lerp(2.4f, 0.8f, contourProgress) * intensity;
            float contourScaleBoost = MathHelper.Lerp(1.1f, 0.72f, contourProgress);

            for (int sample = 0; sample < samplesPerContour; sample++) {
                float angle = MathHelper.TwoPi * sample / samplesPerContour;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);

                Vector2 starPoint = new(
                    MathF.Sign(cos) * MathF.Pow(MathF.Abs(cos), 3f),
                    MathF.Sign(sin) * MathF.Pow(MathF.Abs(sin), 3f));

                Vector2 tangent = new(
                    -3f * cos * MathF.Abs(cos) * sin,
                    3f * sin * MathF.Abs(sin) * cos);

                Vector2 outward = starPoint.SafeNormalize(Vector2.UnitY);
                Vector2 tangentDirection = tangent.SafeNormalize(Vector2.UnitX);
                Vector2 position = center + starPoint * contourRadius + Main.rand.NextVector2Circular(contourJitter, contourJitter);

                float cuspness = MathF.Pow(Math.Max(MathF.Abs(cos), MathF.Abs(sin)), 4f);
                Vector2 velocity = outward * MathHelper.Lerp(0.55f, 2.9f, cuspness) * intensity
                    + tangentDirection * Main.rand.NextFloat(-0.35f, 0.35f);

                int dustType = cuspness > 0.78f
                    ? DustID.WhiteTorch
                    : sample % 3 == 0
                        ? DustID.Flare
                        : DustID.GoldFlame;
                Color color = cuspness > 0.78f
                    ? new Color(255, 250, 238)
                    : Color.Lerp(new Color(255, 176, 92), new Color(255, 232, 170), cuspness * 0.6f + Main.rand.NextFloat(0.25f));

                Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 92, color,
                    baseScale * contourScaleBoost * MathHelper.Lerp(0.85f, 1.5f, cuspness));
                dust.noGravity = true;
            }
        }
    }
}
