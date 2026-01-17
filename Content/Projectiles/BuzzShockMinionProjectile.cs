using Microsoft.Xna.Framework;
using System;
using Ben10Mod.Enums;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class BuzzShockMinionProjectile : ModProjectile {

        // Follow tuning
        private const float FollowLerp     = 0.12f;
        private const float MaxFollowSpeed = 12f;

        // Combat tuning
        private const float DetectRange         = 700f;
        private const float AttackRange         = 520f;
        private const int   AttackCooldownTicks = 60;   // base cooldown (~1s)
        private const float ThrowSpeed          = 13f;

        public override void SetStaticDefaults() {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type]      = true;
            Main.projFrames[Projectile.type]                = 1;
        }

        public override void SetDefaults() {
            Projectile.width  = 40;
            Projectile.height = 52;

            Projectile.minion      = true;
            Projectile.minionSlots = 0.5f;
            Projectile.friendly    = false;
            Projectile.DamageType  = DamageClass.Summon;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.penetrate    = -1;
            Projectile.netImportant = true;
        }

        public override void OnSpawn(IEntitySource source) {
            // Personal offset so each minion stays out of sync forever (deterministic, MP-safe)
            // 0..19 ticks extra delay depending on whoAmI
            Projectile.localAI[0] = Projectile.whoAmI % 20;

            // Start cooldown already offset (prevents first volley syncing)
            Projectile.ai[0] = Projectile.localAI[0];
        }

        public override void AI() {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead) {
                Projectile.Kill();
                return;
            }

            // --- Idle follow position behind player (slot-aware, stable with 0.5 minionSlots) ---
            float slotPos = Projectile.minionPos; // computed by Terraria using minionSlots
            float spacingPerSlot = 44f;           // spacing per 1.0 summon slot

            Vector2 behind = new Vector2(
                -player.direction * (56f + slotPos * spacingPerSlot),
                -24f
            );

            // stable stagger + gentle bob so they don't overlap perfectly
            behind.Y += ((Projectile.whoAmI & 1) == 0) ? 0f : 10f;
            behind.Y += (float)Math.Sin((Main.GameUpdateCount + (ulong)Projectile.whoAmI) * 0.08f) * 3f;

            Vector2 idlePos = player.Center + behind;
            
            float distToIdle = Vector2.Distance(Projectile.Center, idlePos);
            if (distToIdle > 1000f) {
                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
                Random random = new Random();
                for (int i = 0; i < 50; i++) {
                    int dustNum = Dust.NewDust(Projectile.position - new Vector2(1, 1), Projectile.width + 1, Projectile.height + 1, DustID.UltraBrightTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White, 2);
                    Main.dust[dustNum].noGravity = true;
                }
                Projectile.Center = idlePos;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
                for (int i = 0; i < 50; i++) {
                    int dustNum = Dust.NewDust(Projectile.position - new Vector2(1, 1), Projectile.width + 1, Projectile.height + 1, DustID.UltraBrightTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White, 2);
                    Main.dust[dustNum].noGravity = true;
                }
            }

            // --- Targeting ---
            int targetIndex = FindTarget(player, out float targetDist);
            bool hasTarget = targetIndex != -1;
            NPC target = hasTarget ? Main.npc[targetIndex] : null;

            // --- Movement ---
            // IMPORTANT CHANGE: don't move all minions to the same "attackOffset".
            // Keep their pack spacing and only raise them a bit while in combat.
            Vector2 desiredPos = idlePos;

            if (hasTarget && targetDist < AttackRange) {
                desiredPos += new Vector2(0f, -14f); // small combat lift, keeps spacing
            }

            MoveToward(desiredPos);

            // --- Face direction ---
            if (hasTarget) {
                Projectile.direction = (target.Center.X > Projectile.Center.X) ? 1 : -1;
                Projectile.spriteDirection = Projectile.direction;
            }
            else {
                Projectile.direction = player.direction;
                Projectile.spriteDirection = Projectile.direction;
            }

            // --- Attack ---
            if (Projectile.ai[0] > 0)
                Projectile.ai[0]--;

            if (hasTarget && targetDist < AttackRange) {
                if (Main.myPlayer == Projectile.owner && Projectile.ai[0] <= 0) {
                    if (Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1)) {
                        Vector2 from = Projectile.Center + new Vector2(Projectile.direction * 10f, -6f);
                        Vector2 to = target.Center;

                        Vector2 vel = to - from;
                        if (vel.LengthSquared() < 0.001f)
                            vel = Vector2.UnitX * Projectile.direction;

                        vel.Normalize();
                        vel *= ThrowSpeed;

                        int projType = ModContent.ProjectileType<BuzzShockProjectile>();

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            from,
                            vel,
                            projType,
                            Projectile.damage,
                            Projectile.knockBack,
                            Projectile.owner
                        );

                        // IMPORTANT CHANGE: per-minion offset so they don't shoot in sync
                        Projectile.ai[0] = AttackCooldownTicks + (int)Projectile.localAI[0];
                        Projectile.netUpdate = true;
                    }
                    else {
                        // If we can't see the target, don't "spam attempt" every tick.
                        Projectile.ai[0] = 10 + (int)Projectile.localAI[0];
                    }
                }
            }
        }

        private void MoveToward(Vector2 destination) {
            Vector2 toDest = destination - Projectile.Center;
            float dist = toDest.Length();

            if (dist < 6f) {
                Projectile.velocity *= 0.9f;
                return;
            }

            Vector2 desiredVel = toDest * FollowLerp;
            if (desiredVel.Length() > MaxFollowSpeed)
                desiredVel = Vector2.Normalize(desiredVel) * MaxFollowSpeed;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.25f);
        }

        private int FindTarget(Player player, out float bestDist) {
            bestDist = DetectRange;

            if (player.HasMinionAttackTargetNPC) {
                NPC forced = Main.npc[player.MinionAttackTargetNPC];
                if (forced.CanBeChasedBy(this)) {
                    float d = Vector2.Distance(Projectile.Center, forced.Center);
                    if (d < DetectRange) {
                        bestDist = d;
                        return forced.whoAmI;
                    }
                }
            }

            int best = -1;
            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(Projectile.Center, npc.Center);
                if (d < bestDist) {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }

        public override void PostAI() {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead ||
                player.GetModPlayer<OmnitrixPlayer>().currTransformation != TransformationEnum.BuzzShock) {
                Projectile.Kill();
            }
        }
    }
}
