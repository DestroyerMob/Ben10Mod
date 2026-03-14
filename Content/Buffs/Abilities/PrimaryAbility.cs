using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Abilities;

public class PrimaryAbility : ModBuff {
    public override void Update(Player player, ref int buffIndex) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();

        omp.PrimaryAbilityEnabled = true;
    }
    
    public override bool RightClick(int buffIndex) => false;
}
