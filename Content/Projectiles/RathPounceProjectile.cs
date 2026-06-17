using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class RathPounceProjectile : ModProjectile {
    public const int MinPounceFrames = 8;
    public const int MaxPounceFrames = 18;

    private const float BaseDashSpeed = 21f;
    private const float RageDashSpeed = 24f;

    protected virtual int DashWidth => RageVariant ? 70 : 62;
    protected virtual int DashHeight => RageVariant ? 46 : 40;
    protected virtual int DashLifetime => MaxPounceFrames;
    protected virtual int HitDebuffType => BuffID.Bleeding;
    protected virtual int HitDebuffDuration => RageVariant ? 260 : 190;
    protected virtual float DashSpeed => GetDashSpeed(RageVariant);
    protected virtual float DashLift => RageVariant ? -0.75f : -1.05f;
    protected virtual float ForwardOffset => RageVariant ? 38f : 34f;
    protected virtual Color OuterColor => RageVariant ? new Color(236, 84, 56, 230) : new Color(210, 120, 70, 220);
    protected virtual Color InnerColor => RageVariant ? new Color(255, 238, 174, 195) : new Color(255, 220, 165, 180);
    protected virtual int TrailDustType => DustID.Smoke;
    protected virtual Color TrailDustColor => RageVariant ? new Color(255, 122, 82) : new Color(255, 170, 100);
    protected virtual int PrimaryImpactDustType => DustID.Blood;
    protected virtual int SecondaryImpactDustType => DustID.Smoke;
    protected virtual Color ImpactDustColor => RageVariant ? new Color(255, 152, 100) : new Color(255, 185, 120);
    protected virtual bool UsesRathPounceImpact => true;
    protected virtual bool UsesRathPreyGuidance => UsesRathPounceImpact;
    protected virtual float PreyGuidanceStrength => RageVariant ? 0.38f : 0.28f;

    private bool RageVariant => Projectile.ai[0] >= 0.5f;
    private int PreyTargetIndex => (int)Math.Round(Projectile.ai[1]) - 1;

    public static float GetDashSpeed(bool raging) => raging ? RageDashSpeed : BaseDashSpeed;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = DashWidth;
        Projectile.height = DashHeight;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = DashLifetime;
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

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        NPC preyTarget = ResolvePreyTarget(owner);
        if (preyTarget != null) {
            Vector2 preyDirection = owner.DirectionTo(preyTarget.Center);
            if (preyDirection != Vector2.Zero)
                direction = Vector2.Lerp(direction, preyDirection, PreyGuidanceStrength).SafeNormalize(direction);
        }

        float dashSpeed = DashSpeed;
        Projectile.velocity = direction * dashSpeed;
        Projectile.rotation = direction.ToRotation();
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, RageVariant ? 12 : 9);
        owner.noKnockback = true;
        owner.noFallDmg = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.velocity = direction * dashSpeed + new Vector2(0f, DashLift);

        Projectile.Center = owner.Center + direction * ForwardOffset;
        UpdateDashHitbox();

        if (Main.rand.NextBool(RageVariant ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                TrailDustType, -direction * Main.rand.NextFloat(1f, RageVariant ? 3.2f : 2.2f),
                120, TrailDustColor, Main.rand.NextFloat(1f, RageVariant ? 1.32f : 1.14f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float xScale = RageVariant ? 30f : 24f;
        float yScale = RageVariant ? 21f : 18f;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), OuterColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(xScale, yScale), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), InnerColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(11f, 8f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (UsesRathPounceImpact && (target.HasBuff(BuffID.Bleeding) || IsPreyTarget(target)))
            Projectile.localAI[1] = 1f;

        if (HitDebuffType > 0 && HitDebuffDuration > 0)
            target.AddBuff(HitDebuffType, HitDebuffDuration);

        Vector2 dashDirection = Projectile.rotation.ToRotationVector2();
        if (UsesRathPounceImpact) {
            target.velocity = new Vector2(
                MathHelper.Clamp(target.velocity.X + dashDirection.X * (RageVariant ? 6.5f : 5f), -14f, 14f),
                MathHelper.Clamp(target.velocity.Y - (RageVariant ? 1.4f : 0.8f), -9f, 10f));
            target.netUpdate = true;
        }

        int dustCount = Projectile.localAI[1] > 0f || RageVariant ? 24 : 18;
        for (int i = 0; i < dustCount; i++) {
            Vector2 burstVelocity = dashDirection.RotatedByRandom(0.58f) * Main.rand.NextFloat(1.4f, RageVariant ? 5f : 4f);
            int dustType = i % 3 == 0 ? SecondaryImpactDustType : PrimaryImpactDustType;
            Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, burstVelocity, 90, ImpactDustColor,
                Main.rand.NextFloat(1.05f, RageVariant ? 1.48f : 1.26f));
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        if (UsesRathPounceImpact)
            owner.velocity *= RageVariant ? 0.36f : 0.44f;
        owner.noKnockback = false;
    }

    private void UpdateDashHitbox() {
        int width = DashWidth;
        int height = DashHeight;
        if (Projectile.width == width && Projectile.height == height)
            return;

        Vector2 center = Projectile.Center;
        Projectile.width = width;
        Projectile.height = height;
        Projectile.Center = center;
    }

    private NPC ResolvePreyTarget(Player owner) {
        if (!UsesRathPreyGuidance || PreyTargetIndex < 0 || PreyTargetIndex >= Main.maxNPCs)
            return null;

        NPC target = Main.npc[PreyTargetIndex];
        if (!target.CanBeChasedBy())
            return null;

        if (Vector2.DistanceSquared(target.Center, owner.Center) > 640f * 640f)
            return null;

        return target;
    }

    private bool IsPreyTarget(NPC target) {
        return UsesRathPreyGuidance && PreyTargetIndex == target.whoAmI;
    }
}
