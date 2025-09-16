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
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
            //if (item.type == ItemID.SkeletronBossBag) {
            //    foreach (var rule in itemLoot.Get()) {
            //        if (rule is OneFromOptionsNotScaledWithLuckDropRule oneFromOptionsDrop) {
            //            var original = oneFromOptionsDrop.dropIds.ToList();
            //            original.Add(ModContent.ItemType<PrototypeOmnitrix>());
            //            oneFromOptionsDrop.dropIds = original.ToArray();
            //        }
            //    }
            //}
        }
    }
}
