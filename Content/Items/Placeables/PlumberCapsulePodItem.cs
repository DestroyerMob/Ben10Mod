using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Tiles
{
    public class PlumberCapsulePodItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width      = 32;
            Item.height     = 32;
            Item.useTime    = Item.useAnimation = 15;
            Item.useStyle   = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<PlumberCapsulePod>();
            Item.rare       = ItemRarityID.Orange;
        }
    }
}