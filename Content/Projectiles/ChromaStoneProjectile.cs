using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneProjectile : ModProjectile {
    public const int ModeVolleyBolt = 0;
    public const int ModePrismBolt = 1;
    public const int ModeVolleyShard = 2;
    public const int ModeBurstShard = 3;

    private bool initialized;
    private bool splitTriggered;
    private int ricochetCount;

    private int Mode => (int)Math.Round(Projectile.ai[0]);
    private float PowerRatio => MathHelper.Clamp(Projectile.ai[1], 0f, 1f);
    private bool IsPrismBolt => Mode == ModePrismBolt;
    private bool IsShard => Mode == ModeVolleyShard || Mode == ModeBurstShard;
    private bool IsBurstShard => Mode == ModeBurstShard;

    public override string Texture => "Ben10Mod/Content/Projectiles/ChromaStoneProjectile";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 92;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (!initialized) {
            initialized = true;
            ApplyModeSetup();
        }

        float acceleration = IsPrismBolt ? 1.018f : IsShard ? 1.006f : 1.012f;
        float maxSpeed = IsPrismBolt ? 23.5f : IsBurstShard ? 16.5f : IsShard ? 15.5f : 19.5f;
        if (Projectile.velocity.LengthSquared() < maxSpeed * maxSpeed)
            Projectile.velocity *= acceleration;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(PowerRatio * 2.6f + Projectile.identity * 0.07f,
            IsPrismBolt ? 1.28f : IsShard ? 1.08f : 1.16f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * (IsPrismBolt ? 0.95f : IsShard ? 0.52f : 0.68f));

        if (!Main.dedServ && Main.rand.NextBool(IsPrismBolt ? 1 : 2)) {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float dustRadius = IsPrismBolt ? 9f : IsShard ? 5f : 7f;
            Dust dust = Dust.NewDustPerfect(Projectile.Center - direction * 3f + Main.rand.NextVector2Circular(dustRadius, dustRadius),
                DustID.WhiteTorch, -direction * Main.rand.NextFloat(IsPrismBolt ? 0.45f : 0.25f, IsPrismBolt ? 1.25f : 0.85f) +
                Main.rand.NextVector2Circular(0.28f, 0.28f), 65, prismColor,
                Main.rand.NextFloat(IsPrismBolt ? 1.35f : IsShard ? 0.95f : 1.08f, IsPrismBolt ? 1.85f : IsShard ? 1.18f : 1.42f));
            dust.noGravity = true;
            dust.fadeIn = IsPrismBolt ? 1.1f : 0.72f;

            if (IsPrismBolt && Main.rand.NextBool(2)) {
                Dust sparkle = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GemDiamond, Main.rand.NextVector2Circular(0.8f, 0.8f) - direction * Main.rand.NextFloat(0.2f, 0.65f),
                    55, Color.White, Main.rand.NextFloat(0.85f, 1.18f));
                sparkle.noGravity = true;
                sparkle.fadeIn = 0.9f;
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (IsShard || ricochetCount >= 1)
            return true;

        ricochetCount++;
        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X;
        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y;

        Projectile.velocity *= 0.92f;
        Projectile.netUpdate = true;
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        float multiplier = 1f + PowerRatio * 0.1f;
        if (IsPrismBolt)
            multiplier += 0.18f;
        else if (Mode == ModeBurstShard)
            multiplier += 0.06f;
        else if (IsShard)
            multiplier *= 0.86f;

        modifiers.SourceDamage *= multiplier;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (splitTriggered || Projectile.owner != Main.myPlayer || IsShard)
            return;

        splitTriggered = true;
        int shardCount = IsPrismBolt ? 3 : 2;
        int shardDamage = Math.Max(1, (int)Math.Round(Projectile.damage * (IsPrismBolt ? 0.44f : 0.4f)));
        float spread = IsPrismBolt ? 0.32f : 0.24f;
        Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);

        for (int i = 0; i < shardCount; i++) {
            float progress = shardCount <= 1 ? 0.5f : i / (float)(shardCount - 1);
            float angleOffset = MathHelper.Lerp(-spread, spread, progress);
            Vector2 shardVelocity = baseDirection.RotatedBy(angleOffset) * Main.rand.NextFloat(11.5f, 15.5f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, shardVelocity, Type, shardDamage,
                Projectile.knockBack * 0.7f, Projectile.owner, ModeVolleyShard, PowerRatio);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float rotation = direction.ToRotation();
        float spriteRotation = rotation + MathHelper.PiOver2;
        Vector2 spriteOrigin = texture.Size() / 2f;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.14f + PowerRatio * 1.4f, IsPrismBolt ? 1.14f : 1.02f);
        Color middle = ChromaStonePrismHelper.GetSpectrumColor(0.62f + PowerRatio * 1.1f, 1.08f);
        Color core = new Color(245, 250, 255, 230);

        if (IsShard) {
            DrawShardTrail(pixel, direction, rotation);
            DrawShardBody(pixel, center, rotation, normal, outer, middle, core);
            return false;
        }

        if (!IsPrismBolt) {
            DrawVolleyTrail(pixel, direction, rotation);
            DrawVolleyBody(pixel, texture, center, rotation, spriteRotation, spriteOrigin, normal, outer, middle, core);
            return false;
        }

        DrawPrismBoltTrail(pixel, texture, spriteRotation, spriteOrigin);
        DrawPrismBoltBody(pixel, texture, center, rotation, spriteRotation, spriteOrigin, normal, outer, middle, core);
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        int dustCount = IsPrismBolt ? 24 : IsShard ? 10 : 16;
        for (int i = 0; i < dustCount; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.24f + PowerRatio * 0.6f,
                IsPrismBolt ? 1.24f : 1.12f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                Main.rand.NextVector2Circular(IsPrismBolt ? 5.6f : 3.6f, IsPrismBolt ? 5.6f : 3.6f), 75,
                prismColor, Main.rand.NextFloat(1.05f, IsPrismBolt ? 1.65f : 1.34f));
            dust.noGravity = true;
            dust.fadeIn = IsPrismBolt ? 1f : 0.72f;
        }
    }

    private void DrawVolleyTrail(Texture2D pixel, Vector2 direction, float rotation) {
        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float progress = i / (float)Projectile.oldPos.Length;
            Vector2 trailCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Color trailColor = Color.Lerp(new Color(115, 214, 255), Color.White, 0.5f) * ((1f - progress) * 0.68f);
            Vector2 scale = new Vector2(MathHelper.Lerp(23f, 6f, progress), MathHelper.Lerp(5.6f, 2f, progress)) *
                            Projectile.scale;
            ChromaStonePrismHelper.DrawRotatedRect(pixel, trailCenter - direction * progress * 3f, rotation, scale, trailColor);
        }
    }

    private void DrawVolleyBody(Texture2D pixel, Texture2D texture, Vector2 center, float rotation, float spriteRotation,
        Vector2 spriteOrigin, Vector2 normal, Color outer, Color middle, Color core) {
        float bodyLength = 24f + PowerRatio * 3f;
        float bodyWidth = 4.8f + PowerRatio * 0.8f;
        Color blueOuter = Color.Lerp(new Color(76, 184, 255), outer, 0.22f);
        Color blueMiddle = Color.Lerp(new Color(178, 238, 255), middle, 0.28f);

        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength * 1.45f, bodyWidth * 2.1f) * Projectile.scale, blueOuter * 0.42f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength, bodyWidth) * Projectile.scale, blueMiddle * 0.9f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + normal * bodyWidth * 0.58f, rotation + 0.5f,
            new Vector2(bodyLength * 0.2f, Math.Max(1.4f, bodyWidth * 0.25f)) * Projectile.scale, blueOuter * 0.62f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center - normal * bodyWidth * 0.58f, rotation - 0.5f,
            new Vector2(bodyLength * 0.2f, Math.Max(1.4f, bodyWidth * 0.25f)) * Projectile.scale, blueOuter * 0.62f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength * 0.48f, Math.Max(1.8f, bodyWidth * 0.28f)) * Projectile.scale, core);

        Main.EntitySpriteDraw(texture, center, null, Color.White * 0.92f, spriteRotation, spriteOrigin,
            Projectile.scale * (1.15f + PowerRatio * 0.1f), SpriteEffects.None, 0);
    }

    private void DrawShardTrail(Texture2D pixel, Vector2 direction, float rotation) {
        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float progress = i / (float)Projectile.oldPos.Length;
            Vector2 trailCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Color trailColor = ChromaStonePrismHelper.GetSpectrumColor(progress * 1.5f + i * 0.06f) *
                               ((1f - progress) * (IsBurstShard ? 0.5f : 0.38f));
            Vector2 scale = new Vector2(MathHelper.Lerp(IsBurstShard ? 14f : 11f, 3.5f, progress),
                MathHelper.Lerp(IsBurstShard ? 3.8f : 3f, 1.4f, progress)) * Projectile.scale;
            ChromaStonePrismHelper.DrawRotatedRect(pixel, trailCenter - direction * progress * 2f, rotation, scale, trailColor);
        }
    }

    private void DrawShardBody(Texture2D pixel, Vector2 center, float rotation, Vector2 normal, Color outer, Color middle,
        Color core) {
        float bodyLength = IsBurstShard ? 16f : 12f;
        float bodyWidth = IsBurstShard ? 4.4f : 3.2f;
        float wobble = (Projectile.identity % 2 == 0 ? 1f : -1f) * 0.24f;

        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation + wobble,
            new Vector2(bodyLength, bodyWidth) * Projectile.scale, outer * 0.72f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + normal * bodyWidth * 0.45f, rotation + 0.75f,
            new Vector2(bodyLength * 0.34f, Math.Max(1.2f, bodyWidth * 0.2f)) * Projectile.scale, middle * 0.5f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center - normal * bodyWidth * 0.45f, rotation - 0.75f,
            new Vector2(bodyLength * 0.34f, Math.Max(1.2f, bodyWidth * 0.2f)) * Projectile.scale, middle * 0.5f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength * 0.35f, Math.Max(1.2f, bodyWidth * 0.18f)) * Projectile.scale, core * 0.78f);
    }

    private void DrawPrismBoltTrail(Texture2D pixel, Texture2D texture, float spriteRotation, Vector2 spriteOrigin) {
        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float progress = i / (float)Projectile.oldPos.Length;
            Vector2 trailCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Color trailColor = ChromaStonePrismHelper.GetSpectrumColor(progress * 2.4f + i * 0.1f) *
                               ((1f - progress) * 0.62f);
            float trailScale = MathHelper.Lerp(1.58f, 0.48f, progress) * Projectile.scale;
            Main.EntitySpriteDraw(texture, trailCenter, null, trailColor, spriteRotation, spriteOrigin,
                trailScale, SpriteEffects.None, 0);

            Vector2 beamScale = new(MathHelper.Lerp(34f, 8f, progress), MathHelper.Lerp(7.5f, 2f, progress));
            ChromaStonePrismHelper.DrawRotatedRect(pixel, trailCenter, Projectile.rotation, beamScale * Projectile.scale,
                trailColor * 0.46f);
        }
    }

    private void DrawPrismBoltBody(Texture2D pixel, Texture2D texture, Vector2 center, float rotation, float spriteRotation,
        Vector2 spriteOrigin, Vector2 normal, Color outer, Color middle, Color core) {
        float bodyLength = 36f + PowerRatio * 5f;
        float bodyWidth = 10f + PowerRatio * 1.2f;

        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength * 1.42f, bodyWidth * 1.78f) * Projectile.scale, outer * 0.36f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength, bodyWidth) * Projectile.scale, outer * 0.92f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength * 0.58f, bodyWidth * 0.34f) * Projectile.scale, middle * 0.82f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + normal * bodyWidth * 0.48f, rotation + 0.55f,
            new Vector2(bodyLength * 0.32f, Math.Max(2.2f, bodyWidth * 0.24f)) * Projectile.scale, middle * 0.68f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center - normal * bodyWidth * 0.48f, rotation - 0.55f,
            new Vector2(bodyLength * 0.32f, Math.Max(2.2f, bodyWidth * 0.24f)) * Projectile.scale, middle * 0.68f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength * 0.4f, Math.Max(2.4f, bodyWidth * 0.2f)) * Projectile.scale, core);

        Main.EntitySpriteDraw(texture, center, null, outer * 0.58f, spriteRotation, spriteOrigin,
            Projectile.scale * (2.12f + PowerRatio * 0.18f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, center, null, middle * 0.9f, spriteRotation, spriteOrigin,
            Projectile.scale * (1.72f + PowerRatio * 0.16f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, center, null, Color.White, spriteRotation, spriteOrigin,
            Projectile.scale * (1.36f + PowerRatio * 0.14f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, center, null, core, spriteRotation, spriteOrigin,
            Projectile.scale * (0.96f + PowerRatio * 0.1f), SpriteEffects.None, 0);
    }

    private void ApplyModeSetup() {
        Projectile.scale = IsPrismBolt
            ? 1.18f + PowerRatio * 0.18f
            : IsBurstShard
                ? 0.84f + PowerRatio * 0.08f
                : IsShard
                    ? 0.76f + PowerRatio * 0.08f
                    : 1f + PowerRatio * 0.12f;

        Projectile.timeLeft = IsPrismBolt ? 110 : IsBurstShard ? 58 : IsShard ? 50 : 86;
        Projectile.extraUpdates = IsPrismBolt ? 2 : 1;
        Projectile.penetrate = IsPrismBolt ? 3 : IsShard ? 1 : 2;
        Projectile.localNPCHitCooldown = IsPrismBolt ? 8 : IsShard ? 14 : 10;
    }
}
