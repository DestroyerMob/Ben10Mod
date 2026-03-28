using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FasttrackPursuitRushProjectile : ModProjectile {
    public const int MinRushFrames = 7;
    public const int MaxRushFrames = 22;
    private const float BaseRushSpeed = 27f;
    private const float SurgeRushSpeed = 33f;

    private float MomentumRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private bool Overdrive => Projectile.ai[1] >= 0.5f;

    public static float GetRushSpeed(bool empowered) => empowered ? SurgeRushSpeed : BaseRushSpeed;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 48;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinRushFrames;
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
        float rushSpeed = MathHelper.Lerp(BaseRushSpeed, Overdrive ? SurgeRushSpeed + 2f : SurgeRushSpeed, MomentumRatio);
        Projectile.velocity = direction * rushSpeed;
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = owner.Center + direction * MathHelper.Lerp(19f, 24f, MomentumRatio);

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, Overdrive ? 16 : 12);
        owner.noKnockback = true;
        owner.noFallDmg = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;

        if (Main.rand.NextBool(Overdrive ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                Main.rand.NextBool() ? DustID.GreenFairy : DustID.GemEmerald,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.14f), 105, new Color(150, 255, 220),
                Main.rand.NextFloat(0.9f, Overdrive ? 1.24f : 1.08f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.BrokenArmor, Overdrive || MomentumRatio >= 0.7f ? 180 : 120);
        target.netUpdate = true;
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.velocity *= 0.5f;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 16; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GemEmerald : DustID.GreenFairy,
                Main.rand.NextVector2Circular(3.4f, 3.4f), 105, new Color(150, 255, 220), Main.rand.NextFloat(0.92f, 1.18f));
            dust.noGravity = true;
        }

        if (Projectile.owner != Main.myPlayer)
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        int shockDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.6f));
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + direction * 14f, direction * 14f,
            ModContent.ProjectileType<FasttrackClawWaveProjectile>(), shockDamage, Projectile.knockBack, Projectile.owner,
            MomentumRatio, Overdrive ? 1f : 0f);
    }
}
