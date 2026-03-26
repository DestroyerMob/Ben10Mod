using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class DiamondHeadPrismPincerProjectile : ModProjectile {
    private const int LifetimeTicks = 16;
    private const float StartScale = 0.84f;
    private const float EndScale = 1.08f;
    private const int BurstShardCount = 3;

    public override string Texture => "Ben10Mod/Content/Projectiles/GiantDiamondProjectile";

    public override void SetDefaults() {
        Projectile.width = 54;
        Projectile.height = 108;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 14;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.scale = StartScale;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
    }

    public override void AI() {
        Vector2 target = new(Projectile.ai[0], Projectile.ai[1]);
        Vector2 toTarget = target - Projectile.Center;
        float distanceToTarget = toTarget.Length();
        float stepDistance = Projectile.velocity.Length();

        if (distanceToTarget <= stepDistance + 10f) {
            Projectile.Center = target;
            Projectile.Kill();
            return;
        }

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        Projectile.scale = MathHelper.Lerp(StartScale, EndScale, progress);
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        Lighting.AddLight(Projectile.Center, 0.22f, 0.34f, 0.46f);
        SpawnTravelDust();
    }

    public override void OnKill(int timeLeft) {
        Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        if (Projectile.owner == Main.myPlayer) {
            int shardDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.45f));
            for (int i = 0; i < BurstShardCount; i++) {
                float spread = BurstShardCount == 1 ? 0f : MathHelper.Lerp(-0.2f, 0.2f, i / (float)(BurstShardCount - 1));
                Vector2 shardVelocity = forward.RotatedBy(spread) * Main.rand.NextFloat(12f, 14.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shardVelocity,
                    ModContent.ProjectileType<DiamondHeadProjectile>(), shardDamage, Projectile.knockBack * 0.8f, Projectile.owner);
            }
        }

        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 26f), DustID.GemDiamond,
                Main.rand.NextVector2Circular(3.2f, 3.2f), 95, new Color(220, 255, 255), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private void SpawnTravelDust() {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        Vector2 lateral = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
        Vector2 dustPosition = Projectile.Center + lateral * Main.rand.NextFloat(-Projectile.width * 0.18f, Projectile.width * 0.18f) +
            Main.rand.NextVector2Circular(4f, 8f);

        Dust dust = Dust.NewDustPerfect(dustPosition, DustID.GemDiamond,
            -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.08f), 105, new Color(205, 255, 255), Main.rand.NextFloat(0.95f, 1.2f));
        dust.noGravity = true;
    }
}
