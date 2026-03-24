using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WaterHazardPressureProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 84;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.08f, 0.38f, 0.55f));

        Dust mist = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
            Main.rand.NextBool(4) ? DustID.Water : DustID.DungeonWater,
            -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 105, new Color(120, 215, 255),
            Main.rand.NextFloat(0.9f, 1.18f));
        mist.noGravity = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 60);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust splash = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.Water : DustID.DungeonWater,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 95, new Color(155, 225, 255), Main.rand.NextFloat(0.95f, 1.2f));
            splash.noGravity = true;
        }
    }
}
