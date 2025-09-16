using Ben10Mod.Common.Systems.GemPasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Ben10Mod.Common.Systems {
    public class WorldSystem : ModSystem {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
            int shiniesIndex = tasks.FindIndex(t => t.Name.Equals("Shinies"));
            if (shiniesIndex != -1) {
                tasks.Insert(shiniesIndex + 1, new CongealedCodonOreGenPass("Congealed Ore Pass", 10));
            }
        }
    }
}
