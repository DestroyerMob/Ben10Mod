using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ben10Mod.Enums;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles {
    public class BuzzShockMinionProjectile : ModProjectile {
        
        public override void SetStaticDefaults() {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults() {
            
            Projectile.CloneDefaults(ProjectileID.BatOfLight);
            AIType = ProjectileID.BatOfLight;
            
            Projectile.width = 40;
            Projectile.height = 52;

            Projectile.minion      = true;
            Projectile.minionSlots = 0.5f;
            Projectile.friendly    = true;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.netImportant = true;
        }

        public override void PostAI() {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.GetModPlayer<OmnitrixPlayer>().currTransformation !=
                TransformationEnum.BuzzShock) {
                Projectile.Kill();
            }
        }
    }
}
