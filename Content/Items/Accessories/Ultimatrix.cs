using Ben10Mod.Common.Systems;
using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Ben10Mod.Content.Interface;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Ben10Mod.Content.Items.Consumable;
using Ben10Mod.Content.Items.Placeables;

namespace Ben10Mod.Content.Items.Accessories
{
    public class Ultimatrix : Omnitrix
    {
        public override int  MaxOmnitrixEnergy          => 750;
        public override int  ItemValue                  => Item.buyPrice(gold: 12);
        public override bool UseEnergyForTransformation => true;
        public override int  OmnitrixEnergyRegen        => 4;
        public override int  OmnitrixEnergyDrain        => 6;
        public override int  TranformationSwapCost      => 75;
        public override bool EvolutionFeature           => true;
        public override string CooldownHandsOnTextureKey => "UltimatrixAlt";

        public override string Texture => $"Ben10Mod/Content/Items/Accessories/{this.Name}";

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.HandsOn}", EquipType.HandsOn, name: "Ultimatrix");
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.HandsOn}", EquipType.HandsOn, name: "UltimatrixAlt");
        }

        public override ModItem Clone(Item item)
        {
            Ultimatrix clone = (Ultimatrix)base.Clone(item);
            clone.transformationNum   = transformationNum;
            clone.transformationSlots = (string[])transformationSlots?.Clone();
            return clone;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["selectedAlien"] = transformationNum;
        }

        public override void LoadData(TagCompound tag)
        {
            tag.TryGet("selectedAlien", out transformationNum);
        }

        public override void SetStaticDefaults()
        {
            dynamicTexture = ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/Ultimatrix").Value;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (player == null)
                return true;

            dynamicTexture = player.GetModPlayer<OmnitrixPlayer>().onCooldown
                ? ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/UltimatrixAlt").Value
                : ModContent.Request<Texture2D>("Ben10Mod/Content/Items/Accessories/Ultimatrix").Value;

            spriteBatch.Draw(dynamicTexture, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void AddRecipes()
        {
            base.AddRecipes();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RecalibratedOmnitrix>())
                .AddIngredient(ModContent.ItemType<UltimatrixCore>())
                .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 16)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddIngredient(ItemID.SoulofSight, 8)
                .AddTile(TileID.MythrilAnvil)
                .AddTransformationCondition("Ben10Mod:GrayMatter")
                .Register();
        }
    }
}
