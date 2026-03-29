using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class WildVineBomb : ModProjectile {

        public override void SetDefaults() {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90;
            Projectile.penetrate = 1;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Grenade;
            Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI() {
            Projectile.rotation += Projectile.velocity.X * 0.08f;

            if (Main.rand.NextBool(2)) {
                Dust seedDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    Main.rand.NextBool() ? DustID.Grass : DustID.Poisoned,
                    -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.08f),
                    100, new Color(140, 205, 95), Main.rand.NextFloat(0.9f, 1.1f));
                seedDust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.Poisoned, 5 * 60);
            Projectile.Kill();
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.Kill();
            return false;
        }

        public override void OnKill(int timeLeft) {
            if (Projectile.owner == Main.myPlayer || Main.netMode != NetmodeID.MultiplayerClient) {
                int cloudDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.6f));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<WildVineGasCloudProjectile>(), cloudDamage, 0f, Projectile.owner);
            }

            for (int i = 0; i < 18; i++) {
                Vector2 velocity = Main.rand.NextVector2Circular(2.6f, 2.6f);
                Dust burst = Dust.NewDustPerfect(Projectile.Center,
                    i % 3 == 0 ? DustID.Poisoned : DustID.Grass,
                    velocity, 95, new Color(155, 225, 115), Main.rand.NextFloat(0.95f, 1.2f));
                burst.noGravity = true;
            }

            for (int i = 0; i < 10; i++) {
                Dust spores = Dust.NewDustPerfect(Projectile.Center, DustID.JunglePlants,
                    Main.rand.NextVector2Circular(1.8f, 1.8f), 110, new Color(110, 180, 80), Main.rand.NextFloat(0.85f, 1f));
                spores.noGravity = true;
            }
        }
    }
}
