using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberSeniorDeputyBadge : PlumbersBadge {
    public override int    BaseDamage     => 30;
    public override string BadgeRankName  => "SeniorDeputy";
    public override int    BadgeRankValue => 3;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberDeputyBadgeCrimtane>())
            .AddIngredient(ItemID.MeteoriteBar, 15)
            .AddIngredient(ItemID.TissueSample, 5)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberDeputyBadgeDemonite>())
            .AddIngredient(ItemID.MeteoriteBar, 15)
            .AddIngredient(ItemID.ShadowScale, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
