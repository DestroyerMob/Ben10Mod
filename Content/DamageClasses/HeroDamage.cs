using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.DamageClasses {
    public class HeroDamage : DamageClass {

        public override void SetDefaultStats(Player player) {
            base.SetDefaultStats(player);
        }

        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
            if (damageClass == DamageClass.Generic)
                return StatInheritanceData.Full;

            return StatInheritanceData.None;
        }

        public override bool UseStandardCritCalcs => true;
    }
}
