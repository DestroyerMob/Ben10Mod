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
        Projectile.Center = owner.MountedCenter + swingDirection * 32f;

        if (Main.rand.NextBool()) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Blood,
                swingDirection.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f, 2f), 120, new Color(255, 180, 120), 1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 outerRect = new(28f, 10f);
        Vector2 innerRect = new(18f, 4f);

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 180, 90, 220),
            Projectile.rotation - MathHelper.PiOver2, new Vector2(0.5f, 0.5f), outerRect, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 245, 215, 210),
            Projectile.rotation - MathHelper.PiOver2, new Vector2(0.5f, 0.5f), innerRect, SpriteEffects.None, 0f);
        return false;
    }
}
