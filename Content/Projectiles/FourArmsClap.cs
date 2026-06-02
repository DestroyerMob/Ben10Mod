using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsClap : ModProjectile {
    private const int Lifetime = 24;
    private const int PotisLifetime = 30;
    private const int LaunchDustCount = 18;
    private const int ImpactDustCount = 32;
    private const float BaseReach = 86f;
    private const float ReachGrowth = 34f;
    private const float BaseCollisionWidth = 22f;
    private const float MaxCollisionWidth = 34f;
    private const float SideWingOffset = 14f;
    private const float ImpactSpread = 0.92f;
    private const float ImpactSurfaceInset = 6f;

    private bool PotisInfused => Projectile.ai[1] >= 0.5f;
    private int LifetimeFrames => PotisInfused ? PotisLifetime : Lifetime;
    private float PotisScale => PotisInfused ? 1.32f : 1f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 56;
        Projectile.height = 42;
        Projectile.friendly = true;
        Projectile.timeLeft = Lifetime;
        Projectile.knockBack = 500f;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override void AI() {
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;
            if (PotisInfused)
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, PotisLifetime);
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float speed = Projectile.ai[0] > 0f ? Projectile.ai[0] : Projectile.velocity.Length();
        if (speed <= 0f)
            speed = 18f;

        Projectile.ai[0] = speed;
        Projectile.velocity = direction * speed;
        Projectile.rotation = direction.ToRotation();
        Projectile.scale = MathHelper.Lerp(PotisInfused ? 1.02f : 0.92f, PotisInfused ? 1.54f : 1.28f, GetProgress());
        Projectile.GetGlobalProjectile<OmnitrixProjectile>().EnableScaleHitboxSync(Projectile);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SpawnLaunchBurst(direction);
        }

        SpawnTrailDust(direction);
        Lighting.AddLight(Projectile.Center,
            (PotisInfused ? new Vector3(0.94f, 0.42f, 0.12f) : new Vector3(0.58f, 0.28f, 0.1f)) *
            (0.38f + (PotisInfused ? 0.28f : 0.16f) * GetProgress()));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float progress = GetProgress();
        float reach = (BaseReach + ReachGrowth * progress) * Projectile.scale * PotisScale;
        float collisionWidth = MathHelper.Lerp(BaseCollisionWidth, MaxCollisionWidth, progress) * Projectile.scale *
                               (PotisInfused ? 1.18f : 1f);
        Vector2 origin = Projectile.Center - direction * (16f * Projectile.scale);
        Vector2 end = Projectile.Center + direction * reach;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), origin, end,
                   collisionWidth, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   origin + normal * SideWingOffset * Projectile.scale, end,
                   collisionWidth * 0.72f, ref collisionPoint)
               || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                   origin - normal * SideWingOffset * Projectile.scale, end,
                   collisionWidth * 0.72f, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float progress = GetProgress();
        float reach = (BaseReach + ReachGrowth * progress) * Projectile.scale * PotisScale;
        float outerWidth = MathHelper.Lerp(PotisInfused ? 24f : 18f, PotisInfused ? 42f : 30f, progress) * Projectile.scale;
        float innerWidth = outerWidth * 0.56f;
        Vector2 origin = Projectile.Center - direction * (18f * Projectile.scale) - Main.screenPosition;
        Vector2 end = Projectile.Center + direction * reach - Main.screenPosition;

        Color outer = PotisInfused ? new Color(185, 58, 22, 118) : new Color(120, 14, 14, 96);
        Color middle = PotisInfused ? new Color(255, 144, 58, 170) : new Color(225, 95, 68, 142);
        Color inner = PotisInfused ? new Color(255, 238, 178, 218) : new Color(255, 231, 196, 190);

        DrawBeam(pixel, origin + normal * 12f * Projectile.scale, end, outer, outerWidth * 0.9f);
        DrawBeam(pixel, origin - normal * 12f * Projectile.scale, end, outer, outerWidth * 0.9f);
        DrawBeam(pixel, origin, end, middle, outerWidth);
        DrawBeam(pixel, origin, end, inner, innerWidth);

        Vector2 head = end + direction * 12f * Projectile.scale;
        Main.spriteBatch.Draw(pixel, head, new Rectangle(0, 0, 1, 1),
            PotisInfused ? new Color(255, 230, 166, 210) : new Color(255, 228, 205, 180), 0f,
            new Vector2(0.5f, 0.5f), new Vector2(18f * Projectile.scale, 18f * Projectile.scale), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 impactDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 impactPoint = GetImpactPoint(target, impactDirection);

        SpawnImpactBurst(impactPoint, impactDirection);
        TrySpawnPotisFissure(impactPoint, impactDirection);
        target.velocity = new Vector2(
            MathHelper.Clamp(target.velocity.X + impactDirection.X * (PotisInfused ? 8.2f : 6.5f), -15f, 15f),
            MathHelper.Clamp(target.velocity.Y - (PotisInfused ? 2.7f : 2.1f), -10f, 10f));
        target.netUpdate = true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Vector2 impactDirection = oldVelocity.SafeNormalize(Vector2.UnitX);
        SpawnImpactBurst(Projectile.Center, impactDirection);
        TrySpawnPotisFissure(Projectile.Center, impactDirection);
        return true;
    }

    private float GetProgress() {
        return MathHelper.Clamp(1f - Projectile.timeLeft / (float)LifetimeFrames, 0f, 1f);
    }

    private void SpawnLaunchBurst(Vector2 direction) {
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        int dustCount = PotisInfused ? LaunchDustCount + 8 : LaunchDustCount;
        for (int i = 0; i < dustCount; i++) {
            float lateral = Main.rand.NextFloat(-18f, 18f);
            Vector2 velocity = direction * Main.rand.NextFloat(PotisInfused ? 3.2f : 2.6f, PotisInfused ? 7.2f : 6f) +
                               normal * Main.rand.NextFloat(-1.8f, 1.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + normal * lateral, PotisInfused && i % 4 == 0 ? DustID.WhiteTorch : DustID.Torch,
                velocity, 90, PotisInfused ? new Color(255, 205, 125) : new Color(255, 180, 120),
                Main.rand.NextFloat(PotisInfused ? 1.18f : 1.05f, PotisInfused ? 1.65f : 1.42f));
            dust.noGravity = true;
        }
    }

    private void SpawnTrailDust(Vector2 direction) {
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float progress = GetProgress();
        float reach = BaseReach * (0.45f + 0.35f * progress) * PotisScale;

        int dustCount = PotisInfused ? 3 : 2;
        for (int i = 0; i < dustCount; i++) {
            float t = Main.rand.NextFloat();
            Vector2 dustPosition = Projectile.Center + direction * (t * reach) + normal * Main.rand.NextFloat(-20f, 20f);
            Vector2 velocity = direction * Main.rand.NextFloat(0.5f, 2.1f) + normal * Main.rand.NextFloat(-0.45f, 0.45f);
            int dustType = PotisInfused && i == 0 ? DustID.WhiteTorch : i == 0 && Main.rand.NextBool(3) ? DustID.Smoke : DustID.Torch;
            Color dustColor = dustType == DustID.Smoke
                ? new Color(185, 185, 185)
                : PotisInfused ? new Color(255, 205, 125) : new Color(255, 185, 120);
            Dust dust = Dust.NewDustPerfect(dustPosition, dustType, velocity, 110, dustColor,
                Main.rand.NextFloat(PotisInfused ? 1.06f : 0.96f, PotisInfused ? 1.52f : 1.28f) + progress * 0.16f);
            dust.noGravity = true;
        }
    }

    private void SpawnImpactBurst(Vector2 impactPoint, Vector2 burstDirection) {
        int dustCount = PotisInfused ? ImpactDustCount + 16 : ImpactDustCount;
        for (int i = 0; i < dustCount; i++) {
            float spread = Main.rand.NextFloat(-ImpactSpread, ImpactSpread);
            Vector2 direction = burstDirection.RotatedBy(spread).SafeNormalize(burstDirection);
            float speed = Main.rand.NextFloat(PotisInfused ? 5.6f : 4.8f, PotisInfused ? 12.4f : 10.6f);
            Vector2 velocity = direction * speed;
            Vector2 position = impactPoint + direction * Main.rand.NextFloat(0f, 8f);

            Dust dust = Dust.NewDustPerfect(position, DustID.WhiteTorch, velocity, 70,
                PotisInfused ? new Color(255, 226, 165) : Color.White,
                Main.rand.NextFloat(PotisInfused ? 1.9f : 1.6f, PotisInfused ? 3f : 2.7f));
            dust.noGravity = true;

            if (Main.rand.NextBool(3)) {
                dust.velocity += burstDirection * Main.rand.NextFloat(0.8f, 1.6f);
            }
        }

        for (int i = 0; i < (PotisInfused ? 18 : 12); i++) {
            Vector2 direction = burstDirection.RotatedBy(Main.rand.NextFloat(-0.42f, 0.42f)).SafeNormalize(burstDirection);
            Dust coreDust = Dust.NewDustPerfect(impactPoint, PotisInfused && i % 3 == 0 ? DustID.Torch : DustID.Smoke,
                direction * Main.rand.NextFloat(1.2f, PotisInfused ? 4f : 3.2f), 105,
                PotisInfused ? new Color(255, 214, 150) : new Color(240, 240, 240),
                Main.rand.NextFloat(1.1f, PotisInfused ? 1.85f : 1.6f));
            coreDust.noGravity = true;
        }
    }

    private void TrySpawnPotisFissure(Vector2 origin, Vector2 direction) {
        if (!PotisInfused || Projectile.ai[2] > 0f || Projectile.owner != Main.myPlayer)
            return;

        Projectile.ai[2] = 1f;
        float shockDirection = direction.X == 0f ? 1f : Math.Sign(direction.X);
        int fissureDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.52f));
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), origin + new Vector2(shockDirection * 8f, 10f), Vector2.Zero,
            ModContent.ProjectileType<FourArmsFissureProjectile>(), fissureDamage, Projectile.knockBack + 1.2f,
            Projectile.owner, shockDirection, 1f);
    }

    private static void DrawBeam(Texture2D pixel, Vector2 start, Vector2 end, Color color, float width) {
        Vector2 edge = end - start;
        float rotation = edge.ToRotation();
        Main.spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, rotation,
            new Vector2(0f, 0.5f), new Vector2(edge.Length(), width), SpriteEffects.None, 0f);
    }

    private Vector2 GetImpactPoint(NPC target, Vector2 impactDirection) {
        Rectangle hitbox = target.Hitbox;
        Vector2 surfacePoint = new(
            MathHelper.Clamp(Projectile.Center.X, hitbox.Left, hitbox.Right),
            MathHelper.Clamp(Projectile.Center.Y, hitbox.Top, hitbox.Bottom));
        return surfacePoint - impactDirection * ImpactSurfaceInset;
    }
}
