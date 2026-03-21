using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberSeniorAgentBadge : PlumbersBadge {
    public override int    BaseDamage     => 58;
    public override string BadgeRankName  => "SeniorAgent";
    public override int    BadgeRankValue => 5;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberAgentBadge>())
            .AddIngredient(ItemID.HallowedBar, 25)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
