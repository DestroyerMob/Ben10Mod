using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.IO;

namespace Ben10Mod.Content.Projectiles.Gwen;

public class ManaBarrierProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private Vector2 _syncedAimDirection = Vector2.UnitX;
    private bool _hasSyncedAimDirection;
    private int _aimSyncTimer;

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

        Vector2 aimDirection = GetAimDirection(owner);
        Projectile.rotation = aimDirection.ToRotation();
        Projectile.Center = owner.Center + aimDirection * 108f;
        owner.heldProj = Projectile.whoAmI;

        Lighting.AddLight(Projectile.Center, new Vector3(1.35f, 0.45f, 0.95f));
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            RepelNearbyNPCs(owner, aimDirection);
            BlockHostileProjectiles();
        }

        SpawnBarrierDust();
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_syncedAimDirection.X);
        writer.Write(_syncedAimDirection.Y);
        writer.Write(_hasSyncedAimDirection);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _syncedAimDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        _hasSyncedAimDirection = reader.ReadBoolean();
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

    private Vector2 GetAimDirection(Player owner) {
        if (Main.netMode == NetmodeID.SinglePlayer || Projectile.owner == Main.myPlayer) {
            Vector2 localAimDirection = owner.DirectionTo(Main.MouseWorld);
            if (localAimDirection == Vector2.Zero)
                localAimDirection = new Vector2(owner.direction, 0f);

            SyncAimDirection(localAimDirection);
            return localAimDirection;
        }

        if (_hasSyncedAimDirection && _syncedAimDirection.LengthSquared() > 0.0001f)
            return _syncedAimDirection;

        return new Vector2(owner.direction, 0f);
    }

    private void SyncAimDirection(Vector2 direction) {
        bool changed = !_hasSyncedAimDirection || Vector2.DistanceSquared(direction, _syncedAimDirection) > 0.0004f;
        _aimSyncTimer++;
        if (!changed && _aimSyncTimer < 6)
            return;

        _syncedAimDirection = direction;
        _hasSyncedAimDirection = true;
        _aimSyncTimer = 0;
        if (Main.netMode != NetmodeID.SinglePlayer)
            Projectile.netUpdate = true;
    }
}
