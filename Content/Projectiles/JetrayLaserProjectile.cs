using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class JetrayLaserProjectile : ModProjectile {
    public override string Texture => "Ben10Mod/Content/Projectiles/EyeGuyLaserbeam";

    public override void SetDefaults() {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 84;
        Projectile.extraUpdates = 2;
        Projectile.alpha = 18;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 9;
    }

    public override void AI() {
        if (Projectile.velocity.LengthSquared() < 1156f)
            Projectile.velocity *= 1.0125f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.12f, 0.95f, 0.72f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.GreenTorch : DustID.Electric,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.12f), 100, new Color(120, 255, 215),
                Main.rand.NextFloat(0.9f, 1.18f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() * 0.5f;
        float rotation = Projectile.rotation + MathHelper.PiOver2;

        Main.EntitySpriteDraw(texture, drawPosition, null, new Color(80, 255, 210, 220) * Projectile.Opacity, rotation,
            origin, Projectile.scale * 1.04f, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, drawPosition, null, new Color(220, 255, 245, 210) * Projectile.Opacity, rotation,
            origin, Projectile.scale * 0.68f, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Electrified, 90);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.Electric : DustID.GreenTorch,
                Main.rand.NextVector2Circular(2.3f, 2.3f), 95, new Color(170, 255, 235), Main.rand.NextFloat(0.95f, 1.25f));
            dust.noGravity = true;
        }
    }
}
