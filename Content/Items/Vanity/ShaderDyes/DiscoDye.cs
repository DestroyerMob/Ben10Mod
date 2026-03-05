using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Vanity.ShaderDyes;

public class DiscoDye : ModItem {
    public override string Texture => "Terraria/Images/Item_" + ItemID.None;
    
    public override void SetDefaults() {
        Item.width    = 20;
        Item.height   = 20;
        Item.maxStack = 1;
        Item.value    = Item.sellPrice(gold: 1);
        Item.rare     = ItemRarityID.Blue;
    }
}