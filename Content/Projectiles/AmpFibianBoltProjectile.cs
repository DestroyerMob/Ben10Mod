using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AmpFibianBoltProjectile : ModProjectile {
    private Vector2 _startPosition;
    private Vector2 _baseVelocity;
    private bool _initialized;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 3;
        Projectile.timeLeft = 48;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        if (!_initialized) {
            _initialized = true;
            _startPosition = Projectile.Center;
            _baseVelocity = Projectile.velocity;
            if (_baseVelocity.LengthSquared() <= 0.001f)
                _baseVelocity = new Vector2(Projectile.direction, 0f) * 18f;
        }

        float elapsed = (48 - Projectile.timeLeft) * (Projectile.extraUpdates + 1);
        Vector2 forward = _baseVelocity.SafeNormalize(Vector2.UnitX);
        Vector2 perpendicular = forward.RotatedBy(MathHelper.PiOver2);
        Vector2 previousCenter = Projectile.Center;
        Projectile.Center = _startPosition + _baseVelocity * elapsed + perpendicular * System.MathF.Sin(elapsed * 0.32f) * 10f;

        Vector2 travel = Projectile.Center - previousCenter;
        if (travel.LengthSquared() > 0.001f)
            Projectile.rotation = travel.ToRotation();

        Lighting.AddLight(Projectile.Center, new Vector3(0.18f, 0.45f, 0.95f));
        SpawnBoltDust();
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SpawnImpactDust();
        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Electrified, 180);
        SpawnImpactDust();
    }

    private void SpawnBoltDust() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 2; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.Electric,
                Main.rand.NextVector2Circular(1.2f, 1.2f), 100, new Color(120, 220, 255), Main.rand.NextFloat(0.9f, 1.25f));
            dust.noGravity = true;
        }
    }

    private void SpawnImpactDust() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 16; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(3.1f, 3.1f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                velocity, 100, new Color(150, 235, 255), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }
}
