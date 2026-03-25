using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UpgradeBladeWaveProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private int FlagMask => (int)Math.Round(Projectile.ai[1]);
    private bool Overclocked => (FlagMask & 1) != 0;
    private bool FullyIntegrated => (FlagMask & 2) != 0;
    private UpgradeAttackVariant Variant => (UpgradeAttackVariant)((FlagMask >> 2) & 0x3);

    public override void SetDefaults() {
        Projectile.width = 42;
        Projectile.height = 42;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 18;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.scale = Variant == UpgradeAttackVariant.Construct ? 0.9f : 1f;
            if (Variant != UpgradeAttackVariant.Primary)
                Projectile.scale += 0.18f;
            if (Overclocked)
                Projectile.scale += 0.06f;
            if (FullyIntegrated)
                Projectile.scale += 0.12f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Projectile.velocity *= Variant == UpgradeAttackVariant.Construct ? 0.95f : 0.92f;
        Lighting.AddLight(Projectile.Center, 0.2f, 1f, 0.62f);

        for (int i = 0; i < 2; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                i == 0 ? DustID.GreenTorch : DustID.SilverCoin,
                Projectile.velocity.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.08f, 0.24f), 95,
                new Color(175, 255, 200), Main.rand.NextFloat(0.9f, 1.1f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float opacity = Utils.GetLerpValue(0f, 0.18f, 1f - Projectile.timeLeft / 18f, true) *
                        Utils.GetLerpValue(1f, 0.4f, Projectile.timeLeft / 18f, true);
        float scale = Projectile.scale;

        Main.EntitySpriteDraw(pixel, center, null, new Color(110, 255, 185, 210) * opacity, Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(7f * scale, 48f * scale), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(245, 255, 250, 240) * opacity, Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(2.8f * scale, 36f * scale), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.BrokenArmor, FullyIntegrated ? 180 : 120);
        if (Variant != UpgradeAttackVariant.Primary)
            target.AddBuff(ModContent.BuffType<EnemySlow>(), FullyIntegrated ? 150 : 105);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.SilverCoin,
                Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.45f) * Main.rand.NextFloat(1f, 3f), 90,
                new Color(175, 255, 200), 1.05f);
            dust.noGravity = true;
        }
    }
}
