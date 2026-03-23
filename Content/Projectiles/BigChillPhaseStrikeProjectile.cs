using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.BigChill;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BigChillPhaseStrikeProjectile : ModProjectile {
    public const float DashSpeed = 34f;
    public const int MinDashFrames = 6;
    public const int MaxDashFrames = 20;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 46;
        Projectile.height = 46;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinDashFrames;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.velocity = direction * DashSpeed;
        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, 12);
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);

        Projectile.Center = owner.Center + direction * 20f;
        Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.Frost,
                -Projectile.velocity * 0.08f, 120, new Color(180, 240, 255), 1.08f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemyFrozen>(), BigChillTransformation.UltimateFreezeDuration);
        target.AddBuff(BuffID.Frostburn2, BigChillTransformation.UltimateFreezeDuration);
        target.netUpdate = true;

        for (int i = 0; i < 16; i++) {
            Vector2 burstVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.8f) *
                Main.rand.NextFloat(1.6f, 4.2f);
            Dust dust = Dust.NewDustPerfect(target.Center, DustID.Frost, burstVelocity, 110,
                new Color(190, 245, 255), 1.2f);
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        owner.noKnockback = false;
        owner.velocity *= 0.35f;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 20; i++) {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(14f, 18f), DustID.Frost,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 110, new Color(180, 240, 255), 1.1f);
            dust.noGravity = true;
        }
    }
}
