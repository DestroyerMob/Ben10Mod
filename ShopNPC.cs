using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Consumable;
using Ben10Mod.Content.Items.Vanity;
using Terraria;
using Terraria.GameContent.ItemDropRules;
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

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
        if (npc.type == NPCID.Plantera) {
            if (Main.expertMode) {
                npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<MasterControlKey>()));
            }
        }
    }
}