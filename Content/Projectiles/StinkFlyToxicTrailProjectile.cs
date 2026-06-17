using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class StinkFlyToxicTrailProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 42;
        Projectile.height = 26;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 4;
        Projectile.timeLeft = 36;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override void AI() {
        float pressure = MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
        Projectile.scale = MathHelper.Lerp(0.84f, 1.22f, pressure) * MathHelper.Lerp(0.75f, 1f,
            Projectile.timeLeft / 36f);
        Projectile.velocity *= 0.965f;
        Projectile.rotation += 0.035f * Projectile.direction;

        Lighting.AddLight(Projectile.Center, new Vector3(0.08f, 0.18f, 0.04f));

        if (Main.rand.NextBool(2)) {
            Dust mist = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 10f),
                Main.rand.NextBool() ? DustID.GreenBlood : DustID.Poisoned,
                Main.rand.NextVector2Circular(0.35f, 0.25f), 115, new Color(186, 232, 80),
                Main.rand.NextFloat(0.7f, 1.05f));
            mist.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle source = new(0, 0, 1, 1);
        Vector2 origin = new(0.5f, 0.5f);
        float fade = MathHelper.Clamp(Projectile.timeLeft / 18f, 0f, 1f);

        Main.EntitySpriteDraw(pixel, drawPosition, source, new Color(92, 154, 56, 105) * fade, Projectile.rotation,
            origin, new Vector2(38f, 18f) * Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, drawPosition + new Vector2(0f, -2f), source, new Color(224, 246, 112, 90) * fade,
            Projectile.rotation * 0.6f, origin, new Vector2(20f, 8f) * Projectile.scale, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Poisoned, 2 * 60);

        if (target.HasBuff(ModContent.BuffType<EnemySlow>()))
            target.AddBuff(BuffID.Venom, 90);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 7; i++) {
            Dust puff = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 10f),
                i % 2 == 0 ? DustID.GreenBlood : DustID.Poisoned, Main.rand.NextVector2Circular(1.1f, 0.8f),
                110, new Color(196, 235, 88), Main.rand.NextFloat(0.75f, 1.1f));
            puff.noGravity = true;
        }
    }
}
