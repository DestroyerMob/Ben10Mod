using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberAgentBadge : PlumbersBadge {
    public override int    BaseDamage    => 40;
    public override string BadgeRankName => "Agent";

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<ProvisionalAgentBadgeCrimtane>())
            .AddIngredient(ItemID.MeteoriteBar, 15)
            .AddIngredient(ItemID.TissueSample, 5)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ModContent.ItemType<ProvisionalAgentBadgeDemonite>())
            .AddIngredient(ItemID.MeteoriteBar, 15)
            .AddIngredient(ItemID.ShadowScale, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}