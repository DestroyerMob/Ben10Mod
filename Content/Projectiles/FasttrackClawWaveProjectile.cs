using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FasttrackClawWaveProjectile : ModProjectile {
    private bool Empowered => Projectile.ai[0] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 34;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity *= Empowered ? 0.992f : 0.986f;
        Lighting.AddLight(Projectile.Center, Empowered ? new Vector3(0.12f, 0.68f, 0.42f) : new Vector3(0.08f, 0.42f, 0.28f));

        if (Main.rand.NextBool(Empowered ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                Main.rand.NextBool() ? DustID.GemEmerald : DustID.GreenFairy,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.16f), 105, new Color(145, 255, 215),
                Main.rand.NextFloat(0.85f, Empowered ? 1.18f : 1.02f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 lineStart = Projectile.Center - direction * 18f;
        Vector2 lineEnd = Projectile.Center + direction * (Empowered ? 34f : 28f);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
            Empowered ? 24f : 18f, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float rotation = direction.ToRotation();
        Color outer = Empowered ? new Color(20, 24, 28, 225) : new Color(12, 16, 20, 220);
        Color inner = Empowered ? new Color(150, 255, 220, 180) : new Color(95, 220, 190, 170);
        Color core = Empowered ? new Color(225, 255, 245, 150) : new Color(190, 255, 235, 130);

        for (int i = -1; i <= 1; i++) {
            Vector2 offset = normal * i * (Empowered ? 7f : 5f);
            Main.EntitySpriteDraw(pixel, center + offset, null, outer, rotation, Vector2.One * 0.5f,
                new Vector2(Empowered ? 44f : 36f, 8f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(pixel, center + offset, null, inner, rotation, Vector2.One * 0.5f,
                new Vector2(Empowered ? 31f : 26f, 4.6f), SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(pixel, center, null, core, rotation, Vector2.One * 0.5f,
            new Vector2(Empowered ? 20f : 17f, 2.4f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), Empowered ? 120 : 90);
        target.AddBuff(BuffID.BrokenArmor, Empowered ? 150 : 105);
        target.netUpdate = true;
    }
}
