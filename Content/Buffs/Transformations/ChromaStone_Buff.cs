using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Transformations;

public class ChromaStone_Buff : ModBuff {
    public override string Texture => "Ben10Mod/Content/Buffs/Transformations/EmptyTransformation";

    public override void Update(Player player, ref int buffIndex) {
        OmnitrixPlayer omnitrixPlayer = player.GetModPlayer<OmnitrixPlayer>();
        omnitrixPlayer.currentTransformationId = "Ben10Mod:ChromaStone";
        omnitrixPlayer.isTransformed = true;
    }

    public override bool RightClick(int buffIndex) => false;
}
