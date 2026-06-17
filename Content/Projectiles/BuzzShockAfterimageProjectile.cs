using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BuzzShockAfterimageProjectile : ModProjectile {
    public const float TeleportAfterimageMode = 0f;
    public const float TaggedDetonationMode = 1f;

    private const float AfterimageRadius = 92f;
    private const float DetonationRadius = 74f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = 42;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override void AI() {
        Projectile.velocity *= 0f;

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SpawnBurstDust(TaggedDetonation ? 28 : 22);
        }

        Lighting.AddLight(Projectile.Center, 0.16f, 0.46f, 0.72f);

        if (Main.rand.NextBool(2)) {
            Vector2 dustPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(CurrentRadius, CurrentRadius) *
                Main.rand.NextFloat(0.15f, 1f);
            Dust dust = Dust.NewDustPerfect(dustPosition, DustID.UltraBrightTorch,
                (Projectile.Center - dustPosition).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2.2f),
                90, Color.White, TaggedDetonation ? 1.45f : 1.18f);
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 closest = new(
            MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
            MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
        return Vector2.DistanceSquared(Projectile.Center, closest) <= CurrentRadius * CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool wasTagged = BuzzShockTargeting.IsTagged(target);
        target.AddBuff(BuzzShockTargeting.TagBuffType, TaggedDetonation ? 360 : 260);
        if (TaggedDetonation || wasTagged)
            target.AddBuff(BuffID.Electrified, TaggedDetonation ? 150 : 90);

        Vector2 shove = target.Center - Projectile.Center;
        if (shove != Vector2.Zero) {
            shove.Normalize();
            target.velocity += shove * (TaggedDetonation ? 2.4f : 1.4f);
        }
    }

    private bool TaggedDetonation => Projectile.ai[0] >= TaggedDetonationMode;

    private float CurrentRadius => TaggedDetonation ? DetonationRadius : AfterimageRadius;

    private void SpawnBurstDust(int count) {
        for (int i = 0; i < count; i++) {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.1f, TaggedDetonation ? 5.4f : 4.1f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                i % 3 == 0 ? DustID.GemSapphire : DustID.UltraBrightTorch, velocity, 90, Color.White,
                Main.rand.NextFloat(1.1f, TaggedDetonation ? 1.75f : 1.45f));
            dust.noGravity = true;
        }
    }
}
