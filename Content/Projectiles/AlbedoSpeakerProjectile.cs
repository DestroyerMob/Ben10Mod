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
                Vector2 offset = new Vector2(Main.rand.NextFloat(-180f, 180f), Main.rand.NextFloat(-120f, -40f));
                Projectile.ai[0] = target.Center.X + offset.X;
                Projectile.ai[1] = target.Center.Y + offset.Y;
                Projectile.localAI[0] = 1f;
                Projectile.netUpdate = true;
            }

            Vector2 targetPosition = new(Projectile.ai[0], Projectile.ai[1]);
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, 0.08f);
            Projectile.rotation += 0.08f;

            if (++Projectile.localAI[1] % 45f == 0f && Main.netMode != NetmodeID.MultiplayerClient) {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                Vector2 velocity = Projectile.DirectionTo(target.Center) * 10f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                    ModContent.ProjectileType<AlbedoSonicBlastProjectile>(), Projectile.damage, 0f, Main.myPlayer);
            }
        }
    }
}
