using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.BigChill;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BigChillPhaseStrikeProjectile : ModProjectile {
    public const float DashSpeed = 24f;
    public const int MinDashFrames = 6;
    public const int MaxDashFrames = 12;

    private int trailTimer;
    private bool AbsoluteZero => Projectile.ai[0] >= 0.5f;
    private bool UltimateForm =>
        Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && BigChillTransformation.IsUltimateBigChill(Main.player[Projectile.owner]);

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
        Projectile.ownerHitCheck = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead ||
            !BigChillStatePlayer.IsBigChillTransformationId(owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId)) {
            Projectile.Kill();
            return;
        }

        owner.GetModPlayer<OmnitrixPlayer>().RegisterActiveLunge();
        owner.GetModPlayer<BigChillStatePlayer>().StartPhaseDrift();

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.velocity = direction * (AbsoluteZero ? DashSpeed + 3f : UltimateForm ? DashSpeed + 1.5f : DashSpeed);
        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, AbsoluteZero ? 14 : 12);
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;

        Projectile.Center = owner.Center + direction * 18f;
        Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;

        trailTimer++;
        if (trailTimer >= 3) {
            trailTimer = 0;
            BigChillTransformation.SpawnPhaseTrailSegment(owner, Projectile.Center - direction * 10f, AbsoluteZero);
        }

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                AbsoluteZero ? DustID.IceTorch : DustID.Frost,
                -Projectile.velocity * 0.08f, 120, new Color(180, 240, 255), AbsoluteZero ? 1.18f : 1.08f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        BigChillTransformation.ResolvePhaseDriftHit(Projectile, target, damageDone);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        owner.noKnockback = false;
        owner.velocity *= 0.42f;

        if (UltimateForm && owner.whoAmI == Main.myPlayer)
            BigChillTransformation.TriggerSpectralPhasePulse(owner, AbsoluteZero);

        if (Main.dedServ)
            return;

        for (int i = 0; i < 20; i++) {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(14f, 18f),
                AbsoluteZero ? DustID.IceTorch : DustID.Frost,
                Main.rand.NextVector2Circular(2.4f, 2.4f), 110, new Color(180, 240, 255), AbsoluteZero ? 1.2f : 1.1f);
            dust.noGravity = true;
        }
    }
}
