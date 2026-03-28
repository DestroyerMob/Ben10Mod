using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AstrodactylDiveProjectile : ModProjectile {
    public const int MinDiveFrames = 6;
    public const int MaxDiveFrames = 22;
    private const float BaseDiveSpeed = 26f;
    private const float HyperflightDiveSpeed = 33f;

    private bool Hyperflight => Projectile.ai[0] >= 0.5f;
    private float AirSupremacyRatio => MathHelper.Clamp(Projectile.ai[1], 0f, 1f);

    public static float GetDiveSpeed(bool hyperflight) => hyperflight ? HyperflightDiveSpeed : BaseDiveSpeed;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 42;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinDiveFrames;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
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

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float diveSpeed = GetDiveSpeed(Hyperflight) + AirSupremacyRatio * 3f;
        Projectile.velocity = direction * diveSpeed;
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = owner.Center + direction * MathHelper.Lerp(Hyperflight ? 22f : 18f, 28f, AirSupremacyRatio);

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, Hyperflight ? 14 : 10);
        owner.noKnockback = true;
        owner.noFallDmg = true;
        owner.armorEffectDrawShadow = true;
        owner.fallStart = (int)(owner.position.Y / 16f);

        if (Main.rand.NextBool(Hyperflight ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                Main.rand.NextBool(3) ? DustID.GreenTorch : DustID.GemEmerald,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.13f), 105, new Color(180, 255, 220),
                Main.rand.NextFloat(0.9f, MathHelper.Lerp(Hyperflight ? 1.2f : 1.08f, 1.3f, AirSupremacyRatio)));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, (int)MathHelper.Lerp(Hyperflight ? 240 : 180, 300f, AirSupremacyRatio));
        target.AddBuff(BuffID.Oiled, 90);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.velocity *= 0.58f;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.GemEmerald,
                Main.rand.NextVector2Circular(3.2f, 3.2f), 105, new Color(185, 255, 225), Main.rand.NextFloat(0.95f, 1.18f));
            dust.noGravity = true;
        }
    }
}
