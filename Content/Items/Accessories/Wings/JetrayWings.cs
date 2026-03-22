using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories.Wings {
    [AutoloadEquip(EquipType.Wings)]
    public class JetrayWings : ModItem {
        public override string Texture => "Ben10Mod/Content/Items/Accessories/Wings/StinkFlyWings";

        public override void SetStaticDefaults() {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(160, 11f, 3f);
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
            ascentWhenFalling = 0.95f;
            ascentWhenRising = 0.18f;
            maxCanAscendMultiplier = 1.15f;
            maxAscentMultiplier = 3.25f;
            constantAscend = 0.16f;
        }
    }
}
