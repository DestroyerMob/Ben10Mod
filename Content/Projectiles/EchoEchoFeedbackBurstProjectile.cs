using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.EchoEcho;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoFeedbackBurstProjectile : ModProjectile {
    private const int ActiveLifetimeTicks = 18;

    private int SourceId => (int)Math.Round(Projectile.ai[1]);
    private ref float DelayTicks => ref Projectile.ai[0];
    private bool Released => Projectile.localAI[0] >= 1f;
    private float ActiveTicks => Projectile.localAI[1];
    private float Progress => MathHelper.Clamp(ActiveTicks / ActiveLifetimeTicks, 0f, 1f);

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = ActiveLifetimeTicks + 6;
    }

    public override void AI() {
        if (!Released) {
            if (DelayTicks > 0f) {
                DelayTicks--;
                SpawnChargeDust();
                return;
            }

            Projectile.localAI[0] = 1f;
            Projectile.friendly = true;
            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.12f, Volume = 0.42f }, Projectile.Center);
        }

        Projectile.localAI[1]++;
        if (ActiveTicks >= ActiveLifetimeTicks)
            Projectile.Kill();

        Lighting.AddLight(Projectile.Center, new Vector3(0.34f, 0.52f, 0.95f) * 0.54f);
        SpawnBurstDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (!Released)
            return false;

        float radius = MathHelper.Lerp(18f, 64f, (float)Math.Sqrt(Progress));
        return targetHitbox.Distance(Projectile.Center) <= radius;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.IsEchoEchoFracturedFor(Projectile.owner))
            modifiers.SourceDamage *= 1.12f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        EchoEchoTransformation.ResolveResonanceHit(Projectile, target, damageDone, SourceId, heavyHit: true);
        Vector2 pushDirection = target.Center.DirectionFrom(Projectile.Center);
        if (pushDirection == Vector2.Zero)
            pushDirection = new Vector2(Main.rand.NextBool() ? -1f : 1f, -0.15f);

        target.velocity += pushDirection.SafeNormalize(Vector2.UnitX) * (target.boss ? 2.2f : 4.8f);
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    private void SpawnChargeDust() {
        if (Main.dedServ || !Main.rand.NextBool(3))
            return;

        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.GemSapphire,
            Main.rand.NextVector2Circular(0.2f, 0.2f), 120, new Color(188, 225, 255), 0.88f);
        dust.noGravity = true;
    }

    private void SpawnBurstDust() {
        if (Main.dedServ)
            return;

        float radius = MathHelper.Lerp(12f, 56f, Progress);
        int dustCount = Progress > 0.45f ? 4 : 3;
        for (int i = 0; i < dustCount; i++) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 velocity = offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.3f, 1.1f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, i % 2 == 0 ? DustID.WhiteTorch : DustID.GemSapphire,
                velocity, 105, new Color(170, 220, 255), Main.rand.NextFloat(0.82f, 1.18f));
            dust.noGravity = true;
        }
    }
}
