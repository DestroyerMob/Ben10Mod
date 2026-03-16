using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.NPCs.Bosses;

namespace Ben10Mod.Content.Items.Consumable {
    public class AlbedoSignal : ModItem {
        public override string Texture => "Ben10Mod/Content/Items/Consumable/MasterControlKey";

        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTurn = true;
            Item.consumable = true;
            Item.noMelee = true;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Roar;
        }

        public override bool CanUseItem(Player player) {
            return Main.hardMode && !NPC.AnyNPCs(ModContent.NPCType<AlbedoBoss>());
        }

        public override bool? UseItem(Player player) {
            if (player.whoAmI != Main.myPlayer)
                return false;

            NPC.SpawnBoss((int)player.Center.X, (int)player.Center.Y, ModContent.NPCType<AlbedoBoss>(), player.whoAmI);
            Item.stack--;

            return true;
        }

        public override bool ConsumeItem(Player player) {
            return false;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofFright)
                .AddIngredient(ItemID.SoulofMight)
                .AddIngredient(ItemID.SoulofSight)
                .AddIngredient(ModContent.ItemType<Items.Materials.HeroFragment>(), 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
