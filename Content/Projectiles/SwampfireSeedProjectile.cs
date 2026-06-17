using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SwampfireSeedProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 1;
        Projectile.timeLeft = 120;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
    }

    public override void AI() {
        Projectile.rotation += 0.22f * Projectile.direction;
        Projectile.velocity.Y += 0.18f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.52f, 0.95f, 0.22f) * 0.7f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Grass,
                -Projectile.velocity * 0.06f, 120, new Color(130, 220, 90), 1f);
            dust.noGravity = true;
        }

        if (Main.rand.NextBool(3)) {
            Dust ember = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.Torch,
                Main.rand.NextVector2Circular(0.8f, 0.8f), 100, new Color(255, 155, 75), 0.95f);
            ember.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(100, 180, 60, 220),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(12f, 12f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 170, 70, 160),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(5f, 5f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<FuelVapour>(), 5 * 60);
        target.AddBuff(BuffID.OnFire3, 90);
        PlantBloomPod(target.Bottom, 0.3f);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        PlantBloomPod(Projectile.Bottom, 0.25f);

        Projectile.Kill();
        return false;
    }

    private void PlantBloomPod(Vector2 groundPoint, float startingGrowth) {
        if (Projectile.owner != Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Vector2 podCenter = groundPoint + new Vector2(0f, -SwampfireVineProjectile.DefaultHeight * 0.5f + 8f);
        SwampfireVineProjectile.CreateOrGrow(Projectile.GetSource_FromThis(), podCenter, Projectile.damage,
            Projectile.owner, startingGrowth, SwampfireVineProjectile.BaseLifetime);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 16; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Torch : DustID.Grass,
                Main.rand.NextVector2Circular(2.2f, 2.2f), 120, Color.White, 1.1f);
            dust.noGravity = true;
        }
    }
}
