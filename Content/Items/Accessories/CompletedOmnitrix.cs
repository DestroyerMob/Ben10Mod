using System.Collections.Generic;
using Ben10Mod.Content.Items.Materials;
using Ben10Mod.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Ben10Mod.Content.Items.Accessories {
    public class CompletedOmnitrix : Omnitrix {
        public override int MaxOmnitrixEnergy => 900;
        public override int ItemValue => Item.buyPrice(gold: 20);
        public override bool UseEnergyForTransformation => true;
        public override bool BuiltInTransformationFailsafe => true;
        public override int OmnitrixEnergyRegen => 6;
        public override int OmnitrixEnergyDrain => 4;
        public override int TranformationSwapCost => 25;
        public override string HandsOnTextureKey => "Ultimatrix";
        public override string CooldownHandsOnTextureKey => "UltimatrixAlt";

        public override string Texture => $"Ben10Mod/Content/Items/Accessories/{Name}";

        public override ModItem Clone(Item item) {
            CompletedOmnitrix clone = (CompletedOmnitrix)base.Clone(item);
            clone.transformationNum = transformationNum;
            clone.transformationSlots = (string[])transformationSlots?.Clone();
            return clone;
        }

        public override void SaveData(TagCompound tag) {
            tag["selectedAlien"] = transformationNum;
        }

        public override void LoadData(TagCompound tag) {
            tag.TryGet("selectedAlien", out transformationNum);
        }

        public override void SetStaticDefaults() {
            dynamicTexture = ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/CompletedOmnitrix").Value;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "CompletedSummary",
                "Perfected Omnitrix access with built-in failsafe protection"));
            tooltips.Add(new TooltipLine(Mod, "CompletedMasterControl",
                "Grants Master Control while equipped and reduces transformation friction"));
            tooltips.Add(new TooltipLine(Mod, "CompletedSync",
                "Swapping to a different alien grants Omni Sync for 5 seconds and restores Omnitrix Energy"));
            tooltips.Add(new TooltipLine(Mod, "CompletedFailsafe",
                "Includes the Reversion Failsafe effect"));
            base.ModifyTooltips(tooltips);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if (player == null)
                return true;

            dynamicTexture = ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/CompletedOmnitrix").Value;
            spriteBatch.Draw(dynamicTexture, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) {
            OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
            omp.completedOmnitrixEquipped = true;
            omp.transformationFailsafeEquipped = true;
            omp.transformationDurationMultiplier *= 1.15f;
            omp.cooldownDurationMultiplier *= 0.8f;
            omp.primaryAbilityCooldownMultiplier *= 0.84f;
            omp.secondaryAbilityCooldownMultiplier *= 0.84f;
            omp.tertiaryAbilityCooldownMultiplier *= 0.84f;
            omp.ultimateAbilityCooldownMultiplier *= 0.88f;
            omp.heroArmorPenBonus += 6;
            omp.transformedMoveSpeedBonus += 0.08f;
            omp.transformedRunAccelerationBonus += 0.12f;

            base.UpdateAccessory(player, hideVisual);
        }

        public override void AddRecipes() {
            base.AddRecipes();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Ultimatrix>())
                .AddIngredient(ModContent.ItemType<HeroConvergenceEmblem>())
                .AddIngredient(ModContent.ItemType<OmniCoreReactor>())
                .AddIngredient(ModContent.ItemType<ReversionFailsafe>())
                .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 20)
                .AddIngredient(ItemID.LunarBar, 14)
                .AddIngredient(ItemID.FragmentSolar, 8)
                .AddIngredient(ItemID.FragmentVortex, 8)
                .AddIngredient(ItemID.FragmentNebula, 8)
                .AddIngredient(ItemID.FragmentStardust, 8)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
