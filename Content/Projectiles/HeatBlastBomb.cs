using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.HeatBlast;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class HeatBlastBomb : ModProjectile {

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        public override void SetDefaults() {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow;

            AIType = ProjectileID.Bullet;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        }

        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
            Player player    = null;
            bool   gotPlayer = Projectile.TryGetOwner(out player);
            var    omp       = gotPlayer ? player.GetModPlayer<OmnitrixPlayer>() : null;
            int    dust      = gotPlayer ? omp.snowflake ? DustID.IceTorch : DustID.Torch : DustID.Torch;
            int    dustNum   = Dust.NewDust(boxPosition, 1, 1, dust, 0, 0, 1, Color.White, 5);
            Main.dust[dustNum].noGravity = true;
        }

        public override void OnKill(int timeLeft) {
            HeatBlastTransformation.OnBombDetonated(Projectile);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.velocity = oldVelocity * 0.1f;
            return true;
        }
    }
}
