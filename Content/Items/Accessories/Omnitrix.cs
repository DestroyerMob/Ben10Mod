using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Items.Accessories
{
    public class Omnitrix : ModItem {

        private Player player = null;
        public int transformationNum = 0;

        public override string Texture => $"Terraria/Images/Item_{ItemID.None}";

        public override void SetDefaults() {
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Master;
            Item.DamageType = ModContent.GetInstance<HeroDamage>();
        }
    }
}