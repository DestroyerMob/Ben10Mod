using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.BigChill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BigChillGraveMistProjectile : ModProjectile {
    public const float VariantMist = 0f;
    public const float VariantTrail = 1f;

    private bool IsTrail => Projectile.ai[0] >= VariantTrail;
    private bool AbsoluteZero => Projectile.ai[1] >= 0.5f;
    private bool UltimateForm =>
        Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && BigChillTransformation.IsUltimateBigChill(Main.player[Projectile.owner]);
    private int MaxLifetime => IsTrail
        ? (AbsoluteZero ? 75 : UltimateForm ? 68 : 60)
        : (AbsoluteZero ? 7 * 60 : UltimateForm ? 7 * 60 : 6 * 60);
    private float MaxRadius => IsTrail
        ? (AbsoluteZero ? 56f : UltimateForm ? 50f : 44f)
        : (AbsoluteZero ? 144f : UltimateForm ? 124f : 108f);

    private float CurrentRadius {
        get {
            float progress = 1f - MathHelper.Clamp(Projectile.timeLeft / (float)MaxLifetime, 0f, 1f);
            float baseRadius = IsTrail ? 16f : 24f;
            return MathHelper.Lerp(baseRadius, MaxRadius, progress);
        }
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 6 * 60;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.timeLeft = MaxLifetime;
        Projectile.localNPCHitCooldown = IsTrail
            ? (AbsoluteZero ? 12 : UltimateForm ? 13 : 15)
            : (AbsoluteZero ? 10 : UltimateForm ? 12 : 14);
    }

    public override void AI() {
        Projectile.rotation += IsTrail ? 0.02f : 0.012f;
        Projectile.velocity *= IsTrail ? 0.92f : 0.97f;

        if (!IsTrail) {
            float drift = (float)System.Math.Sin((Main.GameUpdateCount + Projectile.identity * 9) * 0.06f) * 0.03f;
            Projectile.velocity = Projectile.velocity.RotatedBy(drift);
        }

        Lighting.AddLight(Projectile.Center,
            AbsoluteZero ? new Vector3(0.22f, 0.44f, 0.7f) : UltimateForm ? new Vector3(0.18f, 0.38f, 0.64f) : new Vector3(0.14f, 0.32f, 0.58f));

        SlowHostileProjectiles();
        SpawnMistDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity *= IsTrail ? 0.88f : UltimateForm ? 0.76f : 0.8f;
        target.AddBuff(ModContent.BuffType<EnemySlow>(), IsTrail ? 30 : 75);
        BigChillTransformation.ResolveMistHit(Projectile, target, damageDone, IsTrail);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float lifetimeProgress = 1f - Projectile.timeLeft / (float)MaxLifetime;
        float fadeIn = Utils.GetLerpValue(0f, 0.18f, lifetimeProgress, true);
        float fadeOut = Utils.GetLerpValue(0f, 0.25f, Projectile.timeLeft / (float)MaxLifetime, true);
        float opacity = fadeIn * fadeOut;

        DrawRing(pixel, center, CurrentRadius * 0.82f, IsTrail ? 3.8f : 4.8f,
            (AbsoluteZero ? new Color(126, 215, 255, 96) : UltimateForm ? new Color(96, 206, 255, 90) : new Color(112, 194, 255, 82)) * opacity, Projectile.rotation);
        DrawRing(pixel, center, CurrentRadius * 0.54f, IsTrail ? 3.2f : 4f,
            (AbsoluteZero ? new Color(186, 245, 255, 104) : UltimateForm ? new Color(166, 240, 255, 98) : new Color(172, 232, 255, 94)) * opacity, -Projectile.rotation * 1.2f);
        DrawRing(pixel, center, CurrentRadius * 0.28f, IsTrail ? 2.4f : 3.2f,
            (AbsoluteZero ? new Color(230, 250, 255, 110) : UltimateForm ? new Color(220, 246, 255, 106) : new Color(218, 244, 255, 102)) * opacity, Projectile.rotation * 1.6f);
        return false;
    }

    private void SlowHostileProjectiles() {
        float radius = CurrentRadius * (IsTrail ? 0.8f : 1f);
        Rectangle area = Utils.CenteredRectangle(Projectile.Center, new Vector2(radius * 2f));
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile other = Main.projectile[i];
            if (!other.active || !other.hostile || other.friendly || other.owner == Projectile.owner)
                continue;

            if (!area.Intersects(other.Hitbox))
                continue;

            other.velocity *= IsTrail ? 0.985f : UltimateForm ? 0.964f : 0.972f;
        }
    }

    private void SpawnMistDust() {
        if (Main.dedServ)
            return;

        int attempts = IsTrail ? 1 : 2;
        for (int i = 0; i < attempts; i++) {
            if (!Main.rand.NextBool(IsTrail ? 2 : 1))
                continue;

            Vector2 offset = Main.rand.NextVector2Circular(CurrentRadius * 0.32f, CurrentRadius * 0.26f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool() ? DustID.IceTorch : DustID.Frost,
                new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.7f, -0.08f)),
                105, AbsoluteZero ? new Color(190, 240, 255) : UltimateForm ? new Color(154, 232, 255) : new Color(168, 225, 255),
                Main.rand.NextFloat(IsTrail ? 0.82f : 0.92f, AbsoluteZero ? 1.22f : 1.08f));
            dust.noGravity = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 16;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.1f), SpriteEffects.None, 0f);
        }
    }
}
