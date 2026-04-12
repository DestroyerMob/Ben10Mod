using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.EchoEcho;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoSonicBlastProjectile : ModProjectile {
    private const int LifetimeTicks = 34;
    private const float Reach = 52f;
    private const float CollisionWidth = 16f;

    private int SourceId => (int)Math.Round(Projectile.ai[0]);
    private ref float DelayTicks => ref Projectile.ai[1];
    private Vector2 StoredVelocity => new(Projectile.localAI[0], Projectile.localAI[1]);
    private bool Released => DelayTicks <= 0f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 2;
        Projectile.timeLeft = LifetimeTicks + 16;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        EnsureStoredVelocity();
        Vector2 direction = StoredVelocity.SafeNormalize(Vector2.UnitX);

        if (!Released) {
            Projectile.velocity = Vector2.Zero;
            Projectile.friendly = false;
            DelayTicks--;
            SpawnChargeDust(direction);
            return;
        }

        if (Projectile.velocity == Vector2.Zero)
            Projectile.velocity = StoredVelocity;

        Projectile.friendly = true;
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.48f, 0.7f, 0.95f) * 0.45f);
        SpawnWaveDust(direction);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (!Released)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 start = Projectile.Center - direction * 8f;
        Vector2 end = Projectile.Center + direction * Reach;
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
            start, end, CollisionWidth, ref collisionPoint);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.IsEchoEchoFracturedFor(Projectile.owner))
            modifiers.SourceDamage *= 1.1f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        EchoEchoTransformation.ResolveResonanceHit(Projectile, target, damageDone, SourceId, heavyHit: false);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.25f, Volume = 0.34f }, Projectile.Center);
        return true;
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    private void EnsureStoredVelocity() {
        if (Projectile.localAI[0] != 0f || Projectile.localAI[1] != 0f)
            return;

        Vector2 launchVelocity = Projectile.velocity;
        if (launchVelocity.LengthSquared() <= 0.01f)
            launchVelocity = new Vector2(Main.player[Projectile.owner].direction == 0 ? 1 : Main.player[Projectile.owner].direction, 0f) * 14f;

        Projectile.localAI[0] = launchVelocity.X;
        Projectile.localAI[1] = launchVelocity.Y;
        if (DelayTicks > 0f)
            Projectile.velocity = Vector2.Zero;
    }

    private void SpawnChargeDust(Vector2 direction) {
        if (Main.dedServ || !Main.rand.NextBool(3))
            return;

        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 offset = normal * Main.rand.NextFloatDirection() * Main.rand.NextFloat(4f, 10f);
        Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GemSapphire,
            Main.rand.NextVector2Circular(0.15f, 0.15f), 120, new Color(188, 230, 255), 0.82f);
        dust.noGravity = true;
    }

    private void SpawnWaveDust(Vector2 direction) {
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        for (int i = 0; i < 3; i++) {
            float along = Main.rand.NextFloat(0f, Reach * 0.7f);
            Vector2 dustPosition = Projectile.Center + direction * along + normal * Main.rand.NextFloat(-10f, 10f);
            Vector2 velocity = direction * Main.rand.NextFloat(0.8f, 2.2f) + normal * Main.rand.NextFloat(-0.4f, 0.4f);
            Dust dust = Dust.NewDustPerfect(dustPosition, DustID.WhiteTorch, velocity, 115,
                new Color(168, 230, 255), Main.rand.NextFloat(0.82f, 1.16f));
            dust.noGravity = true;
        }
    }
}
