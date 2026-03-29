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
        ProjectileID.Sets.TrailCacheLength[Type] = 9;
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
        int trailTextureType = Snowflake ? ProjectileID.BallofFrost : ProjectileID.ImpFireball;
        Texture2D trailTexture = TextureAssets.Projectile[trailTextureType].Value;
        int trailFrameCount = Main.projFrames[trailTextureType] > 0 ? Main.projFrames[trailTextureType] : 1;
        Rectangle trailFrame = trailTexture.Frame(1, trailFrameCount, 0,
            (int)(Main.GameUpdateCount / 3 + Projectile.identity) % trailFrameCount);
        Vector2 trailOrigin = trailFrame.Size() * 0.5f;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        float rotation = direction.ToRotation();
        Vector2 sideDirection = direction.RotatedBy(MathHelper.PiOver2);
        float launchPulse = Utils.GetLerpValue(10f, 0f, SpawnTime, true);
        Color outerColor = Snowflake ? new Color(105, 205, 255, 110) : new Color(255, 115, 28, 110);
        Color innerColor = Snowflake ? new Color(235, 248, 255, 220) : new Color(255, 236, 185, 220);

        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 rearCenter = center - direction * 9f;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix);

        DrawTrailRibbon(pixel, trailTexture, trailFrame, trailOrigin, outerColor, innerColor);
        DrawRing(pixel, rearCenter, MathHelper.Lerp(10f, 28f, launchPulse) * Projectile.scale, 4.2f * Projectile.scale,
            outerColor * (launchPulse * 0.55f), SpawnTime * 0.2f);
        DrawPixel(pixel, rearCenter, rotation, new Vector2(34f, 18f) * Projectile.scale, outerColor * (0.48f + 0.58f * launchPulse));
        DrawPixel(pixel, center - direction * 11f, rotation, new Vector2(56f, 14f) * Projectile.scale, outerColor);
        DrawPixel(pixel, center - direction * 5f + sideDirection * 6f, rotation + 0.52f, new Vector2(24f, 5.2f) * Projectile.scale,
            outerColor * 0.88f);
        DrawPixel(pixel, center - direction * 5f - sideDirection * 6f, rotation - 0.52f, new Vector2(24f, 5.2f) * Projectile.scale,
            outerColor * 0.88f);
        Main.EntitySpriteDraw(trailTexture, center, trailFrame, innerColor, rotation + MathHelper.PiOver2, trailOrigin,
            Projectile.scale * 1.28f, SpriteEffects.None, 0);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix);

        DrawPixel(pixel, center - direction * 10f, rotation, new Vector2(42f, 9f) * Projectile.scale, outerColor * 0.92f);
        DrawPixel(pixel, center, rotation, new Vector2(30f, 6.1f) * Projectile.scale, innerColor);
        DrawPixel(pixel, center + direction * 9f, rotation + 0.18f, new Vector2(14f, 2.8f) * Projectile.scale,
            innerColor * 0.72f);
        DrawPixel(pixel, center + direction * 9f, rotation - 0.18f, new Vector2(14f, 2.8f) * Projectile.scale,
            innerColor * 0.72f);
        DrawPixel(pixel, center + direction * 7f, rotation, new Vector2(16f, 3.1f) * Projectile.scale,
            Color.White * Projectile.Opacity);
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

    private void DrawTrailRibbon(Texture2D pixel, Texture2D trailTexture, Rectangle trailFrame, Vector2 trailOrigin,
        Color outerColor, Color innerColor) {
        Vector2 previousCenter = Projectile.Center;
        int segmentCount = 0;

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            Vector2 oldPosition = Projectile.oldPos[i];
            if (oldPosition == Vector2.Zero)
                break;

            segmentCount++;
            Vector2 currentCenter = oldPosition + Projectile.Size * 0.5f;
            float progress = i / (float)Projectile.oldPos.Length;
            float opacity = (1f - progress) * (Empowered ? 1f : 0.88f);
            Color segmentOuter = Color.Lerp(innerColor, outerColor, 0.35f + progress * 0.45f) * (opacity * 0.82f);
            Color segmentInner = Color.White * (opacity * 0.55f);

            DrawBeam(pixel, previousCenter - Main.screenPosition, currentCenter - Main.screenPosition,
                MathHelper.Lerp(24f, 8f, progress) * Projectile.scale, segmentOuter);
            DrawBeam(pixel, previousCenter - Main.screenPosition, currentCenter - Main.screenPosition,
                MathHelper.Lerp(10f, 3.4f, progress) * Projectile.scale, segmentInner);

            Vector2 drawPosition = currentCenter - Main.screenPosition;
            float flameScale = MathHelper.Lerp(1.18f, 0.42f, progress) * Projectile.scale;
            float flameRotation = Main.GlobalTimeWrappedHourly * 4.6f + i * 0.65f;
            Main.EntitySpriteDraw(trailTexture, drawPosition, trailFrame, segmentOuter, flameRotation, trailOrigin,
                flameScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(trailTexture, drawPosition, trailFrame, innerColor * (opacity * 0.48f), -flameRotation,
                trailOrigin, flameScale * 0.62f, SpriteEffects.None, 0);

            previousCenter = currentCenter;
        }

        if (segmentCount == 0)
            return;
    }

    private static void DrawBeam(Texture2D pixel, Vector2 start, Vector2 end, float width, Color color) {
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length <= 0.5f)
            return;

        Main.EntitySpriteDraw(pixel, start, null, color, delta.ToRotation(), new Vector2(0f, 0.5f),
            new Vector2(length, width), SpriteEffects.None, 0);
    }
}
