using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class NRGLaserProjectile : ModProjectile {
    public override string Texture => "Ben10Mod/Content/Projectiles/EyeGuyLaserbeam";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 2;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 2;
        Projectile.timeLeft = 90;
        Projectile.alpha = 24;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 1f, 0.1f, 0.08f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, Projectile.velocity * 0.16f, 100,
                new Color(255, 70, 50), 1.25f);
            dust.noGravity = true;
        }

        if (Main.rand.NextBool(3)) {
            Dust glowDust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Projectile.velocity * 0.1f, 90,
                new Color(255, 185, 120), 1f);
            glowDust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() * 0.5f;
        float rotation = Projectile.rotation + MathHelper.PiOver2;

        Main.EntitySpriteDraw(texture, drawPosition, null, new Color(255, 90, 70, 220) * Projectile.Opacity, rotation,
            origin, Projectile.scale * 1.08f, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, drawPosition, null, new Color(255, 225, 205, 210) * Projectile.Opacity, rotation,
            origin, Projectile.scale * 0.72f, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 180);
    }
}
