using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class PlumberBlasters : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.Handgun}";

    public override void SetDefaults() {
        Item.width = 40;
        Item.height = 24;
        Item.damage = 26;
        Item.DamageType = ModContent.GetInstance<HeroDamage>();
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 2f;
        Item.value = Item.buyPrice(gold: 2, silver: 20);
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item91 with { Pitch = -0.08f, Volume = 0.68f };
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<PlumberBlasterBoltProjectile>();
        Item.shootSpeed = 13.5f;
    }

    public override Vector2? HoldoutOffset() {
        return new Vector2(-4f, 0f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        int type, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        Vector2 lateral = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 muzzleOffset = direction * 18f;
        int boltDamage = Math.Max(1, (int)Math.Round(damage * 0.62f));

        for (int i = -1; i <= 1; i += 2) {
            Vector2 spawnPosition = position + muzzleOffset + lateral * i * 5f;
            Vector2 shotVelocity = velocity.RotatedBy(i * 0.035f) * Main.rand.NextFloat(0.96f, 1.04f);
            int projectileIndex = Projectile.NewProjectile(source, spawnPosition, shotVelocity, type, boltDamage, knockback,
                player.whoAmI, 1f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].netUpdate = true;
        }

        return false;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IllegalGunParts)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddIngredient(ItemID.FallenStar, 8)
            .AddIngredient(ItemID.Glass, 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
