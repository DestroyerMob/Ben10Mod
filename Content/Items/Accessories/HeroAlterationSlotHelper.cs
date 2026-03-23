using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Content.Interface;

namespace Ben10Mod.Content.Items.Accessories;

public interface IHeroAlterationAccessory {
}

public static class HeroAlterationSlotHelper {
    public static bool IsHeroAlterationAccessory(Item item) {
        return item?.ModItem is IHeroAlterationAccessory;
    }

    public static bool CanEquipOnlyInHeroAlterationSlot(bool modded, int slot) {
        if (!modded)
            return false;

        return slot == ModContent.GetInstance<OmnitrixSlot>().Type;
    }
}
