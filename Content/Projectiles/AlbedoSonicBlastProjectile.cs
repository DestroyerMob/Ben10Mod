using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoSonicBlastProjectile : ModProjectile {
        private const int ShotMode = 0;
        private const int PulseMode = 1;
        private const int PulseActiveLifetime = 28;

        private ref float ReleaseTimer => ref Projectile.ai[0];
        private int Mode => (int)Projectile.ai[1];
        private bool Released => ReleaseTimer < 0f;
        private Vector2 StoredVelocity => new(Projectile.localAI[0], Projectile.localAI[1]);
        private float PulseProgress => 1f - Projectile.timeLeft / (float)PulseActiveLifetime;

        public override string Texture => "Ben10Mod/Content/Projectiles/EyeGuyUltimateBeam";

        public override void SetDefaults() {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI() {
            EnsureStoredVelocity();

            if (!Released) {
                Projectile.extraUpdates = 0;
                Projectile.velocity = Vector2.Zero;
                Projectile.hostile = false;
                Projectile.alpha = 255;

                if (ReleaseTimer > 0f) {
                    ReleaseTimer--;
                    SpawnReleaseWarningDust();
                    return;
                }

                ReleaseTimer = -1f;
                Projectile.hostile = true;
                if (Mode == PulseMode) {
                    Projectile.alpha = 255;
                    Projectile.penetrate = -1;
                    Projectile.timeLeft = PulseActiveLifetime;
                    if (!Main.dedServ)
                        SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.22f, Volume = 0.4f }, Projectile.Center);
                }
                else {
                    Projectile.alpha = 0;
                    Projectile.timeLeft = 150;
                    Projectile.velocity = StoredVelocity;
                    Projectile.extraUpdates = 1;
                    if (!Main.dedServ)
                        SoundEngine.PlaySound(SoundID.Item33 with { Pitch = 0.22f, Volume = 0.35f }, Projectile.Center);
                }
            }

            if (Mode == PulseMode) {
                Projectile.velocity = Vector2.Zero;
                Projectile.extraUpdates = 0;
                Projectile.alpha = 255;
                ReleaseTimer--;
                Lighting.AddLight(Projectile.Center, 0.45f, 0.25f, 0.7f);
                SpawnPulseDust();
                return;
            }

            if (Projectile.velocity == Vector2.Zero)
                Projectile.velocity = StoredVelocity;

            Projectile.alpha = 0;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.2f);
            SpawnWaveDust();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if (Mode != PulseMode || !Released)
                return null;

            float radius = MathHelper.Lerp(24f, 108f, (float)System.Math.Sqrt(MathHelper.Clamp(PulseProgress, 0f, 1f)));
            return targetHitbox.Distance(Projectile.Center) <= radius;
        }

        public override bool PreDraw(ref Color lightColor) {
            return Released && Mode != PulseMode;
        }

        private void EnsureStoredVelocity() {
            if (Projectile.localAI[0] != 0f || Projectile.localAI[1] != 0f || Mode == PulseMode)
                return;

            Vector2 launchVelocity = Projectile.velocity;
            if (launchVelocity.LengthSquared() <= 0.01f) {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                launchVelocity = target.active ? Projectile.DirectionTo(target.Center) * 10f : Vector2.UnitX * 10f;
            }

            Projectile.localAI[0] = launchVelocity.X;
            Projectile.localAI[1] = launchVelocity.Y;
        }

        private void SpawnReleaseWarningDust() {
            if (Main.dedServ)
                return;

            if (Mode == PulseMode) {
                float radius = Main.rand.NextFloat(24f, 92f);
                Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GemSapphire,
                    offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * 0.5f, 130,
                    new Color(190, 225, 255), 0.95f);
                dust.noGravity = true;
                return;
            }

            Vector2 direction = StoredVelocity.SafeNormalize(Vector2.UnitX);
            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
            for (int i = 0; i < 2; i++) {
                float distance = Main.rand.NextFloat(34f, 460f);
                Vector2 offset = direction * distance + normal * Main.rand.NextFloatDirection() * 8f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GemSapphire,
                    -direction * 0.25f, 130, new Color(210, 235, 255), 0.88f);
                dust.noGravity = true;
            }
        }

        private void SpawnWaveDust() {
            if (Main.dedServ || !Main.rand.NextBool(2))
                return;

            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 offset = normal * Main.rand.NextFloatDirection() * Main.rand.NextFloat(4f, 12f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GemSapphire,
                normal * Main.rand.NextFloatDirection() * 0.35f, 145, new Color(215, 235, 255), 0.95f);
            dust.noGravity = true;
        }

        private void SpawnPulseDust() {
            if (Main.dedServ)
                return;

            float progress = MathHelper.Clamp(PulseProgress, 0f, 1f);
            float radius = MathHelper.Lerp(22f, 104f, (float)System.Math.Sqrt(progress));
            for (int i = 0; i < 4; i++) {
                Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
                Vector2 velocity = offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) *
                                   Main.rand.NextFloat(0.35f, 1.1f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                    i % 2 == 0 ? DustID.WhiteTorch : DustID.GemSapphire, velocity, 120,
                    new Color(180, 220, 255), Main.rand.NextFloat(0.82f, 1.12f));
                dust.noGravity = true;
            }
        }
    }
}
