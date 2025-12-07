using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Transformations {
    public class RipJaws_Buff : ModBuff {
        public override string Texture => "Ben10Mod/Content/Buffs/Transformations/EmptyTransformation";
        private OmnitrixPlayer p;
        public override void Update(Player player, ref int buffIndex) {
            p = player.GetModPlayer<OmnitrixPlayer>();

            p.isTransformed = true;
            p.wasTransformed = true;
        }
        public override bool RightClick(int buffIndex) => false;
    }
}
