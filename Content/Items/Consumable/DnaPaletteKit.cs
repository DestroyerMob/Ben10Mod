using Ben10Mod.Content.Interface;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Consumable;

public class DnaPaletteKit : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.DyeVat}";

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useTurn = true;
        Item.noMelee = true;
        Item.consumable = false;
        Item.maxStack = 1;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.buyPrice(gold: 2);
        Item.UseSound = SoundID.MenuOpen;
    }

    public override bool? UseItem(Player player) {
        if (Main.dedServ || player.whoAmI != Main.myPlayer)
            return false;

        UISystem uiSystem = ModContent.GetInstance<UISystem>();
        if (uiSystem?.MyInterface == null || uiSystem.TPS == null) {
            Main.NewText("The DNA palette interface is not available right now.", Color.Red);
            return false;
        }

        uiSystem.ShowPaletteUI();
        player.GetModPlayer<OmnitrixPlayer>().showingUI = true;
        return true;
    }
}
