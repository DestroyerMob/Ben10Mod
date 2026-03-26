using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.DiamondHead
{
    public class DiamondHead : ModItem {
        public static string TransformationDescription =>
            "A durable Petrosapien built to anchor fights with piercing crystal fire, fortified bulk, and punishing prism strikes.";

        public static IReadOnlyList<string> TransformationAbilities => new[] {
            "Main attack: precise crystal shard shot with strong armor penetration.",
            "Secondary attack: shard barrage for wide close-range pressure.",
            "Primary ability: bulwark stance with huge defense, damage reduction, and retaliatory shards when struck.",
            "Secondary ability: call a prism pincer at the cursor that collapses inward and erupts into shard crossfire.",
            "Ultimate attack: call a giant diamond strike from above.",
            "Role: tanky ranged bruiser that dominates while holding ground."
        };

        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new DiamondHeadHead());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this, equipTexture: new DiamondHeadHead());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this, equipTexture: new DiamondHeadHead());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Back}", EquipType.Back, this, equipTexture: new DiamondHeadHead());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.Body}", EquipType.Body, this, "DiamondHeadAlt");
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.Legs}", EquipType.Legs, this, "DiamondHeadAlt");
        }

        private void SetupDrawing() {
            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
        }

        public override void SetStaticDefaults() {
            SetupDrawing();
        }

        public override void SetDefaults() {
            Item.width = 40;
            Item.height = 80;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player) {
            return !TransformationHandler.HasTransformation(player, "Ben10Mod:DiamondHead");
        }

        public override bool? UseItem(Player player) {
            TransformationHandler.AddTransformation(player, "Ben10Mod:DiamondHead");
            return true;
        }

    }

    public class DiamondHeadHead : EquipTexture {
        public override bool IsVanitySet(int head, int body, int legs) => true;
    }
}
