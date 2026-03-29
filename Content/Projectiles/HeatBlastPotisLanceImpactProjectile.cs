using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastPotisLanceImpactProjectile : ModProjectile {
    private const int SnowflakeFlag = 1;
    private const int EmpoweredFlag = 2;

    private int FlagMask => (int)System.Math.Round(Projectile.ai[1]);
    private bool Snowflake => (FlagMask & SnowflakeFlag) != 0;
    private bool Empowered => (FlagMask & EmpoweredFlag) != 0;
    private float ImpactRotation => Projectile.ai[0];
    private float Progress => 1f - Projectile.timeLeft / 14f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 14;
        Projectile.hide = true;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Vector3 lightColor = Snowflake ? new Vector3(0.24f, 0.56f, 0.86f) : new Vector3(0.86f, 0.34f, 0.06f);
        Lighting.AddLight(Projectile.Center, lightColor * (1f - Progress * 0.4f));
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float pulse = System.MathF.Sin(Progress * MathHelper.Pi);
        float rotation = ImpactRotation;
        float scale = Projectile.scale * (Empowered ? 1.16f : 1f);
        Color outerColor = Snowflake ? new Color(110, 210, 255, 120) : new Color(255, 118, 40, 120);
        Color innerColor = Snowflake ? new Color(236, 246, 255, 220) : new Color(255, 236, 188, 220);
        float rayLength = MathHelper.Lerp(18f, 56f, Progress) * scale;
        float crossLength = MathHelper.Lerp(10f, 34f, Progress) * scale;

        DrawBeam(pixel, center, rotation, new Vector2(rayLength, 9f * scale), outerColor * (pulse * 0.85f));
        DrawBeam(pixel, center, rotation, new Vector2(rayLength * 0.58f, 4.6f * scale), innerColor * pulse);
        DrawBeam(pixel, center, rotation + MathHelper.PiOver2, new Vector2(crossLength, 6.2f * scale),
            outerColor * (pulse * 0.68f));
        DrawBeam(pixel, center, rotation + 0.72f, new Vector2(crossLength * 0.78f, 4.2f * scale),
            innerColor * (pulse * 0.74f));
        DrawBeam(pixel, center, rotation - 0.72f, new Vector2(crossLength * 0.78f, 4.2f * scale),
            innerColor * (pulse * 0.74f));
        DrawRing(pixel, center, MathHelper.Lerp(8f, 26f, Progress) * scale, 3.2f * scale,
            outerColor * (pulse * 0.54f), Progress * 0.8f);
        DrawRing(pixel, center, MathHelper.Lerp(4f, 15f, Progress) * scale, 2.1f * scale,
            innerColor * (pulse * 0.74f), -Progress);
        Main.EntitySpriteDraw(pixel, center, null, Color.White * (pulse * 0.92f), 0f, Vector2.One * 0.5f,
            new Vector2(12f, 12f) * scale, SpriteEffects.None, 0);
        return false;
    }

    private static void DrawBeam(Texture2D pixel, Vector2 center, float rotation, Vector2 scale, Color color) {
        Main.EntitySpriteDraw(pixel, center, null, color, rotation, Vector2.One * 0.5f, scale, SpriteEffects.None, 0);
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotation) {
        const int Segments = 16;
        for (int i = 0; i < Segments; i++) {
            float angle = rotation + MathHelper.TwoPi * i / Segments;
            Vector2 drawPosition = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, drawPosition, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.2f), SpriteEffects.None, 0);
        }
    }
}
