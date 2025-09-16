using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Interface
{
    public class OmnitrixSlot : ModAccessorySlot {
        public override string Name => "OmnitrixSlot";

        public override string FunctionalTexture => "Ben10Mod/Content/Items/Accessories/PrototypeOmnitrix";

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
            return ModContent.GetModItem(checkItem.type) is Omnitrix;
        }

        public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) {
            return ModContent.GetModItem(item.type) is Omnitrix;
        }

        public override void OnMouseHover(AccessorySlotType context) {
            Main.hoverItemName = context switch { 
                AccessorySlotType.FunctionalSlot => "Omnitrix",
                AccessorySlotType.VanitySlot => "Vanity Omnitrix",
                AccessorySlotType.DyeSlot => "Dye"
            };
            
            base.OnMouseHover(context);
        }

        public override bool IsHidden() {
            return Player.GetModPlayer<OmnitrixPlayer>().isTransformed;
        }
    }
}
