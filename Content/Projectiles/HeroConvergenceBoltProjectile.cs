using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class HeroConvergenceBoltProjectile : ModProjectile {
    private const float HomingDistance = 640f;
    private const float HomingSpeed = 18f;
    private const float HomingInertia = 12f;
    private static readonly Color OuterTrailColor = new(88, 255, 178, 0);
    private static readonly Color MidTrailColor = new(185, 255, 208, 0);
    private static readonly Color InnerTrailColor = new(255, 245, 205, 0);
    private static readonly Color CoreColor = new(255, 252, 232, 0);

    private int TargetIndex => (int)Projectile.ai[0] - 1;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        NPC target = FindTarget();
        if (target != null) {
            Vector2 desiredVelocity = Projectile.Center.DirectionTo(target.Center) * HomingSpeed;
            Projectile.velocity = (Projectile.velocity * (HomingInertia - 1f) + desiredVelocity) / HomingInertia;
        }
        else {
            Projectile.velocity *= 0.995f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.38f, 0.62f, 0.24f));

        if (Main.dedServ)
            return;

        if (Main.rand.NextBool()) {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 orbitOffset = perpendicular * Main.rand.NextFloatDirection() * Main.rand.NextFloat(2f, 5f);

            Dust outerDust = Dust.NewDustPerfect(Projectile.Center + orbitOffset,
                Main.rand.NextBool() ? DustID.GoldFlame : DustID.GreenTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.06f, 0.12f) - orbitOffset * 0.08f,
                95, new Color(170, 255, 205), Main.rand.NextFloat(1f, 1.24f));
            outerDust.noGravity = true;

            Dust coreDust = Dust.NewDustPerfect(Projectile.Center + direction * Main.rand.NextFloat(4f, 8f),
                DustID.Enchanted_Gold, -Projectile.velocity * Main.rand.NextFloat(0.02f, 0.06f), 90,
                new Color(255, 245, 210), Main.rand.NextFloat(0.95f, 1.12f));
            coreDust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        float time = Main.GlobalTimeWrappedHourly * 10f + Projectile.identity * 0.6f;
        float pulse = 1f + (float)System.Math.Sin(time) * 0.08f;
        float ribbonWave = (float)System.Math.Sin(time * 1.35f);

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            float progress = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
            float fade = progress * progress;
            Vector2 oldDrawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            float ribbonOffset = (1f - progress) * 6f;

            DrawBoltSegment(pixel, oldDrawPosition, Projectile.rotation, MathHelper.Lerp(12f, 30f, fade),
                MathHelper.Lerp(3f, 8f, fade), OuterTrailColor * (0.14f + 0.28f * fade));
            DrawBoltSegment(pixel, oldDrawPosition, Projectile.rotation, MathHelper.Lerp(8f, 18f, fade),
                MathHelper.Lerp(1.8f, 4.6f, fade), MidTrailColor * (0.14f + 0.3f * fade));
            DrawBoltSegment(pixel, oldDrawPosition, Projectile.rotation, MathHelper.Lerp(5f, 11f, fade),
                MathHelper.Lerp(1.1f, 2.8f, fade), InnerTrailColor * (0.12f + 0.32f * fade));

            DrawBoltSegment(pixel, oldDrawPosition + perpendicular * ribbonOffset * ribbonWave, Projectile.rotation + 0.16f,
                MathHelper.Lerp(6f, 12f, fade), MathHelper.Lerp(0.8f, 2.1f, fade), new Color(136, 255, 190, 0) * (0.08f + 0.14f * fade));
            DrawBoltSegment(pixel, oldDrawPosition - perpendicular * ribbonOffset * ribbonWave, Projectile.rotation - 0.16f,
                MathHelper.Lerp(6f, 12f, fade), MathHelper.Lerp(0.8f, 2.1f, fade), new Color(255, 225, 155, 0) * (0.08f + 0.14f * fade));
        }

        DrawPulseRing(pixel, drawPosition - direction * 2f, 8f * pulse, 2.4f, new Color(110, 255, 178, 0) * 0.42f, time * 0.35f);
        DrawPulseRing(pixel, drawPosition - direction * 4f, 5f * pulse, 1.6f, new Color(255, 228, 155, 0) * 0.38f, -time * 0.48f);

        DrawBoltSegment(pixel, drawPosition - direction * 1.5f, Projectile.rotation, 42f * pulse, 9.4f, OuterTrailColor * 0.82f);
        DrawBoltSegment(pixel, drawPosition + direction * 1.5f, Projectile.rotation, 28f * pulse, 5.8f, MidTrailColor * 0.92f);
        DrawBoltSegment(pixel, drawPosition + direction * 4.5f, Projectile.rotation, 16f, 3.1f, InnerTrailColor * 0.98f);
        DrawBoltSegment(pixel, drawPosition + direction * 9f, Projectile.rotation, 9f, 5.4f, CoreColor);

        DrawBoltSegment(pixel, drawPosition - direction * 2f + perpendicular * 6f, Projectile.rotation + 0.18f, 15f, 2.2f,
            new Color(128, 255, 188, 0) * 0.48f);
        DrawBoltSegment(pixel, drawPosition - direction * 2f - perpendicular * 6f, Projectile.rotation - 0.18f, 15f, 2.2f,
            new Color(128, 255, 188, 0) * 0.48f);
        DrawBoltSegment(pixel, drawPosition - direction * 6f + perpendicular * 3.5f, Projectile.rotation + 0.42f, 12f, 1.7f,
            new Color(255, 230, 165, 0) * 0.34f);
        DrawBoltSegment(pixel, drawPosition - direction * 6f - perpendicular * 3.5f, Projectile.rotation - 0.42f, 12f, 1.7f,
            new Color(255, 230, 165, 0) * 0.34f);

        Main.EntitySpriteDraw(pixel, drawPosition + direction * 10f, null, new Color(255, 250, 220, 0) * 0.95f, 0f,
            Vector2.One * 0.5f, new Vector2(6f * pulse, 6f * pulse), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SpawnImpactDust();
    }

    public override void OnKill(int timeLeft) {
        SpawnImpactDust();
    }

    private NPC FindTarget() {
        if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs) {
            NPC lockedTarget = Main.npc[TargetIndex];
            if (lockedTarget.CanBeChasedBy(Projectile) &&
                Vector2.Distance(Projectile.Center, lockedTarget.Center) <= HomingDistance)
                return lockedTarget;
        }

        NPC bestTarget = null;
        float bestDistance = HomingDistance;

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private void SpawnImpactDust() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GoldFlame : DustID.Enchanted_Gold,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 90, new Color(255, 238, 165), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }

    private static void DrawBoltSegment(Texture2D pixel, Vector2 position, float rotation, float length, float thickness,
        Color color) {
        Main.EntitySpriteDraw(pixel, position, null, color, rotation, Vector2.One * 0.5f, new Vector2(length, thickness),
            SpriteEffects.None, 0);
    }

    private static void DrawPulseRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color, float rotation) {
        const int Segments = 12;
        for (int i = 0; i < Segments; i++) {
            float angle = rotation + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.2f), SpriteEffects.None, 0);
        }
    }
}
