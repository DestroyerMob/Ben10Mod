using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class ConquestDroneBoltProjectile : ModProjectile {
    private const float HomingRange = 460f;
    private const float HomingStrength = 0.06f;

    private int TargetIndex => (int)Projectile.ai[0] - 1;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PurpleLaser}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        NPC target = FindTarget();
        if (target != null) {
            Vector2 desiredVelocity = Projectile.Center.DirectionTo(target.Center) * Projectile.velocity.Length();
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, HomingStrength);
        }

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, new Vector3(0.08f, 0.22f, 0.34f));

        if (Main.dedServ || !Main.rand.NextBool(3))
            return;

        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.Electric : DustID.BlueTorch,
            -Projectile.velocity * 0.06f, 100, new Color(145, 230, 255), Main.rand.NextFloat(0.82f, 1.04f));
        dust.noGravity = true;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(2f, 2f), 100, new Color(145, 230, 255), Main.rand.NextFloat(0.86f, 1.08f));
            dust.noGravity = true;
        }
    }

    private NPC FindTarget() {
        if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs) {
            NPC lockedTarget = Main.npc[TargetIndex];
            if (lockedTarget.CanBeChasedBy(Projectile) &&
                Vector2.Distance(Projectile.Center, lockedTarget.Center) <= HomingRange)
                return lockedTarget;
        }

        NPC bestTarget = null;
        float bestDistance = HomingRange;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestTarget = npc;
        }

        return bestTarget;
    }
}
