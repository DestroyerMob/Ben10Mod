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
    public class Explosion : ModProjectile {

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;           // makes it damage enemies
            Projectile.DamageType = DamageClass.Ranged; // or Throwing/Magic/etc
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;              // short lifetime
            Projectile.tileCollide = false;
            Projectile.hide = true;               // invisible
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int explosionRadius = (int)Projectile.ai[0]; // explosion size
            hitbox = new Rectangle(
                (int)(Projectile.Center.X - explosionRadius / 2),
                (int)(Projectile.Center.Y - explosionRadius / 2),
                explosionRadius,
                explosionRadius
            );
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // Optional visuals
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            for (int i = 0; i < 30; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke,
                    Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-6, 6));
        }
    }
}
