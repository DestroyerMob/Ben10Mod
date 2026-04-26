using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberDeputyBadgeDemonite : PlumbersBadge {
    public override int    BaseDamage     => 28;
    public override string BadgeRankName  => "Deputy";
    public override int    BadgeRankValue => 2;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PlumberCadetBadge>())
            .AddIngredient(ItemID.DemoniteBar, 15)
            .AddIngredient(ItemID.ShadowScale, 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
    
}
