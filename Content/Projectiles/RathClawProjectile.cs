using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class RathClawProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private const int SlashLifetime = 10;

    public override void SetDefaults() {
        Projectile.width = 44;
        Projectile.height = 44;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = 1;
        Projectile.timeLeft = SlashLifetime;
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

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float progress = 1f - Projectile.timeLeft / (float)SlashLifetime;
        float arc = MathHelper.Lerp(-0.7f, 0.75f, progress) * owner.direction;
        Vector2 swingDirection = direction.RotatedBy(arc);
        Projectile.rotation = swingDirection.ToRotation() + MathHelper.PiOver2;
        Projectile.Center = owner.MountedCenter + swingDirection * 34f;

        if (Main.rand.NextBool()) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + swingDirection * Main.rand.NextFloat(6f, 16f), DustID.Blood,
                swingDirection.RotatedByRandom(0.22f) * Main.rand.NextFloat(0.8f, 2.8f), 110, new Color(255, 180, 120), 1.08f);
            dust.noGravity = true;
        }

        if (Main.rand.NextBool(2)) {
            Dust slashDust = Dust.NewDustPerfect(Projectile.Center + swingDirection * Main.rand.NextFloat(10f, 20f), DustID.Torch,
                swingDirection.RotatedByRandom(0.18f) * Main.rand.NextFloat(0.5f, 1.6f), 110, new Color(255, 225, 170), 0.95f);
            slashDust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 swingDirection = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
        Vector2 normal = swingDirection.RotatedBy(MathHelper.PiOver2);
        float lifeProgress = 1f - Projectile.timeLeft / (float)SlashLifetime;
        float opacity = Utils.GetLerpValue(0f, 0.18f, lifeProgress, true) * Utils.GetLerpValue(1f, 0.45f, lifeProgress, true);

        for (int i = 0; i < 6; i++) {
            float t = i / 5f;
            Vector2 segmentCenter = Projectile.Center - Main.screenPosition
                + swingDirection * MathHelper.Lerp(-12f, 22f, t)
                + normal * MathHelper.Lerp(-18f, 18f, t) * 0.65f;
            float rotation = swingDirection.ToRotation() + MathHelper.Lerp(-0.4f, 0.5f, t);
            Vector2 outerRect = new(MathHelper.Lerp(22f, 36f, t), MathHelper.Lerp(14f, 8f, t));
            Vector2 innerRect = outerRect * new Vector2(0.62f, 0.42f);

            Main.spriteBatch.Draw(pixel, segmentCenter, new Rectangle(0, 0, 1, 1), new Color(255, 120, 45, 210) * opacity,
                rotation, new Vector2(0.5f, 0.5f), outerRect, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(pixel, segmentCenter, new Rectangle(0, 0, 1, 1), new Color(255, 220, 170, 235) * opacity,
                rotation, new Vector2(0.5f, 0.5f), innerRect, SpriteEffects.None, 0f);
        }

        Vector2 flashCenter = Projectile.Center - Main.screenPosition + swingDirection * 10f;
        Main.spriteBatch.Draw(pixel, flashCenter, new Rectangle(0, 0, 1, 1), new Color(255, 245, 215, 150) * opacity,
            swingDirection.ToRotation(), new Vector2(0.5f, 0.5f), new Vector2(18f, 18f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center, i % 2 == 0 ? DustID.Blood : DustID.Torch,
                Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.45f) * Main.rand.NextFloat(1f, 3.4f), 90,
                new Color(255, 200, 145), 1.15f);
            dust.noGravity = true;
        }
    }
}
