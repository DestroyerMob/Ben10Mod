using Ben10Mod.Content.Items.Materials;
using Ben10Mod.Content.Projectiles.Gwen;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons.Gwen;

public class ManaBarrier : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.DemonScythe}";

    public override void SetDefaults() {
        Item.width = 34;
        Item.height = 34;
        Item.damage = 30;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 16;
        Item.useTime = 26;
        Item.useAnimation = 26;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noMelee = true;
        Item.knockBack = 4f;
        Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item29;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<ManaBarrierProjectile>();
        Item.shootSpeed = 0f;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] < 1;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Microsoft.Xna.Framework.Vector2 position,
        Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(source, player.Center, Microsoft.Xna.Framework.Vector2.Zero, type, damage, knockback,
            player.whoAmI);
        return false;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Book, 1)
            .AddIngredient(ItemID.FallenStar, 10)
            .AddIngredient(ItemID.Bone, 20)
            .AddIngredient(ItemID.Silk, 8)
            .AddIngredient(ItemID.DemoniteBar, 10)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 8)
            .AddTile(TileID.Bookcases)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.Book, 1)
            .AddIngredient(ItemID.FallenStar, 10)
            .AddIngredient(ItemID.Bone, 20)
            .AddIngredient(ItemID.Silk, 8)
            .AddIngredient(ItemID.CrimtaneBar, 10)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 8)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}
