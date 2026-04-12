using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.EchoEcho;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateEchoEchoSonicBlastProjectile : ModProjectile {
    private const int MaxLifetime = 42;

    private int EncodedSourceId => (int)Math.Round(Projectile.ai[0]);
    private ref float DelayTicks => ref Projectile.ai[1];
    private Vector2 StoredVelocity => new(Projectile.localAI[0], Projectile.localAI[1]);

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 1;
        Projectile.timeLeft = MaxLifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        EnsureStoredVelocity();
        if (DelayTicks > 0f) {
            Projectile.velocity = Vector2.Zero;
            Projectile.friendly = false;
            DelayTicks--;
            SpawnChargeDust();
            return;
        }

        if (Projectile.velocity == Vector2.Zero)
            Projectile.velocity = StoredVelocity;

        Projectile.friendly = true;
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.82f, 0.72f) * 0.94f);
        SpawnWaveDust();
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            return;

        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        UltimateEchoEchoShotKind kind = UltimateEchoEchoTransformation.DecodeShotKind(EncodedSourceId);
        if (identity.IsEchoEchoFracturedFor(Projectile.owner) &&
            UltimateEchoEchoTransformation.IsSpeakerDrivenShot(kind) &&
            owner.GetModPlayer<UltimateEchoEchoStatePlayer>().CataclysmActive) {
            modifiers.SourceDamage *= 1.18f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            return;

        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        UltimateEchoEchoShotKind kind = UltimateEchoEchoTransformation.DecodeShotKind(EncodedSourceId);
        bool targetWasFocused = identity.IsUltimateEchoEchoFocusedFor(Projectile.owner);

        if (UltimateEchoEchoTransformation.IsPlayerPrimaryShot(kind))
            identity.ApplyUltimateEchoEchoFocus(Projectile.owner, UltimateEchoEchoTransformation.FocusedDurationTicks);

        EchoEchoTransformation.ResolveResonanceHit(Projectile, target, damageDone, EncodedSourceId,
            UltimateEchoEchoTransformation.IsHeavyResonanceShot(kind));

        if (kind == UltimateEchoEchoShotKind.PlayerCenter &&
            targetWasFocused &&
            owner.GetModPlayer<UltimateEchoEchoStatePlayer>().EffectiveOverclockActive) {
            UltimateEchoEchoTransformation.TryTriggerImmediateVolley(owner, target, Math.Max(Projectile.damage, damageDone),
                Projectile.knockBack + 0.65f);
        }

        target.netUpdate = true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.28f, Volume = 0.34f }, Projectile.Center);
        return true;
    }

    public override bool PreDraw(ref Color lightColor) => false;

    private void EnsureStoredVelocity() {
        if (Projectile.localAI[0] != 0f || Projectile.localAI[1] != 0f)
            return;

        Vector2 launchVelocity = Projectile.velocity;
        if (launchVelocity.LengthSquared() <= 0.01f) {
            Player owner = Main.player[Projectile.owner];
            launchVelocity = new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f) * 14f;
        }

        Projectile.localAI[0] = launchVelocity.X;
        Projectile.localAI[1] = launchVelocity.Y;
        if (DelayTicks > 0f)
            Projectile.velocity = Vector2.Zero;
    }

    private void SpawnChargeDust() {
        if (Main.dedServ || !Main.rand.NextBool(3))
            return;

        Vector2 direction = StoredVelocity.SafeNormalize(Vector2.UnitX);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 offset = normal * Main.rand.NextFloatDirection() * Main.rand.NextFloat(4f, 10f);
        Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GemDiamond,
            Main.rand.NextVector2Circular(0.2f, 0.2f), 120, new Color(220, 240, 255), 1f);
        dust.noGravity = true;
    }

    private void SpawnWaveDust() {
        float progress = 1f - Projectile.timeLeft / (float)MaxLifetime;
        float growthProgress = (float)Math.Sqrt(progress);
        float fade = Utils.GetLerpValue(0f, 0.12f, progress, true) *
            Utils.GetLerpValue(0f, 0.24f, Projectile.timeLeft / (float)MaxLifetime, true);
        float radius = MathHelper.Lerp(14f, 34f, growthProgress);
        float arcHalfWidth = MathHelper.Lerp(0.58f, 0.9f, growthProgress);
        int segments = 11;

        for (int i = 0; i < segments; i++) {
            float completion = i / (float)(segments - 1);
            float angle = Projectile.rotation + MathHelper.Lerp(-arcHalfWidth, arcHalfWidth, completion);
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 tangentVelocity = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.18f +
                                      Projectile.velocity * 0.02f;

            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GemDiamond, tangentVelocity, 160,
                new Color(220, 245, 255) * fade, 0.96f + progress * 0.2f);
            dust.noGravity = true;
            dust.fadeIn = 0.55f;
            dust.scale *= 0.92f;
            dust.velocity *= 0.7f;
            dust.alpha = 185;

            if (i % 2 == 0) {
                Dust innerDust = Dust.NewDustPerfect(Projectile.Center + offset * 0.92f, DustID.Smoke,
                    tangentVelocity * 0.35f, 175, new Color(255, 255, 245) * fade, 0.72f + progress * 0.12f);
                innerDust.noGravity = true;
                innerDust.velocity *= 0.45f;
                innerDust.alpha = 205;
            }
        }
    }
}
