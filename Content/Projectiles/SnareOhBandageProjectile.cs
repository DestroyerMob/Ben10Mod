using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SnareOhBandageProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 65;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.34f, 0.28f, 0.14f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                Main.rand.NextBool() ? DustID.Sand : DustID.GoldFlame,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.1f), 100, new Color(235, 215, 160),
                Main.rand.NextFloat(0.85f, 1.08f));
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool exposedCore = OwnerExposedCore();
        target.AddBuff(ModContent.BuffType<EnemySlow>(), exposedCore ? 150 : 90);
        target.velocity *= exposedCore ? 0.08f : 0.22f;
        if (exposedCore)
            target.AddBuff(BuffID.BrokenArmor, 180);

        target.netUpdate = true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.GoldFlame : DustID.Sand,
                Main.rand.NextVector2Circular(2f, 2f), 100, new Color(230, 215, 175), Main.rand.NextFloat(0.85f, 1.1f));
            dust.noGravity = true;
        }
    }

    private bool OwnerExposedCore() {
        Player owner = Main.player[Projectile.owner];
        return owner.active && !owner.dead && owner.GetModPlayer<OmnitrixPlayer>().PrimaryAbilityEnabled;
    }
}
