using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.Gwen;

public class AnoditeLanceProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 45;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, new Vector3(1.25f, 0.45f, 0.9f) * 0.95f);

        for (int i = 0; i < 6; i++) {
            Vector2 spiralOffset = Projectile.velocity.SafeNormalize(Vector2.UnitY)
                .RotatedBy(MathHelper.PiOver2)
                * Main.rand.NextFloat(-10f, 10f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + spiralOffset, DustID.PinkTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.08f, 0.18f) + Main.rand.NextVector2Circular(0.6f, 0.6f),
                85, new Color(255, 130, 220), Main.rand.NextFloat(1.15f, 1.55f));
            dust.noGravity = true;
        }

        if (Main.rand.NextBool()) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby,
                Main.rand.NextVector2Circular(0.5f, 0.5f), 100, new Color(255, 225, 245), 0.95f);
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Confused, 45);
        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 100, new Color(255, 110, 190), 1.35f);
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 100, new Color(255, 110, 190), 1.35f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        float rotation = velocity.ToRotation() + MathHelper.PiOver2;

        DrawLanceLayer(pixel, center, rotation, new Vector2(15f, 48f), new Color(255, 70, 170, 135));
        DrawLanceLayer(pixel, center, rotation, new Vector2(9f, 36f), new Color(255, 130, 220, 195));
        DrawLanceLayer(pixel, center, rotation, new Vector2(4.8f, 24f), new Color(255, 240, 255, 245));

        Vector2 tip = center - velocity * 18f;
        Main.EntitySpriteDraw(pixel, tip, null, new Color(255, 230, 245, 235), rotation, Vector2.One * 0.5f,
            new Vector2(16f, 16f), SpriteEffects.None, 0);
        return false;
    }

    private static void DrawLanceLayer(Texture2D pixel, Vector2 center, float rotation, Vector2 scale, Color color) {
        Main.EntitySpriteDraw(pixel, center, null, color, rotation, Vector2.One * 0.5f, scale, SpriteEffects.None, 0);
    }
}
