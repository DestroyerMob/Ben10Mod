using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class RathPounceProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 52;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity *= 0.93f;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, -Projectile.velocity * 0.2f,
                120, new Color(255, 170, 100), 1.1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(210, 120, 70, 220),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(32f, 16f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 220, 165, 180),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(14f, 6f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Bleeding, 180);
    }
}
