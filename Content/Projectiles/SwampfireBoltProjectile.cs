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
        Projectile.rotation += 0.2f * Projectile.direction;
        Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.52f, 0.12f) * 1.15f);

        if (Main.rand.NextBool()) {
            Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch,
                -Projectile.velocity * 0.04f, 80, new Color(255, 170, 70), 1.35f);
            fire.noGravity = true;
        }

        if (Main.rand.NextBool(2)) {
            Dust ember = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(9f, 9f), DustID.InfernoFork,
                Projectile.velocity.RotatedByRandom(0.45f) * 0.06f, 90, new Color(255, 230, 160), 0.95f);
            ember.noGravity = true;
            ember.fadeIn = 1.05f;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float pulse = 1f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 12f) * 0.08f;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 90, 20, 110),
            0f, new Vector2(0.5f, 0.5f), new Vector2(30f, 30f) * pulse, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 145, 40, 210),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(18f, 18f) * pulse, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 230, 150, 220),
            0f, new Vector2(0.5f, 0.5f), new Vector2(8f, 8f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 12; i++) {
            Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.Torch,
                Main.rand.NextVector2Circular(2.5f, 2.5f), 90, Color.White, 1.2f);
            fire.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 240);
    }
}
