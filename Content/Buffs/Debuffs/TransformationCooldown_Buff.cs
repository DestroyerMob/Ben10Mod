using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Transformations {
    public class UltimateAbility_Cooldown : ModBuff {
        public override string Texture => "Ben10Mod/Content/Buffs/Transformations/TransformationCooldown";
        private OmnitrixPlayer p;
        public override void Update(Player player, ref int buffIndex) {
            p = player.GetModPlayer<OmnitrixPlayer>();
        }

        public override bool RightClick(int buffIndex) => false;
    }
}
