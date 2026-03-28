using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AlienXGravityPulseProjectile : ModProjectile {
    private const int LifetimeTicks = 28;
    private bool Deliberation => Projectile.ai[0] >= 0.5f;
    private float CurrentRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
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
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        if (Projectile.velocity.LengthSquared() < 1f)
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * (Deliberation ? 4f : 10f);

        if (Deliberation)
            Projectile.velocity *= 0.992f;
        else if (Projectile.velocity.LengthSquared() < 196f)
            Projectile.velocity *= 1.015f;

        Projectile.rotation = Projectile.velocity.ToRotation();

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        CurrentRadius = MathHelper.Lerp(18f, Deliberation ? 54f : 44f, progress);
        Lighting.AddLight(Projectile.Center, new Vector3(0.34f, 0.46f, 0.88f) * 0.85f);
        SpawnWaveDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 lineStart = Projectile.Center - perpendicular * CurrentRadius;
        Vector2 lineEnd = Projectile.Center + perpendicular * CurrentRadius;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd,
            18f + CurrentRadius * 0.32f, ref collisionPoint);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        int judgement = target.GetGlobalNPC<AlienIdentityGlobalNPC>().GetAlienXJudgementStacks(Projectile.owner);
        if (judgement > 0)
            modifiers.SourceDamage *= 1f + judgement * 0.08f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        identity.ApplyAlienXJudgement(Projectile.owner, Deliberation ? 2 : 1, Deliberation ? 320 : 240);

        int judgement = identity.GetAlienXJudgementStacks(Projectile.owner);
        Vector2 pushDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float pushForce = (Deliberation ? 18f : 14f) + judgement * 1.35f;
        if (target.boss)
            pushForce *= 0.45f;
        else if (target.knockBackResist > 0f)
            pushForce *= MathHelper.Lerp(0.72f, 1.15f, target.knockBackResist);

        target.velocity = Vector2.Lerp(target.velocity, pushDirection * pushForce + new Vector2(0f, -1.4f),
            target.boss ? 0.18f : 0.58f);
        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    private void SpawnWaveDust() {
        if (Main.dedServ)
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);

        for (int i = -1; i <= 1; i++) {
            float sideOffset = i * CurrentRadius * 0.55f + Main.rand.NextFloat(-4f, 4f);
            Vector2 position = Projectile.Center + perpendicular * sideOffset + direction * Main.rand.NextFloat(-4f, 10f);
            Vector2 velocity = direction * Main.rand.NextFloat(0.4f, 1.6f) + perpendicular * i * Main.rand.NextFloat(0.3f, 0.9f);

            Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.GemDiamond : DustID.ShadowbeamStaff,
                velocity, 110, Color.Lerp(new Color(150, 180, 255), new Color(235, 240, 255), Main.rand.NextFloat()),
                Main.rand.NextFloat(0.95f, 1.28f));
            dust.noGravity = true;
        }

        if (Main.rand.NextBool(2)) {
            Dust core = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.GemSapphire,
                -direction * Main.rand.NextFloat(0.2f, 0.8f), 120, new Color(120, 160, 255),
                Main.rand.NextFloat(0.8f, 1.05f));
            core.noGravity = true;
        }
    }
}
