using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Transformations;

public class NRG_Buff : ModBuff {
    public override string Texture => "Ben10Mod/Content/Buffs/Transformations/EmptyTransformation";

    public override void Update(Player player, ref int buffIndex) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.currentTransformationId = "Ben10Mod:NRG";
        omp.isTransformed = true;
    }

    public override bool RightClick(int buffIndex) => false;
}
