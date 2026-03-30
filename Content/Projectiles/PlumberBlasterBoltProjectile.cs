using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class PlumberBlasterBoltProjectile : ModProjectile {
    private bool StrongVariant => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MartianTurretBolt}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 72;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.scale = StrongVariant ? 1.04f : 0.9f;
            if (StrongVariant) {
                Projectile.penetrate = 2;
                Projectile.timeLeft = System.Math.Max(Projectile.timeLeft, 90);
            }
        }

        if (StrongVariant)
            Projectile.velocity = Projectile.velocity.RotatedBy(MathF.Sin((Main.GameUpdateCount + Projectile.identity) * 0.07f) * 0.008f);

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, StrongVariant ? new Vector3(0.2f, 0.9f, 1f) : new Vector3(0.12f, 0.68f, 0.82f));

        if (Main.dedServ || !Main.rand.NextBool(3))
            return;

        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.Electric : DustID.BlueTorch,
            -Projectile.velocity * 0.08f, 95, StrongVariant ? new Color(165, 255, 255) : new Color(110, 220, 255),
            StrongVariant ? Main.rand.NextFloat(0.95f, 1.18f) : Main.rand.NextFloat(0.82f, 1.02f));
        dust.noGravity = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = frame.Size() * 0.5f;

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            float alpha = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
            Color trailColor = (StrongVariant ? new Color(120, 255, 250, 0) : new Color(95, 210, 255, 0)) * (0.42f * alpha);
            Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Main.EntitySpriteDraw(texture, drawPosition, frame, trailColor, Projectile.rotation, origin,
                Projectile.scale * (0.85f + alpha * 0.22f), SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, origin,
            Projectile.scale, SpriteEffects.None, 0);

        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Color coreColor = StrongVariant ? new Color(180, 255, 250, 0) : new Color(125, 225, 255, 0);
        Main.EntitySpriteDraw(pixel, Projectile.Center - Main.screenPosition, null, coreColor * 0.72f, Projectile.rotation,
            new Vector2(0.5f, 0.5f), new Vector2(18f, StrongVariant ? 4.6f : 3.4f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (StrongVariant)
            target.AddBuff(BuffID.Electrified, 75);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(2.3f, 2.3f), 100,
                StrongVariant ? new Color(165, 255, 255) : new Color(110, 220, 255),
                Main.rand.NextFloat(0.9f, 1.12f));
            dust.noGravity = true;
        }
    }
}
