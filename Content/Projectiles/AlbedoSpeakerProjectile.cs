using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class AlbedoSpeakerProjectile : ModProjectile {
        public override string Texture => "Ben10Mod/Content/Projectiles/BuzzShockMinionProjectile";

        public override void SetDefaults() {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
        }

        public override void AI() {
            if (Projectile.localAI[0] == 0f) {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                Vector2[] offsets = {
                    new(-220f, -150f),
                    new(-110f, -80f),
                    new(0f, -190f),
                    new(110f, -80f),
                    new(220f, -150f)
                };
                Vector2 offset = offsets[Projectile.identity % offsets.Length];
                Projectile.ai[0] = target.Center.X + offset.X;
                Projectile.ai[1] = target.Center.Y + offset.Y;
                Projectile.localAI[0] = 1f;
                Projectile.netUpdate = true;
            }

            Vector2 targetPosition = new(Projectile.ai[0], Projectile.ai[1]);
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, 0.08f);
            Projectile.rotation += 0.08f;

            if (++Projectile.localAI[1] % 40f == 0f && Main.netMode != NetmodeID.MultiplayerClient) {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                for (int i = -1; i <= 1; i++) {
                    Vector2 velocity = Projectile.DirectionTo(target.Center).RotatedBy(MathHelper.ToRadians(8f * i)) * 10f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                        ModContent.ProjectileType<AlbedoSonicBlastProjectile>(), Projectile.damage, 0f, Main.myPlayer);
                }
            }
        }
    }
}
