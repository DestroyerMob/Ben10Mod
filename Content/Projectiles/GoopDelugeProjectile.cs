using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GoopDelugeProjectile : ModProjectile {
    private const int SwellFrames = 7;
    private const int DashFrames = 13;
    private const float DashSpeed = 21f;
    private const float IdleOffset = 10f;
    private const float DashOffset = 30f;
    private const float SwellScaleX = 2.28f;
    private const float SwellScaleY = 0.48f;
    private const float DashScaleX = 1.9f;
    private const float DashScaleY = 0.58f;
    private const float ImpactScaleX = 0.84f;
    private const float ImpactScaleY = 1.16f;
    private const int BurstPuddleCount = 6;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    private int State {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private int StateTimer {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private bool HasImpacted {
        get => Projectile.localAI[0] > 0f;
        set => Projectile.localAI[0] = value ? 1f : 0f;
    }

    private bool SpawnedStartBurst {
        get => Projectile.localAI[1] > 0f;
        set => Projectile.localAI[1] = value ? 1f : 0f;
    }

    public override void SetDefaults() {
        Projectile.width = 72;
        Projectile.height = 44;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = SwellFrames + DashFrames + 8;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 999;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, 12);
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);

        if (State == 0) {
            Projectile.friendly = false;
            owner.velocity *= 0.75f;
            owner.itemRotation = direction.ToRotation() * owner.direction;
            omp.GoopVisualScale = Vector2.Lerp(omp.GoopVisualScale, new Vector2(SwellScaleX, SwellScaleY), 0.45f);
            Projectile.Center = owner.Center + direction * IdleOffset;
            Projectile.rotation = direction.ToRotation();

            if (!SpawnedStartBurst) {
                SpawnedStartBurst = true;
                SpawnPuddleBurst(owner, Projectile.GetSource_FromThis(), 0.42f, 0.7f);
            }

            EmitSwellDust(owner.Center, direction);

            StateTimer++;
            if (StateTimer >= SwellFrames) {
                State = 1;
                StateTimer = 0;
                Projectile.friendly = true;
                Projectile.netUpdate = true;
            }

            return;
        }

        owner.velocity = direction * DashSpeed;
        owner.itemRotation = direction.ToRotation() * owner.direction;
        Projectile.Center = owner.Center + direction * DashOffset;
        Projectile.rotation = direction.ToRotation();
        omp.GoopVisualScale = Vector2.Lerp(omp.GoopVisualScale, new Vector2(DashScaleX, DashScaleY), 0.5f);

        Lighting.AddLight(owner.Center, 0.08f, 0.28f, 0.08f);
        EmitDashDust(owner.Center, direction);

        StateTimer++;
        if (StateTimer >= DashFrames)
            Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Venom, 6 * 60);

        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead) {
            OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
            Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
            owner.velocity = -direction * 2.8f;
            omp.GoopVisualScale = new Vector2(ImpactScaleX, ImpactScaleY);
        }

        EmitImpactDust(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX));
        HasImpacted = true;
        Projectile.Kill();
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead) {
            OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
            owner.noKnockback = false;
            if (!HasImpacted) {
                owner.velocity *= 0.3f;
                SpawnPuddleBurst(owner, Projectile.GetSource_FromThis(), 0.34f, 0.55f);
            }
            omp.GoopVisualScale = HasImpacted ? new Vector2(ImpactScaleX, ImpactScaleY) : new Vector2(1.12f, 0.92f);
        }

        if (!HasImpacted)
            EmitImpactDust(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX));
    }

    private static void EmitSwellDust(Vector2 center, Vector2 direction) {
        if (Main.rand.NextBool(2)) {
            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 position = center + direction * Main.rand.NextFloat(-8f, 10f) + normal * Main.rand.NextFloat(-18f, 18f);
            Vector2 velocity = direction * Main.rand.NextFloat(0.2f, 1.2f) + normal * Main.rand.NextFloat(-0.7f, 0.7f);
            Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.GreenTorch : DustID.GreenMoss,
                velocity, 90, new Color(130, 245, 150), Main.rand.NextFloat(1.05f, 1.3f));
            dust.noGravity = false;
        }
    }

    private static void EmitDashDust(Vector2 center, Vector2 direction) {
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        for (int i = 0; i < 2; i++) {
            Vector2 position = center + direction * Main.rand.NextFloat(-20f, 22f) + normal * Main.rand.NextFloat(-20f, 20f);
            Vector2 velocity = -direction * Main.rand.NextFloat(1.2f, 3.4f) + normal * Main.rand.NextFloat(-1.1f, 1.1f);
            Dust dust = Dust.NewDustPerfect(position, i == 0 ? DustID.GreenTorch : DustID.GreenMoss,
                velocity, 95, new Color(125, 245, 145), Main.rand.NextFloat(1f, 1.28f));
            dust.noGravity = true;
        }
    }

    private static void EmitImpactDust(Vector2 center, Vector2 direction) {
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        for (int i = 0; i < 20; i++) {
            Vector2 velocity = direction.RotatedByRandom(0.45f) * Main.rand.NextFloat(1.2f, 4.6f) +
                               normal * Main.rand.NextFloat(-1.6f, 1.6f);
            Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(8f, 8f),
                i % 3 == 0 ? DustID.GreenTorch : DustID.GreenMoss, velocity, 90,
                new Color(130, 245, 150), Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = false;
        }
    }

    private void SpawnPuddleBurst(Player owner, IEntitySource source, float damageMultiplier, float speedMultiplier) {
        if (Projectile.owner != Main.myPlayer)
            return;

        int puddleDamage = Math.Max(1, (int)(Projectile.damage * damageMultiplier));
        for (int i = 0; i < BurstPuddleCount; i++) {
            float angle = MathHelper.TwoPi * i / BurstPuddleCount + Main.rand.NextFloat(-0.12f, 0.12f);
            Vector2 direction = angle.ToRotationVector2();
            Vector2 velocity = direction * Main.rand.NextFloat(4.8f, 7.6f) * speedMultiplier + new Vector2(0f, -2.2f);
            Projectile.NewProjectile(source, owner.Center + direction * Main.rand.NextFloat(6f, 14f), velocity,
                ModContent.ProjectileType<GoopPuddleBombProjectile>(), puddleDamage, 0f, owner.whoAmI);
        }
    }
}
