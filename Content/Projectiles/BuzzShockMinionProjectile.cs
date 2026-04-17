using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using System;
using Ben10Mod.Content.Buffs.Summons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class BuzzShockMinionProjectile : ModProjectile {

        private const float IdleInertia = 40f;
        private const float IdleSpeed = 10f;

        private const float ChargeSpeed = 22f;
        private const float ChargeInertia = 8f;
        private const float ChargeOvershoot = 110f;
        private const float MaxTargetRange = 700f;
        private const float LostTargetRange = 950f;

        private const float RecoverSpeed = 12f;
        private const float RecoverInertia = 18f;
        private const int RecoverTime = 20;

        private ref float State => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];

        private const int State_Idle = 0;
        private const int State_Charge = 1;
        private const int State_Recover = 2;

        public override void SetStaticDefaults() {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults() {
            Projectile.width = 40;
            Projectile.height = 52;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 18000;
            Projectile.netImportant = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool MinionContactDamage() => true;

        public override void AI() {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead) {
                player.ClearBuff(ModContent.BuffType<BuzzShockMinionBuff>());
                return;
            }

            if (player.HasBuff(ModContent.BuffType<BuzzShockMinionBuff>())) {
                Projectile.timeLeft = 2;
            }

            Vector2 idlePosition = GetIdlePosition(player);
            NPC target = FindTarget(player, MaxTargetRange);

            if (State == State_Recover) {
                DoRecoverMovement(idlePosition);

                Projectile.rotation = Projectile.velocity.X * 0.05f;
                Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
                return;
            }

            if (target == null) {
                State = State_Idle;
                DoIdleMovement(idlePosition);
            }
            else {
                if (State != State_Charge) {
                    State = State_Charge;
                    Timer = target.whoAmI;
                    Projectile.netUpdate = true;
                }

                DoChargeMovement(target, idlePosition);
            }

            Projectile.rotation = Projectile.velocity.X * 0.05f;
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(ModContent.BuffType<BuzzShockTagBuff>(), 240);
            State = State_Recover;
            Timer = RecoverTime;

            Vector2 away = Projectile.Center - target.Center;
            if (away == Vector2.Zero)
                away = new Vector2(Projectile.spriteDirection == 0 ? 1f : Projectile.spriteDirection, 0f);

            away.Normalize();
            Projectile.velocity = away * 14f;
            Projectile.netUpdate = true;

            for (int i = 0; i < 25; i++) {
                int dustNum = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.UltraBrightTorch, Scale: Main.rand.Next(1, 4));
                Main.dust[dustNum].noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, Projectile.position);
        }

        private Vector2 GetIdlePosition(Player player) {
            int index = 0;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile other = Main.projectile[i];
                if (!other.active || other.owner != Projectile.owner || other.type != Projectile.type || other.whoAmI == Projectile.whoAmI)
                    continue;

                if (other.whoAmI < Projectile.whoAmI)
                    index++;
            }

            float side = index % 2 == 0 ? 1f : -1f;
            float row = index / 2;

            return player.Center + new Vector2(56f * side, -60f - row * 36f);
        }

        private void DoIdleMovement(Vector2 idlePosition) {
            Vector2 toIdle = idlePosition - Projectile.Center;
            float distance = toIdle.Length();

            if (distance > 2000f) {
                Projectile.Center = idlePosition;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            if (distance > 16f) {
                toIdle.Normalize();
                toIdle *= IdleSpeed;
                Projectile.velocity = (Projectile.velocity * (IdleInertia - 1f) + toIdle) / IdleInertia;
            }
            else if (Projectile.velocity.Length() > 1f) {
                Projectile.velocity *= 0.96f;
            }
        }

        private void DoChargeMovement(NPC target, Vector2 idlePosition) {
            if (!target.active || target.friendly || target.dontTakeDamage) {
                State = State_Idle;
                Projectile.netUpdate = true;
                return;
            }

            float distanceToTarget = Vector2.Distance(Projectile.Center, target.Center);
            if (distanceToTarget > LostTargetRange) {
                State = State_Idle;
                Projectile.netUpdate = true;
                return;
            }

            Vector2 chargeDirection = Projectile.Center.DirectionTo(target.Center);
            if (chargeDirection == Vector2.Zero)
                chargeDirection = Vector2.UnitX * Projectile.spriteDirection;

            Vector2 chargeDestination = target.Center + chargeDirection * ChargeOvershoot;
            Vector2 toChargeDestination = chargeDestination - Projectile.Center;
            float distanceToDestination = toChargeDestination.Length();

            if (distanceToDestination > 8f) {
                toChargeDestination.Normalize();
                toChargeDestination *= ChargeSpeed;
                Projectile.velocity = (Projectile.velocity * (ChargeInertia - 1f) + toChargeDestination) / ChargeInertia;
            }
            else {
                State = State_Recover;
                Timer = 10f;

                Vector2 toIdle = idlePosition - Projectile.Center;
                if (toIdle != Vector2.Zero) {
                    toIdle.Normalize();
                    Projectile.velocity = toIdle * 10f;
                }

                Projectile.netUpdate = true;
            }
        }

        private void DoRecoverMovement(Vector2 idlePosition) {
            Timer--;

            Vector2 toIdle = idlePosition - Projectile.Center;
            float distance = toIdle.Length();

            if (distance > 16f) {
                toIdle.Normalize();
                toIdle *= RecoverSpeed;
                Projectile.velocity = (Projectile.velocity * (RecoverInertia - 1f) + toIdle) / RecoverInertia;
            }
            else if (Projectile.velocity.Length() > 1f) {
                Projectile.velocity *= 0.94f;
            }

            if (Timer <= 0f) {
                State = State_Idle;
                Projectile.netUpdate = true;
            }
        }

        private NPC FindTarget(Player player, float maxDetectDistance) {
            NPC selectedTarget = null;
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
                    selectedTarget = npc;
                }
            }

            return selectedTarget;
        }
    }
}
