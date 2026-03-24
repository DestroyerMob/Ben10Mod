using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SnareOhWrapProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 80;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 16;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity *= 0.992f;
        Lighting.AddLight(Projectile.Center, new Vector3(0.38f, 0.3f, 0.14f));

        if (Main.rand.NextBool()) {
            Vector2 tangent = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                Main.rand.NextBool(3) ? DustID.GoldFlame : DustID.Sand,
                tangent * Main.rand.NextFloat(-1.4f, 1.4f) - Projectile.velocity * Main.rand.NextFloat(0.03f, 0.09f), 100,
                new Color(235, 210, 165), Main.rand.NextFloat(0.95f, 1.18f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool exposedCore = OwnerExposedCore();
        target.AddBuff(ModContent.BuffType<EnemySlow>(), exposedCore ? 210 : 150);

        Vector2 constrictDirection = (Projectile.Center - target.Center).SafeNormalize(Vector2.Zero);
        target.velocity = Vector2.Lerp(target.velocity, constrictDirection * 1.8f, 0.6f);
        target.velocity *= 0.25f;

        if (exposedCore)
            target.AddBuff(BuffID.BrokenArmor, 240);

        target.netUpdate = true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 12; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 4 == 0 ? DustID.GoldFlame : DustID.Sand,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 105, new Color(230, 205, 160), Main.rand.NextFloat(0.95f, 1.22f));
            dust.noGravity = true;
        }
    }

    private bool OwnerExposedCore() {
        Player owner = Main.player[Projectile.owner];
        return owner.active && !owner.dead && owner.GetModPlayer<OmnitrixPlayer>().PrimaryAbilityEnabled;
    }
}
