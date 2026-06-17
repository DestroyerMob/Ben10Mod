using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class GhostFreakProjectile : ModProjectile {
        private bool Phased => Projectile.ai[0] >= 0.5f;

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        private int oddEven = 0;

        public override void SetDefaults() {
            Projectile.width       = 16;
            Projectile.height      = 16;
            Projectile.scale       = 0.72f;
            Projectile.friendly    = true;
            Projectile.hostile     = false;
            Projectile.penetrate   = 2;
            Projectile.timeLeft    = 52;
            Projectile.DamageType  = ModContent.GetInstance<HeroDamage>();
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;

        }

        public override void AI() {
            // Rotate velocity a tiny random amount each tick -> tentacle-like curve
            float maxCurve = 0.15f; // radians; tweak for more/less wiggle
            Projectile.velocity = Projectile.velocity.RotatedByRandom(maxCurve);

            // Slow down over time so it doesn’t go forever
            // Projectile.velocity *= 0.97f;

            // Face along direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyGhostFreakFear(Projectile.owner, Phased ? 2 : 1,
                Phased ? 260 : 210);
            if (!target.boss)
                target.AddBuff(BuffID.Confused, Phased ? 120 : 75);
        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            oddEven++;
            int dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.WhiteTorch, 0, 0, 1,
                oddEven % 2 == 0 ? Color.White : Color.Black, Projectile.timeLeft / 10);
            Main.dust[dustNum].noGravity = true;
        }
    }
}
