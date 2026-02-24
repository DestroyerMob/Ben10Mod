using System.Linq;
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
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsExpert(),
                ModContent.ItemType<MasterControlKey>(),
                10
            ));
        }

        if (npc.type == NPCID.WallofFlesh) {
            // Remove the original vanilla emblem rule
            npcLoot.RemoveWhere(rule => rule is OneFromOptionsNotScaledWithLuckDropRule optionsRule
                                        && optionsRule.dropIds != null
                                        && optionsRule.dropIds.Contains(ItemID.WarriorEmblem));

            // New pool: 4 vanilla emblems + your HeroEmblem (equal chance)
            npcLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
                ItemID.WarriorEmblem,
                ItemID.RangerEmblem,
                ItemID.SorcererEmblem,
                ItemID.SummonerEmblem,
                ModContent.ItemType<HeroEmblem>()
            ));
        }
    }
}