using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class XLR8DashAccessory : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.EoCShield}";

    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[ItemID.EoCShield] = Type;
    }

    public override void SetDefaults() {
        Item.DefaultToAccessory(28, 30);
        Item.value = Item.sellPrice(gold: 2);
        Item.rare = ItemRarityID.Blue;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "XLR8Dash", "Double tap left or right to perform an XLR8-style dash"));
        tooltips.Add(new TooltipLine(Mod, "XLR8DashVisual", "Dashing briefly shifts you into XLR8 before snapping back"));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<OmnitrixPlayer>().xlr8DashAccessoryEquipped = true;
    }
}
