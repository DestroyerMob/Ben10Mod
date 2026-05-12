using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.Items.Materials;
using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public class OmniSyncedAvatarSummoner : ModItem {
    public override string Texture => $"Terraria/Images/Item_{ItemID.OpticStaff}";

    public override void SetDefaults() {
        Item.width = 32;
        Item.height = 32;
        Item.damage = 32;
        Item.knockBack = 2f;
        Item.mana = 10;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.value = Item.buyPrice(gold: 5);
        Item.rare = ItemRarityID.LightRed;
        Item.UseSound = SoundID.Item44 with { Pitch = 0.18f, Volume = 0.72f };
        Item.DamageType = DamageClass.Summon;
        Item.buffType = ModContent.BuffType<OmniSyncedAvatarBuff>();
        Item.shoot = ModContent.ProjectileType<OmniSyncedAvatarProjectile>();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "OmniSync",
            "Summons an avatar that mirrors your active transformation's attacks"));
        tooltips.Add(new TooltipLine(Mod, "AvatarDamage",
            "The avatar's base damage comes from this summon weapon"));
    }

    public override bool CanUseItem(Player player) {
        return player.GetModPlayer<OmnitrixPlayer>().GetActiveOmnitrix() != null;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Microsoft.Xna.Framework.Vector2 position,
        Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback) {
        SoundEngine.PlaySound(SoundID.AbigailSummon, player.position);
        player.AddBuff(Item.buffType, 2);
        player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback);
        return false;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<IllegalCircuits>(10)
            .AddIngredient<HeroFragment>(4)
            .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
            .AddIngredient(ItemID.SoulofSight, 6)
            .AddIngredient(ItemID.Wire, 35)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
