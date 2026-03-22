using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArmodrilloUltimateSlamProjectile : ModProjectile {
    private const int LaunchFrames = 28;
    private const float LaunchVelocity = -17f;
    private const float LaunchEndVelocity = -5.5f;
    private const float SlamStartVelocity = 16f;
    private const float SlamVelocity = 34f;
    private const float SlamAcceleration = 2.7f;
    private const float HorizontalDamp = 0.82f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 80;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.timeLeft = 90;
    }

    public override bool? CanDamage() => false;

    public override bool PreDraw(ref Color lightColor) => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;

        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.noFallDmg = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, 8);

        switch ((int)Projectile.ai[0]) {
            case 0:
                UpdateLaunch(owner);
                break;
            default:
                UpdateSlam(owner);
                break;
        }
    }

    private void UpdateLaunch(Player owner) {
        Projectile.localAI[0]++;
        float launchProgress = Projectile.localAI[0] / LaunchFrames;

        if (Projectile.localAI[0] == 1f) {
            owner.velocity.X *= 0.4f;
            owner.velocity.Y = LaunchVelocity;
            SoundEngine.PlaySound(SoundID.Item74, owner.Center);
            EmitLaunchDust(owner);
        }

        owner.velocity.X *= 0.94f;
        owner.velocity.Y = Math.Min(owner.velocity.Y, MathHelper.Lerp(LaunchVelocity, LaunchEndVelocity, launchProgress));

        if (Projectile.localAI[0] >= LaunchFrames) {
            Projectile.ai[0] = 1f;
            Projectile.localAI[0] = 0f;
            Projectile.netUpdate = true;
        }
    }

    private void UpdateSlam(Player owner) {
        Projectile.localAI[0]++;
        owner.velocity.X *= HorizontalDamp;
        owner.velocity.Y = MathHelper.Clamp(owner.velocity.Y + SlamAcceleration, SlamStartVelocity, SlamVelocity);

        if (Main.rand.NextBool()) {
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(14f, 10f),
                DustID.Smoke, new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(0.8f, 2.8f)),
                110, new Color(200, 190, 180), Main.rand.NextFloat(1.05f, 1.35f));
            dust.noGravity = true;
        }

        if (!HasImpactedGround(owner))
            return;

        owner.velocity.Y = 0f;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.immuneTime = Math.Max(owner.immuneTime, 16);
        SoundEngine.PlaySound(SoundID.Item14, owner.Center);
        EmitImpactDust(owner);
        SpawnShockwaves(owner);
        Projectile.Kill();
    }

    private static bool HasImpactedGround(Player owner) {
        return owner.velocity.Y >= 0f &&
               Collision.SolidCollision(owner.position + new Vector2(0f, owner.height - 2f), owner.width, 8);
    }

    private void SpawnShockwaves(Player owner) {
        Vector2 quakeOrigin = owner.Bottom + new Vector2(0f, -8f);
        var source = Projectile.GetSource_FromThis();

        Projectile.NewProjectile(source, quakeOrigin + new Vector2(10f, 0f), Vector2.Zero,
            ModContent.ProjectileType<ArmodrilloQuakeProjectile>(), Projectile.damage, Projectile.knockBack,
            owner.whoAmI, 1f);
        Projectile.NewProjectile(source, quakeOrigin + new Vector2(-10f, 0f), Vector2.Zero,
            ModContent.ProjectileType<ArmodrilloQuakeProjectile>(), Projectile.damage, Projectile.knockBack,
            owner.whoAmI, -1f);
    }

    private static void EmitLaunchDust(Player owner) {
        for (int i = 0; i < 18; i++) {
            Vector2 dustVelocity = new Vector2(Main.rand.NextFloat(-2.6f, 2.6f), Main.rand.NextFloat(0.6f, 3.2f));
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(20f, 8f),
                i % 3 == 0 ? DustID.GemDiamond : DustID.Smoke, dustVelocity, 115, Color.White, Main.rand.NextFloat(1.05f, 1.45f));
            dust.noGravity = true;
        }
    }

    private static void EmitImpactDust(Player owner) {
        for (int i = 0; i < 28; i++) {
            Vector2 dustVelocity = new Vector2(Main.rand.NextFloat(-5.6f, 5.6f), Main.rand.NextFloat(-3.2f, 0.2f));
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(24f, 10f),
                i % 4 == 0 ? DustID.GemDiamond : DustID.Smoke, dustVelocity, 115, Color.White, Main.rand.NextFloat(1.15f, 1.7f));
            dust.noGravity = true;
        }
    }
}
