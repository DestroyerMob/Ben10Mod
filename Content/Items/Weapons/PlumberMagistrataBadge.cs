using Ben10Mod.Content.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberMagistrataBadge : PlumbersBadge {
    public override int    BaseDamage     => 210;
    public override string BadgeRankName  => "Magistrata";
    public override int    BadgeRankValue => 9;

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberMagisterBadge>())
            .AddIngredient(ItemID.LunarBar, 12)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 15)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}
