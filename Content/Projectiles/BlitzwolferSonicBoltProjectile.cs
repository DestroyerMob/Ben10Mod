using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BlitzwolferSonicBoltProjectile : ModProjectile {
    private const int MaxLifetime = 72;
    private bool Echolocating => Projectile.ai[0] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = MaxLifetime;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Echolocating) {
            NPC resonantTarget = FindResonantTarget(380f);
            if (resonantTarget != null) {
                float speed = Projectile.velocity.Length();
                Vector2 desiredVelocity = Projectile.DirectionTo(resonantTarget.Center) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.1f);
            }
        }

        if (Projectile.velocity.LengthSquared() < 625f)
            Projectile.velocity *= 1.01f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.22f, 0.95f, 0.34f) * 0.55f);

        if (Main.rand.NextBool(2)) {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 offset = perpendicular * Main.rand.NextFloatDirection() * Main.rand.NextFloat(4f, 9f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? DustID.GemEmerald : DustID.GreenTorch,
                Projectile.velocity * 0.04f, 120, new Color(150, 255, 150), Main.rand.NextFloat(0.9f, 1.1f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 lineStart = Projectile.Center - direction * 14f;
        Vector2 lineEnd = Projectile.Center + direction * 16f;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, 12f,
            ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float rotation = direction.ToRotation();

        DrawArc(pixel, Projectile.Center - direction * 4f, 12f, 2.8f, 0.55f, rotation, new Color(80, 185, 95, 110));
        DrawArc(pixel, Projectile.Center + direction * 2f, 18f, 2.2f, 0.65f, rotation, new Color(165, 255, 155, 150));
        DrawArc(pixel, Projectile.Center + direction * 6f, 24f, 1.8f, 0.75f, rotation, new Color(70, 255, 125, 110));
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int stacks = identity.GetBlitzwolferResonanceStacks(Projectile.owner);
        if (stacks > 0)
            modifiers.SourceDamage *= 1f + stacks * 0.06f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyBlitzwolferResonance(Projectile.owner, Echolocating ? 3 : 2,
            Echolocating ? 260 : 220);
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 9; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GemEmerald : DustID.GreenTorch,
                Main.rand.NextVector2Circular(2.6f, 2.6f), 100, new Color(170, 255, 170), Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }

    private static void DrawArc(Texture2D pixel, Vector2 center, float radius, float thickness, float arcHalfWidth,
        float rotation, Color color) {
        const int Segments = 9;
        for (int i = 0; i < Segments; i++) {
            float completion = i / (float)(Segments - 1);
            float angle = rotation + MathHelper.Lerp(-arcHalfWidth, arcHalfWidth, completion);
            Vector2 position = center + angle.ToRotationVector2() * radius - Main.screenPosition;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.2f), SpriteEffects.None, 0);
        }
    }

    private NPC FindResonantTarget(float maxDistance) {
        NPC bestTarget = null;
        int highestStacks = 0;
        float bestDistanceSquared = maxDistance * maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            int stacks = identity.GetBlitzwolferResonanceStacks(Projectile.owner);
            if (stacks <= 0)
                continue;

            float distanceSquared = Vector2.DistanceSquared(Projectile.Center, npc.Center);
            if (stacks < highestStacks || distanceSquared >= bestDistanceSquared)
                continue;

            highestStacks = stacks;
            bestDistanceSquared = distanceSquared;
            bestTarget = npc;
        }

        return bestTarget;
    }
}
