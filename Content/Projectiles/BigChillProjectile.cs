using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Game = Terraria.Server.Game;

namespace Ben10Mod.Content.Projectiles;

public class BigChillProjectile : ModProjectile {
    private const float MaxSearchDistance = 250f;
    private const float LostTargetRange   = 400f;
    private const float ChargeSpeed       = 22f;
    private const float ChargeInertia     = 8f;
    private const float ChargeOvershoot   = 110f;
    
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 3;
    }

    public override void SetDefaults() {
        Projectile.width       = 52;
        Projectile.height      = 52;
        Projectile.penetrate   = 15;
        Projectile.tileCollide = false;
        Projectile.friendly    = true;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        NPC target = FindTarget(player, MaxSearchDistance);
        if (target != null)
            DoChargeMovement(target);
        Projectile.rotation += 0.15f;
        if (Main.GameUpdateCount % 20 == 0) {
            if (Projectile.frame == 2) {
                Projectile.frame = 0;
            }
            else {
                Projectile.frame++;
            }
        }
    }

    private NPC FindTarget(Player player, float maxDetectDistance) {
        NPC   selectedTarget       = null;
        float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

        if (player.HasMinionAttackTargetNPC) {
            NPC npc = Main.npc[player.MinionAttackTargetNPC];
            if (npc.CanBeChasedBy(this)) {
                float sqrDistanceToTarget = Vector2.DistanceSquared(npc.Center, Projectile.Center);
                if (sqrDistanceToTarget < sqrMaxDetectDistance) {
                    return npc;
                }
            }
        }

        for (int k = 0; k < Main.maxNPCs; k++) {
            NPC npc = Main.npc[k];
            if (!npc.CanBeChasedBy(this))
                continue;

            float sqrDistanceToTarget = Vector2.DistanceSquared(npc.Center, Projectile.Center);
            if (sqrDistanceToTarget < sqrMaxDetectDistance) {
                sqrMaxDetectDistance = sqrDistanceToTarget;
                selectedTarget       = npc;
            }
        }

        return selectedTarget;
    }
    
    private void DoChargeMovement(NPC target) {
        if (!target.active || target.friendly || target.dontTakeDamage)
            return;

        float distanceToTarget = Vector2.Distance(Projectile.Center, target.Center);
        if (distanceToTarget > LostTargetRange) {
            Projectile.Kill();
            return;
        }

        Vector2 chargeDirection = Projectile.Center.DirectionTo(target.Center);
        if (chargeDirection == Vector2.Zero)
            chargeDirection = Vector2.UnitX * Projectile.spriteDirection;

        Vector2 chargeDestination     = target.Center + chargeDirection * ChargeOvershoot;
        Vector2 toChargeDestination   = chargeDestination - Projectile.Center;
        float   distanceToDestination = toChargeDestination.Length();

        if (distanceToDestination > 8f) {
            toChargeDestination.Normalize();
            toChargeDestination *= ChargeSpeed;
            Projectile.velocity =  (Projectile.velocity * (ChargeInertia - 1f) + toChargeDestination) / ChargeInertia;
        }
        else {
            Projectile.Kill();
        }
    }
}