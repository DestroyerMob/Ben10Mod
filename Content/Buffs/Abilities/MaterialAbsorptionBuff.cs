using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Abilities;

public class MaterialAbsorptionBuff : ModBuff {
    public override void Update(Player player, ref int buffIndex) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.absorbedMaterialTime = player.buffTime[buffIndex];
    }

    public override bool RightClick(int buffIndex) => true;
}
