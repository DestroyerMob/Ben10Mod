using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Consumable {
    public class UltimatrixCore : ModItem {
        public override string Texture => "Ben10Mod/Content/Items/Materials/HeroFragment";

        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults() {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 999;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(gold: 10);
        }
    }
}
