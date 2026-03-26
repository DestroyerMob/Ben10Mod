using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class CannonboltRollProjectile : ModProjectile {
    private const int VariantPrimary = 0;
    private const int VariantSecondary = 1;
    private const int VariantUltimate = 2;

    private int Variant => (int)Math.Round(Projectile.ai[0]);
    private bool Empowered => Projectile.ai[1] > 0f;

    private int BaseWidth => Variant switch {
        VariantSecondary => 68,
        VariantUltimate => 84,
        _ => 56
    };

    private int BaseHeight => Variant switch {
        VariantSecondary => 54,
        VariantUltimate => 66,
        _ => 44
    };

    private int DashLifetime => Variant switch {
        VariantSecondary => 22,
        VariantUltimate => 30,
        _ => 16
    };

    private float DashSpeed => (Variant switch {
        VariantSecondary => 18f,
        VariantUltimate => 22f,
        _ => 14.5f
    }) * (Empowered ? 1.12f : 1f);

    private float DashLift => Variant switch {
        VariantSecondary => -0.15f,
        VariantUltimate => 0f,
        _ => -0.35f
    };

    private float ForwardOffset => Variant switch {
        VariantSecondary => 34f,
        VariantUltimate => 38f,
        _ => 28f
    };

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 56;
        Projectile.height = 44;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = 36;
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

        owner.GetModPlayer<OmnitrixPlayer>().RegisterActiveLunge();

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.timeLeft = DashLifetime;
            SpawnLaunchDust();
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.velocity = direction;

        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = 6;
        owner.noKnockback = true;
        owner.noFallDmg = true;
        owner.armorEffectDrawShadow = true;
        owner.velocity = direction * DashSpeed + new Vector2(0f, DashLift);

        int width = (int)(BaseWidth * (Empowered ? 1.08f : 1f));
        int height = (int)(BaseHeight * (Empowered ? 1.08f : 1f));
        if (Projectile.width != width || Projectile.height != height) {
            Vector2 center = Projectile.Center;
            Projectile.width = width;
            Projectile.height = height;
            Projectile.Center = center;
        }

        Projectile.Center = owner.Center + direction * ForwardOffset;
        Projectile.rotation += direction.X * (0.22f + DashSpeed * 0.012f);

        Lighting.AddLight(Projectile.Center, Empowered ? new Vector3(0.72f, 0.68f, 0.5f) : new Vector3(0.5f, 0.46f, 0.36f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * 0.18f, Projectile.height * 0.18f),
                Main.rand.NextBool(3) ? DustID.Smoke : DustID.GemTopaz,
                -direction * Main.rand.NextFloat(0.8f, 2.2f), 120, Color.White, Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 scale = new(Projectile.width * 0.48f, Projectile.height * 0.48f);
        Color outerColor = Empowered ? new Color(240, 235, 185, 220) : new Color(200, 185, 140, 220);
        Color innerColor = Variant == VariantUltimate
            ? new Color(255, 245, 210, 205)
            : new Color(250, 235, 195, 175);

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), outerColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), innerColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), scale * 0.52f, SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.BrokenArmor, Variant == VariantUltimate ? 300 : 150);

        SpawnImpactDust();

        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead) {
            Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
            float reboundSpeed = Variant switch {
                VariantSecondary => 8f,
                VariantUltimate => 5.5f,
                _ => 4.5f
            };
            float reboundLift = Variant switch {
                VariantSecondary => -2.4f,
                VariantUltimate => -1.6f,
                _ => -0.8f
            };

            owner.velocity = -direction * reboundSpeed + new Vector2(0f, reboundLift);
            owner.immune = true;
            owner.immuneNoBlink = true;
            owner.immuneTime = Math.Max(owner.immuneTime, 12);
            owner.noKnockback = false;
            owner.armorEffectDrawShadow = true;
        }

        if (Variant == VariantUltimate)
            SpawnImpactBurst(1.3f, 0.75f);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active)
            owner.noKnockback = false;

        if (Variant == VariantUltimate)
            SpawnImpactBurst(1.1f, 0.55f);
    }

    private void SpawnLaunchDust() {
        for (int i = 0; i < 14; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.8f, 2.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.GemTopaz : DustID.Smoke,
                velocity, 110, Color.White, Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }

    private void SpawnImpactDust() {
        for (int i = 0; i < 18; i++) {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.6f, 5f);
            int dustType = i % 4 == 0 ? DustID.GemTopaz : DustID.Smoke;
            Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, velocity, 115, Color.White, Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private void SpawnImpactBurst(float scale, float damageMultiplier) {
        if (Projectile.localAI[1] > 0f)
            return;

        Projectile.localAI[1] = 1f;
        int burstDamage = Math.Max(1, (int)Math.Round(Projectile.damage * damageMultiplier));
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
            ModContent.ProjectileType<CannonboltImpactBurstProjectile>(), burstDamage, Projectile.knockBack + 2f,
            Projectile.owner, scale);
    }
}
