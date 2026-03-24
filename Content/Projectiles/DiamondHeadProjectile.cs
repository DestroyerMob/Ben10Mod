using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class DiamondHeadProjectile : ModProjectile {
    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 24;
        Projectile.scale = 1.05f;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 150;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.velocity = Projectile.velocity.RotatedByRandom(0.045f);
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.18f, 0.34f, 0.4f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemDiamond,
                -Projectile.velocity * 0.08f, 110, new Color(210, 255, 255), Main.rand.NextFloat(0.8f, 1.1f));
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 6; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemDiamond,
                Main.rand.NextVector2Circular(1.8f, 1.8f), 100, new Color(225, 255, 255), Main.rand.NextFloat(0.9f, 1.2f));
            dust.noGravity = true;
        }
    }
}
