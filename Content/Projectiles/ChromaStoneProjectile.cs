using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneProjectile : ModProjectile {
    private float RadianceRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private bool CrystalGuard => Projectile.ai[1] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 88;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.scale = 1f + RadianceRatio * 0.18f + (CrystalGuard ? 0.08f : 0f);
            if (RadianceRatio >= 0.72f)
                Projectile.penetrate++;
        }

        float desiredSpeed = 15f + RadianceRatio * 4f + (CrystalGuard ? 1.2f : 0f);
        if (Projectile.velocity.LengthSquared() < desiredSpeed * desiredSpeed)
            Projectile.velocity *= CrystalGuard ? 1.016f : 1.012f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(RadianceRatio * 2.5f + Projectile.identity * 0.07f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * (0.42f + RadianceRatio * 0.18f));

        if (!Main.dedServ && Main.rand.NextBool(CrystalGuard ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.WhiteTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 100, prismColor,
                Main.rand.NextFloat(0.92f, 1.22f));
            dust.noGravity = true;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.SourceDamage *= 1f + RadianceRatio * 0.14f + (CrystalGuard ? 0.05f : 0f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity *= 0.94f;
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float rotation = direction.ToRotation();

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float progress = i / (float)Projectile.oldPos.Length;
            Vector2 trailCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Color trailColor = ChromaStonePrismHelper.GetSpectrumColor(progress * 2.1f + i * 0.08f) *
                ((1f - progress) * 0.4f);
            ChromaStonePrismHelper.DrawRotatedRect(pixel, trailCenter, rotation,
                new Vector2(MathHelper.Lerp(28f, 10f, progress), MathHelper.Lerp(8f, 3f, progress)) * Projectile.scale,
                trailColor);
        }

        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.14f + RadianceRatio * 1.4f, 1.04f) * 0.6f;
        Color middle = ChromaStonePrismHelper.GetSpectrumColor(0.62f + RadianceRatio * 1.1f, 1.06f) * 0.88f;
        Color core = new Color(245, 250, 255, 230);

        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(34f, 10f) * Projectile.scale, outer);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(22f, 4.6f) * Projectile.scale, middle);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + normal * 5.4f, rotation + 0.55f,
            new Vector2(12f, 2.6f) * Projectile.scale, middle * 0.72f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center - normal * 5.4f, rotation - 0.55f,
            new Vector2(12f, 2.6f) * Projectile.scale, middle * 0.72f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation, new Vector2(12f, 2.2f) * Projectile.scale, core);
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.24f + RadianceRatio * 0.6f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 95, prismColor, Main.rand.NextFloat(0.95f, 1.28f));
            dust.noGravity = true;
        }
    }
}
