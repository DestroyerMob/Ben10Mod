using Terraria.ID;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberCadetBadge : PlumbersBadge {
    public override int    BaseDamage     => 10;
    public override string BadgeRankName  => "Cadet";
    public override int    BadgeRankValue => 1;

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