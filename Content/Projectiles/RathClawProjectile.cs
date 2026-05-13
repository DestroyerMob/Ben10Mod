using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class RathClawProjectile : ModProjectile {
    private const int SlashLifetime = 11;
    private const int BaseHitboxSize = 46;
    private const float DefaultForwardRange = 68f;
    private const float DefaultRageForwardRange = 92f;

    private int ComboStep => Utils.Clamp((int)Math.Round(Projectile.ai[0]), 0, 2);
    private bool Finisher => ComboStep >= 2;
    private bool RageSlash => Projectile.ai[1] > 1.1f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = BaseHitboxSize;
        Projectile.height = BaseHitboxSize;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = SlashLifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = SlashLifetime + 2;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            if (Projectile.velocity != Vector2.Zero)
                Projectile.ai[2] = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f)).ToRotation();

            float fallbackRange = RageSlash ? DefaultRageForwardRange : DefaultForwardRange;
            Projectile.localAI[1] = Vector2.Distance(owner.MountedCenter, Projectile.Center);
            if (Projectile.localAI[1] <= 0.01f)
                Projectile.localAI[1] = fallbackRange;
        }

        float slashScale = Projectile.ai[1] > 0f ? Projectile.ai[1] : 1f;
        float forwardRange = Projectile.localAI[1] > 0f
            ? Projectile.localAI[1]
            : (RageSlash ? DefaultRageForwardRange : DefaultForwardRange);
        float anchorAngle = Projectile.ai[2];
        Vector2 anchorDirection = anchorAngle.ToRotationVector2();
        Vector2 anchorPoint = owner.MountedCenter + anchorDirection * forwardRange;
        float lineRotation = ResolveSlashLineRotation(anchorAngle);
        Vector2 slashAxis = lineRotation.ToRotationVector2();

        Projectile.rotation = lineRotation + MathHelper.PiOver2;
        Projectile.Center = anchorPoint;
        Projectile.velocity = slashAxis * (Finisher ? 7.5f : 6f);
        Projectile.scale = slashScale;
        owner.itemRotation = MathHelper.WrapAngle(anchorAngle) * owner.direction;
        owner.itemTime = Math.Max(owner.itemTime, 2);
        owner.itemAnimation = Math.Max(owner.itemAnimation, 2);
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, anchorAngle - MathHelper.PiOver2);

        UpdateHitboxSize(slashScale);
        SpawnSlashDustLine(anchorPoint, slashAxis, slashScale);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        float slashScale = Projectile.ai[1] > 0f ? Projectile.ai[1] : 1f;
        float lifeProgress = 1f - Projectile.timeLeft / (float)SlashLifetime;
        float opacity = Utils.GetLerpValue(0f, 0.2f, lifeProgress, true) * Utils.GetLerpValue(1f, 0.45f, lifeProgress, true);
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outerColor = Finisher ? new Color(255, 132, 86, 230) : new Color(220, 224, 235, 210);
        Color innerColor = RageSlash ? new Color(255, 238, 186, 240) : Color.White;
        float length = Finisher ? 78f : 62f;
        float thickness = Finisher ? 11f : 8f;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), outerColor * opacity,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(thickness * slashScale, length * slashScale),
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), innerColor * opacity,
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(3.5f * slashScale, (length - 14f) * slashScale),
            SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Bleeding, Finisher ? 260 : RageSlash ? 210 : 150);

        Vector2 slashDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + slashDirection.X * (Finisher ? 3.4f : 1.8f), -12f, 12f),
            MathHelper.Clamp(target.velocity.Y - (Finisher ? 1.1f : 0.4f), -8f, 10f));
        target.netUpdate = true;

        int dustCount = Finisher ? 20 : 14;
        for (int i = 0; i < dustCount; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center, i % 3 == 0 ? DustID.Smoke : DustID.Blood,
                slashDirection.RotatedByRandom(0.52f) * Main.rand.NextFloat(1.2f, Finisher ? 4.6f : 3.2f), 90,
                new Color(255, 214, 190), Main.rand.NextFloat(1f, Finisher ? 1.42f : 1.18f));
            dust.noGravity = true;
        }
    }

    private float ResolveSlashLineRotation(float anchorAngle) {
        return ComboStep switch {
            0 => anchorAngle + MathHelper.PiOver2 - 0.45f,
            1 => anchorAngle + MathHelper.PiOver2 + 0.45f,
            _ => anchorAngle + MathHelper.PiOver2
        };
    }

    private void UpdateHitboxSize(float slashScale) {
        int targetSize = (int)Math.Round(BaseHitboxSize * slashScale * (Finisher ? 1.12f : 1f));
        if (Projectile.width == targetSize && Projectile.height == targetSize)
            return;

        Vector2 center = Projectile.Center;
        Projectile.width = targetSize;
        Projectile.height = targetSize;
        Projectile.Center = center;
    }

    private void SpawnSlashDustLine(Vector2 center, Vector2 lineDirection, float slashScale) {
        if (Main.dedServ)
            return;

        float halfLength = (Finisher ? 40f : 30f) * slashScale;
        Vector2 normal = lineDirection.RotatedBy(MathHelper.PiOver2);
        int dustLines = Finisher ? 4 : 3;

        for (int i = 0; i < dustLines; i++) {
            float along = Main.rand.NextFloat(-halfLength, halfLength);
            float across = Main.rand.NextFloat(-3f, 3f) * slashScale;
            Vector2 dustPosition = center + lineDirection * along + normal * across;
            Dust smoke = Dust.NewDustPerfect(dustPosition, Main.rand.NextBool(3) ? DustID.Blood : DustID.Smoke,
                lineDirection * Main.rand.NextFloat(0.3f, 1f), 110, new Color(248, 218, 196),
                Main.rand.NextFloat(0.95f, RageSlash ? 1.25f : 1.08f));
            smoke.noGravity = true;
        }
    }
}
