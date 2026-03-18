using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace Ben10Mod.Content.Projectiles.Gwen;

public class ManaBarrierProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 112;
        Projectile.height = 132;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 210;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
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
        Projectile.Center = owner.Center + aimDirection * 108f;
        owner.heldProj = Projectile.whoAmI;

        Lighting.AddLight(Projectile.Center, new Vector3(1.35f, 0.45f, 0.95f));
        RepelNearbyNPCs(owner, aimDirection);
        BlockHostileProjectiles();
        SpawnBarrierDust();
    }

    public override bool? CanHitNPC(NPC target) {
        return target.CanBeChasedBy(Projectile) && target.Hitbox.Intersects(GetBarrierHitbox()) ? null : false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.Knockback += 8f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 pushDirection = (target.Center - Main.player[Projectile.owner].Center).SafeNormalize(Vector2.UnitX);
        target.velocity = pushDirection * Math.Max(target.velocity.Length(), 8f);
        target.AddBuff(BuffID.Confused, 90);
    }

    private Rectangle GetBarrierHitbox() {
        return Utils.CenteredRectangle(Projectile.Center, new Vector2(94f, 120f));
    }

    private void SpawnBarrierDust() {
        float start = Projectile.rotation - 0.85f;
        float step = 1.7f / 11f;
        float radius = 46f;
        for (int i = 0; i < 4; i++) {
            int segment = Main.rand.Next(12);
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

    private void RepelNearbyNPCs(Player owner, Vector2 aimDirection) {
        Rectangle barrierHitbox = GetBarrierHitbox();

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.active || !npc.CanBeChasedBy(Projectile) || !npc.Hitbox.Intersects(barrierHitbox))
                continue;

            Vector2 push = (npc.Center - owner.Center).SafeNormalize(aimDirection);
            npc.velocity = Vector2.Lerp(npc.velocity, push * 7.5f, 0.35f);
        }
    }

    private void BlockHostileProjectiles() {
        Rectangle barrierHitbox = GetBarrierHitbox();

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile other = Main.projectile[i];
            if (!other.active || !other.hostile || other.friendly || other.owner == Projectile.owner)
                continue;

            if (!other.Hitbox.Intersects(barrierHitbox))
                continue;

            for (int d = 0; d < 8; d++) {
                Dust dust = Dust.NewDustPerfect(other.Center, d % 2 == 0 ? DustID.PinkTorch : DustID.GemRuby,
                    Main.rand.NextVector2Circular(2.2f, 2.2f), 100, new Color(255, 210, 245), 1.1f);
                dust.noGravity = true;
            }

            other.Kill();
        }
    }
}
