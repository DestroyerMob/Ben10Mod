using Ben10Mod.Content.Buffs.Debuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.Gwen;

public class HexCircleProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 96;
        Projectile.height = 96;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 120;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += 0.008f;

        float pulse = 0.92f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 0.03f;
        Projectile.scale = pulse;
        Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.25f, 0.6f) * 0.65f);

        for (int i = 0; i < 2; i++) {
            Vector2 offset = Main.rand.NextVector2CircularEdge(38f, 38f) * Projectile.scale;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GemRuby,
                Vector2.Zero, 120, new Color(255, 120, 190), 1.1f);
            dust.noGravity = true;
        }
    }

    public override bool? CanHitNPC(NPC target) {
        return target.CanBeChasedBy(Projectile) && Projectile.Distance(target.Center) <= 42f ? null : false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 90);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = new(255, 105, 180, 140);
        Color inner = new(255, 225, 245, 180);

        DrawRing(pixel, center, 40f * Projectile.scale, 4.8f, outer, Projectile.rotation);
        DrawRing(pixel, center, 26f * Projectile.scale, 3.4f, inner, -Projectile.rotation * 0.6f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 170, 215, 70), 0f, Vector2.One * 0.5f,
            new Vector2(18f * Projectile.scale, 18f * Projectile.scale), SpriteEffects.None, 0);
        return false;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 30;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.4f), SpriteEffects.None, 0);
        }
    }
}
