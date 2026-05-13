using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Humungousaur;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HumungousaurCrushingChargeProjectile : ModProjectile {
    private const int BaseChargeFrames = 15;
    private const float BaseChargeSpeed = 18f;
    private const float ForwardOffset = 26f;

    private float GrowthScale => Projectile.ai[0] <= 0f ? 1f : MathF.Abs(Projectile.ai[0]);
    private bool HitSomething => Projectile.localAI[1] > 0f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 54;
        Projectile.height = 38;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = BaseChargeFrames;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        float growthRatio = GetGrowthRatio();
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.timeLeft = BaseChargeFrames + (int)MathF.Round(growthRatio * 5f);
            SpawnLaunchDust(owner);
        }

        owner.GetModPlayer<OmnitrixPlayer>().RegisterActiveLunge();

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float speed = BaseChargeSpeed + growthRatio * 4f;
        Projectile.velocity = direction * speed;
        Projectile.rotation = direction.ToRotation();
        Projectile.scale = MathHelper.Clamp(GrowthScale, 0.9f, HumungousaurTransformation.RampageScale);
        Projectile.GetGlobalProjectile<OmnitrixProjectile>().EnableScaleHitboxSync(Projectile);
        Projectile.Center = owner.Center + direction * (ForwardOffset * Projectile.scale);

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, 10 + (int)MathF.Round(growthRatio * 5f));
        owner.noKnockback = true;
        owner.noFallDmg = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;
        owner.GetModPlayer<HumungousaurCombatPlayer>().RegisterAttackGuard(3, 0.16f + growthRatio * 0.06f);

        SpawnTrailDust(direction);
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Projectile.localAI[1] = 1f;
        target.AddBuff(BuffID.BrokenArmor, GrowthScale > 1.2f ? 180 : 120);

        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + pushDirection.X * (5.4f + GetGrowthRatio() * 2.2f), -16f, 16f),
            MathHelper.Clamp(target.velocity.Y - (1.2f + GetGrowthRatio() * 0.9f), -9f, 10f));
        target.netUpdate = true;

        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.GetModPlayer<HumungousaurCombatPlayer>().RegisterImpactGuard(target, GrowthScale, shockwave: false,
                heavyHit: true);

        SpawnImpactDust(target.Center, pushDirection);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.velocity *= HitSomething ? 0.34f : 0.5f;

        if (!Main.dedServ)
            SpawnFinishDust();

        if (Projectile.owner != Main.myPlayer)
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        int shockDamage = Math.Max(1, (int)Math.Round(Projectile.damage * (HitSomething ? 0.58f : 0.38f)));
        Vector2 shockPosition = owner.Bottom + new Vector2(direction.X * 16f, -8f);
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), shockPosition, direction * (12f + GetGrowthRatio() * 2f),
            ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), shockDamage, Projectile.knockBack + 0.6f,
            Projectile.owner, GrowthScale * (HitSomething ? 0.92f : 0.82f), HitSomething ? 1f : 0f);
    }

    private float GetGrowthRatio() {
        return MathHelper.Clamp((GrowthScale - 1f) / (HumungousaurTransformation.RampageScale - 1f), 0f, 1f);
    }

    private void SpawnLaunchDust(Player owner) {
        if (Main.dedServ)
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        for (int i = 0; i < 16; i++) {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(18f, 18f),
                i % 3 == 0 ? DustID.Torch : DustID.Smoke,
                direction * Main.rand.NextFloat(1.8f, 4.8f) + Main.rand.NextVector2Circular(1.3f, 1.3f),
                105, new Color(255, 170, 105), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private void SpawnTrailDust(Vector2 direction) {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(14f, 12f),
            Main.rand.NextBool(3) ? DustID.Torch : DustID.Smoke,
            -direction * Main.rand.NextFloat(0.8f, 2.2f), 115, new Color(235, 182, 142),
            Main.rand.NextFloat(0.95f, 1.22f));
        dust.noGravity = true;
    }

    private static void SpawnImpactDust(Vector2 impactPosition, Vector2 direction) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 20; i++) {
            Dust dust = Dust.NewDustPerfect(impactPosition + Main.rand.NextVector2Circular(12f, 12f),
                i % 3 == 0 ? DustID.Torch : DustID.Smoke,
                direction.RotatedByRandom(0.52f) * Main.rand.NextFloat(2.4f, 6.2f), 95,
                new Color(255, 180, 118), Main.rand.NextFloat(1.1f, 1.65f));
            dust.noGravity = true;
        }
    }

    private void SpawnFinishDust() {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 dustCenter = Projectile.Center - direction * 8f;
        for (int i = 0; i < 18; i++) {
            Dust dust = Dust.NewDustPerfect(dustCenter + Main.rand.NextVector2Circular(16f, 8f),
                i % 2 == 0 ? DustID.Smoke : DustID.Stone,
                Main.rand.NextVector2Circular(3.2f, 2.2f), 105, new Color(220, 198, 184),
                Main.rand.NextFloat(0.95f, 1.28f));
            dust.noGravity = true;
        }
    }
}
