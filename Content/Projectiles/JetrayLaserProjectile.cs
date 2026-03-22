using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class JetrayLaserProjectile : ModProjectile, IMagistrataOutlineProvider {
    public override string Texture => "Ben10Mod/Content/Projectiles/EyeGuyLaserbeam";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 2;
        Projectile.timeLeft = 240;
        Projectile.alpha = 40;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.1f, 0.95f, 0.35f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenFairy, Projectile.velocity * 0.18f, 100,
                Color.LimeGreen, 1.25f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            lightColor * Projectile.Opacity,
            Projectile.rotation + MathHelper.PiOver2,
            new Vector2(texture.Width / 2f, texture.Height / 2f),
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    public bool TryGetMagistrataOutlineDrawData(out MagistrataOutlineDrawData drawData) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        drawData = new MagistrataOutlineDrawData(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            texture.Size() * 0.5f,
            Projectile.rotation + MathHelper.PiOver2,
            Projectile.scale,
            SpriteEffects.None
        );
        return true;
    }
}
