using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Vanity {
    [AutoloadEquip(EquipType.Body)]
    public class Ben10Shirt : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 14;

            Item.value = 010000;

            Item.vanity = true;
        }
    }
}
