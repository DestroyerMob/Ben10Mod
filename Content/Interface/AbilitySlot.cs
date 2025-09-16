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
    public class AbilitySlot : ModAccessorySlot {
        public override string Name => "AbilitySlot";

        public override string FunctionalTexture => "Ben10Mod/Content/Items/Accessories/PrototypeOmnitrix";



        public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) {
            return false;
        }

        public override bool IsHidden() {
            return true;
        }
    }
}
