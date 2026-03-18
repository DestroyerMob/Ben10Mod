using Ben10Mod.Content.Projectiles.Gwen;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons.Gwen;

public class HexCircle : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.SpellTome}";

    public override void SetDefaults() {
        Item.width = 32;
        Item.height = 32;
        Item.damage = 22;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 14;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noMelee = true;
        Item.knockBack = 1f;
        Item.value = Item.buyPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item20;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<HexCircleProjectile>();
        Item.shootSpeed = 0f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        int type, int damage, float knockback) {
        Vector2 spawnPosition = Main.MouseWorld;
        Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, type, damage, knockback, player.whoAmI);
        return false;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Book, 1)
            .AddIngredient(ItemID.FallenStar, 8)
            .AddIngredient(ItemID.Ruby, 6)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddIngredient(ItemID.JungleSpores, 10)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}
