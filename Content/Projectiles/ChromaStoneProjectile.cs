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

    public override string Texture => "Terraria/Images/Projectile_0";

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
            IsPrismBolt ? 1.14f : 1f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * (IsPrismBolt ? 0.58f : 0.4f));

        if (!Main.dedServ && Main.rand.NextBool(IsPrismBolt ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.WhiteTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 100, prismColor,
                Main.rand.NextFloat(0.88f, IsPrismBolt ? 1.3f : 1.14f));
            dust.noGravity = true;
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
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float rotation = direction.ToRotation();

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float progress = i / (float)Projectile.oldPos.Length;
            Vector2 trailCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Color trailColor = ChromaStonePrismHelper.GetSpectrumColor(progress * 2f + i * 0.08f) *
                ((1f - progress) * (IsPrismBolt ? 0.56f : 0.38f));
            ChromaStonePrismHelper.DrawRotatedRect(pixel, trailCenter, rotation,
                new Vector2(MathHelper.Lerp(IsPrismBolt ? 38f : 28f, 10f, progress),
                    MathHelper.Lerp(IsPrismBolt ? 12f : 8f, 3f, progress)) * Projectile.scale, trailColor);
        }

        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.14f + PowerRatio * 1.4f, IsPrismBolt ? 1.12f : 1.04f) * 0.62f;
        Color middle = ChromaStonePrismHelper.GetSpectrumColor(0.62f + PowerRatio * 1.1f, 1.08f) * 0.9f;
        Color core = new Color(245, 250, 255, 230);
        float bodyLength = IsPrismBolt ? 36f : IsShard ? 20f : 28f;
        float bodyWidth = IsPrismBolt ? 12f : IsShard ? 6f : 8.8f;

        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength, bodyWidth) * Projectile.scale, outer);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength * 0.66f, bodyWidth * 0.46f) * Projectile.scale, middle);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center + normal * bodyWidth * 0.48f, rotation + 0.55f,
            new Vector2(bodyLength * 0.36f, Math.Max(2.2f, bodyWidth * 0.28f)) * Projectile.scale, middle * 0.7f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center - normal * bodyWidth * 0.48f, rotation - 0.55f,
            new Vector2(bodyLength * 0.36f, Math.Max(2.2f, bodyWidth * 0.28f)) * Projectile.scale, middle * 0.7f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, rotation,
            new Vector2(bodyLength * 0.38f, Math.Max(2f, bodyWidth * 0.2f)) * Projectile.scale, core);
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        int dustCount = IsPrismBolt ? 14 : 10;
        for (int i = 0; i < dustCount; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.24f + PowerRatio * 0.6f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                Main.rand.NextVector2Circular(IsPrismBolt ? 3.6f : 2.8f, IsPrismBolt ? 3.6f : 2.8f), 95,
                prismColor, Main.rand.NextFloat(0.95f, IsPrismBolt ? 1.3f : 1.16f));
            dust.noGravity = true;
        }
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
