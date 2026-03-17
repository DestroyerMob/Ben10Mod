using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HumungousaurPunchProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 54;
        Projectile.height = 54;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 14;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.scale = Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0];
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Projectile.velocity *= 0.92f;
        Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.45f, 0.18f) * 0.65f);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = Projectile.rotation;

        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 125, 60, 110), rotation, Vector2.One * 0.5f,
            new Vector2(20f * Projectile.scale, 20f * Projectile.scale), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 220, 180, 170), rotation, Vector2.One * 0.5f,
            new Vector2(10f * Projectile.scale, 10f * Projectile.scale), SpriteEffects.None, 0);
        return false;
    }
}
