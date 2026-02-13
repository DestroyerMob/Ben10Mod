using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs;

public class GhostFreakPossesion : ModBuff {
    public override bool RightClick(int buffIndex) => false;

    public override void Update(NPC npc, ref int buffIndex) {
        npc.lifeRegen -= 12;
    }
}