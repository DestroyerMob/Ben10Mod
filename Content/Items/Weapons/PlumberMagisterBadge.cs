using Ben10Mod.Content.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberMagisterBadge : PlumbersBadge {
    public override int    BaseDamage     => 95;
    public override string BadgeRankName  => "Magister";
    public override int    BadgeRankValue => 8;

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberFieldProctorBadge>())
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 25)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}