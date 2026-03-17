using Ben10Mod.Content.Items.Materials;
using Ben10Mod.Content.Projectiles.Gwen;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons.Gwen;

public class AegisCharm : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.CrystalStorm}";

    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.damage = 38;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 20;
        Item.useTime = 56;
        Item.useAnimation = 56;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noMelee = true;
        Item.knockBack = 5f;
        Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item29;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<AegisCharmWardProjectile>();
        Item.shootSpeed = 0f;
    }

    public override bool CanUseItem(Player player) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active || projectile.owner != player.whoAmI || projectile.type != Item.shoot)
                continue;

            if (projectile.ai[2] < 1f)
                return false;
        }

        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        int type, int damage, float knockback) {
        for (int i = 0; i < 3; i++) {
            float angleOffset = MathHelper.TwoPi * i / 3f;
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI,
                angleOffset);
        }

        return false;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.SpellTome)
            .AddIngredient(ItemID.CrystalShard, 10)
            .AddIngredient(ItemID.SoulofLight, 8)
            .AddIngredient(ItemID.PixieDust, 15)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
