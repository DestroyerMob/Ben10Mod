using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Content.Transformations;

namespace Ben10Mod.Content.Buffs.Transformations
{
    public class BigChill_Buff : ModBuff
    {
        public override string Texture => "Ben10Mod/Content/Buffs/Transformations/EmptyTransformation";

        public override void Update(Player player, ref int buffIndex)
        {
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            // Keep the transformation active (sync with new string system)
            omp.currentTransformationId = "Ben10Mod:BigChill";
            omp.isTransformed = true;
        }

        public override bool RightClick(int buffIndex) => false;
    }
}
