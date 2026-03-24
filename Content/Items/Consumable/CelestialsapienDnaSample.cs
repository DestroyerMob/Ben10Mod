using Ben10Mod.Content.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Consumable;

public class CelestialsapienDnaSample : ModItem {
    public override string Texture => "Ben10Mod/Content/Items/Materials/HeroFragment";

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.width = 24;
        Item.height = 24;
        Item.maxStack = Item.CommonMaxStack;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useAnimation = 45;
        Item.useTime = 45;
        Item.useTurn = true;
        Item.consumable = true;
        Item.noMelee = true;
        Item.rare = ItemRarityID.Red;
        Item.value = Item.buyPrice(gold: 25);
        Item.UseSound = SoundID.Unlock;
    }

    public override bool CanUseItem(Player player) {
        return NPC.downedMoonlord && !TransformationHandler.HasTransformation(player, "Ben10Mod:AlienX");
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, "Ben10Mod:AlienX");

        if (player.whoAmI == Main.myPlayer) {
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = -0.15f }, player.Center);
            CombatText.NewText(player.getRect(), new Color(180, 255, 255), "Alien X unlocked!", dramatic: true);
        }

        return true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.FragmentSolar, 12)
            .AddIngredient(ItemID.FragmentVortex, 12)
            .AddIngredient(ItemID.FragmentNebula, 12)
            .AddIngredient(ItemID.FragmentStardust, 12)
            .AddIngredient(ItemID.LunarBar, 10)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 15)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}
