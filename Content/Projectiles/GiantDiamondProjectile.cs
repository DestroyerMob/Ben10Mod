using Ben10Mod.Content.DamageClasses;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GiantDiamondProjectile : ModProjectile {
    private const float SideDiamondOffset = 72f;

    public override void SetDefaults() {
        Projectile.width = 64;
        Projectile.height = 128;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void OnSpawn(IEntitySource source) {
        if (Projectile.ai[0] != 0f)
            return;

        SpawnSideDiamond(source, -SideDiamondOffset, 0.82f);
        SpawnSideDiamond(source, SideDiamondOffset, 0.82f);
    }

    public override void AI() {
        Projectile.velocity.X *= 0.98f;
        Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + 0.7f, 10f, 24f);
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.22f, 0.34f, 0.46f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 34f), DustID.GemDiamond,
                Projectile.velocity * Main.rand.NextFloat(0.08f, 0.18f), 105, new Color(210, 255, 255), Main.rand.NextFloat(1.1f, 1.45f));
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 18; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 38f), DustID.GemDiamond,
                Main.rand.NextVector2Circular(4.5f, 4.5f), 95, new Color(225, 255, 255), Main.rand.NextFloat(1.15f, 1.7f));
            dust.noGravity = true;
        }
    }

    private void SpawnSideDiamond(IEntitySource source, float horizontalOffset, float damageMultiplier) {
        int projectileIndex = Projectile.NewProjectile(source,
            Projectile.Center + new Vector2(horizontalOffset, 0f),
            new Vector2(horizontalOffset * 0.015f, Projectile.velocity.Y),
            Type,
            Math.Max(1, (int)(Projectile.damage * damageMultiplier)),
            Projectile.knockBack,
            Projectile.owner,
            1f);

        if (projectileIndex < 0 || projectileIndex >= Main.maxProjectiles)
            return;

        Main.projectile[projectileIndex].scale = 0.85f;
        Main.projectile[projectileIndex].rotation = horizontalOffset < 0f ? -0.15f : 0.15f;
        Main.projectile[projectileIndex].netUpdate = true;
    }
}
