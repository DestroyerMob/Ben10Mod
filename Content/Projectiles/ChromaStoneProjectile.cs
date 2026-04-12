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
    private const int ModeMain = 0;
    private const int ModeShard = 1;
    private const int ModeOverload = 2;

    private float PrismChargeRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private int Mode => (int)Math.Round(Projectile.ai[1]);
    private bool IsShard => Mode == ModeShard;
    private bool IsOverloaded => Mode == ModeOverload;

    private ref float InitializedFlag => ref Projectile.localAI[0];
    private ref float SplitFlag => ref Projectile.localAI[1];

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
        Projectile.timeLeft = 86;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (InitializedFlag == 0f) {
            InitializedFlag = 1f;
            ApplyModeSetup();
        }

        float acceleration = IsOverloaded ? 1.02f : IsShard ? 1.008f : 1.012f;
        float maxSpeed = IsOverloaded ? 24f : IsShard ? 16.5f : 19.5f;
        if (Projectile.velocity.LengthSquared() < maxSpeed * maxSpeed)
            Projectile.velocity *= acceleration;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(PrismChargeRatio * 2.5f + Projectile.identity * 0.07f,
            IsOverloaded ? 1.08f : 1f);
        Lighting.AddLight(Projectile.Center, prismColor.ToVector3() * (IsOverloaded ? 0.58f : 0.42f));

        if (!Main.dedServ && Main.rand.NextBool(IsOverloaded ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.WhiteTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 100, prismColor,
                Main.rand.NextFloat(0.88f, IsOverloaded ? 1.34f : 1.18f));
            dust.noGravity = true;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (IsShard || Projectile.penetrate == -1)
            return true;
        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X;
        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y;

        Projectile.velocity *= 0.92f;
        Projectile.tileCollide = false;
        Projectile.netUpdate = true;
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        float multiplier = 1f + PrismChargeRatio * (IsOverloaded ? 0.18f : 0.1f);
        if (IsShard)
            multiplier *= 0.86f;

        modifiers.SourceDamage *= multiplier;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (SplitFlag > 0f || Projectile.owner != Main.myPlayer)
            return;

        if (IsShard)
            return;

        SplitFlag = 1f;
        int shardCount = IsOverloaded ? 3 : 2;
        int shardDamage = Math.Max(1, (int)Math.Round(Projectile.damage * (IsOverloaded ? 0.46f : 0.42f)));
        float spread = IsOverloaded ? 0.34f : 0.26f;
        Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);

        for (int i = 0; i < shardCount; i++) {
            float progress = shardCount <= 1 ? 0.5f : i / (float)(shardCount - 1);
            float angleOffset = MathHelper.Lerp(-spread, spread, progress);
            Vector2 shardVelocity = baseDirection.RotatedBy(angleOffset) * Main.rand.NextFloat(11.5f, 15.5f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, shardVelocity, Type, shardDamage,
                Projectile.knockBack * 0.72f, Projectile.owner, PrismChargeRatio, ModeShard);
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
            Color trailColor = ChromaStonePrismHelper.GetSpectrumColor(progress * 2.1f + i * 0.08f) *
                ((1f - progress) * (IsOverloaded ? 0.52f : 0.38f));
            ChromaStonePrismHelper.DrawRotatedRect(pixel, trailCenter, rotation,
                new Vector2(MathHelper.Lerp(IsOverloaded ? 36f : 28f, 10f, progress),
                    MathHelper.Lerp(IsOverloaded ? 12f : 8f, 3f, progress)) * Projectile.scale, trailColor);
        }

        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.14f + PrismChargeRatio * 1.4f, IsOverloaded ? 1.12f : 1.04f) * 0.62f;
        Color middle = ChromaStonePrismHelper.GetSpectrumColor(0.62f + PrismChargeRatio * 1.1f, 1.08f) * 0.9f;
        Color core = new Color(245, 250, 255, 230);
        float bodyLength = IsOverloaded ? 38f : IsShard ? 22f : 30f;
        float bodyWidth = IsOverloaded ? 12f : IsShard ? 6.4f : 9f;

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

        int dustCount = IsOverloaded ? 14 : 10;
        for (int i = 0; i < dustCount; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.24f + PrismChargeRatio * 0.6f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                Main.rand.NextVector2Circular(IsOverloaded ? 3.4f : 2.8f, IsOverloaded ? 3.4f : 2.8f), 95,
                prismColor, Main.rand.NextFloat(0.95f, IsOverloaded ? 1.36f : 1.18f));
            dust.noGravity = true;
        }
    }

    private void ApplyModeSetup() {
        Projectile.scale = IsOverloaded
            ? 1.16f + PrismChargeRatio * 0.18f
            : IsShard
                ? 0.76f + PrismChargeRatio * 0.08f
                : 1f + PrismChargeRatio * 0.12f;

        Projectile.timeLeft = IsOverloaded ? 112 : IsShard ? 54 : 82;
        Projectile.extraUpdates = IsOverloaded ? 2 : 1;
        Projectile.penetrate = IsOverloaded ? -1 : IsShard ? 1 : 2;
        Projectile.localNPCHitCooldown = IsOverloaded ? 7 : IsShard ? 14 : 10;
    }
}
