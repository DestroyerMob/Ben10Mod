using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Abilities.DiamondHead
{
    public class DiamondHead_Primary_Buff : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            OmnitrixPlayer p = player.GetModPlayer<OmnitrixPlayer>();
            p.DiamondHeadPrimaryAbilityEnabled = true;
            p.DiamondHeadPrimaryAbilityWasEnabled = true;
        }

        public override bool RightClick(int buffIndex) => false;
    }
}
