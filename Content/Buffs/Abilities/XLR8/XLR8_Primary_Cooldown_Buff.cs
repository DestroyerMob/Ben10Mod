using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Abilities.XLR8
{
    public class XLR8_Primary_Cooldown_Buff : ModBuff
    {
        public override bool RightClick(int buffIndex) {
            return false;
        }

        public override void Update(Player player, ref int buffIndex) {
            if (player.buffTime[player.FindBuffIndex(ModContent.BuffType<XLR8_Primary_Cooldown_Buff>())] == 1) {
                SoundEngine.PlaySound(SoundID.MenuTick, player.position);
            }
        }
    }
}
