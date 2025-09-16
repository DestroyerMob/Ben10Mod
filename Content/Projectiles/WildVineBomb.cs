using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class WildVineBomb : ModProjectile {

        private const int DefaultWidthHeight = 15;
        private const int ExplosionWidthHeight = 250;

        private bool IsChild {
            get => Projectile.localAI[0] == 1;
            set => Projectile.localAI[0] = value.ToInt();
        }

        public override void SetStaticDefaults() {
            ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;

            ProjectileID.Sets.Explosive[Type] = true;
        }

        public override void SetDefaults() {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 2;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 3 * 60;

            Projectile.penetrate = -1;

            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Grenade;
        }

        public override void OnKill(int timeLeft) {

            if (Projectile.owner == Main.myPlayer && !IsChild) {
                for (int i = 0; i < 3; i++) {
                    // Random upward vector.
                    Vector2 launchVelocity = new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-5, -3));
                    // Importantly, IsChild is set to true here. This is checked in OnTileCollide to prevent bouncing and here in OnKill to prevent an infinite chain of splitting projectiles.
                    Projectile child = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, launchVelocity, Projectile.type, Projectile.damage, Projectile.knockBack, Main.myPlayer, 0, 1);
                    (child.ModProjectile as WildVineBomb).IsChild = true;
                    // Usually editing a projectile after NewProjectile would require sending MessageID.SyncProjectile, but IsChild only affects logic running for the owner so it is not necessary here.
                }
            }

            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            // Smoke Dust spawn
            for (int i = 0; i < 50; i++) {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                dust.velocity *= 1.4f;
            }

            // Fire Dust spawn
            for (int i = 0; i < 80; i++) {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenBlood, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity *= 5f;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenBlood, 0f, 0f, 100, default, 1f);
                dust.velocity *= 3f;
            }

            // Large Smoke Gore spawn
            for (int g = 0; g < 2; g++) {
                var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X += 1.5f;
                gore.velocity.Y += 1.5f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X -= 1.5f;
                gore.velocity.Y += 1.5f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X += 1.5f;
                gore.velocity.Y -= 1.5f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1.5f;
                gore.velocity.X -= 1.5f;
                gore.velocity.Y -= 1.5f;
            }

            int explosionRadius = 7; // Bomb: 4, Dynamite: 7, Explosives & TNT Barrel: 10
            int minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
            int maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
            int minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
            int maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

            Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

            Projectile.PrepareBombToBlow();
            //Projectile.ExplodeTiles(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY, false);
        }
    }
}
