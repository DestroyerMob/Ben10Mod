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
        if (omp.currentTransformationId != "Ben10Mod:AmpFibian" || !omp.IsUltimateAbilityActive) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        Projectile.timeLeft = 2;
        Projectile.rotation += 0.09f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.12f, 0.3f, 0.62f));
        SpawnBarrierDust();
    }

    private void SpawnBarrierDust() {
        if (Main.dedServ)
            return;

        float radius = 44f;
        for (int i = 0; i < 6; i++) {
            float angle = Projectile.rotation + MathHelper.TwoPi * Main.rand.NextFloat();
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 tangentialVelocity = offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * 0.35f;

            Dust outer = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Electric, tangentialVelocity, 95,
                new Color(100, 205, 255), Main.rand.NextFloat(1.05f, 1.35f));
            outer.noGravity = true;

            Dust inner = Dust.NewDustPerfect(Projectile.Center + offset * 0.78f, DustID.BlueTorch,
                tangentialVelocity * 0.45f, 110, new Color(220, 250, 255), Main.rand.NextFloat(0.85f, 1.1f));
            inner.noGravity = true;
        }
    }
}
