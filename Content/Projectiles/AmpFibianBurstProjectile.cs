using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AmpFibianBurstProjectile : ModProjectile {
    public const float NormalMode = 0f;
    public const float PhaseDischargeMode = 1f;
    public const float BarrierPulseMode = 2f;
    public const float PhaseContactMode = 3f;

    private const int MaxLifetimeTicks = 18;
    private const float DefaultStartRadius = 18f;

    private float CurrentRadius {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private float PreviousRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float Mode => Projectile.ai[1];
    private float StoredChargeRatio => MathHelper.Clamp(Projectile.ai[2] / 100f, 0f, 1f);
    private bool PhaseDischarge => Mode == PhaseDischargeMode;
    private bool BarrierPulse => Mode == BarrierPulseMode;
    private bool PhaseContact => Mode == PhaseContactMode;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxLifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        int lifetime = GetLifetime();
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, lifetime);
        }

        float progress = 1f - Projectile.timeLeft / (float)lifetime;
        float easedProgress = 1f - MathF.Pow(1f - progress, PhaseDischarge ? 2.8f : 2.4f);
        float radius = MathHelper.Lerp(GetStartRadius(), GetMaxRadius(), easedProgress);
        SpawnBurstDust(radius, PreviousRadius);
        PreviousRadius = radius;
        CurrentRadius = radius;
        Lighting.AddLight(Projectile.Center, PhaseDischarge
            ? new Vector3(0.28f, 0.58f, 0.98f)
            : BarrierPulse
                ? new Vector3(0.22f, 0.48f, 0.88f)
                : new Vector3(0.15f, 0.36f, 0.72f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Electrified, PhaseDischarge ? 330 : BarrierPulse ? 270 : PhaseContact ? 150 : 220);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (PhaseDischarge) {
            modifiers.ArmorPenetration += 8;
            modifiers.Knockback *= 1.25f;
        }
        else if (BarrierPulse) {
            modifiers.ArmorPenetration += 5 + (int)Math.Round(StoredChargeRatio * 8f);
            modifiers.SourceDamage *= 1f + StoredChargeRatio * 0.12f;
        }
        else if (PhaseContact) {
            modifiers.SourceDamage *= 0.72f;
            modifiers.Knockback *= 0.65f;
        }
    }

    private int GetLifetime() {
        return PhaseContact ? 8 : PhaseDischarge ? 18 : BarrierPulse ? 15 : 14;
    }

    private float GetStartRadius() {
        return PhaseContact ? 10f : DefaultStartRadius;
    }

    private float GetMaxRadius() {
        return PhaseDischarge ? 132f
            : BarrierPulse ? 104f + StoredChargeRatio * 18f
            : PhaseContact ? 58f
            : 92f;
    }

    private void SpawnBurstDust(float radius, float previousRadius) {
        if (Main.dedServ)
            return;

        float innerRadius = Math.Max(0f, Math.Max(previousRadius, radius - 18f));
        int points = Math.Max(PhaseContact ? 5 : 8, (int)Math.Round(radius / (PhaseDischarge ? 7.5f : 9f)));
        float rotation = Main.GlobalTimeWrappedHourly * (PhaseDischarge ? 4.4f : 3.2f);

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            float shellOffset = MathHelper.Lerp(innerRadius, radius, Main.rand.NextFloat());
            Vector2 position = Projectile.Center + direction * shellOffset;
            Vector2 velocity = direction * Main.rand.NextFloat(PhaseDischarge ? 1.1f : 0.6f, PhaseDischarge ? 3.4f : 2.4f);

            Dust dust = Dust.NewDustPerfect(position, ResolveDustType(i), velocity, 105,
                Color.Lerp(ResolveOuterColor(), new Color(225, 252, 255), Main.rand.NextFloat()),
                Main.rand.NextFloat(PhaseContact ? 0.72f : 1f, PhaseDischarge ? 1.62f : 1.35f));
            dust.noGravity = true;
        }
    }

    private int ResolveDustType(int index) {
        if (BarrierPulse)
            return index % 2 == 0 ? DustID.Electric : DustID.BlueTorch;

        if (PhaseContact)
            return index % 3 == 0 ? DustID.BlueTorch : DustID.Electric;

        return index % 3 == 0 ? DustID.BlueTorch : DustID.Electric;
    }

    private Color ResolveOuterColor() {
        if (PhaseDischarge)
            return new Color(110, 225, 255);

        if (BarrierPulse)
            return new Color(95, 190, 255);

        if (PhaseContact)
            return new Color(150, 235, 255);

        return new Color(90, 190, 255);
    }
}
