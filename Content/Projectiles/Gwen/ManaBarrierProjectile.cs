using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.Gwen;

public class ManaBarrierProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 78;
        Projectile.height = 100;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Vector2 aimDirection = owner.DirectionTo(Main.MouseWorld);
        if (aimDirection == Vector2.Zero)
            aimDirection = new Vector2(owner.direction, 0f);

        Projectile.rotation = aimDirection.ToRotation();
        Projectile.Center = owner.Center + aimDirection * 62f;
        owner.heldProj = Projectile.whoAmI;

        Lighting.AddLight(Projectile.Center, new Vector3(1.2f, 0.4f, 0.85f) * 0.8f);
        SpawnBarrierDust();
    }

    public override bool? CanHitNPC(NPC target) {
        return target.CanBeChasedBy(Projectile) && target.Hitbox.Intersects(GetBarrierHitbox()) ? null : false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.Knockback += 3f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Confused, 60);
    }

    private Rectangle GetBarrierHitbox() {
        Vector2 forward = Projectile.rotation.ToRotationVector2();
        Vector2 right = forward.RotatedBy(MathHelper.PiOver2);
        Vector2 topLeft = Projectile.Center - right * 24f - forward * 42f;
        return Utils.CenteredRectangle(topLeft + right * 24f + forward * 42f, new Vector2(48f, 84f));
    }

    private void SpawnBarrierDust() {
        float start = Projectile.rotation - 0.85f;
        float step = 1.7f / 8f;
        float radius = 34f;
        for (int i = 0; i < 2; i++) {
            int segment = Main.rand.Next(9);
            float angle = start + step * segment;
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 tangentialVelocity = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.25f;

            Dust outer = Dust.NewDustPerfect(Projectile.Center + offset, DustID.PinkTorch, tangentialVelocity, 95,
                new Color(255, 135, 220), 1.2f);
            outer.noGravity = true;

            Dust inner = Dust.NewDustPerfect(Projectile.Center + offset * 0.82f, DustID.GemRuby, tangentialVelocity * 0.5f,
                120, new Color(255, 235, 250), 0.95f);
            inner.noGravity = true;
        }
    }
}
