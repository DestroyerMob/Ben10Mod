using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastPotisLanceProjectile : ModProjectile {
    private const int ImpactEffectSnowflakeFlag = 1;
    private const int ImpactEffectEmpoweredFlag = 2;

    private bool Empowered => Projectile.ai[0] >= 0.5f;
    private bool Snowflake => Projectile.ai[1] >= 0.5f;
    private float SpawnTime {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private bool HasHandledFinalImpact {
        get => Projectile.localAI[1] >= 0.5f;
        set => Projectile.localAI[1] = value ? 1f : 0f;
    }

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = Empowered ? 3 : 2;
        Projectile.timeLeft = 96;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnSpawn(IEntitySource source) {
        SoundEngine.PlaySound(SoundID.Item20 with {
            Pitch = Snowflake ? 0.08f : -0.22f,
            Volume = Empowered ? 0.72f : 0.6f,
            MaxInstances = 12
        }, Projectile.Center);
    }

    public override void AI() {
        SpawnTime++;
        Projectile.penetrate = Empowered ? 3 : 2;
        if (Projectile.velocity.LengthSquared() < (Empowered ? 576f : 484f))
            Projectile.velocity *= Empowered ? 1.018f : 1.014f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.scale = Empowered ? 1.14f : 1f;
        Projectile.Opacity = Utils.GetLerpValue(0f, 5f, SpawnTime, true);

        Vector3 lightColor = Snowflake ? new Vector3(0.34f, 0.72f, 1.05f) : new Vector3(1.18f, 0.52f, 0.08f);
        Lighting.AddLight(Projectile.Center, lightColor);

        if (Main.rand.NextBool(2)) {
            int dustType = Snowflake ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce) :
                (Main.rand.NextBool(3) ? DustID.InfernoFork : DustID.Flare);
            Color dustColor = Snowflake ? new Color(180, 235, 255) : new Color(255, 178, 88);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), dustType,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.11f), 95, dustColor,
                Main.rand.NextFloat(0.95f, Empowered ? 1.3f : 1.12f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Texture2D streakTexture = TextureAssets.Projectile[ProjectileID.PiercingStarlight].Value;
        int flareTextureType = Snowflake ? ProjectileID.BallofFrost : ProjectileID.ImpFireball;
        Texture2D flareTexture = TextureAssets.Projectile[flareTextureType].Value;
        int flareFrameCount = Main.projFrames[flareTextureType] > 0 ? Main.projFrames[flareTextureType] : 1;
        Rectangle flareFrame = flareTexture.Frame(1, flareFrameCount, 0,
            (int)(Main.GameUpdateCount / 3 + Projectile.identity) % flareFrameCount);
        Vector2 flareOrigin = flareFrame.Size() * 0.5f;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        float rotation = direction.ToRotation();
        Vector2 sideDirection = direction.RotatedBy(MathHelper.PiOver2);
        float launchPulse = Utils.GetLerpValue(10f, 0f, SpawnTime, true);
        float opacity = Projectile.Opacity;
        Color outerColor = Snowflake ? new Color(85, 210, 255, 110) : new Color(255, 110, 26, 110);
        Color midColor = Snowflake ? new Color(165, 234, 255, 180) : new Color(255, 176, 84, 180);
        Color innerColor = Snowflake ? new Color(245, 252, 255, 235) : new Color(255, 242, 198, 235);

        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 rearCenter = center - direction * 9f;
        DrawTexturedTrail(streakTexture, direction, outerColor, midColor, innerColor, opacity);
        DrawRing(pixel, rearCenter, MathHelper.Lerp(10f, 28f, launchPulse) * Projectile.scale, 4.2f * Projectile.scale,
            outerColor * (launchPulse * 0.42f) * opacity, SpawnTime * 0.2f);
        DrawPixel(pixel, rearCenter, rotation, new Vector2(28f, 12f) * Projectile.scale,
            outerColor * (0.28f + 0.3f * launchPulse) * opacity);

        Vector2 worldTip = Projectile.Center + direction * 8f;
        Vector2 worldMid = Projectile.Center - direction * 7f;
        Vector2 worldRear = Projectile.Center - direction * 21f;
        DrawBeam(streakTexture, worldRear, worldTip, 24f * Projectile.scale, outerColor, opacity * 0.62f);
        DrawBeam(streakTexture, worldMid, worldTip, 16f * Projectile.scale, midColor, opacity * 0.92f);
        DrawBeam(streakTexture, Projectile.Center - direction * 2f, worldTip, 9f * Projectile.scale, innerColor, opacity);

        Vector2 wingStart = Projectile.Center - direction * 7f;
        DrawBeam(streakTexture, wingStart + sideDirection * 3f, wingStart - direction * 11f + sideDirection * 11f,
            8.2f * Projectile.scale, outerColor, opacity * 0.52f);
        DrawBeam(streakTexture, wingStart - sideDirection * 3f, wingStart - direction * 11f - sideDirection * 11f,
            8.2f * Projectile.scale, outerColor, opacity * 0.52f);
        DrawBeam(streakTexture, wingStart + sideDirection * 2f, wingStart - direction * 8f + sideDirection * 7f,
            4.8f * Projectile.scale, innerColor, opacity * 0.75f);
        DrawBeam(streakTexture, wingStart - sideDirection * 2f, wingStart - direction * 8f - sideDirection * 7f,
            4.8f * Projectile.scale, innerColor, opacity * 0.75f);

        Main.EntitySpriteDraw(flareTexture, center - direction * 2f, flareFrame, outerColor * (opacity * 0.95f),
            rotation + MathHelper.PiOver2, flareOrigin, Projectile.scale * 1.28f, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(flareTexture, center + direction * 4f, flareFrame, innerColor * opacity,
            rotation + MathHelper.PiOver2, flareOrigin, Projectile.scale * 0.92f, SpriteEffects.None, 0);

        DrawPixel(pixel, center + direction * 6f, rotation + 0.22f, new Vector2(18f, 3.2f) * Projectile.scale,
            innerColor * (opacity * 0.8f));
        DrawPixel(pixel, center + direction * 6f, rotation - 0.22f, new Vector2(18f, 3.2f) * Projectile.scale,
            innerColor * (opacity * 0.8f));
        DrawPixel(pixel, center + direction * 9f, rotation, new Vector2(12f, 2.6f) * Projectile.scale,
            Color.White * (opacity * 0.95f));
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(Snowflake ? BuffID.Frostburn2 : BuffID.OnFire3, Empowered ? 300 : 210);
        Vector2 impactDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 impactPosition = target.Center - impactDirection * (target.width * 0.18f);
        SpawnImpactEffect(impactPosition, impactDirection, false);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Vector2 impactDirection = oldVelocity.SafeNormalize(Vector2.UnitX);
        SpawnImpactEffect(Projectile.Center, impactDirection, true);
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (!HasHandledFinalImpact && timeLeft > 0)
            SpawnImpactEffect(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), true);

        if (Main.dedServ)
            return;

        int primaryDust = Snowflake ? DustID.IceTorch : DustID.InfernoFork;
        int secondaryDust = Snowflake ? DustID.SnowflakeIce : DustID.Flare;
        Color dustColor = Snowflake ? new Color(185, 235, 255) : new Color(255, 182, 92);

        for (int i = 0; i < 12; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? secondaryDust : primaryDust,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 95, dustColor, Main.rand.NextFloat(0.95f, 1.25f));
            dust.noGravity = true;
        }
    }

    private void SpawnImpactEffect(Vector2 position, Vector2 direction, bool markFinalImpact) {
        if (markFinalImpact)
            HasHandledFinalImpact = true;

        if (Projectile.owner == Main.myPlayer) {
            int flags = (Snowflake ? ImpactEffectSnowflakeFlag : 0) | (Empowered ? ImpactEffectEmpoweredFlag : 0);
            int projectileIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<HeatBlastPotisLanceImpactProjectile>(), 0, 0f, Projectile.owner,
                direction.ToRotation(), flags);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Main.projectile[projectileIndex].scale = Projectile.scale;
                Main.projectile[projectileIndex].netUpdate = true;
            }
        }

        SoundEngine.PlaySound(SoundID.Item74 with {
            Pitch = Snowflake ? 0.15f : -0.28f,
            Volume = Empowered ? 0.7f : 0.6f,
            MaxInstances = 16
        }, position);
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotation) {
        const int Segments = 14;
        for (int i = 0; i < Segments; i++) {
            float angle = rotation + MathHelper.TwoPi * i / Segments;
            Vector2 drawPosition = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, drawPosition, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.3f), SpriteEffects.None, 0);
        }
    }

    private static void DrawPixel(Texture2D pixel, Vector2 position, float rotation, Vector2 scale, Color color) {
        Main.EntitySpriteDraw(pixel, position, null, color, rotation, Vector2.One * 0.5f, scale, SpriteEffects.None, 0);
    }

    private void DrawTexturedTrail(Texture2D streakTexture, Vector2 direction, Color outerColor, Color midColor,
        Color innerColor, float opacity) {
        Vector2 previousCenter = Projectile.Center;

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            Vector2 oldPosition = Projectile.oldPos[i];
            if (oldPosition == Vector2.Zero)
                continue;

            Vector2 currentCenter = oldPosition + Projectile.Size * 0.5f;
            float progress = i / (float)Projectile.oldPos.Length;
            float segmentOpacity = (1f - progress) * (Empowered ? 1f : 0.88f) * opacity;
            float outerWidth = MathHelper.Lerp(18f, 6f, progress) * Projectile.scale;
            float innerWidth = outerWidth * 0.62f;

            DrawBeam(streakTexture, previousCenter, currentCenter, outerWidth, outerColor, segmentOpacity * 0.4f);
            DrawBeam(streakTexture, previousCenter, currentCenter, innerWidth, midColor, segmentOpacity * 0.62f);
            DrawBeam(streakTexture, previousCenter, currentCenter, innerWidth * 0.5f, innerColor, segmentOpacity * 0.82f);

            Vector2 nodeCenter = currentCenter - direction * MathHelper.Lerp(0f, 8f, progress);
            Vector2 nodeScreenPosition = nodeCenter - Main.screenPosition;
            float nodeRotation = Projectile.rotation + progress * 0.32f;
            DrawPixel(TextureAssets.MagicPixel.Value, nodeScreenPosition, nodeRotation,
                new Vector2(MathHelper.Lerp(13f, 4f, progress), MathHelper.Lerp(8f, 2.6f, progress)) * Projectile.scale,
                outerColor * (segmentOpacity * 0.34f));
            DrawPixel(TextureAssets.MagicPixel.Value, nodeScreenPosition, nodeRotation,
                new Vector2(MathHelper.Lerp(7f, 2.4f, progress), MathHelper.Lerp(4f, 1.5f, progress)) * Projectile.scale,
                innerColor * (segmentOpacity * 0.45f));

            previousCenter = currentCenter;
        }
    }

    private static void DrawBeam(Texture2D texture, Vector2 worldStart, Vector2 worldEnd, float beamWidth, Color color,
        float opacity) {
        Vector2 beamVector = worldEnd - worldStart;
        float beamLength = beamVector.Length();
        if (beamLength <= 1f)
            return;

        Vector2 center = worldStart + beamVector * 0.5f - Main.screenPosition;
        float rotation = beamVector.ToRotation();
        Vector2 origin = texture.Size() * 0.5f;
        float lengthScale = beamLength / texture.Width;
        float widthScale = beamWidth / texture.Height * 1.7f;

        Main.EntitySpriteDraw(texture, center, null, color * opacity, rotation, origin, new Vector2(lengthScale, widthScale),
            SpriteEffects.None, 0);
    }
}
