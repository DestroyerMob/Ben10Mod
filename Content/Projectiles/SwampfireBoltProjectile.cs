using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SwampfireBoltProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.95f, 0.55f, 0.18f) * 0.8f);

        if (Main.rand.NextBool()) {
            Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * 0.08f,
                120, new Color(255, 170, 70), 1.1f);
            fire.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 120, 40, 210),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(16f, 10f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 245, 190, 190),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(7f, 4f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 240);
    }
}
