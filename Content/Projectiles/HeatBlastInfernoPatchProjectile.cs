using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastInfernoPatchProjectile : ModProjectile {
    private float Radius => Projectile.ai[0] > 0f ? Projectile.ai[0] : 64f;
    private int Lifetime => Projectile.ai[1] > 0f ? (int)Projectile.ai[1] : 180;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.timeLeft = 180;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 24;
    }

    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
        Projectile.timeLeft = Lifetime;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= Radius;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        float lightStrength = Utils.GetLerpValue(0f, Lifetime * 0.25f, Projectile.timeLeft, true);
        Lighting.AddLight(Projectile.Center, new Vector3(0.72f, 0.26f, 0.05f) * lightStrength);

        if (Main.dedServ)
            return;

        int dustBursts = Main.rand.NextBool(2) ? 2 : 1;
        for (int i = 0; i < dustBursts; i++) {
            Vector2 offset = Main.rand.NextVector2Circular(Radius * 0.78f, Radius * 0.42f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool(3) ? DustID.InfernoFork : Main.rand.NextBool() ? DustID.Flare : DustID.Torch,
                new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), Main.rand.NextFloat(-1.4f, -0.35f)),
                100, Color.Lerp(new Color(255, 138, 48), new Color(255, 228, 165), Main.rand.NextFloat()),
                Main.rand.NextFloat(0.85f, 1.35f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float lifeRatio = Projectile.timeLeft / (float)Lifetime;
        float opacity = Utils.GetLerpValue(0f, 0.16f, 1f - lifeRatio, true) * Utils.GetLerpValue(0f, 0.22f, lifeRatio, true);

        DrawRing(pixel, center, Radius * 0.92f, 6.2f, new Color(255, 120, 40, 96) * opacity, Projectile.rotation * 0.5f);
        DrawRing(pixel, center, Radius * 0.62f, 5f, new Color(255, 182, 92, 84) * opacity, -Projectile.rotation * 0.65f);
        DrawRing(pixel, center, Radius * 0.34f, 3.8f, new Color(255, 236, 180, 74) * opacity, Projectile.rotation * 0.9f);
        return false;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color, float rotationOffset) {
        const int Segments = 14;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2f), SpriteEffects.None, 0f);
        }
    }
}
