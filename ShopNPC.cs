using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Vanity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod;

public class ShopNPC : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType == NPCID.Mechanic) {
            shop.Add(ModContent.ItemType<AdvancedCircuitMatrix>());
        }

        if (shop.NpcType == NPCID.Clothier) {
            shop.Add(ModContent.ItemType<Ben10Shirt>());
            shop.Add(ModContent.ItemType<Ben10Pants>());
        }
    }
}