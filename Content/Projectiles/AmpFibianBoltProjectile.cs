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
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 5;
        Projectile.timeLeft = 54;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        if (!_initialized) {
            _initialized = true;
            _startPosition = Projectile.Center;
            _baseVelocity = Projectile.velocity;
            if (_baseVelocity.LengthSquared() <= 0.001f)
                _baseVelocity = new Vector2(Projectile.direction, 0f) * 18f;
        }

        float elapsed = (54 - Projectile.timeLeft) * (Projectile.extraUpdates + 1);
        Vector2 forward = _baseVelocity.SafeNormalize(Vector2.UnitX);
        Vector2 perpendicular = forward.RotatedBy(MathHelper.PiOver2);
        Vector2 previousCenter = Projectile.Center;
        Projectile.Center = _startPosition + _baseVelocity * elapsed + perpendicular * System.MathF.Sin(elapsed * 0.26f) * 8f;

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

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.ArmorPenetration += 4;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Electrified, 150);
        SpawnImpactDust();
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 forward = Projectile.rotation.ToRotationVector2();
        Vector2 center = Projectile.Center - Main.screenPosition;
        float pulse = 0.85f + System.MathF.Sin(Main.GlobalTimeWrappedHourly * 18f + Projectile.identity) * 0.12f;

        Main.EntitySpriteDraw(pixel, center, null, new Color(90, 200, 255, 170), Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(34f, 4.4f) * pulse, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + forward.RotatedBy(MathHelper.PiOver2) * 3.5f, null,
            new Color(210, 248, 255, 205), Projectile.rotation, Vector2.One * 0.5f,
            new Vector2(20f, 2.2f) * pulse, SpriteEffects.None, 0);
        return false;
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
