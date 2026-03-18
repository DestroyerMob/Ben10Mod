using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HumungousaurPunchProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private const int PunchLifetime = 12;

    public override void SetDefaults() {
        Projectile.width = 58;
        Projectile.height = 58;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = PunchLifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.scale = Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0];
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        if (direction.X != 0f)
            owner.direction = direction.X > 0f ? 1 : -1;

        float progress = 1f - Projectile.timeLeft / (float)PunchLifetime;
        float extensionCurve = progress < 0.38f ? progress / 0.38f : 1f - (progress - 0.38f) / 0.62f * 0.55f;
        float extension = MathHelper.Lerp(12f, 34f * Projectile.scale, MathHelper.Clamp(extensionCurve, 0f, 1f));
        Vector2 shoulderOffset = new(owner.direction * 8f, -4f);

        Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;
        Projectile.Center = owner.MountedCenter + shoulderOffset + direction * extension;
        owner.heldProj = Projectile.whoAmI;
        owner.itemRotation = direction.ToRotation() * owner.direction;
        owner.itemTime = 2;
        owner.itemAnimation = 2;

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            for (int i = 0; i < 14; i++) {
                Dust dust = Dust.NewDustPerfect(owner.MountedCenter + shoulderOffset + direction * 30f, DustID.Smoke,
                    direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f), 140, new Color(255, 190, 135), 1.15f);
                dust.noGravity = true;
            }
        }

        if (Main.rand.NextBool(2)) {
            Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), DustID.Torch,
                -direction * Main.rand.NextFloat(0.4f, 1.5f), 120, new Color(255, 170, 95), 1f + Projectile.scale * 0.08f);
            trailDust.noGravity = true;
        }

        Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.45f, 0.18f) * 0.9f);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 direction = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
        Vector2 fistCenter = center - direction * (8f * Projectile.scale);
        float rotation = Projectile.rotation - MathHelper.PiOver2;
        Rectangle outerRect = new(0, 0, (int)(20f * Projectile.scale), (int)(13f * Projectile.scale));
        Rectangle innerRect = new(0, 0, (int)(9f * Projectile.scale), (int)(5f * Projectile.scale));

        Main.spriteBatch.Draw(pixel, fistCenter, outerRect, new Color(196, 116, 67, 235), rotation,
            new Vector2(outerRect.Width * 0.5f, outerRect.Height * 0.5f), 1f, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, fistCenter, innerRect, new Color(255, 214, 170, 175), rotation,
            new Vector2(innerRect.Width * 0.5f, innerRect.Height * 0.5f), 1f, SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int i = 0; i < 16; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center, DustID.Smoke,
                Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 5.5f), 130, new Color(220, 155, 100), 1.2f);
            dust.noGravity = true;
        }
    }
}
