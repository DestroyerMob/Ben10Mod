using Ben10Mod.Content.Projectiles.Gwen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons.Gwen;

public class ManaThread : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.WaterBolt}";

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.damage = 16;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 9;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 2f;
        Item.value = Item.buyPrice(silver: 90);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item43;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<ManaThreadProjectile>();
        Item.shootSpeed = 12f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Book, 1)
            .AddIngredient(ItemID.CrystalShard, 8)
            .AddIngredient(ItemID.SoulofLight, 6)
            .AddIngredient(ItemID.PixieDust, 12)
            .AddIngredient(ItemID.UnicornHorn, 1)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
