using Ben10Mod.Content.Transformations.AmpFibian;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AmpFibianBarrierProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool? CanDamage() => false;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != AmpFibianTransformation.TransformationId || !omp.IsUltimateAbilityActive) {
            Projectile.Kill();
            return;
        }

        AmpFibianPhaseShiftPlayer state = owner.GetModPlayer<AmpFibianPhaseShiftPlayer>();
        Projectile.Center = owner.Center;
        Projectile.timeLeft = 2;
        Projectile.rotation += 0.09f + state.BarrierChargeRatio * 0.045f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.12f + state.BarrierChargeRatio * 0.12f,
            0.3f + state.BarrierChargeRatio * 0.14f, 0.62f + state.BarrierChargeRatio * 0.16f));
        SpawnBarrierDust(state.BarrierChargeRatio);
    }

    private void SpawnBarrierDust(float chargeRatio) {
        if (Main.dedServ)
            return;

        float radius = 44f + chargeRatio * 16f;
        int dustCount = 6 + (int)(chargeRatio * 6f);
        for (int i = 0; i < dustCount; i++) {
            float angle = Projectile.rotation + MathHelper.TwoPi * Main.rand.NextFloat();
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 tangentialVelocity = offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) *
                (0.35f + chargeRatio * 0.25f);

            Dust outer = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Electric, tangentialVelocity, 95,
                Color.Lerp(new Color(100, 205, 255), new Color(190, 245, 255), chargeRatio),
                Main.rand.NextFloat(1.05f, 1.35f + chargeRatio * 0.22f));
            outer.noGravity = true;

            Dust inner = Dust.NewDustPerfect(Projectile.Center + offset * 0.78f, DustID.BlueTorch,
                tangentialVelocity * 0.45f, 110, new Color(220, 250, 255), Main.rand.NextFloat(0.85f, 1.1f));
            inner.noGravity = true;
        }
    }
}
