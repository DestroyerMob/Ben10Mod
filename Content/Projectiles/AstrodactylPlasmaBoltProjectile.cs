using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AstrodactylPlasmaBoltProjectile : ModProjectile {
    private bool Hyperflight => Projectile.ai[0] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 86;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.velocity.LengthSquared() < (Hyperflight ? 1024f : 841f))
            Projectile.velocity *= Hyperflight ? 1.018f : 1.014f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, Hyperflight ? new Vector3(0.25f, 1f, 0.68f) : new Vector3(0.18f, 0.9f, 0.56f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                Main.rand.NextBool(3) ? DustID.GreenTorch : DustID.GemEmerald,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.11f), 100, new Color(170, 255, 210),
                Main.rand.NextFloat(0.9f, Hyperflight ? 1.22f : 1.08f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = direction.ToRotation();

        Main.EntitySpriteDraw(pixel, center, null, new Color(55, 220, 125, 125), rotation, Vector2.One * 0.5f,
            new Vector2(Hyperflight ? 34f : 28f, Hyperflight ? 7.5f : 6f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(225, 255, 235, 215), rotation, Vector2.One * 0.5f,
            new Vector2(Hyperflight ? 18f : 15f, 2.8f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, Hyperflight ? 180 : 120);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.GemEmerald,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 100, new Color(180, 255, 220), Main.rand.NextFloat(0.9f, 1.18f));
            dust.noGravity = true;
        }
    }
}
