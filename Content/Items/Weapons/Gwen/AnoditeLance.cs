using Ben10Mod.Content.Items.Materials;
using Ben10Mod.Content.Projectiles.Gwen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons.Gwen;

public class AnoditeLance : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.GoldenShower}";

    public override void SetDefaults() {
        Item.width = 34;
        Item.height = 34;
        Item.damage = 56;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 18;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 6f;
        Item.value = Item.buyPrice(gold: 4);
        Item.rare = ItemRarityID.LightRed;
        Item.UseSound = SoundID.Item68;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<AnoditeLanceProjectile>();
        Item.shootSpeed = 19f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.SpellTome)
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddIngredient(ItemID.CrystalShard, 20)
            .AddIngredient(ItemID.SoulofFright, 5)
            .AddIngredient(ItemID.SoulofMight, 5)
            .AddIngredient(ItemID.SoulofSight, 5)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 16)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
