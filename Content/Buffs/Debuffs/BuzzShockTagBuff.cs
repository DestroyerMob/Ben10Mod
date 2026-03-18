using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs;

public class BuzzShockTagBuff : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = false;
    }
}
