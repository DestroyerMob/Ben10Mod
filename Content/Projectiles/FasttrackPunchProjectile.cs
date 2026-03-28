using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FasttrackPunchProjectile : ModProjectile {
    private const int StrikeLifetime = 10;
    private const float BaseReach = 112f;
    private const float MaxMomentumReach = 170f;
    private const float BaseCollisionWidth = 14f;
    private const float MaxMomentumCollisionWidth = 20f;
    private const float FistForwardOffset = 11f;
    private const float FistVerticalOffset = -3f;

    private float StrikeScale => Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0];
    private float MomentumRatio => MathHelper.Clamp(Projectile.ai[1], 0f, 1f);
    private bool HighMomentum => MomentumRatio >= 0.65f;
    private float StrikeSide => (Projectile.identity & 1) == 0 ? -1f : 1f;
    private Color BeamOuterColor => Color.Lerp(new Color(8, 16, 18, 230), new Color(14, 30, 28, 240), MomentumRatio);
    private Color BeamInnerColor => Color.Lerp(new Color(72, 200, 160, 205), new Color(165, 255, 225, 225), MomentumRatio);
    private Color BeamCoreColor => Color.Lerp(new Color(190, 255, 235, 125), new Color(235, 255, 248, 165), MomentumRatio);

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PiercingStarlight}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 4;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = StrikeLifetime;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = StrikeLifetime;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        if (direction.X != 0f)
            owner.direction = direction.X > 0f ? 1 : -1;

        float progress = 1f - Projectile.timeLeft / (float)StrikeLifetime;
        float reachProgress = EaseOutQuadratic(progress);
        float laneProgress = 1f - progress;
        Vector2 strikeOrigin = GetStrikeOrigin(owner, direction, StrikeScale);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 laneOffset = normal * StrikeSide * MathHelper.Lerp(6f, 1.25f, reachProgress) * laneProgress * StrikeScale;
        float reach = MathHelper.Lerp(12f, MathHelper.Lerp(BaseReach, MaxMomentumReach, MomentumRatio), reachProgress) * StrikeScale;

        Projectile.scale = StrikeScale;
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = strikeOrigin + laneOffset + direction * reach;
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = Math.Max(owner.itemTime, 2);
        owner.itemAnimation = Math.Max(owner.itemAnimation, 2);
        owner.itemRotation = direction.ToRotation() * owner.direction;
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - MathHelper.PiOver2);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.08f, Volume = 0.56f }, Projectile.Center);
        }

        Lighting.AddLight(Projectile.Center, BeamInnerColor.ToVector3() * 0.0022f);
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return false;

        Texture2D slashTexture = TextureAssets.Projectile[ProjectileID.PiercingStarlight].Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 strikeOrigin = GetStrikeOrigin(owner, direction, Projectile.scale);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float progress = 1f - Projectile.timeLeft / (float)StrikeLifetime;
        float reachProgress = EaseOutQuadratic(progress);
        Vector2 laneOffset = normal * StrikeSide * MathHelper.Lerp(6f, 1.25f, reachProgress) * (1f - progress) * Projectile.scale;
        Vector2 worldStart = strikeOrigin + laneOffset;
        Vector2 worldEnd = Projectile.Center;
        float opacity = Utils.GetLerpValue(0f, 0.14f, progress, true) *
                        Utils.GetLerpValue(0f, 0.35f, Projectile.timeLeft / (float)StrikeLifetime, true);
        float beamWidth = MathHelper.Lerp(12.5f, 17f, MomentumRatio) * Projectile.scale;

        DrawBeam(slashTexture, worldStart, worldEnd, beamWidth * 1.15f, BeamOuterColor, opacity * 0.7f);
        DrawBeam(slashTexture, worldStart, worldEnd, beamWidth, BeamInnerColor, opacity);
        DrawBeam(slashTexture, worldStart, worldEnd, beamWidth * 0.58f, BeamCoreColor, opacity * 0.9f);

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float trailProgress = 1f - i / (float)Projectile.oldPos.Length;
            Vector2 trailEnd = Projectile.oldPos[i] + Projectile.Size * 0.5f;
            DrawBeam(slashTexture, worldStart, trailEnd, beamWidth * MathHelper.Lerp(0.55f, 0.8f, trailProgress),
                BeamInnerColor, opacity * trailProgress * 0.22f);
        }

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float progress = 1f - Projectile.timeLeft / (float)StrikeLifetime;
        float reachProgress = EaseOutQuadratic(progress);
        Vector2 laneOffset = normal * StrikeSide * MathHelper.Lerp(6f, 1.25f, reachProgress) * (1f - progress) * Projectile.scale;
        Vector2 lineStart = GetStrikeOrigin(owner, direction, Projectile.scale) + laneOffset;
        Vector2 lineEnd = Projectile.Center;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            lineStart,
            lineEnd,
            MathHelper.Lerp(BaseCollisionWidth, MaxMomentumCollisionWidth, MomentumRatio) * Projectile.scale,
            ref collisionPoint
        );
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (HighMomentum)
            target.AddBuff(BuffID.BrokenArmor, 90);

        target.netUpdate = true;
    }

    private static Vector2 GetStrikeOrigin(Player owner, Vector2 direction, float scale) {
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        return owner.MountedCenter + new Vector2(owner.direction * FistForwardOffset * scale, FistVerticalOffset * scale) +
               normal * owner.direction * 1.5f * scale;
    }

    private static float EaseOutQuadratic(float value) {
        value = MathHelper.Clamp(value, 0f, 1f);
        return 1f - (1f - value) * (1f - value);
    }

    private static void DrawBeam(Texture2D slashTexture, Vector2 worldStart, Vector2 worldEnd, float beamWidth, Color color, float opacity) {
        Vector2 beamVector = worldEnd - worldStart;
        float beamLength = beamVector.Length();
        if (beamLength <= 1f)
            return;

        Vector2 center = worldStart + beamVector * 0.5f - Main.screenPosition;
        float rotation = beamVector.ToRotation();
        Vector2 origin = slashTexture.Size() * 0.5f;
        float lengthScale = beamLength / slashTexture.Width;
        float widthScale = beamWidth / slashTexture.Height * 1.7f;

        Main.EntitySpriteDraw(slashTexture, center, null, color * opacity, rotation, origin, new Vector2(lengthScale, widthScale),
            SpriteEffects.None, 0);
    }
}
