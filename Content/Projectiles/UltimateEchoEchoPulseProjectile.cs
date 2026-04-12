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

public class UltimateEchoEchoPulseProjectile : ModProjectile {
    public const int ModeFeedbackMain = 0;
    public const int ModeFeedbackSpeaker = 1;
    public const int ModeRelayPulse = 2;
    public const int ModeFinalDischarge = 3;

    private const int ActiveLifetimeTicks = 18;

    private ref float DelayTicks => ref Projectile.ai[0];
    private int Mode => (int)Math.Round(Projectile.ai[1]);
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
                SoundEngine.PlaySound(ResolvePulseSound(), Projectile.Center);
        }

        Projectile.localAI[1]++;
        if (ActiveTicks >= ActiveLifetimeTicks)
            Projectile.Kill();

        Lighting.AddLight(Projectile.Center, new Vector3(0.34f, 0.56f, 0.98f) * ResolveLightStrength());
        SpawnBurstDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (!Released)
            return false;

        return targetHitbox.Distance(Projectile.Center) <= ResolveRadius();
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (Mode == ModeFinalDischarge && identity.IsEchoEchoFracturedFor(Projectile.owner))
            modifiers.SourceDamage *= 1.1f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        EchoEchoTransformation.ResolveResonanceHit(Projectile, target, damageDone,
            UltimateEchoEchoTransformation.EncodePulseSourceId(Mode, Projectile.whoAmI),
            Mode != ModeRelayPulse);

        if (target.boss && Mode != ModeRelayPulse) {
            int fractureTime = identity.IsEchoEchoFracturedFor(Projectile.owner)
                ? Math.Max(identity.EchoEchoFractureTime, 240)
                : 150;
            identity.ApplyEchoEchoFracture(Projectile.owner, fractureTime);
        }

        Vector2 pushDirection = target.Center.DirectionFrom(Projectile.Center);
        if (pushDirection == Vector2.Zero)
            pushDirection = new Vector2(Main.rand.NextBool() ? -1f : 1f, -0.15f);

        float pushStrength = Mode switch {
            ModeFeedbackMain => target.boss ? 2.5f : 5.4f,
            ModeFeedbackSpeaker => target.boss ? 1.8f : 4.1f,
            ModeFinalDischarge => target.boss ? 2f : 4.5f,
            _ => target.boss ? 1.4f : 3.2f
        };
        target.velocity += pushDirection.SafeNormalize(Vector2.UnitX) * pushStrength;
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) => false;

    private SoundStyle ResolvePulseSound() {
        return Mode switch {
            ModeFinalDischarge => SoundID.Item62 with { Pitch = 0.04f, Volume = 0.44f },
            ModeRelayPulse => SoundID.Item8 with { Pitch = 0.24f, Volume = 0.32f },
            _ => SoundID.Item38 with { Pitch = -0.08f, Volume = 0.44f }
        };
    }

    private float ResolveRadius() {
        float baseRadius = Mode switch {
            ModeFeedbackMain => 76f,
            ModeFeedbackSpeaker => 58f,
            ModeFinalDischarge => 66f,
            _ => 44f
        };
        return MathHelper.Lerp(baseRadius * 0.35f, baseRadius, (float)Math.Sqrt(Progress));
    }

    private float ResolveLightStrength() {
        return Mode switch {
            ModeFinalDischarge => 0.62f,
            ModeFeedbackMain => 0.56f,
            ModeFeedbackSpeaker => 0.48f,
            _ => 0.38f
        };
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

        float radius = ResolveRadius();
        int dustCount = Progress > 0.45f ? 4 : 3;
        Color pulseColor = Mode switch {
            ModeFeedbackMain => new Color(170, 220, 255),
            ModeFeedbackSpeaker => new Color(150, 210, 255),
            ModeFinalDischarge => new Color(215, 242, 255),
            _ => new Color(190, 225, 255)
        };

        for (int i = 0; i < dustCount; i++) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 velocity = offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) *
                               Main.rand.NextFloat(0.3f, 1.1f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, i % 2 == 0 ? DustID.WhiteTorch : DustID.GemSapphire,
                velocity, 105, pulseColor, Main.rand.NextFloat(0.82f, 1.18f));
            dust.noGravity = true;
        }
    }
}
