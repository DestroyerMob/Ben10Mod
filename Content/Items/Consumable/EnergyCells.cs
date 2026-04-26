using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Debuffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Consumable
{
    public abstract class EnergyCellBase : ModItem
    {
        private const int HealingPotionCooldownTicks = 60 * 60;

        protected abstract int RestoreAmount { get; }
        protected abstract int TextureItemId { get; }
        protected abstract int Rarity { get; }
        protected abstract int ItemValue { get; }
        protected virtual int CooldownDuration => HealingPotionCooldownTicks;

        public override string Texture => $"Terraria/Images/Item_{TextureItemId}";

        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 30;
        }

        public override void SetDefaults() {
            Item.width = 20;
            Item.height = 26;
            Item.maxStack = 9999;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useTurn = true;
            Item.noMelee = true;
            Item.consumable = true;
            Item.rare = Rarity;
            Item.value = ItemValue;
            Item.UseSound = SoundID.Item3;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "RestoreOE", $"Restores {RestoreAmount} OE"));
        }

        public override bool CanUseItem(Player player) {
            OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
            return !player.HasBuff<OverCharged>() &&
                   omp.GetActiveOmnitrix() != null &&
                   omp.CanRestoreOmnitrixEnergy();
        }

        public override bool? UseItem(Player player) {
            OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
            float restoredAmount = omp.RestoreOmnitrixEnergy(RestoreAmount);
            if (restoredAmount <= 0f)
                return false;

            player.AddBuff(ModContent.BuffType<OverCharged>(), CooldownDuration);
            return true;
        }
    }

    public sealed class LesserEnergyCell : EnergyCellBase
    {
        protected override int RestoreAmount => 50;
        protected override int TextureItemId => ItemID.LesserManaPotion;
        protected override int Rarity => ItemRarityID.White;
        protected override int ItemValue => Item.buyPrice(silver: 2);

        public override void AddRecipes() {
            CreateRecipe(2)
                .AddIngredient(ItemID.Bottle, 2)
                .AddIngredient(ItemID.Gel, 2)
                .AddIngredient(ItemID.Mushroom)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }

    public sealed class EnergyCell : EnergyCellBase
    {
        protected override int RestoreAmount => 100;
        protected override int TextureItemId => ItemID.ManaPotion;
        protected override int Rarity => ItemRarityID.Blue;
        protected override int ItemValue => Item.buyPrice(silver: 6);

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LesserEnergyCell>(), 2)
                .AddIngredient(ItemID.GlowingMushroom)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }

    public sealed class GreaterEnergyCell : EnergyCellBase
    {
        protected override int RestoreAmount => 200;
        protected override int TextureItemId => ItemID.GreaterManaPotion;
        protected override int Rarity => ItemRarityID.Pink;
        protected override int ItemValue => Item.buyPrice(silver: 18);

        public override void AddRecipes() {
            CreateRecipe(3)
                .AddIngredient(ItemID.BottledWater, 3)
                .AddIngredient(ItemID.PixieDust, 3)
                .AddIngredient(ItemID.CrystalShard)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }

    public sealed class SuperEnergyCell : EnergyCellBase
    {
        protected override int RestoreAmount => 300;
        protected override int TextureItemId => ItemID.SuperManaPotion;
        protected override int Rarity => ItemRarityID.Yellow;
        protected override int ItemValue => Item.buyPrice(gold: 1);

        public override void AddRecipes() {
            CreateRecipe(4)
                .AddIngredient(ModContent.ItemType<GreaterEnergyCell>(), 4)
                .AddIngredient(ItemID.FragmentSolar)
                .AddIngredient(ItemID.FragmentVortex)
                .AddIngredient(ItemID.FragmentNebula)
                .AddIngredient(ItemID.FragmentStardust)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}
