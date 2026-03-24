using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Transformations;

public abstract class SimpleTransformationBuffBase : ModBuff {
    protected abstract string TransformationId { get; }

    public override string Texture => "Ben10Mod/Content/Buffs/Transformations/EmptyTransformation";

    public override void Update(Player player, ref int buffIndex) {
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.currentTransformationId = TransformationId;
        omp.isTransformed = true;
    }

    public override bool RightClick(int buffIndex) => false;
}
