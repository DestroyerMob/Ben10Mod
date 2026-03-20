using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Abilities;

public class TertiaryAbility : ModBuff {
    public override string Texture => "Ben10Mod/Content/Buffs/Abilities/PrimaryAbility";

    public override void Update(Player player, ref int buffIndex) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();

        omp.TertiaryAbilityEnabled = true;
    }

    public override bool RightClick(int buffIndex) => false;
}
