using Ben10Mod.Content.Items.Accessories;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Interface
{
    public class OmnitrixSlot : ModAccessorySlot {
        public override string Name => "OmnitrixSlot";

        public override string FunctionalTexture => "Ben10Mod/Content/Items/Accessories/PrototypeOmnitrix";

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
            if (context == AccessorySlotType.DyeSlot)
                return checkItem.IsAir || checkItem.dye > 0;

            return HeroAlterationSlotHelper.IsHeroAlterationAccessory(checkItem);
        }

        public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) {
            var omp = Player.GetModPlayer<OmnitrixPlayer>();
            if (omp.isTransformed)
                return false;

            return HeroAlterationSlotHelper.IsHeroAlterationAccessory(item) || item.dye > 0;
        }

        public override void OnMouseHover(AccessorySlotType context) {
            Main.hoverItemName = context switch { 
                AccessorySlotType.FunctionalSlot => "DNA Alteration",
                AccessorySlotType.VanitySlot => "Vanity DNA Alteration",
                AccessorySlotType.DyeSlot => "Dye"
            };
            
            base.OnMouseHover(context);
        }

        public override bool IsHidden() {
            var omp = Player.GetModPlayer<OmnitrixPlayer>();
            return omp.isTransformed || (omp.omnitrixUpdating && (omp.equippedOmnitrix?.HideWhileUpdating ?? true));
        }
    }
}
