using Ben10Mod.Content.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
    public class OmnitrixItem : GlobalItem {
        public override void SetDefaults(Item entity) {
            if (entity.type == ItemID.FrostCore) {
                entity.accessory = true;
            }
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual) {
            
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            
            if (item.type == ItemID.FrostCore) {
                omp.snowflake = true;
            }
        }
    }
}
