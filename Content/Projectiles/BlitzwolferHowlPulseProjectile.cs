using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BlitzwolferHowlPulseProjectile : ModProjectile {
    private const int MaxLifetime = 22;
    private bool Heightened => Projectile.ai[0] >= 0.5f;

    private float Progress => 1f - Projectile.timeLeft / (float)MaxLifetime;
    private float GrowthProgress => MathF.Sqrt(MathHelper.Clamp(Progress, 0f, 1f));
    private float CollisionRadius => MathHelper.Lerp(16f, Heightened ? 38f : 32f, GrowthProgress);

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 4;
        Projectile.timeLeft = MaxLifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = MaxLifetime + 2;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 1f, 0.26f) * 0.72f);
        SpawnWaveDust();

        if (Main.rand.NextBool(2)) {
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            Vector2 dustOffset = perpendicular * Main.rand.NextFloatDirection() * Main.rand.NextFloat(4f, 11f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.GreenTorch,
                Projectile.velocity * 0.06f, 100, new Color(160, 255, 145), Heightened ? 1.2f : 1.05f);
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CollisionRadius;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int stacks = identity.GetBlitzwolferResonanceStacks(Projectile.owner);
        if (stacks > 0)
            modifiers.SourceDamage *= 1f + stacks * 0.05f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        identity.ApplyBlitzwolferResonance(Projectile.owner, Heightened ? 2 : 1, Heightened ? 280 : 240);

        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        target.velocity = Vector2.Lerp(target.velocity, pushDirection * (Heightened ? 8.5f : 7f), target.boss ? 0.08f : 0.2f);
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    private void SpawnWaveDust() {
        float fade = Utils.GetLerpValue(0f, 0.12f, Progress, true) *
            Utils.GetLerpValue(0f, 0.24f, Projectile.timeLeft / (float)MaxLifetime, true);
        float radius = MathHelper.Lerp(16f, Heightened ? 34f : 30f, GrowthProgress);
        float arcHalfWidth = MathHelper.Lerp(0.58f, Heightened ? 0.95f : 0.86f, GrowthProgress);
        int segments = Heightened ? 13 : 11;

        for (int i = 0; i < segments; i++) {
            float completion = i / (float)(segments - 1);
            float angle = Projectile.rotation + MathHelper.Lerp(-arcHalfWidth, arcHalfWidth, completion);
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 tangentVelocity = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.16f + Projectile.velocity * 0.03f;

            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GreenTorch, tangentVelocity, 150,
                new Color(115, 255, 130) * fade, 1.05f + Progress * 0.28f);
            dust.noGravity = true;
            dust.fadeIn = 0.55f;
            dust.scale *= 0.96f;
            dust.velocity *= 0.74f;
            dust.alpha = 175;

            if (i % 2 == 0) {
                Dust innerDust = Dust.NewDustPerfect(Projectile.Center + offset * 0.9f, DustID.GemEmerald, tangentVelocity * 0.42f, 170,
                    new Color(205, 255, 190) * fade, 0.74f + Progress * 0.14f);
                innerDust.noGravity = true;
                innerDust.velocity *= 0.48f;
                innerDust.alpha = 195;
            }
        }
    }
}
