using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class TerraspinVortexFieldProjectile : ModProjectile {
    private const float Radius = 74f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 4 * 60;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 24;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        SpiralNearbyEnemies();
        Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.24f, 0.28f));
        SpawnPocketDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= Radius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 150);
        Vector2 toCenter = (Projectile.Center - target.Center).SafeNormalize(Vector2.Zero);
        Vector2 tangential = toCenter.RotatedBy(MathHelper.PiOver2);
        target.velocity = Vector2.Lerp(target.velocity, tangential * 3.8f + new Vector2(0f, -1.8f), 0.55f);
    }

    private void SpiralNearbyEnemies() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance > Radius || distance <= 6f)
                continue;

            Vector2 toCenter = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
            Vector2 tangential = toCenter.RotatedBy(MathHelper.PiOver2);
            float strength = MathHelper.Lerp(0.18f, 0.06f, distance / Radius);
            npc.velocity = Vector2.Lerp(npc.velocity,
                tangential * (2.2f + strength * 8f) + toCenter * 1.1f + new Vector2(0f, -0.8f), strength);
        }
    }

    private void SpawnPocketDust() {
        if (Main.dedServ)
            return;

        float rotation = Main.GlobalTimeWrappedHourly * 5.2f;
        const int points = 10;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 unit = angle.ToRotationVector2();
            Vector2 position = Projectile.Center + unit * Main.rand.NextFloat(Radius * 0.45f, Radius);
            Vector2 velocity = unit.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.8f, 2.6f);

            Dust dust = Dust.NewDustPerfect(position, i % 2 == 0 ? DustID.Smoke : DustID.SilverCoin, velocity, 105,
                new Color(225, 240, 240), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }
}
