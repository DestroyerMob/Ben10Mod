using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class TerraspinGustProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 75;
        Projectile.extraUpdates = 0;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity *= 1.01f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.24f, 0.28f, 0.3f));

        if (Main.rand.NextBool()) {
            Vector2 tangent = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Dust gust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                Main.rand.NextBool(3) ? DustID.Smoke : DustID.SilverCoin,
                tangent * Main.rand.NextFloat(-1.6f, 1.6f) - Projectile.velocity * Main.rand.NextFloat(0.03f, 0.08f), 110,
                new Color(220, 240, 240), Main.rand.NextFloat(0.95f, 1.28f));
            gust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 105);
        target.velocity += new Vector2(0f, -2.4f);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 12; i++) {
            Dust burst = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Smoke : DustID.SilverCoin,
                Main.rand.NextVector2Circular(3.4f, 3.4f), 100, new Color(230, 245, 245), Main.rand.NextFloat(1f, 1.35f));
            burst.noGravity = true;
        }
    }
}
