using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberDeputyBadgeCrimtane : PlumbersBadge {
    public override int    BaseDamage     => 28;
    public override string BadgeRankName  => "Deputy";
    public override int    BadgeRankValue => 2;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberCadetBadge>())
            .AddIngredient(ItemID.CrimtaneBar, 15)
            .AddIngredient(ItemID.TissueSample, 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
