using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class StinkFlySlowProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 90;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity *= 0.994f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.12f, 0.24f, 0.08f));

        if (Main.rand.NextBool()) {
            Dust slimeDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextBool(3) ? DustID.GreenMoss : DustID.JungleSpore,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.12f), 105, new Color(165, 220, 95),
                Main.rand.NextFloat(0.9f, 1.15f));
            slimeDust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle source = new(0, 0, 1, 1);
        Vector2 origin = new(0.5f, 0.5f);
        float rotation = Projectile.rotation;

        Main.EntitySpriteDraw(pixel, drawPosition, source, new Color(118, 174, 58, 220), rotation, origin,
            new Vector2(13f, 8f) * Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, drawPosition - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 2f, source,
            new Color(214, 247, 116, 190), rotation, origin, new Vector2(7f, 4f) * Projectile.scale, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 5 * 60);
        target.velocity *= 0.84f;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Dust splash = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.JungleSpore : DustID.GreenMoss,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 100, new Color(185, 240, 105), Main.rand.NextFloat(0.95f, 1.2f));
            splash.noGravity = true;
        }
    }
}
