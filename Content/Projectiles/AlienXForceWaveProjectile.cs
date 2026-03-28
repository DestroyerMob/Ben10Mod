using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AlienXForceWaveProjectile : ModProjectile {
    private const int LifetimeTicks = 18;
    private const float BaseReach = 44f;
    private const float MaxReach = 236f;
    private const float BaseWidth = 34f;
    private const float MaxWidth = 112f;
    private bool Deliberation => Projectile.ai[0] >= 0.5f;

    private float Timer {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float CurrentReach {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private float CurrentWidth {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 36;
        Projectile.height = 36;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Projectile.velocity = direction;

        if (Timer == 0f)
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.28f, Volume = 1.05f }, Projectile.Center);

        Timer++;

        float progress = Utils.GetLerpValue(0f, LifetimeTicks, Timer, true);
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.4f);
        float reachBonus = Deliberation ? 34f : 0f;
        float widthBonus = Deliberation ? 18f : 0f;

        CurrentReach = MathHelper.Lerp(BaseReach, MaxReach + reachBonus, easedProgress);
        CurrentWidth = MathHelper.Lerp(BaseWidth, MaxWidth + widthBonus, easedProgress);

        Lighting.AddLight(Projectile.Center + direction * (CurrentReach * 0.25f), new Vector3(0.58f, 0.62f, 0.96f));
        SpawnBurstDust(direction, progress);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 lineStart = Projectile.Center;
        Vector2 lineEnd = Projectile.Center + direction * CurrentReach;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
            CurrentWidth, ref collisionPoint);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        int judgement = target.GetGlobalNPC<AlienIdentityGlobalNPC>().GetAlienXJudgementStacks(Projectile.owner);
        if (judgement > 0)
            modifiers.SourceDamage *= 1f + judgement * 0.1f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        identity.ApplyAlienXJudgement(Projectile.owner, Deliberation ? 3 : 2, Deliberation ? 340 : 280);

        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float pushForce = Deliberation ? 138f : 102f;
        int judgement = identity.GetAlienXJudgementStacks(Projectile.owner);
        pushForce += judgement * 6f;

        if (target.boss)
            pushForce *= 0.6f;
        else if (target.knockBackResist > 0f)
            pushForce *= MathHelper.Lerp(0.92f, 1.4f, target.knockBackResist);

        float lift = target.boss
            ? (Deliberation ? 5.5f : 4.2f)
            : (Deliberation ? 11.5f : 8.5f);
        Vector2 pushedVelocity = direction * pushForce + new Vector2(0f, -lift);

        if (target.boss)
            target.velocity = Vector2.Lerp(target.velocity, pushedVelocity, 0.42f);
        else
            target.velocity = pushedVelocity;

        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    private void SpawnBurstDust(Vector2 direction, float progress) {
        if (Main.dedServ)
            return;

        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 frontCenter = Projectile.Center + direction * CurrentReach;
        int arcPoints = Deliberation ? 10 : 8;

        for (int i = 0; i < arcPoints; i++) {
            float arcProgress = arcPoints == 1 ? 0f : i / (float)(arcPoints - 1);
            float centered = MathHelper.Lerp(-1f, 1f, arcProgress);
            float curve = 1f - centered * centered;
            Vector2 position = frontCenter
                + perpendicular * centered * CurrentWidth * 0.92f
                - direction * (1f - curve) * 26f;

            Vector2 dustVelocity = direction * MathHelper.Lerp(1.2f, 4.6f, curve)
                + perpendicular * centered * Main.rand.NextFloat(0.2f, 1.5f);

            int dustType = i % 3 == 0 ? DustID.WhiteTorch : i % 2 == 0 ? DustID.GemDiamond : DustID.GemSapphire;
            Color dustColor = i % 3 == 0
                ? new Color(240, 245, 255)
                : Color.Lerp(new Color(125, 175, 255), new Color(205, 228, 255), Main.rand.NextFloat());

            Dust crestDust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(6f, 6f), dustType, dustVelocity, 95,
                dustColor, Main.rand.NextFloat(1.05f, 1.45f));
            crestDust.noGravity = true;
        }

        int spinePoints = Deliberation ? 7 : 5;
        for (int i = 0; i < spinePoints; i++) {
            float spineProgress = spinePoints == 1 ? 0f : i / (float)(spinePoints - 1);
            Vector2 position = Projectile.Center + direction * MathHelper.Lerp(24f, CurrentReach * 0.92f, spineProgress);
            Vector2 dustVelocity = direction * Main.rand.NextFloat(1f, 3.2f) + Main.rand.NextVector2Circular(0.7f, 0.7f);

            Dust spineDust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(CurrentWidth * 0.08f, CurrentWidth * 0.08f),
                i % 2 == 0 ? DustID.GemAmethyst : DustID.ShadowbeamStaff,
                dustVelocity, 105, new Color(145, 150, 255), Main.rand.NextFloat(0.9f, 1.25f));
            spineDust.noGravity = true;
        }

        if ((int)Timer <= 4) {
            int spokes = Deliberation ? 10 : 8;
            float flashReach = MathHelper.Lerp(18f, 52f, progress);
            for (int i = 0; i < spokes; i++) {
                Vector2 spokeDirection = (direction.RotatedBy(MathHelper.Lerp(-0.9f, 0.9f, i / (float)Math.Max(1, spokes - 1))))
                    .SafeNormalize(direction);
                Dust flashDust = Dust.NewDustPerfect(Projectile.Center + spokeDirection * flashReach, DustID.WhiteTorch,
                    spokeDirection * Main.rand.NextFloat(1.4f, 3.8f), 100, new Color(235, 242, 255),
                    Main.rand.NextFloat(1.1f, 1.5f));
                flashDust.noGravity = true;
            }
        }
    }
}
