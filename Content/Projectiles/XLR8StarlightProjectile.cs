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

public class XLR8StarlightProjectile : ModProjectile {
    private const int StrikeLifetime = 7;
    private const float BaseReach = 154f;
    private const float OverdriveReach = 256f;
    private const float BaseCollisionWidth = 14f;
    private const float OverdriveCollisionWidth = 18f;
    private const float FistForwardOffset = 12f;
    private const float FistVerticalOffset = -4f;

    private bool Empowered => Projectile.ai[0] >= 0.5f;
    private int StrikeSerial => (int)Math.Round(Projectile.ai[1]);
    private int ActivationDelay => Math.Max(0, (int)Math.Round(Projectile.ai[2]));
    private float StrikeSide => (StrikeSerial & 1) == 0 ? -1f : 1f;
    private Color StrikeColor => (StrikeSerial & 1) == 0
        ? (Empowered ? new Color(18, 34, 84) : new Color(10, 18, 42))
        : (Empowered ? new Color(90, 190, 255) : new Color(28, 108, 255));

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PiercingStarlight}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
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
        Projectile.localAI[1] = -1f;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        if (Projectile.localAI[1] < 0f) {
            Projectile.localAI[1] = ActivationDelay;
            Projectile.timeLeft = StrikeLifetime + ActivationDelay;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        if (direction.X != 0f)
            owner.direction = direction.X > 0f ? 1 : -1;

        if (Projectile.localAI[1] > 0f) {
            Projectile.localAI[1]--;
            Projectile.Center = GetStrikeOrigin(owner, direction, Empowered ? 1.28f : 1.12f);
            return;
        }

        float progress = 1f - Projectile.timeLeft / (float)StrikeLifetime;
        float reachProgress = EaseOutCubic(progress);
        float laneProgress = 1f - progress;
        float scale = Empowered ? 1.28f : 1.12f;
        Vector2 strikeOrigin = GetStrikeOrigin(owner, direction, scale);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 laneOffset = normal * StrikeSide * MathHelper.Lerp(7f, 1.5f, reachProgress) * laneProgress * scale;
        float reach = MathHelper.Lerp(14f, Empowered ? OverdriveReach : BaseReach, reachProgress) * scale;

        Projectile.scale = scale;
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
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.28f, Volume = 0.62f }, Projectile.Center);
        }

        Color strikeColor = StrikeColor;
        Lighting.AddLight(Projectile.Center, strikeColor.ToVector3() * 0.0026f);
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || Projectile.localAI[1] > 0f)
            return false;

        Texture2D slashTexture = TextureAssets.Projectile[ProjectileID.PiercingStarlight].Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 strikeOrigin = GetStrikeOrigin(owner, direction, Projectile.scale);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float progress = 1f - Projectile.timeLeft / (float)StrikeLifetime;
        float reachProgress = EaseOutCubic(progress);
        Vector2 laneOffset = normal * StrikeSide * MathHelper.Lerp(7f, 1.5f, reachProgress) * (1f - progress) * Projectile.scale;
        Vector2 worldStart = strikeOrigin + laneOffset;
        Vector2 worldEnd = Projectile.Center;
        float opacity = Utils.GetLerpValue(0f, 0.16f, progress, true) *
                        Utils.GetLerpValue(0f, 0.38f, Projectile.timeLeft / (float)StrikeLifetime, true);
        float beamWidth = (Empowered ? 16f : 13.5f) * Projectile.scale;
        Color strikeColor = StrikeColor;

        DrawBeam(slashTexture, worldStart, worldEnd, beamWidth, strikeColor, opacity);

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float previousProgress = MathHelper.Clamp(progress - (i + 1f) / StrikeLifetime, 0f, 1f);
            float previousReachProgress = EaseOutCubic(previousProgress);
            float previousLaneProgress = 1f - previousProgress;
            Vector2 trailStart = strikeOrigin +
                                 normal * StrikeSide * MathHelper.Lerp(7f, 1.5f, previousReachProgress) *
                                 previousLaneProgress * Projectile.scale;
            float trailProgress = 1f - i / (float)Projectile.oldPos.Length;
            Vector2 trailEnd = Projectile.oldPos[i] + Projectile.Size * 0.5f;
            DrawBeam(slashTexture, trailStart, trailEnd, beamWidth * MathHelper.Lerp(0.65f, 0.9f, trailProgress),
                strikeColor, opacity * trailProgress * 0.3f);
        }
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || Projectile.localAI[1] > 0f)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float progress = 1f - Projectile.timeLeft / (float)StrikeLifetime;
        float reachProgress = EaseOutCubic(progress);
        Vector2 laneOffset = normal * StrikeSide * MathHelper.Lerp(7f, 1.5f, reachProgress) * (1f - progress) * Projectile.scale;
        Vector2 lineStart = GetStrikeOrigin(owner, direction, Projectile.scale) + laneOffset;
        Vector2 lineEnd = Projectile.Center;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            lineStart,
            lineEnd,
            (Empowered ? OverdriveCollisionWidth : BaseCollisionWidth) * Projectile.scale,
            ref collisionPoint
        );
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
    }

    private static Vector2 GetStrikeOrigin(Player owner, Vector2 direction, float scale) {
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        return owner.MountedCenter + new Vector2(owner.direction * FistForwardOffset * scale, FistVerticalOffset * scale) +
               normal * owner.direction * 2f * scale;
    }

    private static float EaseOutCubic(float value) {
        value = MathHelper.Clamp(value, 0f, 1f);
        float inverse = 1f - value;
        return 1f - inverse * inverse * inverse;
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
        float widthScale = beamWidth / slashTexture.Height * 1.75f;

        DrawSlashTexture(slashTexture, center, color, rotation, new Vector2(lengthScale, widthScale), origin, opacity);
    }

    private static void DrawSlashTexture(Texture2D slashTexture, Vector2 position, Color color, float rotation,
        Vector2 scale, Vector2 origin, float opacity) {
        Main.EntitySpriteDraw(slashTexture, position, null, color * opacity, rotation, origin, scale, SpriteEffects.None, 0);
    }
}
