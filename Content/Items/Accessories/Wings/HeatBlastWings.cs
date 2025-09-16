using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories.Wings {
    [AutoloadEquip(EquipType.Wings)]
    public class HeatBlastWings : ModItem {

        public override void SetStaticDefaults() {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(1, 1, 1);
        }

        public override void SetDefaults() {
            Item.width = 24;
            Item.height = 26;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
                ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
            ascentWhenFalling = 0.1f; // Falling glide speed
            ascentWhenRising = 1f; // Rising speed
            maxCanAscendMultiplier = 1f;
            maxAscentMultiplier = 1f;
            constantAscend = 1f;
        }

        public override bool WingUpdate(Player player, bool inUse)
        {
            if (player.controlJump)
            {
                Random rand = new Random();
                int dustNum = Dust.NewDust(new Vector2(player.position.X, player.height + player.position.Y), player.width, 0, DustID.SomethingRed, 0, 0, 0, Color.White);
                Main.dust[dustNum].noGravity = true;
                dustNum = Dust.NewDust(new Vector2(player.position.X, player.height + player.position.Y), player.width, 0, DustID.FlameBurst, 0, 0, 0, Color.White);
                Main.dust[dustNum].noGravity = true;
                dustNum = Dust.NewDust(new Vector2(player.position.X, player.height + player.position.Y), player.width, 0, DustID.SolarFlare, 0, 0, 0, Color.White);
                Main.dust[dustNum].noGravity = true;
            }
            return base.WingUpdate(player, inUse);
        }
    }
}
