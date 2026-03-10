using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Content.Items.Placeables;

namespace Ben10Mod.Content.Transformations.XLR8
{
    public class XLR8 : ModItem {
        public override void Load() {
            // The code below runs only if we're not loading on a server
            if (Main.netMode == NetmodeID.Server)
                return;

            // Add equip textures
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new XLR8Head()); // XLR8 default head
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}_alt", EquipType.Head, this, "XLR8_alt", equipTexture: new XLR8Head()); // XLR8 alt head
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this); // XLR8 default body
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this); // XLR8 default legs
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_Tail", EquipType.Back, this); // XLR8 default tail
            
        }

        // Called in SetStaticDefaults
        private void SetupDrawing() {
            // Since the equipment textures weren't loaded on the server, we can't have this code running server-side
            if (Main.netMode == NetmodeID.Server)
                return;
            int equipSlotHeadAlt = EquipLoader.GetEquipSlot(Mod, "XLR8_alt", EquipType.Head);
            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            int equipSlotBack = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Back);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false; // Hide player head under head
            ArmorIDs.Head.Sets.DrawHead[equipSlotHeadAlt] = false; // Hide player head under head
            
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true; // Hide body skin under body
            
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true; // Hide arms under body
            
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true; // Hide skin under legs
            
            ArmorIDs.Back.Sets.DrawInTailLayer[equipSlotBack] = true; // Render tail as a tail
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
            return !TransformationHandler.HasTransformation(player, "Ben10Mod:XLR8");
        }

        public override bool? UseItem(Player player) {
            TransformationHandler.AddTransformation(player, "Ben10Mod:XLR8");
            return true;
        }
    }

    public class XLR8Head : EquipTexture {
        public override bool IsVanitySet(int head, int body, int legs) => true;
    }
}