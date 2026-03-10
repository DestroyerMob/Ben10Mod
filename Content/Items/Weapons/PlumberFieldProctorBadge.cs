using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberFieldProctorBadge : PlumbersBadge {
    public override int    BaseDamage     => 80;
    public override string BadgeRankName  => "FieldProctor";
    public override int    BadgeRankValue => 7;

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberProctorBadge>())
            .AddIngredient(ItemID.ShroomiteBar, 25)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}