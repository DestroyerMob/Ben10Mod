using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class BlankAccessory : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.None}";

    public override void SetDefaults() {
        Item.accessory = true;
    }
}