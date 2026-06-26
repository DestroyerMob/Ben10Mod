using Ben10Mod.Content.Items.Placeables;
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
            int albedoBossType = ModContent.NPCType<AlbedoBoss>();

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                if (player.whoAmI != Main.myPlayer)
                    return false;

                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI,
                    number2: albedoBossType);
                return true;
            }

            NPC.SpawnBoss((int)player.Center.X + (player.direction * 80), (int)player.Center.Y, albedoBossType,
                player.whoAmI);

            return true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofFright)
                .AddIngredient(ItemID.SoulofMight)
                .AddIngredient(ItemID.SoulofSight)
                .AddIngredient(ModContent.ItemType<CongealedCodonBar>(), 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
