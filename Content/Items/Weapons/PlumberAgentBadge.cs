using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberAgentBadge : PlumbersBadge {
    public override int    BaseDamage     => 42;
    public override string BadgeRankName  => "Agent";
    public override int    BadgeRankValue => 4;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberSeniorDeputyBadge>())
            .AddIngredient(ItemID.HellstoneBar, 25)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
