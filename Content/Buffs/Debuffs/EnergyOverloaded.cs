using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs;

public class EnergyOverloaded : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = false;
        BuffID.Sets.LongerExpertDebuff[Type] = false;
    }

    public override bool RightClick(int buffIndex) => false;
}
