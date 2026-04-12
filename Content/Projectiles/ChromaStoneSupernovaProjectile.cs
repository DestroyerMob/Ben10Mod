using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneSupernovaProjectile : ModProjectile {
    private const int LifetimeTicks = 38;
    private const int ChargeTicks = 12;
    private const float ChargeRadius = 56f;
    private const float StartRadius = 22f;
    private const float BaseMaxRadius = 250f;

    private float RadianceRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private bool CrystalGuard => Projectile.ai[1] >= 0.5f;
    private bool IsErupting => Timer >= ChargeTicks;

    private float Timer {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float CurrentRadius {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    public override bool? CanDamage() => IsErupting;

    public override void AI() {
        if (Timer == 0f)
            SpawnChargeBurst();

        Timer++;
        float radius;
        if (!IsErupting) {
            float chargeProgress = Utils.GetLerpValue(0f, ChargeTicks, Timer, true);
            float easedCharge = 1f - MathF.Pow(1f - chargeProgress, 2.1f);
            radius = MathHelper.Lerp(StartRadius, ChargeRadius, easedCharge);
            SpawnChargingDust(radius);
        }
        else {
            if (Timer == ChargeTicks) {
                SpawnEruptionBurst();
                if (Projectile.owner == Main.myPlayer)
                    SpawnPrismVolley();
            }

            float eruptionProgress = Utils.GetLerpValue(ChargeTicks, LifetimeTicks, Timer, true);
            float easedEruption = 1f - MathF.Pow(1f - eruptionProgress, 2.9f);
            float maxRadius = BaseMaxRadius + RadianceRatio * 110f + (CrystalGuard ? 26f : 0f);
            radius = MathHelper.Lerp(ChargeRadius, maxRadius, easedEruption);
            SpawnEruptionDust(radius);

            if (Projectile.owner == Main.myPlayer && (int)(Timer - ChargeTicks) > 0 &&
                (int)(Timer - ChargeTicks) % 6 == 0) {
                SpawnPrismVolley();
            }
        }

        CurrentRadius = radius;
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(0.36f + Timer * 0.08f, 1.12f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * (IsErupting ? 0.7f : 0.48f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (!IsErupting)
            return false;

        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.SourceDamage *= 1f + RadianceRatio * 0.45f + (CrystalGuard ? 0.1f : 0f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 blastDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
        float blastForce = MathHelper.Lerp(10f, 18f, RadianceRatio);
        target.velocity = Vector2.Lerp(target.velocity, blastDirection * blastForce, target.boss ? 0.14f : 0.46f);
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(Timer * 0.08f, 1.08f) * (IsErupting ? 0.62f : 0.44f);
        Color inner = ChromaStonePrismHelper.GetSpectrumColor(0.6f + Timer * 0.11f, 1.1f) * (IsErupting ? 0.78f : 0.55f);

        if (!IsErupting) {
            ChromaStonePrismHelper.DrawRing(pixel, center, CurrentRadius, 4f, outer, Timer * 0.12f, 22);
            ChromaStonePrismHelper.DrawRing(pixel, center, CurrentRadius * 0.62f, 2.8f, inner, -Timer * 0.18f, 18);
            ChromaStonePrismHelper.DrawRotatedRect(pixel, center, Timer * 0.14f, new Vector2(CurrentRadius * 0.82f, 4.8f),
                outer * 0.9f);
            ChromaStonePrismHelper.DrawRotatedRect(pixel, center, Timer * 0.14f + MathHelper.PiOver2,
                new Vector2(CurrentRadius * 0.82f, 4.8f), outer * 0.9f);
            Main.EntitySpriteDraw(pixel, center, null, new Color(245, 250, 255, 230), 0f, Vector2.One * 0.5f,
                new Vector2(18f, 18f), SpriteEffects.None, 0);
            return false;
        }

        ChromaStonePrismHelper.DrawRing(pixel, center, CurrentRadius * 0.86f, 5.2f, outer, Timer * 0.08f, 26);
        ChromaStonePrismHelper.DrawRing(pixel, center, CurrentRadius * 0.56f, 3.4f, inner, -Timer * 0.12f, 22);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, Timer * 0.1f, new Vector2(CurrentRadius * 0.7f, 7.2f),
            outer * 0.75f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, Timer * 0.1f + MathHelper.PiOver2,
            new Vector2(CurrentRadius * 0.7f, 7.2f), outer * 0.75f);

        for (int i = 0; i < 3; i++) {
            float rotation = Timer * 0.05f + i * (MathHelper.Pi / 3f);
            ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
                new Vector2(CurrentRadius * 0.46f, 3.8f), inner * 0.66f);
        }

        Main.EntitySpriteDraw(pixel, center, null, new Color(248, 252, 255, 235), 0f, Vector2.One * 0.5f,
            new Vector2(24f, 24f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 30; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.13f + RadianceRatio * 0.5f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(CurrentRadius * 0.1f, CurrentRadius * 0.1f),
                DustID.WhiteTorch, Main.rand.NextVector2Circular(6f, 6f), 90, prismColor, Main.rand.NextFloat(1.05f, 1.6f));
            dust.noGravity = true;
        }
    }

    private void SpawnChargeBurst() {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.45f, Volume = 0.9f }, Projectile.Center);
        for (int i = 0; i < 24; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.19f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.WhiteTorch,
                Main.rand.NextVector2Circular(3.2f, 3.2f), 95, prismColor, Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private void SpawnEruptionBurst() {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.32f, Volume = 0.92f }, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.55f, Volume = 0.76f }, Projectile.Center);

        for (int i = 0; i < 42; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.11f + 0.28f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), DustID.WhiteTorch,
                Main.rand.NextVector2Circular(7.4f, 7.4f), 90, prismColor, Main.rand.NextFloat(1.1f, 1.85f));
            dust.noGravity = true;
        }
    }

    private void SpawnChargingDust(float radius) {
        if (Main.dedServ)
            return;

        int points = 10;
        for (int i = 0; i < points; i++) {
            float angle = Main.GlobalTimeWrappedHourly * 2.4f + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            Dust dust = Dust.NewDustPerfect(Projectile.Center + direction * radius, DustID.WhiteTorch,
                -direction * Main.rand.NextFloat(0.4f, 1.6f), 95, ChromaStonePrismHelper.GetSpectrumColor(i * 0.2f),
                Main.rand.NextFloat(0.9f, 1.18f));
            dust.noGravity = true;
        }
    }

    private void SpawnEruptionDust(float radius) {
        if (Main.dedServ)
            return;

        int points = Math.Max(20, (int)Math.Round(radius / 8f));
        float rotation = Main.GlobalTimeWrappedHourly * 2.1f;
        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(radius * 0.58f, radius, Main.rand.NextFloat());
            Dust dust = Dust.NewDustPerfect(Projectile.Center + direction * shellOffset, DustID.WhiteTorch,
                direction * Main.rand.NextFloat(1.1f, 4f), 95,
                ChromaStonePrismHelper.GetSpectrumColor(i * 0.14f + shellOffset * 0.01f),
                Main.rand.NextFloat(1f, 1.38f));
            dust.noGravity = true;
        }
    }

    private void SpawnPrismVolley() {
        int boltCount = (CrystalGuard ? 6 : 5) + (RadianceRatio >= 0.75f ? 1 : 0);
        float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
        int boltDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.38f));

        for (int i = 0; i < boltCount; i++) {
            float angle = baseAngle + MathHelper.TwoPi * i / boltCount;
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(12.5f, 16f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + velocity.SafeNormalize(Vector2.UnitX) * 10f,
                velocity, ModContent.ProjectileType<ChromaStoneProjectile>(), boltDamage, Projectile.knockBack * 0.8f,
                Projectile.owner, MathHelper.Clamp(RadianceRatio * 0.92f + 0.08f, 0f, 1f), CrystalGuard ? 1f : 0f);
        }
    }
}
