using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UpgradePulseRoundProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GoldenBullet}";

    private int FlagMask => (int)Math.Round(Projectile.ai[1]);
    private bool Overclocked => (FlagMask & 1) != 0;
    private bool FullyIntegrated => (FlagMask & 2) != 0;
    private UpgradeAttackVariant Variant => (UpgradeAttackVariant)((FlagMask >> 2) & 0x3);

    public override void SetDefaults() {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 84;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.scale = Variant == UpgradeAttackVariant.Construct ? 0.95f : 1.05f;
            if (Overclocked)
                Projectile.scale *= 1.05f;
            if (FullyIntegrated)
                Projectile.penetrate++;
        }

        Projectile.velocity *= 1.01f;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.18f, 0.95f, 0.9f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.12f), 100,
                new Color(170, 255, 235), Main.rand.NextFloat(0.9f, 1.1f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Overclocked || FullyIntegrated)
            target.AddBuff(BuffID.BrokenArmor, FullyIntegrated ? 150 : 90);

        if (Variant != UpgradeAttackVariant.Primary)
            target.AddBuff(ModContent.BuffType<EnemySlow>(), 75);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GoldCoin : DustID.Electric,
                Main.rand.NextVector2Circular(2.2f, 2.2f), 95, new Color(170, 255, 235), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }
}
