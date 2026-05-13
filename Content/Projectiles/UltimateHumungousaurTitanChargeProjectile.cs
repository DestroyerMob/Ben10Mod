using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Humungousaur;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateHumungousaurTitanChargeProjectile : ModProjectile {
    private const int BaseChargeFrames = 24;
    private const int CataclysmChargeFrames = 30;
    private const float BaseChargeSpeed = 21f;
    private const float CataclysmChargeSpeed = 24.5f;
    private const float ForwardOffset = 34f;

    private bool Cataclysm => Projectile.ai[0] >= 0.5f;
    private bool HitSomething => Projectile.localAI[1] > 0f;
    private float ChargeScale => Cataclysm ? 1.26f : 1.08f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 76;
        Projectile.height = 50;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = BaseChargeFrames;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 16;
    }

    public override bool PreDraw(ref Color lightColor) => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.timeLeft = Cataclysm ? CataclysmChargeFrames : BaseChargeFrames;
            SpawnLaunchDust(owner);
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1f : owner.direction, 0f));
        float speed = Cataclysm ? CataclysmChargeSpeed : BaseChargeSpeed;
        Projectile.velocity = direction * speed;
        Projectile.rotation = direction.ToRotation();
        Projectile.scale = ChargeScale;
        Projectile.GetGlobalProjectile<OmnitrixProjectile>().EnableScaleHitboxSync(Projectile);
        Projectile.Center = owner.Center + direction * (ForwardOffset * ChargeScale);

        owner.GetModPlayer<OmnitrixPlayer>().RegisterActiveLunge();
        owner.velocity.X = direction.X * speed;
        owner.velocity.Y = MathHelper.Clamp(owner.velocity.Y, -5f, Cataclysm ? 5f : 7f);
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, Cataclysm ? 15 : 11);
        owner.noKnockback = true;
        owner.noFallDmg = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;
        owner.GetModPlayer<HumungousaurCombatPlayer>().RegisterAttackGuard(3, Cataclysm ? 0.28f : 0.22f);

        SpawnTrailDust(direction);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Projectile.localAI[1] = 1f;
        target.AddBuff(BuffID.BrokenArmor, Cataclysm ? 240 : 180);

        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + pushDirection.X * (Cataclysm ? 9f : 7f), -18f, 18f),
            MathHelper.Clamp(target.velocity.Y - (Cataclysm ? 2.4f : 1.6f), -10f, 10f));
        target.netUpdate = true;

        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.GetModPlayer<HumungousaurCombatPlayer>().RegisterImpactGuard(target, ChargeScale, shockwave: false,
                heavyHit: true);

        SpawnImpactDust(target.Center, pushDirection);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return;

        owner.velocity.X *= HitSomething ? 0.28f : 0.44f;

        if (!Main.dedServ)
            SpawnFinishDust();

        if (Projectile.owner != Main.myPlayer)
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1f : owner.direction, 0f));
        int shockDamage = Math.Max(1, (int)Math.Round(Projectile.damage * (HitSomething
            ? Cataclysm ? 0.78f : 0.62f
            : Cataclysm ? 0.52f : 0.42f)));
        float scale = Cataclysm ? 1.42f : 1.18f;
        int pairCount = Cataclysm ? 2 : 1;
        UltimateHumungousaurTransformation.SpawnShockwaveBurst(owner, Projectile.GetSource_FromThis(),
            owner.Bottom + new Vector2(direction.X * 24f, -8f), shockDamage, Projectile.knockBack + 1f, scale,
            pairCount, Cataclysm ? 2f : 1f);
    }

    private void SpawnLaunchDust(Player owner) {
        if (Main.dedServ)
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1f : owner.direction, 0f));
        int dustCount = Cataclysm ? 26 : 18;
        for (int i = 0; i < dustCount; i++) {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(22f, 24f),
                i % 3 == 0 ? DustID.Torch : DustID.Smoke,
                -direction * Main.rand.NextFloat(1.6f, Cataclysm ? 6.4f : 4.8f) + Main.rand.NextVector2Circular(1.6f, 1.6f),
                110, new Color(255, 176, 112), Main.rand.NextFloat(1f, Cataclysm ? 1.68f : 1.42f));
            dust.noGravity = true;
        }
    }

    private void SpawnTrailDust(Vector2 direction) {
        if (Main.dedServ || !Main.rand.NextBool(Cataclysm ? 1 : 2))
            return;

        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 14f),
            Main.rand.NextBool(3) ? DustID.Torch : DustID.Smoke,
            -direction * Main.rand.NextFloat(1f, Cataclysm ? 3.2f : 2.4f), 115,
            new Color(245, 188, 132), Main.rand.NextFloat(1f, Cataclysm ? 1.42f : 1.24f));
        dust.noGravity = true;
    }

    private static void SpawnImpactDust(Vector2 impactPosition, Vector2 direction) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 24; i++) {
            Dust dust = Dust.NewDustPerfect(impactPosition + Main.rand.NextVector2Circular(14f, 14f),
                i % 3 == 0 ? DustID.Torch : DustID.Smoke,
                direction.RotatedByRandom(0.54f) * Main.rand.NextFloat(2.8f, 7.2f), 95,
                new Color(255, 182, 118), Main.rand.NextFloat(1.08f, 1.7f));
            dust.noGravity = true;
        }
    }

    private void SpawnFinishDust() {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 dustCenter = Projectile.Center - direction * 10f;
        int dustCount = Cataclysm ? 28 : 20;
        for (int i = 0; i < dustCount; i++) {
            Dust dust = Dust.NewDustPerfect(dustCenter + Main.rand.NextVector2Circular(20f, 10f),
                i % 2 == 0 ? DustID.Smoke : DustID.Stone,
                Main.rand.NextVector2Circular(Cataclysm ? 4.2f : 3.2f, 2.6f), 105, new Color(224, 204, 184),
                Main.rand.NextFloat(0.98f, Cataclysm ? 1.46f : 1.28f));
            dust.noGravity = true;
        }
    }
}
