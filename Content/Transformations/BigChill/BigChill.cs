using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Enums;

namespace Ben10Mod.Content.Transformations.BigChill
{
    public class BigChill : ModItem {
        public override void Load() {
            // The code below runs only if we're not loading on a server
            if (Main.netMode == NetmodeID.Server)
                return;

            // Add equip textures
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
            
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Ultimate_{EquipType.Head}", EquipType.Head, name: "UltimateBigChill", equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Ultimate_{EquipType.Body}", EquipType.Body, name: "UltimateBigChill");
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Ultimate_{EquipType.Legs}", EquipType.Legs, name: "UltimateBigChill");
        }

        // Called in SetStaticDefaults
        private void SetupDrawing() {
            // Since the equipment textures weren't loaded on the server, we can't have this code running server-side
            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

            int equipSlotHeadAlt = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Head);
            int equipSlotBodyAlt = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Body);
            int equipSlotLegsAlt = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Legs);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
            
            ArmorIDs.Head.Sets.DrawHead[equipSlotHeadAlt]        = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBodyAlt]    = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBodyAlt]       = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegsAlt] = true;
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

        public override bool CanUseItem(Player player) => !TransformationHandler.HasTransformation(player, TransformationEnum.BigChill);

        public override bool? UseItem(Player player) {
            player.GetModPlayer<OmnitrixPlayer>().unlockedTransformation.Add(TransformationEnum.BigChill);
            return true;
        }
    }

    public class XLR8Head : EquipTexture {
        public override bool IsVanitySet(int head, int body, int legs) => true;
    }
}
