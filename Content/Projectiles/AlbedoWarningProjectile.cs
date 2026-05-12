using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles
{
    public class AlbedoWarningProjectile : ModProjectile
    {
        public const int ModeRushLane = 0;
        public const int ModeRocketLine = 1;
        public const int ModeShockwaveLane = 2;

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2200;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.alpha = 255;
        }

        public override bool? CanDamage() => false;

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(ref Color lightColor) => false;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
            writer.Write(Projectile.timeLeft);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
            int syncedTimeLeft = reader.ReadInt32();
            if (syncedTimeLeft > 0)
                Projectile.timeLeft = syncedTimeLeft;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.alpha = 255;

            Vector2 line = WarningVector;
            float length = line.Length();
            if (length <= 1f)
                return;

            Vector2 direction = line / length;
            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
            Lighting.AddLight(Projectile.Center + line * 0.5f, 0.65f, 0.18f, 0.08f);

            if (Main.dedServ)
                return;

            if (Projectile.timeLeft % 2 != 0)
                return;

            switch ((int)Projectile.ai[0])
            {
                case ModeRocketLine:
                    SpawnRocketLockDust(direction, normal, length);
                    break;
                case ModeShockwaveLane:
                    SpawnShockwaveDust(direction, normal, length);
                    break;
                default:
                    SpawnRushLaneDust(direction, normal, length);
                    break;
            }
        }

        private Vector2 WarningVector => new(Projectile.localAI[0], Projectile.localAI[1]);

        private float VisualWidth => Projectile.ai[1] > 0f ? Projectile.ai[1] : 18f;

        private void SpawnRushLaneDust(Vector2 direction, Vector2 normal, float length)
        {
            float halfWidth = VisualWidth * 0.5f;
            float phase = (Main.GameUpdateCount % 18) / 18f;
            int markerCount = 10;

            for (int i = 0; i <= markerCount; i++)
            {
                float progress = MathHelper.Clamp((i + phase) / markerCount, 0f, 1f);
                Vector2 laneCenter = Projectile.Center + direction * (length * progress);
                SpawnWarningDust(laneCenter + normal * halfWidth, DustID.Firework_Red, new Color(255, 75, 55), 1.15f);
                SpawnWarningDust(laneCenter - normal * halfWidth, DustID.Firework_Red, new Color(255, 75, 55), 1.15f);

                if (i % 2 == 0)
                    SpawnWarningDust(laneCenter, DustID.RedTorch, new Color(255, 140, 90), 0.9f);
            }
        }

        private void SpawnRocketLockDust(Vector2 direction, Vector2 normal, float length)
        {
            float phase = (Main.GameUpdateCount % 14) / 14f;
            int markerCount = 9;

            for (int i = 0; i <= markerCount; i++)
            {
                float progress = MathHelper.Clamp((i + phase) / markerCount, 0f, 1f);
                Vector2 point = Projectile.Center + direction * (length * progress);
                SpawnWarningDust(point + normal * Main.rand.NextFloat(-3f, 3f), DustID.Firework_Red,
                    new Color(255, 75, 60), 1f);
            }

            Vector2 reticleCenter = Projectile.Center + direction * length;
            for (int i = 0; i < 8; i++)
            {
                Vector2 spoke = (MathHelper.TwoPi * i / 8f).ToRotationVector2();
                SpawnWarningDust(reticleCenter + spoke * 26f, DustID.Firework_Red, new Color(255, 95, 70), 1.25f);
            }

            SpawnWarningDust(reticleCenter, DustID.Torch, new Color(255, 190, 120), 1.1f);
        }

        private void SpawnShockwaveDust(Vector2 direction, Vector2 normal, float length)
        {
            float halfWidth = VisualWidth * 0.45f;
            float phase = (Main.GameUpdateCount % 16) / 16f;
            int markerCount = 10;

            for (int i = 0; i <= markerCount; i++)
            {
                float progress = MathHelper.Clamp((i + phase) / markerCount, 0f, 1f);
                Vector2 laneCenter = Projectile.Center + direction * (length * progress);
                Vector2 jitter = normal * Main.rand.NextFloat(-halfWidth, halfWidth);
                SpawnWarningDust(laneCenter + jitter, DustID.Torch, new Color(255, 145, 70), 1.1f);

                if (i % 3 == 0)
                {
                    float crackSide = i % 2 == 0 ? 1f : -1f;
                    SpawnWarningDust(laneCenter + normal * crackSide * halfWidth, DustID.Smoke,
                        new Color(255, 120, 70), 1.2f);
                }
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 spoke = (MathHelper.TwoPi * i / 6f).ToRotationVector2();
                SpawnWarningDust(Projectile.Center + spoke * 22f, DustID.Torch, new Color(255, 170, 90), 1.2f);
            }
        }

        private static void SpawnWarningDust(Vector2 position, int dustType, Color color, float scale)
        {
            Dust dust = Dust.NewDustPerfect(position, dustType, Main.rand.NextVector2Circular(0.25f, 0.25f),
                120, color, scale);
            dust.noGravity = true;
            dust.velocity *= 0.25f;
        }
    }
}
