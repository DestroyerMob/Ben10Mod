using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.FourArms
{
    public class FourArms : ModItem {
        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new FourArmsHead());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
        }

        private void SetupDrawing() {
            if (Main.netMode == NetmodeID.Server)
                return;

            var equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            var equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            var equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            ArmorIDs.Head.Sets.IsTallHat[equipSlotHead] = true;
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
            return !TransformationHandler.HasTransformation(player, "Ben10Mod:FourArms");
        }

        public override bool? UseItem(Player player) {
            TransformationHandler.AddTransformation(player, "Ben10Mod:FourArms");
            return true;
        }
    }

    public class FourArmsHead : EquipTexture {
        public override bool IsVanitySet(int head, int body, int legs) => true;
    }
}
