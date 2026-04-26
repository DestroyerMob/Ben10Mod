using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberProctorBadge : PlumbersBadge {
    public override int    BaseDamage     => 100;
    public override string BadgeRankName  => "Proctor";
    public override int    BadgeRankValue => 6;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberSeniorAgentBadge>())
            .AddIngredient(ItemID.ChlorophyteBar, 25)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
