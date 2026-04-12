using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.BigChill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BigChillAbsoluteZeroPulseProjectile : ModProjectile {
    public const float VariantShatter = 0f;
    public const float VariantFinalPulse = 1f;

    private bool FinalPulse => Projectile.ai[0] >= VariantFinalPulse;
    private bool FrostSpread => Projectile.ai[1] >= 0.5f;
    private int LifetimeTicks => FinalPulse ? 18 : 14;
    private float MaxRadius => FinalPulse ? 168f : (FrostSpread ? 116f : 92f);

    private float CurrentRadius {
        get {
            float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
            return MathHelper.Lerp(16f, MaxRadius, Utils.GetLerpValue(0f, 0.82f, progress, true));
        }
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 14;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.timeLeft = LifetimeTicks;
        Projectile.localNPCHitCooldown = FinalPulse ? 12 : 16;
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center,
            FinalPulse ? new Vector3(0.3f, 0.52f, 0.82f) : new Vector3(0.2f, 0.42f, 0.68f));

        if (Main.rand.NextBool(FinalPulse ? 1 : 2)) {
            Vector2 offset = Main.rand.NextVector2Circular(CurrentRadius * 0.3f, CurrentRadius * 0.3f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool() ? DustID.IceTorch : DustID.Frost,
                offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.8f, 2.1f),
                105, FinalPulse ? new Color(214, 245, 255) : new Color(196, 236, 255),
                Main.rand.NextFloat(0.95f, FinalPulse ? 1.35f : 1.12f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        BigChillTransformation.ResolvePulseHit(Projectile, target, damageDone, FinalPulse || FrostSpread);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float opacity = Utils.GetLerpValue(0f, 0.15f, progress, true) *
                        Utils.GetLerpValue(0f, 0.24f, Projectile.timeLeft / (float)LifetimeTicks, true);

        DrawRing(pixel, center, CurrentRadius * 0.86f, FinalPulse ? 5.8f : 4.8f,
            new Color(150, 220, 255, FinalPulse ? 108 : 92) * opacity);
        DrawRing(pixel, center, CurrentRadius * 0.58f, FinalPulse ? 4.6f : 3.8f,
            new Color(208, 245, 255, FinalPulse ? 118 : 104) * opacity);
        DrawRing(pixel, center, CurrentRadius * 0.26f, FinalPulse ? 3.6f : 3f,
            new Color(245, 250, 255, FinalPulse ? 126 : 112) * opacity);
        return false;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color) {
        const int Segments = 18;
        for (int i = 0; i < Segments; i++) {
            float angle = MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.1f), SpriteEffects.None, 0f);
        }
    }
}
