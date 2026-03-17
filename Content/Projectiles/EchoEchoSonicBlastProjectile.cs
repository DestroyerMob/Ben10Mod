using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoSonicBlastProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, new Vector3(0.95f, 0.45f, 0.45f) * 0.7f);

        if (Main.rand.NextBool()) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Firework_Red,
                -Projectile.velocity * 0.08f, 100, new Color(255, 180, 180), 1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = Projectile.rotation;

        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 90, 90, 120), rotation, Vector2.One * 0.5f,
            new Vector2(10f, 24f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 220, 220, 220), rotation, Vector2.One * 0.5f,
            new Vector2(4f, 14f), SpriteEffects.None, 0);
        return false;
    }
}
