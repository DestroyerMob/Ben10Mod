using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberMagisterBadge : PlumbersBadge {
    public override int    BaseDamage    => 55;
    public override string BadgeRankName         => "Magister";

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberAgentBadge>())
            .AddIngredient(ItemID.HellstoneBar, 25)
            .AddTile(TileID.Anvils)
            .Register();
    }
}