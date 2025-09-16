using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons {
    public class FourArmsFist : ModItem {
        public override void SetDefaults() {
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.damage = 35;
            Item.width = 32;
            Item.height = 32;
        }
    }
}
