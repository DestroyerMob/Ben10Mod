using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs
{
    public class EnemySlow : ModBuff
    {
        public override void Update(NPC npc, ref int buffIndex) {
            npc.velocity *= 0.25f;
            npc.color    =  new Color(0.7f, 0.85f, 1.25f);
        }

        public override bool RightClick(int buffIndex) => false;
    }
}
