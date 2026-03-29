using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class PotisAltiare : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.AvengerEmblem}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 8);
        Item.rare = ItemRarityID.Yellow;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "PotisAltiare", "Enables Potis Altiare kit upgrades when equipped"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<PotisAltiarePlayer>().potisAltiareEquipped = true;
    }
}

public class PotisAltiarePlayer : ModPlayer {
    public bool potisAltiareEquipped;

    public override void ResetEffects() {
        potisAltiareEquipped = false;
    }
}
