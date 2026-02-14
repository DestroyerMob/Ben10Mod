using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class ProvisionalAgentBadgeCrimtane : PlumbersBadge {
    public override int    BaseDamage    => 25;
    public override string BadgeRankName => "Helper";

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberHelperBadge>())
            .AddIngredient(ItemID.CrimtaneBar, 15)
            .AddTile(TileID.Anvils)
            .Register();
    }
}