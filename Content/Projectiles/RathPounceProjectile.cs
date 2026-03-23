using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class RathPounceProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    protected virtual int DashWidth => 60;
    protected virtual int DashHeight => 40;
    protected virtual int DashLifetime => 16;
    protected virtual int HitDebuffType => BuffID.Bleeding;
    protected virtual int HitDebuffDuration => 180;
    protected virtual float DashSpeed => 13f;
    protected virtual float DashLift => -1.2f;
    protected virtual float ForwardOffset => 34f;
    protected virtual Color OuterColor => new(210, 120, 70, 220);
    protected virtual Color InnerColor => new(255, 220, 165, 180);
    protected virtual int TrailDustType => DustID.Smoke;
    protected virtual Color TrailDustColor => new(255, 170, 100);
    protected virtual int PrimaryImpactDustType => DustID.Blood;
    protected virtual int SecondaryImpactDustType => DustID.Smoke;
    protected virtual Color ImpactDustColor => new(255, 185, 120);

    public override void SetDefaults() {
        Projectile.width = DashWidth;
        Projectile.height = DashHeight;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = -1;
        Projectile.timeLeft = DashLifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.velocity = direction;
        Projectile.rotation = direction.ToRotation();
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = 6;
        owner.noKnockback = true;
        owner.velocity = direction * DashSpeed + new Vector2(0f, DashLift);

        Vector2 desiredCenter = owner.Center + direction * ForwardOffset;
        Projectile.Center = desiredCenter;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, TrailDustType, -Projectile.velocity * 0.2f,
                120, TrailDustColor, 1.1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), OuterColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(24f, 18f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), InnerColor,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(10f, 8f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (HitDebuffType > 0 && HitDebuffDuration > 0)
            target.AddBuff(HitDebuffType, HitDebuffDuration);

        for (int i = 0; i < 18; i++) {
            Vector2 burstVelocity = Projectile.rotation.ToRotationVector2().RotatedByRandom(0.55f) * Main.rand.NextFloat(1.2f, 4f);
            int dustType = i % 3 == 0 ? SecondaryImpactDustType : PrimaryImpactDustType;
            Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, burstVelocity, 90, ImpactDustColor, 1.25f);
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        owner.noKnockback = false;
    }
}
