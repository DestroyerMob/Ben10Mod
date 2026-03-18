using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SwampfireVineProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 68;
        Projectile.height = 126;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustPerfect(Projectile.Bottom + new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-12f, 4f)),
                    i % 3 == 0 ? DustID.Torch : DustID.Grass, Main.rand.NextVector2Circular(1.4f, 1.4f), 90,
                    Color.White, 1.15f);
                dust.noGravity = true;
            }
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.34f, 0.85f, 0.18f) * 0.75f);

        if (Main.rand.NextBool()) {
            Vector2 dustPos = Projectile.Bottom + new Vector2(Main.rand.NextFloat(-24f, 24f), Main.rand.NextFloat(-Projectile.height + 12f, 0f));
            Dust dust = Dust.NewDustPerfect(dustPos, Main.rand.NextBool() ? DustID.Grass : DustID.JunglePlants,
                new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.8f, -0.15f)), 120,
                new Color(130, 230, 90), 1f);
            dust.noGravity = true;
        }

        if (Main.rand.NextBool(2)) {
            Vector2 extraPos = Projectile.Bottom + new Vector2(Main.rand.NextFloat(-26f, 26f), Main.rand.NextFloat(-Projectile.height + 16f, -8f));
            Dust extraDust = Dust.NewDustPerfect(extraPos, DustID.JungleGrass, Main.rand.NextVector2Circular(0.3f, 0.6f), 120,
                new Color(110, 215, 90), 0.95f);
            extraDust.noGravity = true;
        }

        if (Main.rand.NextBool(3)) {
            Vector2 emberPos = Projectile.Bottom + new Vector2(Main.rand.NextFloat(-12f, 12f), Main.rand.NextFloat(-40f, 0f));
            Dust ember = Dust.NewDustPerfect(emberPos, DustID.Torch, Main.rand.NextVector2Circular(0.5f, 0.5f), 120,
                new Color(255, 150, 70), 0.9f);
            ember.noGravity = true;
        }
    }

    public override bool? CanHitNPC(NPC target) {
        return !target.friendly && target.CanBeChasedBy(Projectile);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 basePos = Projectile.Bottom - Main.screenPosition;
        float growth = Utils.GetLerpValue(0f, 18f, 600 - Projectile.timeLeft, true);
        float fade = Utils.GetLerpValue(0f, 30f, Projectile.timeLeft, true);
        float opacity = growth * fade;

        for (int i = -4; i <= 4; i++) {
            float bend = i * 8f;
            float width = i == 0 ? 18f : (System.Math.Abs(i) == 1 ? 14f : 11f);
            float height = i == 0 ? 118f : (System.Math.Abs(i) == 1 ? 110f : 102f);
            Vector2 center = basePos + new Vector2(bend, -height * 0.5f);
            Color outerColor = new Color(40, 120, 28, 225) * opacity;
            Color innerColor = new Color(145, 255, 120, 205) * opacity;

            Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), outerColor,
                0.08f * i, new Vector2(0.5f, 0.5f), new Vector2(width, height), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(pixel, center + new Vector2(0f, -6f), new Rectangle(0, 0, 1, 1), innerColor,
                0.08f * i, new Vector2(0.5f, 0.5f), new Vector2(width * 0.42f, height * 0.82f), SpriteEffects.None, 0f);
        }

        for (int i = -3; i <= 3; i++) {
            Vector2 leafCenter = basePos + new Vector2(i * 12f, -58f - System.Math.Abs(i) * 8f);
            Main.spriteBatch.Draw(pixel, leafCenter, new Rectangle(0, 0, 1, 1), new Color(90, 200, 70, 180) * opacity,
                0.65f * i, new Vector2(0.5f, 0.5f), new Vector2(8f, 22f), SpriteEffects.None, 0f);
        }

        for (int i = -2; i <= 2; i++) {
            Vector2 crossLeafCenter = basePos + new Vector2(i * 10f, -32f - System.Math.Abs(i) * 7f);
            Main.spriteBatch.Draw(pixel, crossLeafCenter, new Rectangle(0, 0, 1, 1), new Color(120, 235, 100, 150) * opacity,
                -0.5f * i, new Vector2(0.5f, 0.5f), new Vector2(7f, 18f), SpriteEffects.None, 0f);
        }

        Main.spriteBatch.Draw(pixel, basePos + new Vector2(0f, -10f), new Rectangle(0, 0, 1, 1), new Color(255, 135, 50, 170) * opacity,
            0f, new Vector2(0.5f, 0.5f), new Vector2(38f, 14f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, 180);
        target.AddBuff(BuffID.Poisoned, 180);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 50f),
                i % 2 == 0 ? DustID.Grass : DustID.Torch, Main.rand.NextVector2Circular(2.2f, 2.2f),
                120, Color.White, 1.05f);
            dust.noGravity = true;
        }
    }
}
