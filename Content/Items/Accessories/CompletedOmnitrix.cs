using System.Collections.Generic;
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
        public override int ItemValue => Item.buyPrice(gold: 14);
        public override bool UseEnergyForTransformation => true;
        public override int OmnitrixEnergyRegen => 6;
        public override int OmnitrixEnergyDrain => 4;
        public override int TranformationSwapCost => 35;
        public override string HandsOnTextureKey => "RecalibratedOmnitrix";
        public override string CooldownHandsOnTextureKey => "RecalibratedOmnitrixAlt";

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
                "A perfected Omnitrix with refined energy flow and stable transformation control"));
            tooltips.Add(new TooltipLine(Mod, "CompletedEmergencyTransform",
                "Lethal damage triggers an emergency alien transformation, restoring health and 10 OE"));
            tooltips.Add(new TooltipLine(Mod, "CompletedEmergencyCooldown",
                "Emergency transformation has a 60-second cooldown"));
            tooltips.Add(new TooltipLine(Mod, "CompletedSync",
                "Swapping to a different alien grants Omni Sync for 5 seconds and restores Omnitrix Energy"));
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
            omp.transformationDurationMultiplier *= 1.15f;
            omp.cooldownDurationMultiplier *= 0.8f;
            omp.primaryAbilityCooldownMultiplier *= 0.84f;
            omp.secondaryAbilityCooldownMultiplier *= 0.84f;
            omp.tertiaryAbilityCooldownMultiplier *= 0.84f;
            omp.heroArmorPenBonus += 6;
            omp.transformedMoveSpeedBonus += 0.08f;
            omp.transformedRunAccelerationBonus += 0.12f;

            base.UpdateAccessory(player, hideVisual);
        }

        public override void AddRecipes() {
            base.AddRecipes();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RecalibratedOmnitrix>())
                .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 18)
                .AddIngredient(ItemID.BeetleHusk, 8)
                .AddIngredient(ItemID.ChlorophyteBar, 10)
                .AddIngredient(ItemID.LihzahrdPowerCell)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
