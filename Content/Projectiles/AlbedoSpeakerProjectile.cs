using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoSpeakerProjectile : ModProjectile {
        public override string Texture => "Ben10Mod/Content/Projectiles/BuzzShockMinionProjectile";

        public override void SetDefaults() {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI() {
            Vector2 anchor = new(Projectile.ai[0], Projectile.ai[1]);
            if (anchor == Vector2.Zero)
                anchor = Projectile.Center;

            Projectile.Center = Vector2.Lerp(Projectile.Center, anchor, 0.13f);
            Projectile.velocity *= 0.86f;

            if (TryGetFormationFacing(out Vector2 formationDirection)) {
                Projectile.rotation = formationDirection.ToRotation() + MathHelper.PiOver2;
            }
            else {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                if (target.active)
                    Projectile.rotation = Projectile.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;
            }

            Projectile.localAI[0]++;
            Lighting.AddLight(Projectile.Center, 0.34f, 0.18f, 0.18f);
            SpawnWarningDust();
        }

        private bool TryGetFormationFacing(out Vector2 direction) {
            direction = Vector2.Zero;
            int speakerCount = 0;
            Vector2 speakerCenter = Vector2.Zero;
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!speaker.active || speaker.type != Projectile.type)
                    continue;

                Vector2 speakerAnchor = new(speaker.ai[0], speaker.ai[1]);
                if (speakerAnchor == Vector2.Zero)
                    speakerAnchor = speaker.Center;

                speakerCount++;
                speakerCenter += speakerAnchor;
                minX = Math.Min(minX, speakerAnchor.X);
                maxX = Math.Max(maxX, speakerAnchor.X);
                minY = Math.Min(minY, speakerAnchor.Y);
                maxY = Math.Max(maxY, speakerAnchor.Y);
            }

            if (speakerCount < 5)
                return false;

            speakerCenter /= speakerCount;
            float horizontalSpan = maxX - minX;
            float verticalSpan = maxY - minY;
            if (speakerCount >= 5 && horizontalSpan > 1f && verticalSpan > 1f) {
                float aspectRatio = horizontalSpan / verticalSpan;
                if (aspectRatio > 0.80f && aspectRatio < 1.25f) {
                    Vector2 orbitAnchor = new(Projectile.ai[0], Projectile.ai[1]);
                    if (orbitAnchor == Vector2.Zero)
                        orbitAnchor = Projectile.Center;

                    direction = (orbitAnchor - speakerCenter).SafeNormalize(Vector2.UnitY);
                    return true;
                }
            }

            if (speakerCount < 6)
                return false;

            bool verticalLanes = verticalSpan > horizontalSpan;
            Vector2 anchor = new(Projectile.ai[0], Projectile.ai[1]);
            if (anchor == Vector2.Zero)
                anchor = Projectile.Center;

            direction = verticalLanes
                ? (anchor.Y < speakerCenter.Y ? Vector2.UnitY : -Vector2.UnitY)
                : (anchor.X < speakerCenter.X ? Vector2.UnitX : -Vector2.UnitX);
            return true;
        }

        private void SpawnWarningDust() {
            if (Main.dedServ)
                return;

            float pulse = 0.72f + 0.25f * MathF.Sin(Projectile.localAI[0] * 0.16f);
            if (Main.rand.NextBool(3)) {
                Dust core = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Firework_Red, Main.rand.NextVector2Circular(0.3f, 0.3f), 120,
                    new Color(255, 115, 115), pulse);
                core.noGravity = true;
            }

            if (!Main.rand.NextBool(8))
                return;

            Vector2 ringOffset = Main.rand.NextVector2CircularEdge(18f, 18f);
            Dust ring = Dust.NewDustPerfect(Projectile.Center + ringOffset, DustID.GemSapphire,
                ringOffset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * 0.6f, 135,
                new Color(175, 225, 255), 0.92f);
            ring.noGravity = true;
        }
    }
}
