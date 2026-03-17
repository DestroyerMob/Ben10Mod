using Ben10Mod.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Consumable;

public class AlbedoTransformationCache : ModItem {
    public override string Texture => "Ben10Mod/Content/Items/Consumable/MasterControlKey";

    public override void SetDefaults() {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 99;
        Item.useAnimation = 25;
        Item.useTime = 25;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = true;
        Item.rare = ItemRarityID.Lime;
        Item.UseSound = SoundID.Item4;
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, "Ben10Mod:EchoEcho");
        TransformationHandler.AddTransformation(player, "Ben10Mod:Humungousaur");
        return true;
    }
}
