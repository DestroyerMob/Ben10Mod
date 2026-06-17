using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BuzzShockProjectile : ModProjectile {
    private const int MaxPrimaryForkDepth = 2;
    private const float ForkSearchRange = 520f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        AIType = ProjectileID.Bullet;
        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 72;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, 0.12f, 0.38f, 0.58f);
        if (!Main.rand.NextBool(2))
            return;

        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
            DustID.UltraBrightTorch, -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.4f, 1.6f),
            90, Color.White, 0.95f);
        dust.noGravity = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool wasTagged = BuzzShockTargeting.IsTagged(target);
        target.AddBuff(BuzzShockTargeting.TagBuffType, wasTagged ? 360 : 240);
        target.AddBuff(BuffID.Electrified, wasTagged ? 90 : 45);
        SpawnImpactDust(target, wasTagged ? 14 : 8);

        if (!wasTagged || Projectile.ai[0] >= MaxPrimaryForkDepth)
            return;

        NPC chainTarget = BuzzShockTargeting.FindTarget(target.Center, ForkSearchRange, preferTagged: false,
            preferUntagged: true, excludedWhoAmI: target.whoAmI);
        if (chainTarget == null)
            return;

        Vector2 direction = target.Center.DirectionTo(chainTarget.Center);
        if (direction == Vector2.Zero)
            direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

        Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, direction * 18f, Type,
            Math.Max(1, (int)(Projectile.damage * 0.55f)), Projectile.knockBack * 0.65f, Projectile.owner,
            Projectile.ai[0] + 1f);
    }

    private static void SpawnImpactDust(NPC target, int count) {
        for (int i = 0; i < count; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                DustID.UltraBrightTorch, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.3f, 3.4f),
                90, Color.White, Main.rand.NextFloat(0.9f, 1.25f));
            dust.noGravity = true;
        }
    }
}
