using Ben10Mod.Content.Projectiles.Gwen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons.Gwen;

public class AnoditeOrb : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.BookofSkulls}";

    public override void SetDefaults() {
        Item.width = 34;
        Item.height = 34;
        Item.damage = 34;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 15;
        Item.useTime = 24;
        Item.useAnimation = 24;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.value = Item.buyPrice(gold: 2, silver: 50);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item20;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<AnoditeOrbProjectile>();
        Item.shootSpeed = 8f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.SpellTome)
            .AddIngredient(ItemID.HallowedBar, 8)
            .AddIngredient(ItemID.SoulofLight, 10)
            .AddIngredient(ItemID.CrystalShard, 16)
            .AddIngredient(ItemID.PixieDust, 20)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
