using Terraria.ID;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberHelperBadge : PlumbersBadge {
    public override int    BaseDamage    => 15;
    public override string BadgeRankName => "Helper";

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 15)
            .AddIngredient(ItemID.Glass, 5)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 15)
            .AddIngredient(ItemID.Glass, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}