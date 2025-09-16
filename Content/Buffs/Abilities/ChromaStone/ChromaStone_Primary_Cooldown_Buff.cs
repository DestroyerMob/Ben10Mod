using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Abilities.ChromaStone
{
    public class ChromaStone_Primary_Cooldown_Buff : ModBuff
    {
        public override bool RightClick(int buffIndex) {
            return false;
        }
    }
}
