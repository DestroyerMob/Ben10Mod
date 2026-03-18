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

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.ai[0] = Main.rand.NextFloat(-0.28f, 0.28f);
        }

        float slashRotation = -MathHelper.PiOver2 + Projectile.ai[0];
        Vector2 swingDirection = slashRotation.ToRotationVector2();
        Vector2 handOffset = new Vector2(owner.direction * 12f, -4f);
        Projectile.rotation = slashRotation;
        Projectile.Center = owner.MountedCenter + handOffset;
        owner.itemRotation = Projectile.rotation;
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

        if (Main.rand.NextBool()) {
            Vector2 lineOffset = swingDirection * Main.rand.NextFloat(-24f, 24f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + lineOffset, DustID.Smoke,
                swingDirection.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.5f, 1.6f), 110, new Color(240, 240, 240), 1.08f);
            dust.noGravity = true;
        }

        if (Main.rand.NextBool(2)) {
            Vector2 lineOffset = swingDirection * Main.rand.NextFloat(-28f, 28f);
            Dust slashDust = Dust.NewDustPerfect(Projectile.Center + lineOffset, DustID.SilverCoin,
                swingDirection.RotatedByRandom(0.14f) * Main.rand.NextFloat(0.4f, 1.2f), 110, new Color(255, 255, 255), 1f);
            slashDust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 swingDirection = Projectile.rotation.ToRotationVector2();
        float lifeProgress = 1f - Projectile.timeLeft / (float)SlashLifetime;
        float opacity = Utils.GetLerpValue(0f, 0.18f, lifeProgress, true) * Utils.GetLerpValue(1f, 0.45f, lifeProgress, true);
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(210, 220, 235, 210) * opacity,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(8f, 62f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 255, 255, 235) * opacity,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(3f, 46f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), new Color(255, 255, 255, 145) * opacity,
            0f, new Vector2(0.5f, 0.5f), new Vector2(10f, 10f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center, i % 2 == 0 ? DustID.Smoke : DustID.SilverCoin,
                Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.45f) * Main.rand.NextFloat(1f, 3.4f), 90,
                new Color(255, 255, 255), 1.15f);
            dust.noGravity = true;
        }
    }
}
