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
    private const int PotisStrikeLifetime = 8;
    private const float BaseReach = 154f;
    private const float OverdriveReach = 256f;
    private const float PotisBaseReach = 210f;
    private const float PotisOverdriveReach = 305f;
    private const float BaseCollisionWidth = 14f;
    private const float OverdriveCollisionWidth = 18f;
    private const float PotisBaseCollisionWidth = 20f;
    private const float PotisOverdriveCollisionWidth = 24f;
    private const float FistForwardOffset = 12f;
    private const float FistVerticalOffset = -4f;

    private int StrikeMode => (int)Math.Round(Projectile.ai[0]);
    private bool PotisInfused => StrikeMode >= 2;
    private bool Empowered => StrikeMode == 1 || StrikeMode == 3;
    private int StrikeSerial => (int)Math.Round(Projectile.ai[1]);
    private int ActivationDelay => Math.Max(0, (int)Math.Round(Projectile.ai[2]));
    private int LifetimeFrames => PotisInfused ? PotisStrikeLifetime : StrikeLifetime;
    private float CurrentReach => PotisInfused
        ? Empowered ? PotisOverdriveReach : PotisBaseReach
        : Empowered ? OverdriveReach : BaseReach;
    private float CurrentCollisionWidth => PotisInfused
        ? Empowered ? PotisOverdriveCollisionWidth : PotisBaseCollisionWidth
        : Empowered ? OverdriveCollisionWidth : BaseCollisionWidth;
    private float CurrentScale => PotisInfused
        ? Empowered ? 1.34f : 1.18f
        : Empowered ? 1.28f : 1.12f;
    private float StrikeSide => (StrikeSerial & 1) == 0 ? -1f : 1f;
    private Color StrikeColor {
        get {
            if (PotisInfused) {
                return (StrikeSerial & 1) == 0
                    ? (Empowered ? new Color(80, 255, 238) : new Color(44, 218, 255))
                    : (Empowered ? new Color(255, 246, 150) : new Color(168, 242, 255));
            }

            return (StrikeSerial & 1) == 0
                ? (Empowered ? new Color(18, 34, 84) : new Color(10, 18, 42))
                : (Empowered ? new Color(90, 190, 255) : new Color(28, 108, 255));
        }
    }

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
            Projectile.timeLeft = LifetimeFrames + ActivationDelay;
            Projectile.localNPCHitCooldown = LifetimeFrames;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        if (direction.X != 0f)
            owner.direction = direction.X > 0f ? 1 : -1;

        if (Projectile.localAI[1] > 0f) {
            Projectile.localAI[1]--;
            Projectile.Center = GetStrikeOrigin(owner, direction, CurrentScale);
            return;
        }

        int lifetimeFrames = LifetimeFrames;
        float progress = 1f - Projectile.timeLeft / (float)lifetimeFrames;
        float reachProgress = EaseOutCubic(progress);
        float laneProgress = 1f - progress;
        float scale = CurrentScale;
        Vector2 strikeOrigin = GetStrikeOrigin(owner, direction, scale);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 laneOffset = normal * StrikeSide * MathHelper.Lerp(7f, 1.5f, reachProgress) * laneProgress * scale;
        float reach = MathHelper.Lerp(14f, CurrentReach, reachProgress) * scale;

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
        Lighting.AddLight(Projectile.Center, strikeColor.ToVector3() * (PotisInfused ? 0.0048f : 0.0026f));

        if (PotisInfused && Main.rand.NextBool(Empowered ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                Main.rand.NextBool(4) ? DustID.WhiteTorch : DustID.BlueCrystalShard,
                -direction * Main.rand.NextFloat(0.8f, Empowered ? 3.2f : 2.4f), 120,
                strikeColor, Main.rand.NextFloat(0.9f, Empowered ? 1.35f : 1.18f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || Projectile.localAI[1] > 0f)
            return false;

        Texture2D slashTexture = TextureAssets.Projectile[ProjectileID.PiercingStarlight].Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 strikeOrigin = GetStrikeOrigin(owner, direction, Projectile.scale);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        int lifetimeFrames = LifetimeFrames;
        float progress = 1f - Projectile.timeLeft / (float)lifetimeFrames;
        float reachProgress = EaseOutCubic(progress);
        Vector2 laneOffset = normal * StrikeSide * MathHelper.Lerp(7f, 1.5f, reachProgress) * (1f - progress) * Projectile.scale;
        Vector2 worldStart = strikeOrigin + laneOffset;
        Vector2 worldEnd = Projectile.Center;
        float opacity = Utils.GetLerpValue(0f, 0.16f, progress, true) *
                        Utils.GetLerpValue(0f, 0.38f, Projectile.timeLeft / (float)lifetimeFrames, true);
        float beamWidth = (PotisInfused ? Empowered ? 20f : 17f : Empowered ? 16f : 13.5f) * Projectile.scale;
        Color strikeColor = StrikeColor;

        DrawBeam(slashTexture, worldStart, worldEnd, beamWidth, strikeColor, opacity);

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float previousProgress = MathHelper.Clamp(progress - (i + 1f) / lifetimeFrames, 0f, 1f);
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
        float progress = 1f - Projectile.timeLeft / (float)LifetimeFrames;
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
            CurrentCollisionWidth * Projectile.scale,
            ref collisionPoint
        );
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (!PotisInfused || Main.dedServ)
            return;

        Color strikeColor = StrikeColor;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        for (int i = 0; i < (Empowered ? 14 : 10); i++) {
            Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                i % 4 == 0 ? DustID.WhiteTorch : DustID.BlueCrystalShard,
                direction.RotatedByRandom(0.7f) * Main.rand.NextFloat(1.2f, Empowered ? 5.2f : 3.8f),
                95, strikeColor, Main.rand.NextFloat(1f, Empowered ? 1.45f : 1.25f));
            dust.noGravity = true;
        }
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
